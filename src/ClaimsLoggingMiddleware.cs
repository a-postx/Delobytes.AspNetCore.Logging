using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Delobytes.AspNetCore.Logging
{
    /// <summary>
    /// Прослойка логирования удостоверений пользователя.
    /// </summary>
    public class ClaimsLoggingMiddleware
    {
        public ClaimsLoggingMiddleware(RequestDelegate next, IOptions<ClaimsLoggingOptions> options)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        private readonly ClaimsLoggingOptions _options;
        private readonly RequestDelegate _next;

        public async Task InvokeAsync(HttpContext httpContext, ILogger<ClaimsLoggingMiddleware> logger)
        {
            HttpContext context = httpContext ?? throw new ArgumentNullException(nameof(httpContext));

            if (context.User.Identity is not null && context.User.Identity.IsAuthenticated)
            {
                string userIdClaimValue = context.User.GetClaimValue<string>(_options.UserIdClaimName);
                string tenantIdClaimValue = context.User.GetClaimValue<string>(_options.TenantIdClaimName);

                using (logger.BeginScopeWith((LogKeys.UserId, userIdClaimValue), (LogKeys.TenantId, tenantIdClaimValue)))
                {
                    if (_options.ClaimNames.Length > 0)
                    {
                        Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();

                        foreach (string claimName in _options.ClaimNames)
                        {
                            string claimValue = context.User.GetClaimValue<string>(claimName);

                            if (!string.IsNullOrWhiteSpace(claimValue))
                            {
                                keyValuePairs.Add(claimName, claimValue);
                            }
                        }

                        using (logger.BeginScope(keyValuePairs))
                        {
                            await _next(context);
                        }
                    }
                    else
                    {
                        await _next(context);
                    }
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}
