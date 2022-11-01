using System.IO;
using System.IO.Pipelines;
using System.Text;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;

namespace Delobytes.AspNetCore.Logging.Tests;

public class HttpContextLoggingMiddlewareTests
{
    private static readonly string RequestPath = "/json";
    private static readonly string SecretRequestHeader = nameof(SecretRequestHeader);
    private static readonly string SecretResponseHeader = nameof(SecretResponseHeader);
    private static readonly Dictionary<string, StringValues> RequestHeaders = new Dictionary<string, StringValues>
            {
                { "OpenRequestHeader", "openReqHeaderValue" },
                { SecretRequestHeader, "secretRequestValue" }
            };
    private static readonly Dictionary<string, StringValues> ResponseHeaders = new Dictionary<string, StringValues>
            {
                { "OpenResponseHeader", "OpenRespHeaderValue" },
                { SecretResponseHeader, "secretResponseValue" }
            };
    private static readonly string LongBody = @"На другой или на третий день после переезда Епанчиных
с утренним поездом из Москвы прибыл и князь Лев Николаевич Мышкин.";

    [Fact]
    public async Task HttpContextMessagesAdded_WhenDefaultOptions()
    {
        IOptions<HttpContextLoggingOptions> options = Options.Create(new HttpContextLoggingOptions());

        HttpContext ctx = GetContextWithRequestAndResponse(RequestHeaders, ResponseHeaders);

        ITestLoggerFactory loggerFactory = TestLoggerFactory.Create();

        await ActAsync(options, ctx, loggerFactory);

        loggerFactory.Sink.LogEntries.Should().NotBeEmpty().And.HaveCount(3);
        loggerFactory.Sink.Scopes.Should().NotBeEmpty().And.HaveCount(4);

        LogEntry requestLog = loggerFactory.Sink.LogEntries.ElementAt(0);
        requestLog.Message.Should().Be("HTTP request received.");
        requestLog.Scopes.Should().NotBeEmpty().And.HaveCount(2);

        Scope requestHeadersScope = requestLog.Scopes.First();
        requestHeadersScope.Properties.Should().HaveCount(2);
        Scope requestPropertiesScope = requestLog.Scopes.Last();
        requestPropertiesScope.Properties.Should().HaveCount(7);

        LogEntry innerLog = loggerFactory.Sink.LogEntries.ElementAt(1);
        innerLog.Message.Should().Be("test");
        innerLog.Scopes.Should().BeEmpty();

        LogEntry responseLog = loggerFactory.Sink.LogEntries.ElementAt(2);
        responseLog.Message.Should().Be("HTTP request handled.");
        responseLog.Scopes.Should().NotBeEmpty().And.HaveCount(2);

        Scope responseHeadersScope = responseLog.Scopes.First();
        responseHeadersScope.Properties.Should().HaveCount(2);
        Scope responsePropertiesScope = responseLog.Scopes.Last();
        responsePropertiesScope.Properties.Should().HaveCount(9);

        BeginScope? requestHeaderScope = loggerFactory.Sink.Scopes
            .Where(s => s.Properties.Any(e => e.Key.Split(".")[0] == LoggingLogKeys.RequestHeaders)).SingleOrDefault();
        requestHeaderScope.Should().NotBeNull();

        BeginScope? responseHeaderScope = loggerFactory.Sink.Scopes
            .Where(s => s.Properties.Any(e => e.Key.Split(".")[0] == LoggingLogKeys.ResponseHeaders)).SingleOrDefault();
        responseHeaderScope.Should().NotBeNull();
    }

    [Fact]
    public async Task RequestBodyAddedToScope_WhenEnabled()
    {
        IOptions<HttpContextLoggingOptions> options = Options.Create(new HttpContextLoggingOptions());
        options.Value.LogRequestBody = true;

        HttpContext ctx = GetContextWithRequestAndResponse(RequestHeaders, ResponseHeaders);

        ITestLoggerFactory loggerFactory = TestLoggerFactory.Create();

        await ActAsync(options, ctx, loggerFactory);

        loggerFactory.Sink.Scopes.Should().NotBeEmpty().And.HaveCount(5);

        LogEntry requestLog = loggerFactory.Sink.LogEntries.ElementAt(0);
        requestLog.Scopes.Should().NotBeEmpty().And.HaveCount(3);

        Scope requestPropertiesScope = requestLog.Scopes.Last();
        requestPropertiesScope.Properties.Should().HaveCount(1);
        requestPropertiesScope.Properties.First().Key.Should().Be(LoggingLogKeys.RequestBody);
    }

    [Fact]
    public async Task ResponseBodyAddedToScope_WhenEnabled()
    {
        IOptions<HttpContextLoggingOptions> options = Options.Create(new HttpContextLoggingOptions());
        options.Value.LogResponseBody = true;

        HttpContext ctx = GetContextWithRequestAndResponse(RequestHeaders, ResponseHeaders);

        ITestLoggerFactory loggerFactory = TestLoggerFactory.Create();

        await ActAsync(options, ctx, loggerFactory);

        loggerFactory.Sink.Scopes.Should().NotBeEmpty().And.HaveCount(4);

        LogEntry responseLog = loggerFactory.Sink.LogEntries.Last();
        responseLog.Scopes.Should().NotBeEmpty().And.HaveCount(2);

        Scope responsePropertiesScope = responseLog.Scopes.Last();
        responsePropertiesScope.Properties.Should().HaveCount(10);
        responsePropertiesScope.Properties.Any(p => p.Key == LoggingLogKeys.ResponseBody).Should().Be(true);
    }

    [Fact]
    public async Task BodiesNotCut_WhenZeroLength()
    {
        IOptions<HttpContextLoggingOptions> options = Options.Create(new HttpContextLoggingOptions());
        options.Value.LogRequestBody = true;
        options.Value.LogResponseBody = true;

        HttpContext ctx = GetContextWithRequestAndResponse(RequestHeaders, ResponseHeaders, LongBody, LongBody);

        ITestLoggerFactory loggerFactory = TestLoggerFactory.Create();

        await ActAsync(options, ctx, loggerFactory);

        loggerFactory.Sink.Scopes.Should().NotBeEmpty().And.HaveCount(5);

        LogEntry requestLog = loggerFactory.Sink.LogEntries.First();
        Scope requestPropertiesScope = requestLog.Scopes.Last();
        KeyValuePair<string, object> requestBodyProperty = requestPropertiesScope.Properties.First();
        requestBodyProperty.Key.Should().Be(LoggingLogKeys.RequestBody);
        string? requestBody = requestBodyProperty.Value as string;
        requestBody.Should().NotBeNullOrEmpty();
        requestBody.Should().Be(LongBody);


        LogEntry responseLog = loggerFactory.Sink.LogEntries.Last();
        Scope responsePropertiesScope = responseLog.Scopes.Last();
        KeyValuePair<string, object> responseBodyProperty = responsePropertiesScope.Properties
            .Where(e => e.Key == LoggingLogKeys.ResponseBody).First();
        string? responseBody = responseBodyProperty.Value as string;
        responseBody.Should().NotBeNullOrEmpty();
        responseBody.Should().Be(LongBody);
    }

    [Fact]
    public async Task BodiesCut_WhenLengthDefined()
    {
        IOptions<HttpContextLoggingOptions> options = Options.Create(new HttpContextLoggingOptions());
        options.Value.LogRequestBody = true;
        options.Value.LogResponseBody = true;
        options.Value.MaxBodyLength = 1;

        HttpContext ctx = GetContextWithRequestAndResponse(RequestHeaders, ResponseHeaders, LongBody, LongBody);

        ITestLoggerFactory loggerFactory = TestLoggerFactory.Create();

        await ActAsync(options, ctx, loggerFactory);

        loggerFactory.Sink.Scopes.Should().NotBeEmpty().And.HaveCount(5);

        LogEntry requestLog = loggerFactory.Sink.LogEntries.First();
        Scope requestPropertiesScope = requestLog.Scopes.Last();
        KeyValuePair<string, object> requestBodyProperty = requestPropertiesScope.Properties.First();
        requestBodyProperty.Key.Should().Be(LoggingLogKeys.RequestBody);
        string? requestBody = requestBodyProperty.Value as string;
        requestBody.Should().NotBeNullOrEmpty();
        requestBody.Should().Be(LongBody.Substring(0, 1));


        LogEntry responseLog = loggerFactory.Sink.LogEntries.Last();
        Scope responsePropertiesScope = responseLog.Scopes.Last();
        KeyValuePair<string, object> responseBodyProperty = responsePropertiesScope.Properties
            .Where(e => e.Key == LoggingLogKeys.ResponseBody).First();
        string? responseBody = responseBodyProperty.Value as string;
        responseBody.Should().NotBeNullOrEmpty();
        responseBody.Should().Be(LongBody.Substring(0, 1));
    }

    [Fact]
    public async Task PathSkipped_WhenAdded()
    {
        IOptions<HttpContextLoggingOptions> options = Options.Create(new HttpContextLoggingOptions());
        options.Value.SkipPaths = new List<PathString>() { RequestPath };

        HttpContext ctx = GetContextWithRequestAndResponse(RequestHeaders, ResponseHeaders);

        ITestLoggerFactory loggerFactory = TestLoggerFactory.Create();

        await ActAsync(options, ctx, loggerFactory);

        loggerFactory.Sink.Scopes.Should().BeEmpty();
        loggerFactory.Sink.LogEntries.Count().Should().Be(1);
    }

    [Fact]
    public async Task RequestHeaderIsNotLogged_WhenAdded()
    {
        IOptions<HttpContextLoggingOptions> options = Options.Create(new HttpContextLoggingOptions());
        options.Value.SkipRequestHeaders = new List<string>() { SecretRequestHeader };

        HttpContext ctx = GetContextWithRequestAndResponse(RequestHeaders, ResponseHeaders);

        ITestLoggerFactory loggerFactory = TestLoggerFactory.Create();

        await ActAsync(options, ctx, loggerFactory);

        IEnumerable<BeginScope> scopesWithSecretHeader = loggerFactory.Sink.Scopes
            .Where(e => e.Properties.Any(e => e.Key == $"{LoggingLogKeys.RequestHeaders}.{SecretRequestHeader}"));
        scopesWithSecretHeader.Count().Should().Be(0);
    }

    [Fact]
    public async Task ResponseHeaderIsNotLogged_WhenAdded()
    {
        IOptions<HttpContextLoggingOptions> options = Options.Create(new HttpContextLoggingOptions());
        options.Value.SkipResponseHeaders = new List<string>() { SecretResponseHeader };

        HttpContext ctx = GetContextWithRequestAndResponse(RequestHeaders, ResponseHeaders);

        ITestLoggerFactory loggerFactory = TestLoggerFactory.Create();

        await ActAsync(options, ctx, loggerFactory);

        IEnumerable<BeginScope> scopesWithSecretHeader = loggerFactory.Sink.Scopes
            .Where(e => e.Properties.Any(e => e.Key == $"{LoggingLogKeys.ResponseHeaders}.{SecretResponseHeader}"));
        scopesWithSecretHeader.Count().Should().Be(0);
    }

    private static async Task ActAsync(IOptions<HttpContextLoggingOptions> options, HttpContext ctx, ITestLoggerFactory loggerFactory)
    {
        ILogger<HttpContextLoggingMiddleware> logger = loggerFactory.CreateLogger<HttpContextLoggingMiddleware>();

        RequestDelegate requestDelegate = new RequestDelegate((innerContext) =>
        {
            logger.LogInformation("test");
            return Task.FromResult(0);
        });

        HttpContextLoggingMiddleware middleware = new HttpContextLoggingMiddleware(requestDelegate, options);

        await middleware.InvokeAsync(ctx, logger);
    }

    private HttpContext GetContextWithRequestAndResponse(Dictionary<string, StringValues> requestHeaders,
        Dictionary<string, StringValues> responseHeaders, string requestBody = "{ \"request\": true }",
        string responseBody = "{ \"response\": true }")
    {
        Mock<HttpContext> httpCtxMock = new Mock<HttpContext>();
        MockRepository mocks = new MockRepository(MockBehavior.Default);
        Mock<HttpRequest> mockRequest = mocks.Create<HttpRequest>();

        Mock<PipeReader> mockReqBodyReader = mocks.Create<PipeReader>();
        MemoryStream requestBodyMs = new MemoryStream();
        requestBodyMs.WriteAsync(Encoding.UTF8.GetBytes(requestBody));
        requestBodyMs.Seek(0, SeekOrigin.Begin);

        mockRequest.Setup(h => h.Body).Returns(requestBodyMs);
        mockRequest.Setup(h => h.BodyReader).Returns(mockReqBodyReader.Object);
        mockRequest.Setup(h => h.ContentLength).Returns(requestBodyMs.Length);
        mockRequest.Setup(h => h.Path).Returns(PathString.FromUriComponent(RequestPath));
        mockRequest.Setup(h => h.Protocol).Returns("HTTP/1.1");
        mockRequest.Setup(h => h.Scheme).Returns("http");
        mockRequest.Setup(h => h.Host).Returns(HostString.FromUriComponent("yandex.ru"));
        mockRequest.Setup(h => h.Method).Returns("POST");
        mockRequest.Setup(h => h.QueryString).Returns(QueryString.FromUriComponent("?pageSize=5"));
        mockRequest.Setup(p => p.Headers).Returns(new HeaderDictionary(requestHeaders));
        httpCtxMock.Setup(p => p.Request).Returns(mockRequest.Object);

        Mock<HttpResponse> mockResponse = mocks.Create<HttpResponse>();
        MemoryStream responseBodyMs = new MemoryStream();
        responseBodyMs.WriteAsync(Encoding.UTF8.GetBytes(responseBody));
        responseBodyMs.Seek(0, SeekOrigin.Begin);

        mockResponse.Setup(h => h.Body).Returns(responseBodyMs);
        mockResponse.Setup(h => h.StatusCode).Returns(200);
        mockResponse.Setup(p => p.Headers).Returns(new HeaderDictionary(responseHeaders));
        httpCtxMock.Setup(p => p.Response).Returns(mockResponse.Object);

        Mock<IFeatureCollection> mockIFeatureCollection = mocks.Create<IFeatureCollection>();
        mockIFeatureCollection.Setup(p => p.Get<IHttpRequestFeature>())
            .Returns(new HttpRequestFeature
            {
                Path = RequestPath,
                RawTarget = RequestPath
            });
        httpCtxMock.Setup(h => h.Features).Returns(mockIFeatureCollection.Object);

        return httpCtxMock.Object;
    }
}
