using TaskJect.Web.Enums;

namespace TaskJect.Web.Models.Finance
{
    public class IncomeDto
    {
        public Guid? Id { get; set; }
        public double Amount { get; set; }
        public IncomeSourceType IncomeSourceType { get; set; }
        public DateTime IncomingDate { get; set; }
        public DateTime EndDate { get; set; }
        public PeriodType PeriodType { get; set; }
        public string? ProjectId { get; set; }
        public string? Note { get; set; }
    }

    public class IncomeDetailDto
    {
        public Guid Id { get; set; }
        public Guid IncomeId { get; set; }
        public double Amount { get; set; }
        public DateTime CreatedDate { get; set; }
        public IncomeDto Income { get; set; }
    }
}
