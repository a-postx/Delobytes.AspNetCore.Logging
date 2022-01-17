namespace Delobytes.AspNetCore.Logging;

/// <summary>
/// Настройки логирования HTTP-контекста.
/// </summary>
public class HttpContextLoggingOptions
{
    /// <summary>
    /// <para>
    /// Нужно ли логировать тело запроса.
    /// </para>
    /// <para>Default: false</para>
    /// </summary>
    public bool LogRequestBody { get; set; } = false;

    /// <summary>
    /// <para>
    /// Нужно ли логировать тело ответа.
    /// </para>
    /// <para>Default: false</para>
    /// </summary>
    public bool LogResponseBody { get; set; } = false;

    /// <summary>
    /// <para>
    /// Максимальная длина тела в HTTP. Тела запросов и ответов будут обрезаться,
    /// если их длина будет превышать указанный порог.
    /// </para>
    /// <para>Default: 0</para>
    /// </summary>
    public int MaxBodyLength { get; set; } = 0;

    /// <summary>
    /// <para>
    /// Пути, запросы по которым необходимо пропускать.
    /// </para>
    /// <para>Default: Enumerable.Empty</para>
    /// </summary>
    public IEnumerable<PathString> SkipPaths { get; set; } = Enumerable.Empty<PathString>();

    /// <summary>
    /// <para>
    /// Заголовки запроса, которые необходимо исключить из логирования.
    /// </para>
    /// <para>Default: Enumerable.Empty</para>
    /// </summary>
    public IEnumerable<string> SkipRequestHeaders { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// <para>
    /// Заголовки ответа, которые необходимо исключить из логирования.
    /// </para>
    /// <para>Default: Enumerable.Empty</para>
    /// </summary>
    public IEnumerable<string> SkipResponseHeaders { get; set; } = Enumerable.Empty<string>();
}
