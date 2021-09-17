using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using MELT;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace Delobytes.AspNetCore.Logging.Tests
{
    public class NetworkLoggingMiddlewareTests
    {
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
            ipAddress.Value.Should().Be("1.1.1.1");
        }

        [Fact]
        public async Task OriginalForValueAddedToScope()
        {
            HttpContext ctx = GetContextWithHeaders(new Dictionary<string, StringValues>
            {
                { "X-Original-For", "1.1.1.1" }
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
            ipAddress.Value.Should().Be("1.1.1.1");
        }

        [Fact]
        public async Task ClientIpValueAddedToScope()
        {
            HttpContext ctx = GetContextWithHeaders(new Dictionary<string, StringValues>
            {
                { "X-Client-IP", "1.1.1.1" }
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
            ipAddress.Value.Should().Be("1.1.1.1");
        }

        private HttpContext GetContextWithHeaders(Dictionary<string, StringValues> headers)
        {
            Mock<HttpContext> httpCtxMock = new Mock<HttpContext>();
            httpCtxMock.Setup(m => m.Connection.RemoteIpAddress).Returns(IPAddress.Parse("1.1.1.1"));

            MockRepository mocks = new MockRepository(MockBehavior.Default);
            Mock<HttpRequest> mockRequest = mocks.Create<HttpRequest>();

            IHeaderDictionary headerDic = new HeaderDictionary(headers);

            mockRequest.Setup(p => p.Headers).Returns(headerDic);
            httpCtxMock.Setup(p => p.Request).Returns(mockRequest.Object);

            return httpCtxMock.Object;
        }
    }
}
