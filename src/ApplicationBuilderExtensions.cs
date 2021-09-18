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
        /// Добавляет прослойку логирования IP-адреса.
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

        /// <summary>
        /// Добавляет прослойку логирования HTTP-контекста.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseHttpContextLogging(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<HttpContextLoggingMiddleware>();
        }

        /// <summary>
        /// Добавляет прослойку логирования контекста идемпотентности.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseIdempotencyContextLogging(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<IdempotencyLoggingMiddleware>();
        }
    }
}
