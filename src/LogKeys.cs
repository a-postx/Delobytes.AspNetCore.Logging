namespace Delobytes.AspNetCore.Logging
{
    public static class LogKeys
    {
        public const string ClientIP = nameof(ClientIP);
        public const string TenantId = nameof(TenantId);
        public const string UserId = nameof(UserId);
        public const string RequestProtocol = nameof(RequestProtocol);
        public const string RequestScheme = nameof(RequestScheme);
        public const string RequestHost = nameof(RequestHost);
        public const string RequestMethod = nameof(RequestMethod);
        public const string RequestPath = nameof(RequestPath);
        public const string RequestQuery = nameof(RequestQuery);
        public const string RequestPathAndQuery = nameof(RequestPathAndQuery);
        public const string RequestHeaders = nameof(RequestHeaders);
        public const string RequestBody = nameof(RequestBody);
        public const string RequestAborted = nameof(RequestAborted);
        public const string StatusCode = nameof(StatusCode);
        public const string ResponseHeaders = nameof(ResponseHeaders);
        public const string ResponseBody = nameof(ResponseBody);
    }
}
