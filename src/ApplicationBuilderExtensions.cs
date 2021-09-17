using System;
using Microsoft.AspNetCore.Builder;

namespace Delobytes.AspNetCore.Logging
{
    /// <summary>
    /// Методы расширения.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Добавляет прослойку логирования IP-адреса пользователя.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseNetworkLogging(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<NetworkLoggingMiddleware>();
        }

        /// <summary>
        /// Добавляет прослойку логирования удостоверений пользователя. Прослойка обрамляет контекст логирования
        /// специальными удостоверениями пользовательской сущности.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseClaimsLogging(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<ClaimsLoggingMiddleware>();
        }
    }
}
