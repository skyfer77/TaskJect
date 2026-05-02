using Domain.IServices;
namespace Data.Services
{
    internal class TelegramTicketGenerator : ITelegramTicketGenerator
    {
        public string GenerateTicket()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 10);
        }
    }
}
