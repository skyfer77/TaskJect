using Domain.Enums;

namespace Domain.Database
{
	public class PaymentWayForPay
	{
		public Guid Id { get; set; }
		public string? UserId { get; set; }
		public string OrganizationCode { get; set; }
		public string PlanCode { get; set; }
		public SubscriptionPeriodType SubscriptionPeriod { get; set; }

		// Номер замовлення, який переданий у WayForPay (orderReference)
		public string OrderReference { get; set; } = null!;
		public decimal Amount { get; set; }
		public string Currency { get; set; } = "UAH";
		/// <summary>
		/// Статус регулярного платежу (підписки) згідно WayForPay.
		/// Можливі значення:
		/// Created    - регулярний платіж створений, але ще не активований
		/// Active     - регулярний платіж активний, списання працює
		/// Suspended  - регулярний платіж призупинено (наприклад, помилка списання)
		/// Removed    - регулярний платіж видалено (підписка скасована)
		/// Confirmed  - службовий статус, для внутрішніх операцій WayForPay
		/// Completed  - регулярний платіж завершено (термін дії підписки минув або скасовано)
		/// </summary>
		public string Status { get; set; } = "Created";
		public string? RecToken { get; set; }

		public DateTime? DateNext { get; set; }


		// Дата створення
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		// Дата оновлення (коли WayForPay відправив callback)
		public DateTime? UpdatedAt { get; set; }
	}
}
