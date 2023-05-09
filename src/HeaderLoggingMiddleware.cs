using Microsoft.Extensions.Primitives;

namespace Delobytes.AspNetCore.Logging;

/// <summary>
/// Прослойка логирования заголовка.
/// </summary>
public class HeaderLoggingMiddleware
{
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="next">Следующая прослойка в конвейере.</param>
    /// <param name="options">Настроки конфигурации.</param>
    public HeaderLoggingMiddleware(RequestDelegate next, IOptions<HeaderLoggingOptions> options)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    private readonly HeaderLoggingOptions _options;
    private readonly RequestDelegate _next;

    /// <summary>
    /// Обработчик, который добавляет значение заголовка в контекст логирования.
    /// </summary>
    /// <param name="httpContext"><see cref="HttpContext"/> текущего запроса.</param>
    /// <param name="logger">Экземпляр <see cref="ILogger"/>.</param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext httpContext, ILogger<HeaderLoggingMiddleware> logger)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        if (_options.HeaderName != string.Empty && _options.HeaderLogsName!= string.Empty)
        {
            if (httpContext.Request.Headers.TryGetValue(_options.HeaderName, out StringValues idempotencyKeyValue))
            {
                using (logger.BeginScopeWith((_options.HeaderLogsName, idempotencyKeyValue.ToString())))
                {
                    await _next(httpContext);
                }
            }
            else
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
