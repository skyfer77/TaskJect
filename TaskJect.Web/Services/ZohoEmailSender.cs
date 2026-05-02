using System.Net.Mail;
using System.Net;

namespace TaskJect.Web.Services
{
    public class ZohoEmailSender : IEmailSender
    {

        string _senderEmail = "";
        string _password = "";
        public async Task SendEmailAsync(string emailAddress, string subject, string body)
        {
            var smtpClient = new SmtpClient("smtp.zoho.eu")
            {
                Port = 587,
                Credentials = new NetworkCredential(_senderEmail, _password),
                EnableSsl = true,
                //UseDefaultCredentials = false,

            };

            var mailMessage = new MailMessage(_senderEmail, emailAddress)
            {
                Subject = subject,
                Body = body,
                SubjectEncoding = System.Text.Encoding.UTF8,
                IsBodyHtml = true
            };


            try
            {
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                var exmessage = ex.Message;
            }
        }
    }
}
