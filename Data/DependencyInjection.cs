using Data.Database.Repository;
using Data.DbContexts;
using Data.DomainEvent;
using Data.Services;
using Domain.Database;
using Domain.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Data
{                 
    public static class DependencyInjection
    {
        public static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
        {
            var cs = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContextFactory<ApplicationDbContext>(options =>
                options.UseSqlServer(cs));

            // Repository
            services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
            services.AddScoped<ITeamRepository, TeamRepository>();
            services.AddScoped<IMembershipRepository, MembershipRepository>();
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            services.AddScoped<FullDeleteByTransaction>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IProjectUserPermissionRepository, ProjectUserPermissionRepository>();
            services.AddScoped<ITaskRepository, TaskRepository>();
            services.AddScoped<ITariffPlanRepository, TariffPlanRepository>();
            services.AddScoped<ITariffPlanHistoryRepository, TariffPlanHistoryRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IOrganizationAppealRepository, OrganizationAppealRepository>();
            services.AddScoped<IOrganizationFilesRepository, OrganizationFilesRepository>();
            services.AddScoped<IPersonalTodoRepository, PersonalTodoRepository>();
            services.AddScoped<IPersonalTodoTaskRepository, PersonalTodoTaskRepository>();
            services.AddScoped<IPersonalNoteRepository, PersonalNoteRepository>();
            services.AddScoped<IGumroadWebhookLogRepository, GumroadWebhookLogRepository>();
            services.AddScoped<IDataSizeCalculator, DataSizeCalculator>();
            services.AddScoped<IOrganizationLimitationsEnforcer, OrganizationLimitationsEnforcer>();
            services.AddScoped<FullUnlinkGitHubByTransaction>();
            services.AddScoped<IPaymentWayForPayRepository, PaymentWayForPayRepository>();
            services.AddScoped<IPaymentInvoiceRepository, PaymentInvoiceRepository>();

            services.AddScoped<DomainEventDispatcher>();
            
            services.AddSingleton<ITelegramTicketGenerator, TelegramTicketGenerator>();
            services.AddSingleton<IOrganizationStorageChecker, OrganizationStorageChecker>();

            return services;
        }
    }
}
