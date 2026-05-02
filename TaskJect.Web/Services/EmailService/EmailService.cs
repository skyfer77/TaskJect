using TaskJect.Web.Resources;
using Microsoft.Extensions.Localization;
using Task = System.Threading.Tasks.Task;

namespace TaskJect.Web.Services
{
	public class EmailService : IEmailService
	{
		private readonly IEmailSender _emailSender;
		private readonly ITemplateEmailBody _templateEmailBody;
		private readonly IStringLocalizer<SharedResources> _localizer;

		public EmailService(IEmailSender emailSender, ITemplateEmailBody templateEmailBody,
			IStringLocalizer<SharedResources> localizer)
		{
			_emailSender = emailSender;
			_templateEmailBody = templateEmailBody;
			_localizer = localizer;
		}

		public async Task SendEmailAsync(SendEmailParams emailParams)
		{
			switch (emailParams)
			{
				case AccountEmailParams account:
					await sendAccountEmailAsync(account);
					break;

				case SubscriptionEmailParams subscription:
					await sendSubscriptionEmailAsync(subscription);
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(emailParams));
			}
		}

		private async Task sendAccountEmailAsync(AccountEmailParams p)
		{
			switch (p.Type)
			{
				case AccountEmailType.ResetPassword:
					await sendResetPasswordAsync(p.Email, p.CallbackUrl!);
					break;

				case AccountEmailType.ConfirmEmail:
					await sendConfirmEmailAsync(p.Email, p.CallbackUrl!);
					break;

				case AccountEmailType.AccountCreated:
					await sendAccountCreatedAsync(p.Email, p.TempPassword!); 
					break;
			}
		}
		private async Task sendSubscriptionEmailAsync(SubscriptionEmailParams p)
		{
			switch(p.Type)
			{
				case SubscriptionEmailType.Congratulations:
					await sendCongratulationsAsync(p);
					break;

				case SubscriptionEmailType.PaymentFailed:
					await sendPaymentFailedAsync(p);
					break;

				case SubscriptionEmailType.PaymentRefunded:
					await sendPaymentRefundedAsync(p);
					break;

				case SubscriptionEmailType.ExpirationInWeek:
					await sendExpirationInWeekAsync(p);
					break;

				case SubscriptionEmailType.ExpirationIn3Days:
					await sendExpirationIn3DayAsync(p);
					break;

				case SubscriptionEmailType.Expired:
					await sendExpiredAsync(p);
					break;
			}
		}

		#region Account Email
		private async Task sendResetPasswordAsync(string email, string callbackUrl)
		{
			var body = _templateEmailBody.TemplateEmailBodyResetPassword(callbackUrl);
			var subject = $"Taskject — {_localizer["ResetPassword"]}";


			await _emailSender.SendEmailAsync(email, subject, body);
		}

		private async Task sendConfirmEmailAsync(string email, string callbackUrl)
		{
			var body = _templateEmailBody.TemplateEmailBodyConfirmEmail(callbackUrl);
			var subject = $"Taskject — {_localizer["ConfirmEmail"]}";

			await _emailSender.SendEmailAsync(email, subject, body);
		}

		private async Task sendAccountCreatedAsync(string email, string tempPassword)
		{
			var body = _templateEmailBody.SendOnEmailUserLoginData(email, tempPassword);
			var subject = $"Taskject — {_localizer["YourAccountAuthenticationDetails"]}";

			await _emailSender.SendEmailAsync(email, subject, body);
		}

		#endregion

		#region Subscription Email

		private async Task sendCongratulationsAsync(SubscriptionEmailParams p)
		{
			var body = _templateEmailBody.TemplateEmailBodySubscriptionCongratulations(p);
			var subject = _localizer["TaskjectNotification"];

			await _emailSender.SendEmailAsync(p.Email, subject, body);
		}
		private async Task sendPaymentFailedAsync(SubscriptionEmailParams p)
		{
			var body = _templateEmailBody.TemplateEmailBodySubscriptionPaymentFailed(p);
			var subject = _localizer["TaskjectNotification"];

			await _emailSender.SendEmailAsync(p.Email, subject, body);
		}
		private async Task sendPaymentRefundedAsync(SubscriptionEmailParams p)
		{
			var body = _templateEmailBody.TemplateEmailBodySubscriptionPaymentRefunded(p);
			var subject = _localizer["TaskjectNotification"];

			await _emailSender.SendEmailAsync(p.Email, subject, body);
		}
		private async Task sendExpirationInWeekAsync(SubscriptionEmailParams p)
		{
			var body = _templateEmailBody.TemplateEmailBodySubscriptionExpiresInWeek(p);
			var subject = _localizer["TaskjectNotification"];

			await _emailSender.SendEmailAsync(p.Email, subject, body);
		}
		private async Task sendExpirationIn3DayAsync(SubscriptionEmailParams p)
		{
			var body = _templateEmailBody.TemplateEmailBodySubscriptionExpiresIn3Days(p);
			var subject = _localizer["TaskjectNotification"];

			await _emailSender.SendEmailAsync(p.Email, subject, body);
		}
		private async Task sendExpiredAsync(SubscriptionEmailParams p)
		{
			var body = _templateEmailBody.TemplateEmailBodySubscriptionExpired(p);
			var subject = _localizer["TaskjectNotification"];

			await _emailSender.SendEmailAsync(p.Email, subject, body);
		}

		#endregion

		//TODO: to finish / or rework
		private async Task sendFreemiumEndedAsync(SubscriptionEmailParams p)
		{
			var body = _templateEmailBody.TemplateEmailBodyFreemiumEnded(p);
			var subject = _localizer["TaskjectNotification"];

			await _emailSender.SendEmailAsync(p.Email, subject, body);
		}
	}
}
