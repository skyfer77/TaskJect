namespace Domain.Database
{
    public class OrganizationUserInfo
    {
        public string OrganizationId { get; set; }
        public int CountUserOrganization { get; set; }
        public ApplicationUserLiteDto TeamLead { get; set; }
    }
}
