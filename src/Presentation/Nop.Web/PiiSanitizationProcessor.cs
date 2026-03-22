using System.Diagnostics;
using System.Linq;
using OpenTelemetry;
namespace Nop.Web
{
    /// <summary>
    /// An OpenTelemetry processor that intercepts all spans before export 
    /// and redacts any attributes that might contain Personally Identifiable Information (PII).
    /// </summary>
    public class PiiSanitizationProcessor : BaseProcessor<Activity>
    {
        // Keywords to look for in attribute keys
        private static readonly string[] SensitiveKeys = 
        { 
            "email", "password", "phone", "address", "creditcard", "card", "cvv", "zip", "customer.name" 
        };
        public override void OnEnd(Activity activity)
        {
            var tagsToRedact = activity.TagObjects
                .Where(t => IsSensitive(t.Key))
                .ToList();
            foreach (var tag in tagsToRedact)
            {
                activity.SetTag(tag.Key, "[REDACTED]");
            }
            var dbStatement = activity.GetTagItem("db.statement") as string;
            if (!string.IsNullOrEmpty(dbStatement) && IsSensitiveSql(dbStatement))
            {
                activity.SetTag("db.statement", "[REDACTED SQL - POTENTIAL PII]");
            }
        }
        private bool IsSensitive(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            var lowerKey = key.ToLowerInvariant();
            return SensitiveKeys.Any(k => lowerKey.Contains(k));
        }
        private bool IsSensitiveSql(string sql)
        {
            var lowerSql = sql.ToLowerInvariant();
            return lowerSql.Contains("update customer") || 
                   lowerSql.Contains("insert into customer") || 
                   lowerSql.Contains("insert into address");
        }
    }
}