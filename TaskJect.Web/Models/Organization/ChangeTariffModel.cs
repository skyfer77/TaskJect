using Domain.Database;
namespace TaskJect.Web.Models
{
    public class PlanValues
    { 
        public TariffPlanDto TariffPlan { get; set; }
        public string? SubscribeLink { get; set; }
        public DateTime? ExpirationDate { get; set; }

        public PlanValues(TariffPlanDto tariffPlan, string subscribeLink)
        {
            TariffPlan = tariffPlan;
            SubscribeLink = subscribeLink;
        }
        public PlanValues(TariffPlanDto tariffPlan)
        {
            TariffPlan = tariffPlan;
        }

        public PlanValues()
        {

        }
    }

}
