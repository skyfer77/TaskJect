using TaskJect.Web.Resources;
using Microsoft.Extensions.Localization;

namespace TaskJect.Web.Services
{
	public class TemplateEmailBody : ITemplateEmailBody
	{
		private readonly IStringLocalizer<SharedResources> _sharedLocalizer;
		private readonly string _domain;
		
		public TemplateEmailBody(IConfiguration configuration,
			IStringLocalizer<SharedResources> sharedLocalizer)
		{
			_sharedLocalizer = sharedLocalizer;

			_domain = configuration["Domain"]?.Trim().TrimEnd('/');
		}

		private string logoUrl => $"{_domain}/images/brand-logos/toggle-white.png";
        private string myOrganizationUrl => $"{_domain}/Organization";

		#region Account Templates
		public string TemplateEmailBodyResetPassword(string callbackUrl)
		{
			return $@"
            <html>
            <head>
                {stylesEmail()}
            </head>
            <body>
                <table class='container' cellpadding='0' cellspacing='0' border='0'>
                    <tr>
                        <td>
                            <table class='header' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td width='50'>
                                        <img src='{logoUrl}' style='height: 40px;' alt='TASKJECT'>
                                    </td>
                                    <td>
                                        <h2 style='color: white; margin: 0; font-size: 1.5rem;'>{_sharedLocalizer["ResetPassword"]}</h2>
                                    </td>
                                </tr>
                            </table>

                            <table class='body' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td style='padding: 24px; color: #333;'>
                                        <p>{_sharedLocalizer["PleaseResetPasswordClickingHere"]}:</p>
                                    </td>
                                    <td style='padding: 24px 0; color: #333;'>
                                        <p>
                                            <a href='{callbackUrl}' class='button'>{_sharedLocalizer["ResetPassword"]}</a>
                                        </p>
                                    </td>
                                </tr>
                            </table>

                            <table class='footer' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td style='padding: 20px; font-size: 12px; color: #888; text-align: center;'>
                                        {_sharedLocalizer["IfYouDidNotRequestPasswordReset"]}<br/>
                                        {_sharedLocalizer["NoFurtherActionRequired"]}
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";
		}

		public string SendOnEmailUserLoginData(string email, string password)
		{
			return $@"
                <html>
                <head>
                    {stylesEmail()}
                </head>
                <body>
                   <table class='container' cellpadding='0' cellspacing='0' border='0'>
                        <tr>
                            <td>
                                <table class='header' cellpadding='0' cellspacing='0' width='100%'>
                                    <tr>
                                        <td width='50'>
                                            <img src='{logoUrl}' style='height: 40px;' alt='TASKJECT'>
                                        </td>
                                        <td>
                                            <h2 style='color: white; margin: 0; font-size: 1.5rem;'>{_sharedLocalizer["AccountAuthenticationDetails"]}</h2>
                                        </td>
                                    </tr>
                                </table>

                                <table class='body' cellpadding='0' cellspacing='0' width='100%'>
                                    <tr>
                                        <td style='color: #333;'>
                                            <p style='margin-bottom: 0.5rem;'>{_sharedLocalizer["AcceptedYourRequestToUse"]} <a href='{_domain}'>our service</a></p>
                                            <p style='margin-bottom: 0.5rem;'>{_sharedLocalizer["HereYourAccountAuthenticationDetails"]}:</p>
                                            <p style='margin-bottom: 0.5rem;'><strong>Username:</strong> {email}</p>
                                            <p style='margin-bottom: 0.5rem;'><strong>Password:</strong> {password}</p>
                                            <p style='margin-bottom: 0.5rem;'>{_sharedLocalizer["InformationConfidential"]}</p>
                                        </td>
                                    </tr>
                                </table>

                                <table class='footer' cellpadding='0' cellspacing='0' width='100%'>
                                    <tr>
                                        <td style='padding: 20px; font-size: 12px; color: #888; text-align: center;'>
                                            {_sharedLocalizer["IfYouHaveQuestionsContactUs"]}
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>
            ";
		}

        public string TemplateEmailBodyConfirmEmail(string callbackUrl)
        {
			return $@"
            <html>
            <head>
                {stylesEmail()}
            </head>
            <body>
                <table class='container' cellpadding='0' cellspacing='0' border='0'>
                    <tr>
                        <td>
                            <table class='header' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td width='50'>
                                        <img src='{logoUrl}' style='height: 40px;' alt='TASKJECT'>
                                    </td>
                                    <td>
                                        <h2 style='color: white; margin: 0; font-size: 1.5rem;'>{_sharedLocalizer["ConfirmEmail"]}</h2>
                                    </td>
                                </tr>
                            </table>

                            <table class='body' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td style='padding: 24px; color: #333;'>
                                        <p>{_sharedLocalizer["PleaseConfirmEmailClickingHere"]}:</p>
                                    </td>
                                    <td style='padding: 24px 0; color: #333;'>
                                        <p>
                                            <a href='{callbackUrl}' class='button'>{_sharedLocalizer["ConfirmEmail"]}</a>
                                        </p>
                                    </td>
                                </tr>
                            </table>

                            <table class='footer' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td style='padding: 20px; font-size: 12px; color: #888; text-align: center;'>
                                        {_sharedLocalizer["IfYouHaveQuestionsContactUs"]}
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";
		}

		#endregion

		#region Subscription Templates
		public string TemplateEmailBodySubscriptionCongratulations(SubscriptionEmailParams p)
		{
			return $@"
            <html>
            <head>
                {stylesEmail()}
            </head>
            <body>
                <table class='container' cellpadding='0' cellspacing='0' border='0'>
                    <tr>
                        <td>
                            <table class='header' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td width='50'>
                                        <img src='{logoUrl}' style='height: 40px;' alt='TASKJECT'>
                                    </td>
                                    <td>
                                        <h2 style='color: white; margin: 0; font-size: 1.5rem;'>{_sharedLocalizer["SubscriptionCongratulationsTitle"]}</h2>
                                    </td>
                                </tr>
                            </table>

                            <table class='body' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td style='padding: 24px; color: #333;'>
                                        <p>{string.Format(_sharedLocalizer["SubscriptionCongratulationsMessage"], p.PlanName, @$"<strong>{p.EndDateCultureFormat}</strong>")}</p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding: 0 24px; color: #333;'>
                                        <p>
                                            <a href='{myOrganizationUrl}' class='button'>{_sharedLocalizer["CheckYourOptions"]}</a>
                                        </p>
                                    </td>
                                </tr>
                                
                            </table>

                            <table class='footer' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td style='padding: 20px; font-size: 12px; color: #888; text-align: center;'>
                                        {_sharedLocalizer["IfYouHaveQuestionsContactUs"]}
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";
		}

		public string TemplateEmailBodySubscriptionPaymentFailed(SubscriptionEmailParams p)
		{
			return $@"
            <html>
            <head>
                {stylesEmail()}
            </head>
            <body>
                <table class='container' cellpadding='0' cellspacing='0' border='0'>
                    <tr>
                        <td>
                            <table class='header' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td width='50'>
                                        <img src='{logoUrl}' style='height: 40px;' alt='TASKJECT'>
                                    </td>
                                    <td>
                                        <h2 style='color: white; margin: 0; font-size: 1.5rem;'>{_sharedLocalizer["SubscriptionPaymentFailedTitle"]}</h2>
                                    </td>
                                </tr>
                            </table>

                            <table class='body' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td style='padding: 24px; color: #333;'>
                                        <p>{_sharedLocalizer["SubscriptionPaymentFailedMessage"]}</p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding: 0 24px; color: #333;'>
                                        <p>
                                            <a href='{myOrganizationUrl}' class='button'>{_sharedLocalizer["UpdatePaymentMethod"]}</a>
                                        </p>
                                    </td>
                                </tr>
                                
                            </table>

                            <table class='footer' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td style='padding: 20px; font-size: 12px; color: #888; text-align: center;'>
                                        {_sharedLocalizer["IfYouHaveQuestionsContactUs"]}
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";
		}

		public string TemplateEmailBodySubscriptionPaymentRefunded(SubscriptionEmailParams p)
		{
			return $@"
            <html>
            <head>
                {stylesEmail()}
            </head>
            <body>
                <table class='container' cellpadding='0' cellspacing='0' border='0'>
                    <tr>
                        <td>
                            <table class='header' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td width='50'>
                                        <img src='{logoUrl}' style='height: 40px;' alt='TASKJECT'>
                                    </td>
                                    <td>
                                        <h2 style='color: white; margin: 0; font-size: 1.5rem;'>{_sharedLocalizer["SubscriptionPaymentRefundedTitle"]}</h2>
                                    </td>
                                </tr>
                            </table>

                            <table class='body' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td style='padding: 24px; color: #333;'>
                                        <p>{_sharedLocalizer["SubscriptionPaymentRefundedMessage"]}</p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding: 0 24px; color: #333;'>
                                        <p>
                                            <a href='{myOrganizationUrl}' class='button'>{_sharedLocalizer["ChangeYourPlan"]}</a>
                                        </p>
                                    </td>
                                </tr>
                                
                            </table>

                            <table class='footer' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td style='padding: 20px; font-size: 12px; color: #888; text-align: center;'>
                                        {_sharedLocalizer["IfYouHaveQuestionsContactUs"]}
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";
		}

		public string TemplateEmailBodySubscriptionExpiresInWeek(SubscriptionEmailParams p)
		{
			return $@"
            <html>
            <head>
                {stylesEmail()}
            </head>
            <body>
                <table class='container' cellpadding='0' cellspacing='0' border='0'>
                    <tr>
                        <td>
                            <table class='header' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td width='50'>
                                        <img src='{logoUrl}' style='height: 40px;' alt='TASKJECT'>
                                    </td>
                                    <td>
                                        <h2 style='color: white; margin: 0; font-size: 1.5rem;'>{_sharedLocalizer["SubscriptionExpiresInWeekTitle"]}</h2>
                                    </td>
                                </tr>
                            </table>

                            <table class='body' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td style='padding: 24px; color: #333;'>
                                        <p>{_sharedLocalizer["SubscriptionExpiresInWeekMessage"]}</p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding: 0 24px; color: #333;'>
                                        <p>
                                            <a href='{myOrganizationUrl}' class='button'>{_sharedLocalizer["ContinueSubscription"]}</a>
                                        </p>
                                    </td>
                                </tr>
                                
                            </table>

                            <table class='footer' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td style='padding: 20px; font-size: 12px; color: #888; text-align: center;'>
                                        {_sharedLocalizer["IfYouHaveQuestionsContactUs"]}
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";
		}

		public string TemplateEmailBodySubscriptionExpiresIn3Days(SubscriptionEmailParams p)
		{
			return $@"
            <html>
            <head>
                {stylesEmail()}
            </head>
            <body>
                <table class='container' cellpadding='0' cellspacing='0' border='0'>
                    <tr>
                        <td>
                            <table class='header' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td width='50'>
                                        <img src='{logoUrl}' style='height: 40px;' alt='TASKJECT'>
                                    </td>
                                    <td>
                                        <h2 style='color: white; margin: 0; font-size: 1.5rem;'>{_sharedLocalizer["SubscriptionExpiresIn3DaysTitle"]}</h2>
                                    </td>
                                </tr>
                            </table>

                            <table class='body' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td style='padding: 24px; color: #333;'>
                                        <p>{_sharedLocalizer["SubscriptionExpiresIn3DaysMessage"]}</p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding: 0 24px; color: #333;'>
                                        <p>
                                            <a href='{myOrganizationUrl}' class='button'>{_sharedLocalizer["ContinueSubscription"]}</a>
                                        </p>
                                    </td>
                                </tr>
                                
                            </table>

                            <table class='footer' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td style='padding: 20px; font-size: 12px; color: #888; text-align: center;'>
                                        {_sharedLocalizer["IfYouHaveQuestionsContactUs"]}
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";
		}

		public string TemplateEmailBodySubscriptionExpired(SubscriptionEmailParams p)
		{
			return $@"
            <html>
            <head>
                {stylesEmail()}
            </head>
            <body>
                <table class='container' cellpadding='0' cellspacing='0' border='0'>
                    <tr>
                        <td>
                            <table class='header' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td width='50'>
                                        <img src='{logoUrl}' style='height: 40px;' alt='TASKJECT'>
                                    </td>
                                    <td>
                                        <h2 style='color: white; margin: 0; font-size: 1.5rem;'>{_sharedLocalizer["SubscriptionExpiredTitle"]}</h2>
                                    </td>
                                </tr>
                            </table>

                            <table class='body' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td style='padding: 24px; color: #333;'>
                                        <p>{_sharedLocalizer["SubscriptionExpiredMessage"]}</p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding: 0 24px; color: #333;'>
                                        <p>
                                            <a href='{myOrganizationUrl}' class='button'>{_sharedLocalizer["ReactivateSubscription"]}</a>
                                        </p>
                                    </td>
                                </tr>
                                
                            </table>

                            <table class='footer' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td style='padding: 20px; font-size: 12px; color: #888; text-align: center;'>
                                        {_sharedLocalizer["IfYouHaveQuestionsContactUs"]}
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";
		}

		#endregion

		public string TemplateEmailBodyFreemiumEnded(SubscriptionEmailParams p)
		{
			return $@"
            <html>
            <head>
                {stylesEmail()}
            </head>
            <body>
                <table class='container' cellpadding='0' cellspacing='0' border='0'>
                    <tr>
                        <td>
                            <table class='header' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td width='50'>
                                        <img src='{logoUrl}' style='height: 40px;' alt='TASKJECT'>
                                    </td>
                                    <td>
                                        <h2 style='color: white; margin: 0; font-size: 1.5rem;'>{_sharedLocalizer["SubscriptionFreemiumEndedTitle"]}</h2>
                                    </td>
                                </tr>
                            </table>

                            <table class='body' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td style='padding: 24px; color: #333;'>
                                        <p>{_sharedLocalizer["SubscriptionFreemiumEndedMessage"]}</p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding: 0 24px; color: #333;'>
                                        <p>
                                            <a href='{myOrganizationUrl}' class='button'>{_sharedLocalizer["UpgradePlan"]}</a>
                                        </p>
                                    </td>
                                </tr>
                                
                            </table>

                            <table class='footer' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td style='padding: 20px; font-size: 12px; color: #888; text-align: center;'>
                                        {_sharedLocalizer["IfYouHaveQuestionsContactUs"]}
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";
		}

		private string stylesEmail()
		{
			return @"<style>
                    a{
                        color: #845adf !important;
                    }
                    .container {
                        width: 600px;
                        margin: 30px auto;
                        background-color: #ffffff;
                        border-radius: 8px;
                        overflow: hidden;
                        box-shadow: 0 0 10px rgba(0,0,0,0.1);
                        font: message-box;
                        border: 1px solid #343a40;
                    }
                    .header {
                        background-color: #343a40;
                        color: white;
                        padding: 20px;
                        display: flex;
                    }
                    .header img {
                        height: 40px;
                        margin-right: 15px;
                    }
                    .body {
                        padding: 30px;
                        color: #333;
                        display: flex;
                    }
                    .button {
                        display: inline-block;
                        padding: 10px 20px;
                        color: white !important;
                        background-color: #845adf;
                        text-decoration: none;
                        border-radius: 5px;
                        font-weight: bold;
                    }
                    .footer {
                        font-size: 12px;
                        color: #888;
                        text-align: center;
                    }
                </style>";
		}
	}
}
