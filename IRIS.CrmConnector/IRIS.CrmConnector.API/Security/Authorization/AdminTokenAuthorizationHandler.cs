using Abp.Runtime.Security;
using IRIS.CrmConnector.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace IRIS.CrmConnector.API.Security.Authorization;

public class AdminTokenAuthorizationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IConfiguration _configuration;

    public AdminTokenAuthorizationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        IConfiguration configuration,
        UrlEncoder encoder,
        ISystemClock clock,
        ILoggerFactory logger)
        : base(options, logger, encoder, clock)
    {
        _configuration = configuration;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!(Request.Headers.ContainsKey(Constants.ADMIN_TOKEN) && Request.Headers.ContainsKey(Constants.ADMIN_TOKEN.ToLower())))
            return AuthenticateResult.Fail($"Missing {Constants.ADMIN_TOKEN} Header");

        try
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers[Constants.ADMIN_TOKEN]) ?? AuthenticationHeaderValue.Parse(Request.Headers[Constants.ADMIN_TOKEN.ToLower()]);

            if (!string.IsNullOrEmpty(authHeader.Scheme))
            {
                //Extract credentials
                string privateKey = authHeader.Scheme.Trim();

                if (privateKey != _configuration[Constants.ADMIN_TOKEN])
                {
                    return AuthenticateResult.Fail($"Invalid {Constants.ADMIN_TOKEN}");
                }
            }
        }
        catch
        {
            return AuthenticateResult.Fail("Invalid Authorization Header");
        }

        var claims = new[] {
            new Claim(AbpClaimTypes.UserName, "Admin"),
            new Claim(AbpClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.AuthenticationMethod, Constants.AUTHORIZATION_SCHEME.ADMIN_TOKEN_AUTHORIZATION)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
