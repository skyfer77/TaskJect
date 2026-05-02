namespace TaskJect.Web.Services
{
	public interface ITemplateEmailBody
	{
		string TemplateEmailBodyResetPassword(string callbackUrl);
		string SendOnEmailUserLoginData(string email, string password);
		string TemplateEmailBodyConfirmEmail(string callbackUrl);
		string TemplateEmailBodySubscriptionCongratulations(SubscriptionEmailParams p);
		string TemplateEmailBodySubscriptionPaymentFailed(SubscriptionEmailParams p);
		string TemplateEmailBodySubscriptionPaymentRefunded(SubscriptionEmailParams p);
		string TemplateEmailBodySubscriptionExpiresInWeek(SubscriptionEmailParams p);
		string TemplateEmailBodySubscriptionExpiresIn3Days(SubscriptionEmailParams p);
		string TemplateEmailBodySubscriptionExpired(SubscriptionEmailParams p);
		string TemplateEmailBodyFreemiumEnded(SubscriptionEmailParams p);
	}
}
