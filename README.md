# Delobytes.AspNetCore.Logging
Прослойки логирования для веб-апи приложений на .Net 5.

[RU](README.md), [EN](README.en.md)

## NetworkLoggingMiddleware
Для добавления в контекст логирования IP-адреса хоста, с которого пришёл запрос, можно использовать прослойку логирования сети.

**Использование**
```csharp
public void Configure(IApplicationBuilder application)
{
    application.UseNetworkLogging();
}
```

## ClaimsLoggingMiddleware
Для добавления в контекст логирования удостоверений пользователя, можно использовать прослойку логирования удостоверений.

**Использование**
```csharp
public void ConfigureServices(IServiceCollection services)
{
    //...
    services.AddAuthenticationCore();
    services.AddClaimsLogging(options =>
    {
        options.ClaimNames = new [] { "CustomClaimToLog" };
    });
}

public void Configure(IApplicationBuilder application)
{
    application
        .UseAuthentication()
        .UseClaimsLogging();
}
```

## HttpContextLoggingMiddleware
Для добавления дополнительных сообщений с подробными параметрами запроса и ответа можно использовать прослойку логирования HTTP-контекста.

**Использование**
```csharp
public void ConfigureServices(IServiceCollection services)
{
    //...
    services.AddHttpContextLogging(options =>
    {
        options.LogRequestBody = true;
        options.LogResponseBody = true;
        options.MaxBodyLength = 32759;
        options.SkipPaths = new List<PathString> { "/metrics" };
        options.SkipRequestHeaders = new List<string> { "Authorization" };
    });
}

public void Configure(IApplicationBuilder application)
{
    application
        .UseRouting()
        .UseHttpContextLogging();
}
```

## IdempotencyLoggingMiddleware
Для добавления в контекст логирования ключа идемпотентности из заголовка запроса, можно использовать прослойку логирования идемпотентности.

**Использование**
```csharp
public void ConfigureServices(IServiceCollection services)
{
    //...
    services.AddIdempotencyContextLogging(options =>
    {
        options.IdempotencyLogAttribute = "IdempotencyKey";
    });
}

public void Configure(IApplicationBuilder application)
{
    application.UseIdempotencyContextLogging();
}
```

## Лицензия
[МИТ](https://github.com/a-postx/Delobytes.AspNetCore.Logging/blob/master/LICENSE)