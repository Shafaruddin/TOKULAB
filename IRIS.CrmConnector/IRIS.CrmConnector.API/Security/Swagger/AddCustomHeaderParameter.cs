using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using static IRIS.CrmConnector.Shared.Constants;

namespace IRIS.CrmConnector.API.Security.Swagger;

public class AddCustomHeaderParameter
: IOperationFilter
{
    public void Apply(
        OpenApiOperation operation,
        OperationFilterContext context)
    {
        if (operation.Parameters is null)
        {
            operation.Parameters = new List<OpenApiParameter>();
        }

        var globalAttributes = context.ApiDescription.ActionDescriptor.FilterDescriptors.Select(p => p.Filter);
        var controllerAttributes = context.MethodInfo?.DeclaringType?.GetCustomAttributes(true);
        var methodAttributes = context.MethodInfo?.GetCustomAttributes(true);
        var produceAttributes = globalAttributes
            .Union(controllerAttributes ?? throw new InvalidOperationException())
            .Union(methodAttributes)
            .ToList();

        if (produceAttributes != null && produceAttributes.Any())
        {
            if (produceAttributes.Where(x => x.GetType() == typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute)).Any())
            {
                var authorizeAttribute = (AuthorizeAttribute)produceAttributes.Where(x => x.GetType() == typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute)).First();
                if (authorizeAttribute.AuthenticationSchemes != null)
                {
                    if (authorizeAttribute.AuthenticationSchemes.Contains(AUTHORIZATION_SCHEME.ADMIN_TOKEN_AUTHORIZATION))
                    {
                        operation.Parameters.Add(new OpenApiParameter
                        {

                            Description = $"\"{ADMIN_TOKEN}\" Header. Example: \"c6a7fa06-2d7a-4b62-b37d-c37d5ab334ca\"",
                            In = ParameterLocation.Header,
                            Name = ADMIN_TOKEN,
                            AllowEmptyValue = false,
                            Required = true
                        });
                    }
                    if (authorizeAttribute.AuthenticationSchemes.Contains(AUTHORIZATION_SCHEME.BASIC_AUTHENTICATION))
                    {
                        operation.Parameters.Add(new OpenApiParameter
                        {
                            Description = $"Example: \"Basic XXXXXXXX\"",
                            In = ParameterLocation.Header,
                            Name = "Authorization",
                            AllowEmptyValue = true,
                            Required = false
                        });
                    }
                }
            }
        }
    }
}
