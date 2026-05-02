namespace TaskJect.Web.Models.Finance
{
    public class CostDetailsModel
    {
        public List<CostDetailDto> CostDetail { get; set; }
        public List<ProjectModel> Projects { get; set; }
        public List<UserModel> Users { get; set; }
    }
}
