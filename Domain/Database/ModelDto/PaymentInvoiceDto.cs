namespace Domain.Database
{
	public class PaymentInvoiceDto
	{
		public Guid Id { get; set; }
		public Guid PaymentWayForPayId { get; set; }

		public decimal Amount { get; set; }
		public string Currency { get; set; } = "UAH";
		public string TransactionStatus { get; set; } = "Pending";
		public DateTime PaidAt { get; set; }
		public string WayForPayResponse { get; set; } = null!;

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}
