using Domain.Database;
using Domain.IServices;
using Data.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace TaskJect.Web.Services.BackgroundServices
{
    public class TelegramTicketMaintenanceService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TelegramTicketMaintenanceService> _logger;
        private readonly ITelegramTicketGenerator _ticketGenerator;
        public TelegramTicketMaintenanceService(IServiceProvider serviceProvider, 
            ILogger<TelegramTicketMaintenanceService> logger,
            ITelegramTicketGenerator ticketGenerator)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _ticketGenerator = ticketGenerator;
        }

        protected override async System.Threading.Tasks.Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await updateTicketsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Помилка під час оновлення Telegram тікетів");
                }

                // Чекаємо 24 години
                await System.Threading.Tasks.Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }

        private async System.Threading.Tasks.Task updateTicketsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            //користувачі будуть вибиратись пачками, максимум по 500, щоб уникнути перевантаження пам'яті
            //ToListAsync - вантажить все в пам'ять, тому використовуємо Skip/Take
            const int pageSize = 500;
            int page = 0;
            List<ApplicationUser> users;
            do
            {
                users = await dbContext.Users
                    .Skip(page * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                foreach (var user in users)
                {
                    //if (user.TelegramChatId == null)
                    {
                        // Якщо користувач ще не зареєстрований, генеруємо або оновлюємо тікет
                        user.TelegramTicket = _ticketGenerator.GenerateTicket();
                    }
                    /*else
                    {
                        // Якщо користувач вже зареєстрований в Telegram — очищаємо тікет
                        user.TelegramTicket = null;
                    }*/
                }
                page++;
                await dbContext.SaveChangesAsync();
                dbContext.ChangeTracker.Clear();
            }
            while (users.Count > 0);

            _logger.LogInformation("Оновлення Telegram тікетів завершено успішно.");
        }
    }

}
