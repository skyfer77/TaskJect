using TaskJect.Web.Models;
using System.Text.Json;

namespace TaskJect.Web.Services
{
	public interface IWayforpayServices
	{
		Task<string> CreateRegularPaymentAsync(WayforpaySubscriptionView subscription);
		Task<bool> CancelSubscriptionAsync(string organizationCode);
		Task<string?> ChangePlanAsync(WayforpaySubscriptionView newSubscription);
		Task<RegularPaymentStatus?> GetRegularPaymentStatusAsync(string orderReference);
		bool VerifySignature(JsonElement json);
		Task<decimal> GetCurrencyRatesAsync(string currencyCode);
	}
}
