namespace TaskJect.Web.Models.Finance
{
    public class FinanceViewModel
    {
        public List<CostDto> Costs { get; set; }
        public List<IncomeDto> Incomes { get; set; }
        public List<ProjectModel> Projects { get; set; }
        public List<UserModel> Users { get; set; }
        public FinanceViewModel()
        {
            Costs = new List<CostDto>();
            Incomes = new List<IncomeDto>();
            Projects = new List<ProjectModel>();
            Users = new List<UserModel>();
        }
    }
}
