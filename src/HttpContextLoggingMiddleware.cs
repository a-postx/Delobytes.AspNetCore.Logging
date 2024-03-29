using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Http.Features;

namespace Delobytes.AspNetCore.Logging;

/// <summary>
/// Прослойка логирования HTTP-контекста. Прослойка логирует данные запроса и ответа в отдельных событиях.
/// </summary>
public class HttpContextLoggingMiddleware
{
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="next">Следующая прослойка в конвейере.</param>
    /// <param name="options">Настроки конфигурации.</param>
    public HttpContextLoggingMiddleware(RequestDelegate next, IOptions<HttpContextLoggingOptions> options)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    private readonly HttpContextLoggingOptions _options;
    private readonly RequestDelegate _next;

    /// <summary>
    /// Обработчик, который помещает свойства HTTP-контекста в контекст логирования.
    /// </summary>
    /// <param name="httpContext"><see cref="HttpContext"/> текущего запроса.</param>
    /// <param name="logger">Экземпляр <see cref="ILogger"/>.</param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext httpContext, ILogger<HttpContextLoggingMiddleware> logger)
    {
        ArgumentNullException.ThrowIfNull(nameof(httpContext));
        ArgumentNullException.ThrowIfNull(nameof(logger));

        if (_options.SkipPaths is null)
        {
            throw new InvalidOperationException($"{nameof(_options.SkipPaths)} option value is not valid.");
        }

        if (_options.SkipRequestHeaders is null)
        {
            throw new InvalidOperationException($"{nameof(_options.SkipRequestHeaders)} option value is not valid.");
        }

        if (_options.SkipResponseHeaders is null)
        {
            throw new InvalidOperationException($"{nameof(_options.SkipResponseHeaders)} option value is not valid.");
        }

        CancellationToken cancellationToken = httpContext.RequestAborted;

        bool skipLogging = false;

        if (_options.SkipPaths.Any(p => p.Value == httpContext.Request.Path))
        {
            skipLogging = true;
        }

        if (!skipLogging)
        {
            Dictionary<string, object> requestHeaders = GetValidRequestHeaders(httpContext.Request.Headers);

            foreach (string key in _options.SkipRequestHeaders)
            {
                requestHeaders.Remove($"{LoggingLogKeys.RequestHeaders}.{key}");
            }

            using (logger.BeginScope(requestHeaders))
            using (logger.BeginScopeWith((LoggingLogKeys.RequestProtocol, httpContext.Request.Protocol),
                (LoggingLogKeys.RequestScheme, httpContext.Request.Scheme),
                (LoggingLogKeys.RequestHost, httpContext.Request.Host.Value),
                (LoggingLogKeys.RequestMethod, httpContext.Request.Method),
                (LoggingLogKeys.RequestPath, httpContext.Request.Path),
                (LoggingLogKeys.RequestQuery, httpContext.Request.QueryString),
                (LoggingLogKeys.RequestPathAndQuery, GetFullPath(httpContext))))
            {
                if (_options.LogRequestBody)
                {
                    httpContext.Request.EnableBuffering();
                    Stream body = httpContext.Request.Body;
                    byte[] buffer = new byte[Convert.ToInt32(httpContext.Request.ContentLength, CultureInfo.InvariantCulture)];
                    await httpContext.Request.Body.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                    string initialRequestBody = Encoding.UTF8.GetString(buffer);
                    body.Seek(0, SeekOrigin.Begin);
                    httpContext.Request.Body = body;

                    if (_options.MaxBodyLength > 0 && initialRequestBody.Length > _options.MaxBodyLength)
                    {
                        initialRequestBody = initialRequestBody.Substring(0, _options.MaxBodyLength);
                    }

                    using (logger.BeginScopeWith((LoggingLogKeys.RequestBody, initialRequestBody)))
                    {
                        logger.LogInformation("HTTP request received.");
                    }
                }
                else
                {
                    logger.LogInformation("HTTP request received.");
                }
            }

            if (_options.LogResponseBody)
            {
                using MemoryStream responseBodyMemoryStream = new MemoryStream();

                Stream originalResponseBodyReference = httpContext.Response.Body;
                httpContext.Response.Body = responseBodyMemoryStream;

                await _next(httpContext);

                httpContext.Response.Body.Seek(0, SeekOrigin.Begin);

                string responseBody;

                using StreamReader sr = new StreamReader(httpContext.Response.Body);
                responseBody = await sr.ReadToEndAsync();
                httpContext.Response.Body.Seek(0, SeekOrigin.Begin);

                string endResponseBody = (_options.MaxBodyLength > 0 && responseBody.Length > _options.MaxBodyLength)
                    ? responseBody.Substring(0, _options.MaxBodyLength)
                    : responseBody;

                Dictionary<string, object> responseHeaders = GetValidResponseHeaders(httpContext.Response.Headers);

                foreach (string key in _options.SkipResponseHeaders)
                {
                    responseHeaders.Remove($"{LoggingLogKeys.ResponseHeaders}.{key}");
                }

                using (logger.BeginScope(responseHeaders))
                using (logger.BeginScopeWith((LoggingLogKeys.StatusCode, httpContext.Response.StatusCode),
                    (LoggingLogKeys.ResponseBody, endResponseBody),
                    (LoggingLogKeys.RequestProtocol, httpContext.Request.Protocol),
                    (LoggingLogKeys.RequestScheme, httpContext.Request.Scheme),
                    (LoggingLogKeys.RequestHost, httpContext.Request.Host.Value),
                    (LoggingLogKeys.RequestMethod, httpContext.Request.Method),
                    (LoggingLogKeys.RequestPath, httpContext.Request.Path),
                    (LoggingLogKeys.RequestQuery, httpContext.Request.QueryString),
                    (LoggingLogKeys.RequestPathAndQuery, GetFullPath(httpContext)),
                    (LoggingLogKeys.RequestAborted, httpContext.RequestAborted.IsCancellationRequested)))
                {
                    logger.LogInformation("HTTP request handled.");
                }

                await responseBodyMemoryStream.CopyToAsync(originalResponseBodyReference, cancellationToken);
            }
            else
            {
                await _next(httpContext);

                Dictionary<string, object> responseHeaders = GetValidResponseHeaders(httpContext.Response.Headers);

                foreach (string key in _options.SkipResponseHeaders)
                {
                    responseHeaders.Remove($"{LoggingLogKeys.ResponseHeaders}.{key}");
                }

                using (logger.BeginScope(responseHeaders))
                using (logger.BeginScopeWith((LoggingLogKeys.StatusCode, httpContext.Response.StatusCode),
                    (LoggingLogKeys.RequestProtocol, httpContext.Request.Protocol),
                    (LoggingLogKeys.RequestScheme, httpContext.Request.Scheme),
                    (LoggingLogKeys.RequestHost, httpContext.Request.Host.Value),
                    (LoggingLogKeys.RequestMethod, httpContext.Request.Method),
                    (LoggingLogKeys.RequestPath, httpContext.Request.Path),
                    (LoggingLogKeys.RequestQuery, httpContext.Request.QueryString),
                    (LoggingLogKeys.RequestPathAndQuery, GetFullPath(httpContext)),
                    (LoggingLogKeys.RequestAborted, httpContext.RequestAborted.IsCancellationRequested)))
                {
                    logger.LogInformation("HTTP request handled.");
                }
            }
        }
        else
        {
            await _next(httpContext);
        }
    }

    private static Dictionary<string, object> GetValidRequestHeaders(IHeaderDictionary headers)
    {
        Dictionary<string, object> validHeaders = ConvertHeadersToDicWithPrefix(headers, LoggingLogKeys.RequestHeaders);

        //если какое-то значение заголовка приводится к неопределённости, то всё событие
        //может не логироваться у определённых провайдеров (например, ELK), поэтому удаляем
        IEnumerable<string> emptyHeaders = validHeaders.Where(e => e.Value is null).Select(s => s.Key);

        foreach (string key in emptyHeaders)
        {
            validHeaders.Remove(key);
        }

        return validHeaders;
    }

    private static Dictionary<string, object> GetValidResponseHeaders(IHeaderDictionary headers)
    {
        Dictionary<string, object> validHeaders = ConvertHeadersToDicWithPrefix(headers, LoggingLogKeys.ResponseHeaders);

        IEnumerable<string> emptyHeaders = validHeaders.Where(e => e.Value is null).Select(s => s.Key);

        foreach (string key in emptyHeaders)
        {
            validHeaders.Remove(key);
        }

        return validHeaders;
    }

    private static Dictionary<string, object> ConvertHeadersToDicWithPrefix(IHeaderDictionary headers, string keyPrefix)
    {
        return headers.ToDictionary(h => $"{keyPrefix}.{h.Key}", h => h.Value.ToString() as object);
    }

    private static string GetFullPath(HttpContext httpContext)
    {
        /*
            In some cases, like when running integration tests with WebApplicationFactory<T>
            the RawTarget returns an empty string instead of null, in that case we can't use
            ?? as fallback.
        */
        string? requestPath = httpContext.Features.Get<IHttpRequestFeature>()?.RawTarget;

        if (string.IsNullOrEmpty(requestPath))
        {
            requestPath = httpContext.Request.Path.ToString();
        }

        return requestPath;
    }
}
