using System;
using Microsoft.Extensions.DependencyInjection;

namespace Delobytes.AspNetCore.Logging
{
    /// <summary>
    /// Расширения <see cref="IServiceCollection"/> для регистрации сервисов.
    /// </summary>
    public static class LoggingServiceCollectionExtensions
    {
        /// <summary>
        /// Добавляет в <see cref="IServiceCollection"/> настройки логирования удостоверений пользователя.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> в которую нужно добавить логирование.</param>
        /// <param name="configure"><see cref="Action{AuthenticationContextOptions}"/> для настройки <see cref="ClaimsLoggingOptions"/>.</param>
        /// <returns>Ссылка на этот экземпляр после завершения операции.</returns>
        public static IServiceCollection AddClaimsLogging(this IServiceCollection services, Action<ClaimsLoggingOptions> configure)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            services.Configure(configure);

            return services;
        }
    }
}
