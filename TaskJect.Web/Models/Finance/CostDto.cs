using TaskJect.Web.Enums;

namespace TaskJect.Web.Models.Finance
{
    public class CostDto
    {
        public Guid? Id { get; set; }
        public CostTargetType CostTargetType { get; set; }
        public PeriodType PeriodType { get; set; }
        public DateTime CostDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? UserId { get; set; }
        public double Amount { get; set; }
        public string? ProjectId { get; set; }
        public string? Note { get; set; }
    }

    public class CostDetailDto
    {
        public Guid Id { get; set; }
        public Guid CostId { get; set; }
        public double Amount { get; set; }

        public DateTime CreatedDate { get; set; }
        public CostDto Cost { get; set; }
    }
}
