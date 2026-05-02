using System.ComponentModel.DataAnnotations;

namespace Domain.Database
{
    public class GumroadWebhookLog
    {
        /// <summary>
        /// Унікальний ідентифікатор події для ідемпотентності.
        /// Для платежів: sale_id
        /// Для відписок: subscription:{subscription_id}:cancel
        /// Для failed:  subscription:{subscription_id}:failed:{occurrence}
        /// </summary>
        [Key]
        [MaxLength(200)]
        public string EventId { get; set; } = default!;

        public DateTime ProcessedAtUtc { get; set; }

        public GumroadEventType EventType { get; set; }

        [MaxLength(100)]
        public string? SaleId { get; set; }

        [MaxLength(100)]
        public string? SubscriptionId { get; set; }

        public Guid? OrganizationCode { get; set; }
    }

    public enum GumroadEventType
    {
        Unknown = 0,
        Payment,
        FailedPayment,
        Cancel,
        Refund   
    }
}
