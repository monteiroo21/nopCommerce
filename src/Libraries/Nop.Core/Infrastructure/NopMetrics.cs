using System.Diagnostics.Metrics;

namespace Nop.Core.Infrastructure
{
    /// <summary>
    /// Central metrics definitions for NopCommerce custom instrumentation.
    /// Uses System.Diagnostics.Metrics (the .NET built-in API compatible with OpenTelemetry).
    /// </summary>
    public static class NopMetrics
    {
        /// <summary>The Meter that owns all NopCommerce custom instruments.</summary>
        public static readonly Meter Meter = new Meter("NopCommerce", "1.0.0");

        /// <summary>
        /// Counts every order placement attempt.
        /// Tags: order.status = "success" | "failure", payment.method = system name string.
        /// Lets you see order volume and failure rate broken down by payment provider.
        /// </summary>
        public static readonly Counter<long> OrdersPlaced =
            Meter.CreateCounter<long>(
                name: "orders.placed",
                unit: "{order}",
                description: "Number of order placements — tagged by status and payment method");

        /// <summary>
        /// Histogram of inventory adjustment processing time.
        /// Operational insight: DB row locking during inventory updates is a major bottleneck under load. 
        /// Spikes indicate database contention requiring architectural changes (e.g., async queues).
        /// </summary>
        public static readonly Histogram<double> InventoryUpdateDuration =
            Meter.CreateHistogram<double>(
                name: "inventory.update.duration",
                unit: "ms",
                description: "Latency of inventory adjustment operations in the database");

        /// <summary>
        /// Histogram of external payment gateway processing time.
        /// Tags: payment.method.
        /// Passes the "2am test": if latency spikes, an engineer knows the third-party provider is degraded.
        /// </summary>
        public static readonly Histogram<double> PaymentProviderDuration =
            Meter.CreateHistogram<double>(
                name: "payment.provider.duration",
                unit: "ms",
                description: "Latency of external payment gateway processing");
    }
}
