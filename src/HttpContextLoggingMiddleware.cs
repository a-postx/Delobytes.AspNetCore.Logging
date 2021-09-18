using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Delobytes.AspNetCore.Logging
{
    /// <summary>
    /// Прослойка логирования HTTP-контекста.
    /// </summary>
    public class HttpContextLoggingMiddleware
    {
        public HttpContextLoggingMiddleware(RequestDelegate next, IOptions<HttpContextLoggingOptions> options)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        private readonly HttpContextLoggingOptions _options;
        private readonly RequestDelegate _next;

        public async Task InvokeAsync(HttpContext httpContext, ILogger<HttpContextLoggingMiddleware> logger)
        {
            HttpContext context = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            CancellationToken cancellationToken = context.RequestAborted;

            bool skipLogging = false;

            if (_options.SkipPaths is not null && _options.SkipPaths.Any(p => p.Value == context.Request.Path))
            {
                skipLogging = true;
            }

            if (!skipLogging)
            {
                httpContext.Request.EnableBuffering();
                Stream body = httpContext.Request.Body;
                byte[] buffer = new byte[Convert.ToInt32(httpContext.Request.ContentLength, CultureInfo.InvariantCulture)];
                await httpContext.Request.Body.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                string initialRequestBody = Encoding.UTF8.GetString(buffer);
                body.Seek(0, SeekOrigin.Begin);
                httpContext.Request.Body = body;

                //поля logz.io/logstash могут принимать только строки размером 32 тыс. символов, поэтому обрезаем тела
                if (initialRequestBody.Length > _options.MaxBodyLength)
                {
                    initialRequestBody = initialRequestBody.Substring(0, _options.MaxBodyLength);
                }

                //если какое-то значение заголовка приводится к нулл, то событие не логируется (Unable to submit log event to ELK)
                using (logger.BeginScope(context.Request.Headers
                    .ToDictionary(h => $"{LogKeys.RequestHeaders}.{h.Key}", h => h.Value.ToString() as object)))
                using (logger.BeginScopeWith((LogKeys.RequestBody, initialRequestBody),
                    (LogKeys.RequestProtocol, context.Request.Protocol),
                    (LogKeys.RequestScheme, context.Request.Scheme),
                    (LogKeys.RequestHost, context.Request.Host.Value),
                    (LogKeys.RequestMethod, context.Request.Method),
                    (LogKeys.RequestPath, context.Request.Path),
                    (LogKeys.RequestQuery, context.Request.QueryString),
                    (LogKeys.RequestPathAndQuery, GetFullPath(context))))
                {
                    logger.LogInformation("HTTP request received.");
                }

                using MemoryStream responseBodyMemoryStream = new MemoryStream();

                Stream originalResponseBodyReference = context.Response.Body;
                context.Response.Body = responseBodyMemoryStream;

                await _next(context);

                context.Response.Body.Seek(0, SeekOrigin.Begin);

                string responseBody;

                using StreamReader sr = new StreamReader(context.Response.Body);
                responseBody = await sr.ReadToEndAsync();
                context.Response.Body.Seek(0, SeekOrigin.Begin);

                string endResponseBody = (responseBody.Length > _options.MaxBodyLength) ?
                    responseBody.Substring(0, _options.MaxBodyLength) : responseBody;

                //если какое-то значение заголовка приводится к нулл, то событие не логируется (Unable to submit log event to ELK)
                using (logger.BeginScope(context.Response.Headers
                    .ToDictionary(h => $"{LogKeys.ResponseHeaders}.{h.Key}", h => h.Value.ToString() as object)))
                using (logger.BeginScopeWith((LogKeys.StatusCode, context.Response.StatusCode),
                    (LogKeys.ResponseBody, endResponseBody),
                    (LogKeys.RequestProtocol, context.Request.Protocol),
                    (LogKeys.RequestScheme, context.Request.Scheme),
                    (LogKeys.RequestHost, context.Request.Host.Value),
                    (LogKeys.RequestMethod, context.Request.Method),
                    (LogKeys.RequestPath, context.Request.Path),
                    (LogKeys.RequestQuery, context.Request.QueryString),
                    (LogKeys.RequestPathAndQuery, GetFullPath(context)),
                    (LogKeys.RequestAborted, context.RequestAborted.IsCancellationRequested)))
                {
                    logger.LogInformation("HTTP request handled.");
                }

                await responseBodyMemoryStream.CopyToAsync(originalResponseBodyReference, cancellationToken);
            }
            else
            {
                await _next(context);
            }
        }

        private static string GetFullPath(HttpContext httpContext)
        {
            /*
                In some cases, like when running integration tests with WebApplicationFactory<T>
                the RawTarget returns an empty string instead of null, in that case we can't use
                ?? as fallback.
            */
            string requestPath = httpContext.Features.Get<IHttpRequestFeature>()?.RawTarget;

            if (string.IsNullOrEmpty(requestPath))
            {
                requestPath = httpContext.Request.Path.ToString();
            }

            return requestPath;
        }
    }
}
