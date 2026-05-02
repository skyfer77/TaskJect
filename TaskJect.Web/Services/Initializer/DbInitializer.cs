using Domain.Database;

namespace TaskJect.Web.Services
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ITariffPlanRepository _tariffPlan;
        public DbInitializer(ITariffPlanRepository tariffPlan)
        {
            _tariffPlan = tariffPlan;
        }

        public async System.Threading.Tasks.Task InitializeAsync()
        {
            var defaultTariff = await _tariffPlan.Retrieve("Default");
            if (defaultTariff == null)
            {
                var defaultTariffPlan = new TariffPlanDto
                {
                    Code = "Default",
                    Name = "Default",
                    MaxUsers = 7,
                    MaxStorageBytes = 1L * 1024 * 1024 * 1024, // 1 GB
                    CountRequests = 1,
                    HasTelegramIntegration = false
                };

                await _tariffPlan.Insert(defaultTariffPlan);
            }

            var proTariff = await _tariffPlan.Retrieve("Pro");
            if (proTariff == null)
            {
                var proTariffPlan = new TariffPlanDto
                {
                    Code = "Pro",
                    Name = "Pro",
                    MaxUsers = 15,
                    MaxStorageBytes = 5L * 1024 * 1024 * 1024, // 5 GB
                    CountRequests = 3,
                    HasTelegramIntegration = true
                };

                await _tariffPlan.Insert(proTariffPlan);
            }

            var expertTariff = await _tariffPlan.Retrieve("Expert");
            if (expertTariff == null)
            {
                var expertTariffPlan = new TariffPlanDto
                {
                    Code = "Expert",
                    Name = "Expert",
                    MaxUsers = 30,
                    MaxStorageBytes = 10L * 1024 * 1024 * 1024, // 10 GB
                    CountRequests = 5,
                    HasTelegramIntegration = true
                };

                await _tariffPlan.Insert(expertTariffPlan);
            }
        }
    }
}
