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
        /// Histogram of order monetary totals (in store currency).
        /// Shows revenue distribution: are orders typically small or large? Are there outliers?
        /// PII note: amount is an aggregate figure, not linked to any individual — safe to record.
        /// </summary>
        public static readonly Histogram<double> OrderTotalAmount =
            Meter.CreateHistogram<double>(
                name: "order.total.amount",
                unit: "currency_units",
                description: "Distribution of placed order totals in store currency");

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
