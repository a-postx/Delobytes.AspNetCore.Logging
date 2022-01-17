using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Delobytes.AspNetCore.Logging;

/// <summary>
/// Расширения логирования.
/// </summary>
internal static class LoggerExtensions
{
    ///<summary>
    ///Обрамляет контекст логирования дополнительными параметрами.
    ///</summary>
    /// <param name="logger">Логер.</param>
    ///<param name="paramsAndValues">Параметры и их значения, которые нужно добавить в контекст.</param>
    internal static IDisposable BeginScopeWith(this ILogger logger, params (string key, object value)[] paramsAndValues)
    {
        return logger.BeginScope(paramsAndValues.ToDictionary(x => x.key, x => x.value));
    }
}
