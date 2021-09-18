using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MELT;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace Delobytes.AspNetCore.Logging.Tests
{
    public class IdempotencyLoggingMiddlewareTests
    {
        private static readonly string IdempotencyHeaderValue = "84aa95b9-53ef-41f1-8c7e-0dc095414cd1";

        [Fact]
        public async Task NotThrows_WhenIdempotencyHeaderNotFound()
        {
            Exception exception = null;

            HttpContext ctx = GetContextWithHeaders(new Dictionary<string, StringValues>
            {
                { "Header", "NotFound" }
            });

            ITestLoggerFactory loggerFactory = TestLoggerFactory.Create();
            ILogger<IdempotencyLoggingMiddleware> logger = loggerFactory.CreateLogger<IdempotencyLoggingMiddleware>();

            RequestDelegate requestDelegate = new RequestDelegate((innerContext) =>
            {
                logger.LogInformation("test");
                return Task.FromResult(0);
            });

            IOptions<IdempotencyLoggingOptions> options = Options.Create(new IdempotencyLoggingOptions());
            IdempotencyLoggingMiddleware middleware = new IdempotencyLoggingMiddleware(requestDelegate, options);

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
        public async Task IdempotencyValueAddedToScope()
        {
            HttpContext ctx = GetContextWithHeaders(new Dictionary<string, StringValues>
            {
                { "IdempotencyKey", IdempotencyHeaderValue }
            });

            ITestLoggerFactory loggerFactory = TestLoggerFactory.Create();
            ILogger<IdempotencyLoggingMiddleware> logger = loggerFactory.CreateLogger<IdempotencyLoggingMiddleware>();

            RequestDelegate requestDelegate = new RequestDelegate((innerContext) =>
            {
                logger.LogInformation("test");
                return Task.FromResult(0);
            });

            IOptions<IdempotencyLoggingOptions> options = Options.Create(new IdempotencyLoggingOptions());
            IdempotencyLoggingMiddleware middleware = new IdempotencyLoggingMiddleware(requestDelegate, options);

            await middleware.InvokeAsync(ctx, logger);

            LogEntry log = Assert.Single(loggerFactory.Sink.LogEntries);
            log.Message.Should().Be("test");
            loggerFactory.Sink.Scopes.Should().NotBeEmpty().And.ContainSingle();

            BeginScope scope = loggerFactory.Sink.Scopes.First();
            scope.Properties.Should().NotBeNull().And.HaveCount(1);

            KeyValuePair<string, object> idempotencyProperty = scope.Properties.First();
            idempotencyProperty.Key.Should().Be(options.Value.IdempotencyLogAttribute);
            idempotencyProperty.Value.Should().Be(IdempotencyHeaderValue);
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
}