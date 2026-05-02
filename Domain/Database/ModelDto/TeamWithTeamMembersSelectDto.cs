namespace Domain.Database
{
    public class TeamWithTeamMembersSelectDto
    {
        public Guid TeamId { get; set; }
        public string[] SelectedUsersId { get; set; }
    }
}
