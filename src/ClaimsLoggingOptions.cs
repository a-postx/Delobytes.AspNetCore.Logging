using System.Collections.Generic;

namespace Delobytes.AspNetCore.Logging
{
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
    }
}
