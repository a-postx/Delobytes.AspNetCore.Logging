using System.Security.Claims;

namespace Delobytes.AspNetCore.Logging.Tests;

public class ClaimsLoggingMiddlewareTests
{
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

        IOptions<ClaimsLoggingOptions> options = Options.Create(new ClaimsLoggingOptions());
        ClaimsLoggingMiddleware middleware = new ClaimsLoggingMiddleware(requestDelegate, options);

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
    public async Task BaseClaimValuesAddedToScope()
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

        IOptions<ClaimsLoggingOptions> options = Options.Create(new ClaimsLoggingOptions());
        ClaimsLoggingMiddleware middleware = new ClaimsLoggingMiddleware(requestDelegate, options);

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

    [Fact]
    public async Task CustomClaimValuesAddedToScope()
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

        IOptions<ClaimsLoggingOptions> options = Options.Create(new ClaimsLoggingOptions());
        options.Value.ClaimNames = new string[] { "name" };

        ClaimsLoggingMiddleware middleware = new ClaimsLoggingMiddleware(requestDelegate, options);

        await middleware.InvokeAsync(httpCtxMock.Object, logger);

        LogEntry log = Assert.Single(loggerFactory.Sink.LogEntries);
        log.Message.Should().Be("test");
        loggerFactory.Sink.Scopes.Should().NotBeEmpty().And.HaveCount(2);

        BeginScope firstScope = loggerFactory.Sink.Scopes.First();
        firstScope.Properties.Should().NotBeNull().And.HaveCount(2);

        KeyValuePair<string, object> userId = firstScope.Properties.First();
        userId.Key.Should().Be(LogKeys.UserId);
        userId.Value.Should().Be("23efd012-8776-492f-9cbd-72eb11b6b8d4");

        KeyValuePair<string, object> tenantId = firstScope.Properties.Last();
        tenantId.Should().NotBeNull();
        tenantId.Value.Should().Be("ef725698-2a7d-4674-b1b8-9f63498ec87e");

        BeginScope secondScope = loggerFactory.Sink.Scopes.Last();
        secondScope.Properties.Should().NotBeNull().And.HaveCount(1);

        KeyValuePair<string, object> name = secondScope.Properties.First();
        name.Key.Should().Be("name");
        name.Value.Should().Be("Вася Пупкин");
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
