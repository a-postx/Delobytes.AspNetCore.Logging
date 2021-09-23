# Delobytes.AspNetCore.Logging
Logging middlewares for web-API apps on .Net 5.

[RU](README.md), [EN](README.en.md)

## NetworkLoggingMiddleware
You can use network logging middleware to add IP-address property to the logging context.

**Usage**
```csharp
    public void Configure(IApplicationBuilder application)
    {
        application.UseNetworkLogging();
	}
```

## ClaimsLoggingMiddleware
You can use network claims logging middleware to add user claims properties to the logging context.

**Usage**
```csharp
	public void ConfigureServices(IServiceCollection services)
    {
		...
		services.AddAuthenticationCore();
        services.AddClaimsLogging(options =>
        {
            options.ClaimNames = new [] { "CustomClaimToLog" };
        });
		...
	}
	
    public void Configure(IApplicationBuilder application)
    {
        application
		    .UseAuthentication()
            .UseClaimsLogging();
	}
```

## HttpContextLoggingMiddleware
You can use HTTP context logging middleware to log detailed request and response properties in additional log messages.

**Usage**
```csharp
	public void ConfigureServices(IServiceCollection services)
    {
		...
		services.AddHttpContextLogging(options =>
        {
            options.LogRequestBody = true;
            options.LogResponseBody = true;
            options.MaxBodyLength = 32759;
            options.SkipPaths = new List<PathString> { "/metrics" };
            options.SkipRequestHeaders = new List<string> { "Authorization" };
        });
		...
	}
	
    public void Configure(IApplicationBuilder application)
    {
        application
		    .UseRouting()
            .UseHttpContextLogging();
	}
```

## IdempotencyLoggingMiddleware
You can use idempotency logging middleware to add idempotency key header to the logging context.

**Usage**
```csharp
	public void ConfigureServices(IServiceCollection services)
    {
		...
		services.AddIdempotencyContextLogging(options =>
        {
            options.IdempotencyLogAttribute = "IdempotencyKey";
        });
		...
	}
	
    public void Configure(IApplicationBuilder application)
    {
        application.UseIdempotencyContextLogging();
	}
```

## License
[MIT](https://github.com/a-postx/Delobytes.AspNetCore.Logging/blob/master/LICENSE)