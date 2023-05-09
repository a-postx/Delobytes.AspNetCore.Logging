using System;
using Microsoft.Extensions.Primitives;

namespace Delobytes.AspNetCore.Logging.Tests;

public class HeaderLoggingMiddlewareTests
{
    private static readonly string TestHeaderValue = "84aa95b9-63ef-41f1-8c7e-0dc095414cd1";

    [Fact]
    public async Task NotThrows_WhenCustomHeaderNotFound()
    {
        Exception? exception = null;

        HttpContext ctx = GetContextWithHeaders(new Dictionary<string, StringValues>
            {
                { "Header", "NotFound" }
            });

        ITestLoggerFactory loggerFactory = TestLoggerFactory.Create();
        ILogger<HeaderLoggingMiddleware> logger = loggerFactory.CreateLogger<HeaderLoggingMiddleware>();

        RequestDelegate requestDelegate = new RequestDelegate((innerContext) =>
        {
            logger.LogInformation("test");
            return Task.FromResult(0);
        });

        IOptions<HeaderLoggingOptions> options = Options.Create(new HeaderLoggingOptions());
        HeaderLoggingMiddleware middleware = new HeaderLoggingMiddleware(requestDelegate, options);

        try
        {
            await middleware.InvokeAsync(ctx, logger);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        exception.Should().BeNull();

        LogEntry log = Assert.Single(loggerFactory.Sink.LogEntries);
        log.Message.Should().Be("test");
        loggerFactory.Sink.Scopes.Should().BeEmpty();
    }

    [Fact]
    public async Task NotThrows_WhenCustomHeaderIsEmpty()
    {
        Exception? exception = null;

        IOptions<HeaderLoggingOptions> options = Options.Create(new HeaderLoggingOptions());

        HttpContext ctx = GetContextWithHeaders(new Dictionary<string, StringValues>
            {
                { options.Value.HeaderName, TestHeaderValue }
            });

        ITestLoggerFactory loggerFactory = TestLoggerFactory.Create();
        ILogger<HeaderLoggingMiddleware> logger = loggerFactory.CreateLogger<HeaderLoggingMiddleware>();

        RequestDelegate requestDelegate = new RequestDelegate((innerContext) =>
        {
            logger.LogInformation("test");
            return Task.FromResult(0);
        });

        HeaderLoggingMiddleware middleware = new HeaderLoggingMiddleware(requestDelegate, options);

        try
        {
            await middleware.InvokeAsync(ctx, logger);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        exception.Should().BeNull();
    }

    [Fact]
    public async Task CustomHeaderValueAddedToScope()
    {
        IOptions<HeaderLoggingOptions> options = Options.Create(new HeaderLoggingOptions
        {
            HeaderName= "MyHeader", HeaderLogsName = "MyHeaderInLogs"
        });

        HttpContext ctx = GetContextWithHeaders(new Dictionary<string, StringValues>
            {
                { options.Value.HeaderName, TestHeaderValue }
            });

        ITestLoggerFactory loggerFactory = TestLoggerFactory.Create();
        ILogger<HeaderLoggingMiddleware> logger = loggerFactory.CreateLogger<HeaderLoggingMiddleware>();

        RequestDelegate requestDelegate = new RequestDelegate((innerContext) =>
        {
            logger.LogInformation("test");
            return Task.FromResult(0);
        });

        HeaderLoggingMiddleware middleware = new HeaderLoggingMiddleware(requestDelegate, options);

        await middleware.InvokeAsync(ctx, logger);

        LogEntry log = Assert.Single(loggerFactory.Sink.LogEntries);
        log.Message.Should().Be("test");
        loggerFactory.Sink.Scopes.Should().NotBeEmpty().And.ContainSingle();

        BeginScope scope = loggerFactory.Sink.Scopes.First();
        scope.Properties.Should().NotBeNull().And.HaveCount(1);

        KeyValuePair<string, object> customHeaderProperty = scope.Properties.First();
        customHeaderProperty.Key.Should().Be(options.Value.HeaderLogsName);
        customHeaderProperty.Value.Should().Be(TestHeaderValue);
    }

    private HttpContext GetContextWithHeaders(Dictionary<string, StringValues> headers)
    {
        Mock<HttpContext> httpCtxMock = new Mock<HttpContext>();
        MockRepository mocks = new MockRepository(MockBehavior.Default);
        Mock<HttpRequest> mockRequest = mocks.Create<HttpRequest>();

        IHeaderDictionary headerDic = new HeaderDictionary(headers);

        mockRequest.Setup(p => p.Headers).Returns(headerDic);
        httpCtxMock.Setup(p => p.Request).Returns(mockRequest.Object);

        return httpCtxMock.Object;
    }
}
