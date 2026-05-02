namespace TaskJect.Web.Models.Finance
{
    public class CostEditModel
    {
        public CostDto Cost { get; set; }
        public List<ProjectModel> Projects { get; set; }
        public List<UserModel> Users { get; set; }
    }
}
