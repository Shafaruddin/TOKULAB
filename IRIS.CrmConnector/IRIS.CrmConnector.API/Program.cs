using Abp.Extensions;
using IRIS.CrmConnector.API.CRM;
using IRIS.CrmConnector.API.Filters;
using IRIS.CrmConnector.API.Filters.Wrapping;
using IRIS.CrmConnector.API.Swagger;
using IRIS.CrmConnector.Shared;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json.Serialization;

using Hangfire;
using IRIS.CrmConnector.API.BackgroundWorkers;
using Hangfire.Dashboard;
using IRIS.CrmConnector.API.Hangfire;
using IRIS.CrmConnector.API.Storage;
using IRIS.CrmConnector.API.Storage.Interfaces;
using Microsoft.AspNetCore.Authentication;
using static IRIS.CrmConnector.Shared.Constants;
using IRIS.CrmConnector.API.Security.Authorization;
using IRIS.CrmConnector.API.Security.Swagger;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Builder;

var logsPath = Path.Combine(AppContext.BaseDirectory, "Logs");

if (!Directory.Exists(logsPath))
{
    Directory.CreateDirectory(logsPath);
}

var logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.File(Path.Combine(logsPath, "Log.txt"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 60)
    //azure diagnostics
    /*.WriteTo.File(
        System.IO.Path.Combine(Environment.GetEnvironmentVariable("HOME"), "LogFiles", "Application", "diagnostics.txt"),
        rollingInterval: RollingInterval.Day,
        fileSizeLimitBytes: 10 * 1024 * 1024,
        retainedFileCountLimit: 2,
        rollOnFileSizeLimit: true,
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(1)
    )*/
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(logger);

// configure basic authentication 
builder.Services.AddAuthentication(AUTHORIZATION_SCHEME.ADMIN_TOKEN_AUTHORIZATION)
    .AddScheme<AuthenticationSchemeOptions, AdminTokenAuthorizationHandler>(AUTHORIZATION_SCHEME.ADMIN_TOKEN_AUTHORIZATION, null);
builder.Services.AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(AUTHORIZATION_SCHEME.BASIC_AUTHENTICATION, null);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo() { Title = "ACTIVEO IRIS API", Version = "v1" });
    options.DocInclusionPredicate((docName, description) => true);
    options.ParameterFilter<SwaggerEnumParameterFilter>();
    options.SchemaFilter<SwaggerEnumSchemaFilter>();
    options.OperationFilter<SwaggerOperationIdFilter>();
    options.OperationFilter<SwaggerOperationFilter>();
    options.CustomDefaultSchemaIdSelector();
    options.AddSecurityDefinition(AUTHORIZATION_SCHEME.ADMIN_TOKEN_AUTHORIZATION, new OpenApiSecurityScheme
    {
        Description = $"\"{ADMIN_TOKEN}\" Header. Example: \"c6a7fa06-2d7a-4b62-b37d-c37d5ab334ca\"",
        In = ParameterLocation.Header,
        Name = ADMIN_TOKEN,
        Type = SecuritySchemeType.ApiKey
    });
    options.AddSecurityDefinition(AUTHORIZATION_SCHEME.BASIC_AUTHENTICATION, new OpenApiSecurityScheme
    {
        Description = $"username/password auth (Example: Basic XXXXXXXX)",
        In = ParameterLocation.Header,
        Name = AUTHORIZATION_SCHEME.BASIC_AUTHENTICATION,
        Type = SecuritySchemeType.Http,
        Scheme = "basic"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = ADMIN_TOKEN }
            },
            new[] { "ACTIVEO IRIS API" }
        }
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
    options.OperationFilter<AddCustomHeaderParameter>();
}).AddSwaggerGenNewtonsoftSupport();

// The following line enables Application Insights telemetry collection.
var options = new ApplicationInsightsServiceOptions();
options.ConnectionString = builder.Configuration.GetValue<string>(APPLICATIONINSIGHTS_CONNECTIONSTRING);
builder.Services.AddApplicationInsightsTelemetry(options);

builder.Services.AddMvc(options =>
{
    options.Filters.Add(new ExceptionFilter(new SerilogLoggerFactory(logger).CreateLogger<ExceptionFilter>()));
    options.Filters.Add(new ResultFilter(new ActionResultWrapperFactory()));
});


//builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

builder.Services.AddSingleton<ICrmManager, CrmManager>();
builder.Services.AddSingleton<CrmClient>();
builder.Services.AddSingleton<IStorageService, AzureStorageService>();

builder.Services.AddMemoryCache();

builder.Services.AddCors(
    options => options.AddPolicy(
        name: CORS_POLICY,
        corsBuilder => corsBuilder
            .WithOrigins(
                // App:CorsOrigins in appsettings.json can contain more than one address separated by comma.
                builder.Configuration.GetValue<string>(CORS_ORIGINS)
                    .Split(",", StringSplitOptions.RemoveEmptyEntries)
                    .Select(o => o.RemovePostFix("/"))
                    .ToArray()
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    )
);

// Hangfire

builder.Services.AddTransient<IFetchCategoriesAndAdHocCriteriaJob, FetchCategoriesAndAdHocCriteriaJob>();
builder.Services.AddTransient<ISubmitAutoSavedCasesJob, SubmitAutoSavedCasesJob>();

builder.Services.AddHangfire(config =>
{
    config.UseInMemoryStorage();
});

builder.Services.AddHangfireServer();

builder.WebHost.UseKestrel(option => option.AddServerHeader = false);

var app = builder.Build();

var policyCollection = new HeaderPolicyCollection()
        .AddFrameOptionsDeny()
        .AddContentTypeOptionsNoSniff()
        .AddStrictTransportSecurityMaxAgeIncludeSubDomains(maxAgeInSeconds: 60 * 60 * 24 * 365) // maxage = one year in seconds
        .AddReferrerPolicyStrictOriginWhenCrossOrigin()
        .RemoveServerHeader();

app.UseSecurityHeaders(policyCollection);

var recurringJobManager = app.Services.GetService<IRecurringJobManager>();
recurringJobManager.AddOrUpdate(
    nameof(FetchCategoriesAndAdHocCriteriaJob),
    () => app.Services.GetService<IFetchCategoriesAndAdHocCriteriaJob>().Run(null),
    "30 17 * * *"
    ,
    TimeZoneInfo.Utc
);
recurringJobManager.AddOrUpdate(
    nameof(SubmitAutoSavedCasesJob),
    () => app.Services.GetService<ISubmitAutoSavedCasesJob>().Run(null),
    Cron.Minutely()
    ,
    TimeZoneInfo.Utc
);

app.UseSerilogRequestLogging();


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//app.UseSwagger(options: new Swashbuckle.AspNetCore.Swagger.SwaggerOptions());
app.UseHangfireDashboard("/hangfire", new DashboardOptions()
{
    IsReadOnlyFunc = (DashboardContext context) => true,
    Authorization = new[] { new DisableHangfireAuthorizationFilter() }
});
app.UseSwaggerUI();
app.UseSwagger();
//}

app.UseHttpsRedirection();

app.UseCors(CORS_POLICY);

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "Api is running");

var updateDns = builder.Configuration.GetValue<bool>(AZURE.UPDATE_DNS);

if (OperatingSystem.IsLinux() && updateDns)
{
    Log.Information("Updating DNS");
    Process process = new Process();
    process.StartInfo.FileName = "/bin/sh";
    string cmd = "echo nameserver 8.8.8.8 > /etc/resolv.conf && echo nameserver 8.8.4.4 >> /etc/resolv.conf && echo options ndots:0 timeout:15 attempts:2 >> /etc/resolv.conf";
    process.StartInfo.Arguments = $"-c \"{cmd}\"";

    process.StartInfo.RedirectStandardOutput = true;
    process.Start();
    process.WaitForExit();
    Log.Information(process.StandardOutput.ReadToEnd());
}
try
{
    Log.Information("Starting web host");

    app.Run();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
