namespace Domain.Database
{
    public class Membership
    {
        public Guid MembershipId { get; set; }
        public Guid TeamId { get; set; }
        public string UserId { get; set; } = null!;
    }
}


