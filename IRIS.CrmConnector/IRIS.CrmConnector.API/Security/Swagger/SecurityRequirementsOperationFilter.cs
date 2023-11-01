using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using static IRIS.CrmConnector.Shared.Constants;

namespace IRIS.CrmConnector.API.Security.Swagger;

internal class SecurityRequirementsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context != null && operation != null)
        {
            // AuthenticationSchemes map to scopes
            // for class level authentication schemes
            var requiredScopes = context.MethodInfo.DeclaringType
                    .GetCustomAttributes(true)
                    .OfType<AuthorizeAttribute>()
                    .Select(attr => attr.AuthenticationSchemes)
                    .Distinct();

            //  for method level authentication scheme
            var requiredScopes2 = context.MethodInfo
                    .GetCustomAttributes(true)
                    .OfType<AuthorizeAttribute>()
                    .Select(attr => attr.AuthenticationSchemes)
                    .Distinct();

            bool requireAuth = false;
            string id = "";

            if (requiredScopes.Contains(AUTHORIZATION_SCHEME.ADMIN_TOKEN_AUTHORIZATION) || requiredScopes2.Contains(AUTHORIZATION_SCHEME.ADMIN_TOKEN_AUTHORIZATION))
            {
                requireAuth = true;
                id = AUTHORIZATION_SCHEME.ADMIN_TOKEN_AUTHORIZATION;
            }
            //else if (requiredScopes.Contains("ApiKey") || requiredScopes2.Contains("ApiKey"))
            //{
            //    requireAuth = true;
            //    id = "apiKeyAuth";
            //}
            //else if (requiredScopes.Contains("Basic") || requiredScopes2.Contains("Basic"))
            //{
            //    requireAuth = true;
            //    id = "basicAuth";
            //}

            if (requireAuth && !string.IsNullOrEmpty(id))
            {
                operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });

                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = id }
                            },
                            new[] { "ACTIVEO IRIS API" }
                        }
                    }
                };
            }
        }
    }
}
