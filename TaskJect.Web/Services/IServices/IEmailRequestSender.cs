using TaskJect.Web.Models;

namespace TaskJect.Web.Services
{
    public interface IEmailRequestSender
    {
        Task SendRequestEmailAsync(EmailRequestModel request);
    }
}
