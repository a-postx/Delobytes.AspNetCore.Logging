using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using MELT;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Delobytes.AspNetCore.Logging.Tests
{
    public class ClaimsLoggingMiddlewareTests
    {
        public ClaimsLoggingMiddlewareTests()
        {
            _options = Options.Create(new ClaimsLoggingOptions());
        }

        private readonly IOptions<ClaimsLoggingOptions> _options;

        [Fact]
        public async Task NotThrows_WhenClaimIsNotFound()
        {
            Exception exception = null;

            ClaimsIdentity identity = new ClaimsIdentity(GetEmptyClaims(), "TestAuthType");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
            Mock<HttpContext> httpCtxMock = new Mock<HttpContext>();
            httpCtxMock.Setup(m => m.User).Returns(claimsPrincipal);

            ITestLoggerFactory loggerFactory = TestLoggerFactory.Create();
            ILogger<ClaimsLoggingMiddleware> logger = loggerFactory.CreateLogger<ClaimsLoggingMiddleware>();

            RequestDelegate requestDelegate = new RequestDelegate((innerContext) =>
            {
                logger.LogInformation("test");
                return Task.FromResult(0);
            });

            ClaimsLoggingMiddleware middleware = new ClaimsLoggingMiddleware(requestDelegate, _options);

            try
            {
                await middleware.InvokeAsync(httpCtxMock.Object, logger);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            exception.Should().BeNull();
        }

        [Fact]
        public async Task AddedToScope()
        {
            ClaimsIdentity identity = new ClaimsIdentity(GetProperClaims(), "TestAuthType");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
            Mock<HttpContext> httpCtxMock = new Mock<HttpContext>();
            httpCtxMock.Setup(m => m.User).Returns(claimsPrincipal);

            ITestLoggerFactory loggerFactory = TestLoggerFactory.Create();
            ILogger<ClaimsLoggingMiddleware> logger = loggerFactory.CreateLogger<ClaimsLoggingMiddleware>();

            RequestDelegate requestDelegate = new RequestDelegate((innerContext) =>
            {
                logger.LogInformation("test");
                return Task.FromResult(0);
            });

            ClaimsLoggingMiddleware middleware = new ClaimsLoggingMiddleware(requestDelegate, _options);

            await middleware.InvokeAsync(httpCtxMock.Object, logger);

            LogEntry log = Assert.Single(loggerFactory.Sink.LogEntries);
            log.Message.Should().Be("test");
            loggerFactory.Sink.Scopes.Should().NotBeEmpty().And.ContainSingle();

            BeginScope scope = loggerFactory.Sink.Scopes.First();
            scope.Properties.Should().NotBeNull().And.HaveCount(2);

            KeyValuePair<string, object> userId = scope.Properties.First();
            userId.Key.Should().Be(LogKeys.UserId);
            userId.Value.Should().Be("23efd012-8776-492f-9cbd-72eb11b6b8d4");

            KeyValuePair<string, object> tenantId = scope.Properties.Last();
            tenantId.Key.Should().Be(LogKeys.TenantId);
            tenantId.Value.Should().Be("ef725698-2a7d-4674-b1b8-9f63498ec87e");
        }

        private static List<Claim> GetProperClaims()
        {
            List<Claim> result = new List<Claim>()
            {
                new Claim("uid", "23efd012-8776-492f-9cbd-72eb11b6b8d4"),
                new Claim("tid", "ef725698-2a7d-4674-b1b8-9f63498ec87e"),
                new Claim("name", "Вася Пупкин")
            };

            return result;
        }

        private static List<Claim> GetEmptyClaims()
        {
            List<Claim> result = new List<Claim>()
            {
                new Claim("name", "Вася Пупкин")
            };

            return result;
        }
    }
}
