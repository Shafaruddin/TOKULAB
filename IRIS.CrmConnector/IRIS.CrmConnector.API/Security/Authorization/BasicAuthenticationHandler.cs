using Abp.Runtime.Security;
using IRIS.CrmConnector.API.Storage.Interfaces;
using IRIS.CrmConnector.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace IRIS.CrmConnector.API.Security.Authorization;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IStorageService _storageService;
    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IStorageService storageService) :
       base(options, logger, encoder, clock)
    {
        _storageService = storageService;
    }

    protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorizationHeader = Request.Headers["Authorization"].ToString();
        if (authorizationHeader != null && authorizationHeader.StartsWith("basic", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var token = authorizationHeader.Substring("Basic ".Length).Trim();
                var credentialsAsEncodedString = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var credentials = credentialsAsEncodedString.Split(':');
                await _storageService.AuthenticateUser(credentials[0], credentials[1]);

                var claims = new[] {
                    new Claim(AbpClaimTypes.UserName, credentials[0].ToLowerInvariant()),
                    new Claim(AbpClaimTypes.Role, "User"),
                    new Claim(ClaimTypes.AuthenticationMethod, Constants.AUTHORIZATION_SCHEME.BASIC_AUTHENTICATION)
                };

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                return await Task.FromResult(AuthenticateResult.Fail("Authorization Failure"));
            }
        }
        return await Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
    }
}