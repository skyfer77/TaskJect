using TaskJect.Web.Models;
using TaskJect.Web.Resources;
using Microsoft.Extensions.Localization;
using Data;

namespace TaskJect.Web.Services
{
    public class EmailRequestSender : IEmailRequestSender
    {
        private readonly IEmailSender _emailSender;
        private readonly IStringLocalizer<LandingResources> _localizerLanding;
        public EmailRequestSender(IEmailSender emailSender , IStringLocalizer<LandingResources> localizerLanding)
        {
            _emailSender = emailSender;
            _localizerLanding = localizerLanding;
        }

        public async Task SendRequestEmailAsync(EmailRequestModel request)
        {
            var subject = $"Запит на використання сервісу Taskject від {request.Name}";
            var body = $"<p>Повідомлення: {request.Message}</p><br/ ><p><strong>Пошта для зв'язку:</strong> {request.Email}</p><br/>" +
                $"<p><strong>Телефон:</strong> {request.Phone}</p>";
            
            try
            {
                _emailSender.SendEmailAsync(SD.SenderEmail, subject, body);
                await sendRequestFeedback(request);
            }
            catch (Exception ex)
            {
                var exmessage = ex.Message;
            }
        }

        private async Task sendRequestFeedback(EmailRequestModel request)
        {
            var subject = _localizerLanding["FeedbackSubjectForRequest"];
            var body = _localizerLanding["FeedbackBodyForRequest"];
            _emailSender.SendEmailAsync(request.Email, subject, body);
        }
    }
}
