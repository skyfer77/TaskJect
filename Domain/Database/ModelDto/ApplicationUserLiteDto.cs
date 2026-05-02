namespace Domain.Database
{
    public class ApplicationUserLiteDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CardNumber { get; set; }
        public int? RewardLevel { get; set; }
        public DateTime? Birthday { get; set; }
        public string? TelegramUserName { get; set; }
        public string TelegramTicket { get; set; }
        public string? TelegramChatId { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? Role { get; set; }
        public Enums.OrganizationRoles RoleInOrganization { get; set; }
        public string OrganizationCode { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public string? Culture { get; set; }

        //TODO: GitHub OAuth
        //public bool GitHubConnected { get; set; } = false;
        //public string? GitHubLogin { get; set; }
        //public string? GitHubId { get; set; }
        //public string? GitHubAccessToken { get; set; }
        //public string? GitHubRefreshToken { get; set; }
        //public DateTime? GitHubTokenExpiresAt { get; set; }
    }
}