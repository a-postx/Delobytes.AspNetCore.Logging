namespace Delobytes.AspNetCore.Logging;

/// <summary>
/// Настройки логирования заголовка.
/// </summary>
public class HeaderLoggingOptions
{
    /// <summary>
    /// <para>
    /// Заголовок, значение которого нужно добавлять в контекст логирования.
    /// </para>
    /// </summary>
    public string HeaderName { get; set; } = string.Empty;

    /// <summary>
    /// <para>
    /// Имя заголовка, которое будет отображаться в логах.
    /// </para>
    /// </summary>
    public string HeaderLogsName { get; set; } = string.Empty;
}
