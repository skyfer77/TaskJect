using Domain.Database;
using Task = System.Threading.Tasks.Task;

namespace TaskJect.Web.Services
{
	public interface IEmailService
	{
		Task SendEmailAsync(SendEmailParams emailParams);
	}

	public enum AccountEmailType
	{
		ResetPassword,
		ConfirmEmail,
		AccountCreated
	}

	public enum SubscriptionEmailType
	{
		Congratulations,
		PaymentFailed,
		PaymentRefunded,
		ExpirationInWeek,
		ExpirationIn3Days,
		Expired
	}

	public abstract class SendEmailParams
	{
	}

	public sealed class AccountEmailParams : SendEmailParams
	{
		public AccountEmailType Type { get; init; }
		public string Email { get; init; }
		public string? CallbackUrl { get; init; }
		public string? TempPassword { get; init; }
	}

	public sealed class SubscriptionEmailParams : SendEmailParams
	{
		public SubscriptionEmailType Type { get; init; }
		public string OrganizationCode { get; init; }
		public string? Email { get; init; }
		public string? EndDateCultureFormat { get; init; }
		public string? PlanName { get; init; }
	}

}
