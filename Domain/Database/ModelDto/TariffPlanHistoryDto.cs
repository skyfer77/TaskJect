namespace Domain.Database
{
    public class TariffPlanHistoryDto
    {
        public Guid OrganizationCode { get; set; }
        public string TariffPlanCode { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string? SubscriptionCode { get; set; }

    }
}
