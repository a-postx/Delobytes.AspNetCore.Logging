namespace Delobytes.AspNetCore.Logging;

/// <summary>
/// Расширения логирования.
/// </summary>
public static class LoggerExtensions
{
    ///<summary>
    ///Обрамляет контекст логирования дополнительными параметрами.
    ///</summary>
    /// <param name="logger">Логер.</param>
    ///<param name="paramsAndValues">Параметры и их значения, которые нужно добавить в контекст.</param>
    public static IDisposable? BeginScopeWith(this ILogger logger, params (string key, object? value)[] paramsAndValues)
    {
        Dictionary<string, object?> state = paramsAndValues.ToDictionary(x => x.key, x => x.value);
        IDisposable? startedScope = logger.BeginScope(state) ?? throw new InvalidOperationException("Scope is not available");

        return startedScope;
    }
}
