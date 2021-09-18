using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Delobytes.AspNetCore.Logging
{
    public class HttpContextLoggingOptions
    {
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
        /// <para>Default: null</para>
        /// </summary>
        public IEnumerable<PathString> SkipPaths { get; set; } = null;
    }
}
