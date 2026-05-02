using Domain.Database;
namespace TaskJect.Web.Models
{
    public class OrganizationInfo
    {
        public OrganizationDto Organization { get; set; }
        public List<ApplicationUserLiteView> Users { get; set; }
        public TariffPlanDto TariffPlan { get; set; }
        public TariffPlanHistoryDto CurrentTariffPlan { get; set; }
        public int IsAppealCount { get; set; }
    }
}
