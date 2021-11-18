namespace Delobytes.AspNetCore.Logging;

/// <summary>
/// Настройки логирования идемпотентности.
/// </summary>
public class IdempotencyLoggingOptions
{
    /// <summary>
    /// <para>
    /// Заголовок идемпотентности, значение которого нужно добавлять в контекст логирования.
    /// </para>
    /// <para>Default: IdempotencyKey</para>
    /// </summary>
    public string IdempotencyHeader { get; set; } = "Idempotency-Key";

    /// <summary>
    /// <para>
    /// Имя атрибута в логах.
    /// </para>
    /// <para>Default: IdempotencyKey</para>
    /// </summary>
    public string IdempotencyLogAttribute { get; set; } = "Idempotency-Key";
}
