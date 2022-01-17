using System.Net;
using Microsoft.Extensions.Primitives;

namespace Delobytes.AspNetCore.Logging.Tests;

public class NetworkLoggingMiddlewareTests
{
    private static readonly string ConnectionIpAddressValue = "1.1.1.1";
    private static readonly string HeaderIpAddressValue = "2.2.2.2";

    [Fact]
    public async Task ConnectionValueAddedToScope()
    {
        HttpContext ctx = GetContextWithHeaders(new Dictionary<string, StringValues>());

        ITestLoggerFactory loggerFactory = TestLoggerFactory.Create();
        ILogger<NetworkLoggingMiddleware> logger = loggerFactory.CreateLogger<NetworkLoggingMiddleware>();

        RequestDelegate requestDelegate = new RequestDelegate((innerContext) =>
        {
            logger.LogInformation("test");
            return Task.FromResult(0);
        });

        NetworkLoggingMiddleware middleware = new NetworkLoggingMiddleware(requestDelegate);

        await middleware.InvokeAsync(ctx, logger);

        LogEntry log = Assert.Single(loggerFactory.Sink.LogEntries);
        log.Message.Should().Be("test");
        loggerFactory.Sink.Scopes.Should().NotBeEmpty().And.ContainSingle();

        BeginScope scope = loggerFactory.Sink.Scopes.First();
        scope.Properties.Should().NotBeNull().And.HaveCount(1);

        KeyValuePair<string, object> ipAddress = scope.Properties.First();
        ipAddress.Key.Should().Be(LogKeys.ClientIP);
        ipAddress.Value.Should().Be(ConnectionIpAddressValue);
    }

    [Fact]
    public async Task OriginalForValueAddedToScope()
    {
        HttpContext ctx = GetContextWithHeaders(new Dictionary<string, StringValues>
            {
                { "X-Original-For", HeaderIpAddressValue }
            });

        ITestLoggerFactory loggerFactory = TestLoggerFactory.Create();
        ILogger<NetworkLoggingMiddleware> logger = loggerFactory.CreateLogger<NetworkLoggingMiddleware>();

        RequestDelegate requestDelegate = new RequestDelegate((innerContext) =>
        {
            logger.LogInformation("test");
            return Task.FromResult(0);
        });

        NetworkLoggingMiddleware middleware = new NetworkLoggingMiddleware(requestDelegate);

        await middleware.InvokeAsync(ctx, logger);

        LogEntry log = Assert.Single(loggerFactory.Sink.LogEntries);
        log.Message.Should().Be("test");
        loggerFactory.Sink.Scopes.Should().NotBeEmpty().And.ContainSingle();

        BeginScope scope = loggerFactory.Sink.Scopes.First();
        scope.Properties.Should().NotBeNull().And.HaveCount(1);

        KeyValuePair<string, object> ipAddress = scope.Properties.First();
        ipAddress.Key.Should().Be(LogKeys.ClientIP);
        ipAddress.Value.Should().Be(HeaderIpAddressValue);
    }

    [Fact]
    public async Task ClientIpValueAddedToScope()
    {
        HttpContext ctx = GetContextWithHeaders(new Dictionary<string, StringValues>
            {
                { "X-Client-IP", HeaderIpAddressValue }
            });

        ITestLoggerFactory loggerFactory = TestLoggerFactory.Create();
        ILogger<NetworkLoggingMiddleware> logger = loggerFactory.CreateLogger<NetworkLoggingMiddleware>();

        RequestDelegate requestDelegate = new RequestDelegate((innerContext) =>
        {
            logger.LogInformation("test");
            return Task.FromResult(0);
        });

        NetworkLoggingMiddleware middleware = new NetworkLoggingMiddleware(requestDelegate);

        await middleware.InvokeAsync(ctx, logger);

        LogEntry log = Assert.Single(loggerFactory.Sink.LogEntries);
        log.Message.Should().Be("test");
        loggerFactory.Sink.Scopes.Should().NotBeEmpty().And.ContainSingle();

        BeginScope scope = loggerFactory.Sink.Scopes.First();
        scope.Properties.Should().NotBeNull().And.HaveCount(1);

        KeyValuePair<string, object> ipAddress = scope.Properties.First();
        ipAddress.Key.Should().Be(LogKeys.ClientIP);
        ipAddress.Value.Should().Be(HeaderIpAddressValue);
    }

    private HttpContext GetContextWithHeaders(Dictionary<string, StringValues> headers)
    {
        Mock<HttpContext> httpCtxMock = new Mock<HttpContext>();
        httpCtxMock.Setup(m => m.Connection.RemoteIpAddress).Returns(IPAddress.Parse(ConnectionIpAddressValue));

        MockRepository mocks = new MockRepository(MockBehavior.Default);
        Mock<HttpRequest> mockRequest = mocks.Create<HttpRequest>();

        IHeaderDictionary headerDic = new HeaderDictionary(headers);

        mockRequest.Setup(p => p.Headers).Returns(headerDic);
        httpCtxMock.Setup(p => p.Request).Returns(mockRequest.Object);

        return httpCtxMock.Object;
    }
}
