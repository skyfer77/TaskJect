namespace Domain.Database
{
    public class CreateUserByEmailModel
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string OrganizationCode { get; set; }
        public string RoleUser { get; set; }
    }
}
