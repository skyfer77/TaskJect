using Domain.Database;

namespace TaskJect.Web.Common
{
    public static class GumroadWebhookHelper
    {
        public static (string eventId, GumroadEventType type, string? saleId, string? subscriptionId)
            BuildEventIdAndType(IFormCollection payload)
        {
            string Get(string key) => payload.TryGetValue(key, out var v) ? v.ToString() : string.Empty;

            var saleId = Get("sale_id");
            var subscriptionId = Get("subscription_id");
            var cancelled = Get("cancelled").Equals("true", StringComparison.OrdinalIgnoreCase)
                               || Get("subscription_cancelled").Equals("true", StringComparison.OrdinalIgnoreCase);
            var refunded = Get("refunded").Equals("true", StringComparison.OrdinalIgnoreCase);
            var failed1 = Get("failed_charge").Equals("true", StringComparison.OrdinalIgnoreCase);
            var failed2 = Get("charge_failed").Equals("true", StringComparison.OrdinalIgnoreCase);

            // Для повторних платежів Gumroad часто присилає лічильник:
            var occurrenceRaw = Get("charge_occurrence_count");
            var occurrencePart = string.IsNullOrWhiteSpace(occurrenceRaw) ? "" : occurrenceRaw.Trim();

            if (!string.IsNullOrWhiteSpace(saleId))
            {
                // Платіж або рефанд завжди мають sale_id → ідеальний ідентифікатор
                var type = refunded ? GumroadEventType.Refund : GumroadEventType.Payment;
                return (saleId, type, saleId, subscriptionId);
            }

            if (cancelled && !string.IsNullOrWhiteSpace(subscriptionId))
            {
                // Відписка не створює sale_id → будуємо свій стабільний ключ
                var id = $"subscription:{subscriptionId}:cancel";
                return (id, GumroadEventType.Cancel, null, subscriptionId);
            }

            if ((failed1 || failed2) && !string.IsNullOrWhiteSpace(subscriptionId))
            {
                // Невдала спроба списання. Бажано врахувати occurrence, щоб кожну спробу логувати окремо
                var suffix = string.IsNullOrEmpty(occurrencePart) ? DateTime.UtcNow.Ticks.ToString() : occurrencePart;
                var id = $"subscription:{subscriptionId}:failed:{suffix}";
                return (id, GumroadEventType.FailedPayment, null, subscriptionId);
            }

            // Фолбек: щось інше (рідкісні або майбутні типи подій)
            var fallback = !string.IsNullOrWhiteSpace(subscriptionId)
                ? $"subscription:{subscriptionId}:unknown:{DateTime.UtcNow.Ticks}"
                : $"unknown:{Guid.NewGuid()}";

            return (fallback, GumroadEventType.Unknown, string.IsNullOrWhiteSpace(saleId) ? null : saleId, string.IsNullOrWhiteSpace(subscriptionId) ? null : subscriptionId);
        }
    }
}
