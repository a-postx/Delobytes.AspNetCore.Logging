namespace Delobytes.AspNetCore.Logging;

/// <summary>
/// Настройки логирования удостоверений пользователя.
/// </summary>
public class ClaimsLoggingOptions
{
    /// <summary>
    /// <para>
    /// Имя удостоверения пользовательского идентификатора, значение которого нужно логировать.
    /// </para>
    /// <para>Default: uid</para>
    /// </summary>
    public string UserIdClaimName { get; set; } = "uid";

    /// <summary>
    /// <para>
    /// Имя удостоверения идентификатора арендатора, значение которого нужно логировать.
    /// </para>
    /// <para>Default: tid</para>
    /// </summary>
    public string TenantIdClaimName { get; set; } = "tid";

    /// <summary>
    /// <para>
    /// Дополнительные имена удостоверений, значения которых нужно логировать.
    /// </para>
    /// <para>Default: Array.Empty</para>
    /// </summary>
    public string[] ClaimNames { get; set; } = Array.Empty<string>();
}
