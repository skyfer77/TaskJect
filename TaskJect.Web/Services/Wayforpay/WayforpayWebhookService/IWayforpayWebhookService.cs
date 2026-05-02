namespace TaskJect.Web.Services
{
	public interface IWayforpayWebhookService
	{
		Task<bool> ProcessWebhookAsync(string jsonBody);
	}
}
