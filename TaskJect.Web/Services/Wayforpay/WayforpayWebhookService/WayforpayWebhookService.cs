using Data;
using Data.DomainEvent;
using Domain.Database;
using Domain.DomainEvents;
using Domain.Enums;
using Domain.IServices;
using System.Text.Json;

namespace TaskJect.Web.Services
{
	public class WayforpayWebhookService : IWayforpayWebhookService
	{
		private readonly ITariffPlanHistoryRepository _planHistoryRepository;
		private readonly IPaymentWayForPayRepository _paymentWayForPayRepository;
		private readonly IPaymentInvoiceRepository _paymentInvoiceRepository;
		private readonly IWayforpayServices _wayforpayServices;
		private readonly DomainEventDispatcher _dispatcher;
		private readonly ILogger<WayforpayWebhookService> _logger;
		private readonly IOrganizationLimitationsEnforcer _organizationLimitationsEnforcer;

		public WayforpayWebhookService(
			ITariffPlanHistoryRepository planHistoryRepository,
			IPaymentWayForPayRepository paymentWayForPayRepository,
			IPaymentInvoiceRepository paymentInvoiceRepository,
			IWayforpayServices wayforpayServices,
      IOrganizationLimitationsEnforcer organizationLimitationsEnforcer,
			DomainEventDispatcher dispatcher,
			ILogger<WayforpayWebhookService> logger)
		{
			_planHistoryRepository = planHistoryRepository;
			_paymentWayForPayRepository = paymentWayForPayRepository;
			_paymentInvoiceRepository = paymentInvoiceRepository;
			_wayforpayServices = wayforpayServices;
			_dispatcher = dispatcher;
			_logger = logger;
			_organizationLimitationsEnforcer = organizationLimitationsEnforcer;
     }

		public async Task<bool> ProcessWebhookAsync(string jsonBody)
		{
			try
			{
				var json = JsonSerializer.Deserialize<JsonElement>(jsonBody);

				if (!json.TryGetProperty("orderReference", out var orderRefProp) ||
					!json.TryGetProperty("transactionStatus", out var statusProp))
				{
					_logger.LogWarning("ProcessWebhookAsync: Missing required fields. JSON: {Json}", json.ToString());
					return false;
				}

				var orderReference = orderRefProp.GetString() ?? "";
				var transactionStatus = statusProp.GetString() ?? "";
				var currency = json.TryGetProperty("currency", out var cur) ? cur.GetString() ?? "" : "";
				var amount = json.TryGetProperty("amount", out var amt) && amt.TryGetDecimal(out var dec) ? dec : 0m;
				var recToken = json.TryGetProperty("recToken", out var recProp) ? recProp.GetString() : null;
				var processingDate = json.TryGetProperty("processingDate", out var pd) ? pd.GetInt64() : DateTimeOffset.UtcNow.ToUnixTimeSeconds();

				var subscription = await _paymentWayForPayRepository.FindByOrderReferenceAsync(orderReference);
				if (subscription == null)
				{
					_logger.LogWarning("ProcessWebhookAsync: Subscription not found for orderReference={OrderRef}", orderReference);
					return false;
				}

				if (!_wayforpayServices.VerifySignature(json))
				{
					throw new UnauthorizedAccessException("Invalid signature");
				}
				
				subscription.Status = transactionStatus == "Approved" ? "Active" : "Suspended";
				subscription.UpdatedAt = DateTime.UtcNow;
				subscription.RecToken = recToken;

				var domainEvents = new List<IDomainEvent>();

				if (transactionStatus == "Approved")
				{
					var result = await _wayforpayServices.GetRegularPaymentStatusAsync(subscription.OrderReference);
					subscription.Status = result != null ? result.Status : "Suspended";
					subscription.DateNext = result?.DateNext;

					var dateTo = result?.DateNext 
						?? (subscription.SubscriptionPeriod == SubscriptionPeriodType.Year 
							? DateTime.UtcNow.AddYears(1) : DateTime.UtcNow.AddMonths(1));

					var planHistory = new TariffPlanHistoryDto
					{
						OrganizationCode = Guid.Parse(subscription.OrganizationCode),
						TariffPlanCode = subscription.PlanCode,
						DateFrom = DateTime.UtcNow,
						DateTo = dateTo,
						SubscriptionCode = orderReference
					};


					var answer = await _organizationLimitationsEnforcer.ApplyTariffPlan(planHistory, false);
          if (answer)
					{
              await _organizationLimitationsEnforcer.UnlockUsers(subscription.OrganizationCode, subscription.PlanCode);
          }	
					domainEvents.Add(new SubscriptionCongratulationsDomainEvent(subscription.OrganizationCode, 
						subscription.PlanCode, dateTo));
				}
				else if (transactionStatus == "Declined")
				{
					subscription.RecToken = null;
					domainEvents.Add(new SubscriptionPaymentFailedDomainEvent(subscription.OrganizationCode));
				}
				else if (transactionStatus == "Refunded")
				{
					var planHistory = new TariffPlanHistoryDto
					{
						OrganizationCode = Guid.Parse(subscription.OrganizationCode),
						TariffPlanCode = SD.BasicPlanCode,
						DateFrom = DateTime.UtcNow,
						DateTo = new DateTime(9999,12,31),
						SubscriptionCode = orderReference
					};

					await _organizationLimitationsEnforcer.ApplyTariffPlan(planHistory, true);
         
					domainEvents.Add(new SubscriptionPaymentRefundedDomainEvent(subscription.OrganizationCode));
				}

				await _paymentWayForPayRepository.UpdateAsync(subscription);

				await _dispatcher.DispatchAsync(domainEvents);

				var paidAt = DateTimeOffset.FromUnixTimeSeconds(processingDate).UtcDateTime;

				var invoice = new PaymentInvoiceDto
				{
					PaymentWayForPayId = subscription.Id,
					Amount = amount,
					Currency = currency,
					TransactionStatus = transactionStatus,
					PaidAt = paidAt,
					WayForPayResponse = jsonBody,
					CreatedAt = DateTime.UtcNow,
				};

				await _paymentInvoiceRepository.InsertAsync(invoice);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "❌ ProcessWebhookAsync: Error processing WayForPay webhook");
				return false;
			}
		}
	}
}
