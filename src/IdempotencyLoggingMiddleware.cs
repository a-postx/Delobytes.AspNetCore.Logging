using Microsoft.Extensions.Primitives;

namespace Delobytes.AspNetCore.Logging;

/// <summary>
/// Прослойка логирования контекста идемпотентности.
/// </summary>
[Obsolete("Прослойка устарела, пожалуйста используйте HeaderLoggingMiddleware.")]
public class IdempotencyLoggingMiddleware
{
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="next">Следующая прослойка в конвейере.</param>
    /// <param name="options">Настроки конфигурации.</param>
    public IdempotencyLoggingMiddleware(RequestDelegate next, IOptions<IdempotencyLoggingOptions> options)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    private readonly IdempotencyLoggingOptions _options;
    private readonly RequestDelegate _next;

    /// <summary>
    /// Обработчик, который добавляет ключ идемпотентности в контекст логирования.
    /// </summary>
    /// <param name="httpContext"><see cref="HttpContext"/> текущего запроса.</param>
    /// <param name="logger">Экземпляр <see cref="ILogger"/>.</param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext httpContext, ILogger<IdempotencyLoggingMiddleware> logger)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        if (httpContext.Request.Headers.TryGetValue(_options.IdempotencyHeader, out StringValues idempotencyKeyValue))
        {
            using (logger.BeginScopeWith((_options.IdempotencyLogAttribute, idempotencyKeyValue.ToString())))
            {
                await _next(httpContext);
            }
        }
        else
        {
            await _next(httpContext);
        }
    }
}
