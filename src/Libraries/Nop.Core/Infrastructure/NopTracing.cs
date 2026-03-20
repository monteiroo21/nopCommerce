using System.Diagnostics;

namespace Nop.Core.Infrastructure
{
    /// <summary>
    /// Represents the central tracing/observability configuration for NopCommerce
    /// </summary>
    public static class NopTracing
    {
        /// <summary>
        /// The central ActivitySource for NopCommerce custom spans
        /// </summary>
        public static readonly ActivitySource ActivitySource = new ActivitySource("NopCommerce", "1.0.0");
    }
}
