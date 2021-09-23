namespace Delobytes.AspNetCore.Logging
{
    /// <summary>
    /// Названия атрибутов, которые записываются в логи.
    /// </summary>
    public static class LogKeys
    {
        /// <summary>
        /// Адрес клиента.
        /// </summary>
        public const string ClientIP = nameof(ClientIP);
        /// <summary>
        /// Идентификатор арендатора.
        /// </summary>
        public const string TenantId = nameof(TenantId);
        /// <summary>
        /// Идентификатор пользователя.
        /// </summary>
        public const string UserId = nameof(UserId);
        /// <summary>
        /// Протокол запроса.
        /// </summary>
        public const string RequestProtocol = nameof(RequestProtocol);
        /// <summary>
        /// Схема запроса.
        /// </summary>
        public const string RequestScheme = nameof(RequestScheme);
        /// <summary>
        /// Хост, с которого пришёл запрос.
        /// </summary>
        public const string RequestHost = nameof(RequestHost);
        /// <summary>
        /// Метод запроса.
        /// </summary>
        public const string RequestMethod = nameof(RequestMethod);
        /// <summary>
        /// Путь запроса.
        /// </summary>
        public const string RequestPath = nameof(RequestPath);
        /// <summary>
        /// Параметры запроса.
        /// </summary>
        public const string RequestQuery = nameof(RequestQuery);
        /// <summary>
        /// Путь и параметры запроса.
        /// </summary>
        public const string RequestPathAndQuery = nameof(RequestPathAndQuery);
        /// <summary>
        /// Заголовки запроса.
        /// </summary>
        public const string RequestHeaders = nameof(RequestHeaders);
        /// <summary>
        /// Тело запроса.
        /// </summary>
        public const string RequestBody = nameof(RequestBody);
        /// <summary>
        /// Признак отмены запроса.
        /// </summary>
        public const string RequestAborted = nameof(RequestAborted);
        /// <summary>
        /// Код ответа.
        /// </summary>
        public const string StatusCode = nameof(StatusCode);
        /// <summary>
        /// Заголовки ответа.
        /// </summary>
        public const string ResponseHeaders = nameof(ResponseHeaders);
        /// <summary>
        /// Тело ответа.
        /// </summary>
        public const string ResponseBody = nameof(ResponseBody);
    }
}
