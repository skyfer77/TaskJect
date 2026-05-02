namespace Domain.Database
{
    public class TariffPlan
    {
        public string Code { get; set; } 
        public string Name { get; set; }
        public int MaxUsers { get; set; }
        public long MaxStorageBytes { get; set; }
        public int CountRequests { get; set; }
        public string PriceMonth { get; set; }
        public string? PriceMonthlyDiscount { get; set; }
		public string PriceYearlyDiscount { get; set; }
		public bool HasTelegramIntegration { get; set; }
        public bool HasProjectAccessControl { get; set; }
        public bool HasPriorityRequestsProcess { get; set; }
        public bool HasGitHubIntegration { get; set; }
        public bool IsPublic { get; set; }
        public string? Source { get; set; }
	}
}
