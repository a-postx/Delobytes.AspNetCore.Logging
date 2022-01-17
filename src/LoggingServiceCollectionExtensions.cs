using System;
using Microsoft.Extensions.DependencyInjection;

namespace Delobytes.AspNetCore.Logging;

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
    public static IServiceCollection AddClaimsLogging(this IServiceCollection services, Action<ClaimsLoggingOptions> configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.AddOptions();

        return services;
    }

    /// <summary>
    /// Добавляет в <see cref="IServiceCollection"/> настройки логирования HTTP-контекста.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> в которую нужно добавить логирование.</param>
    /// <param name="configure"><see cref="Action{HttpContextLoggingOptions}"/> для настройки <see cref="HttpContextLoggingOptions"/>.</param>
    /// <returns>Ссылка на этот экземпляр после завершения операции.</returns>
    public static IServiceCollection AddHttpContextLogging(this IServiceCollection services, Action<HttpContextLoggingOptions> configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.AddOptions();

        return services;
    }

    /// <summary>
    /// Добавляет в <see cref="IServiceCollection"/> настройки логирования контекста идемпотентности.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> в которую нужно добавить логирование.</param>
    /// <param name="configure"><see cref="Action{IdempotencyContextLoggingOptions}"/> для настройки <see cref="IdempotencyLoggingOptions"/>.</param>
    /// <returns>Ссылка на этот экземпляр после завершения операции.</returns>
    public static IServiceCollection AddIdempotencyContextLogging(this IServiceCollection services, Action<IdempotencyLoggingOptions> configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.AddOptions();

        return services;
    }
}
