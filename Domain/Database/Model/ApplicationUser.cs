using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Domain.Database
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        [MaxLength(16)]
        public string? CardNumber { get; set; }
        public int? RewardLevel { get; set; }
        public DateTime? Birthday { get; set; }
        public string? TelegramUserName { get; set; }
        public string OrganizationCode { get; set; }
        public bool IsNewUser { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public Enums.OrganizationRoles RoleInOrganization { get; set; } = 0;
        public string? TelegramChatId { get; set; }
        public string? TelegramTicket { get; set; }
        public string? Culture { get; set; }

        //TODO: GitHub OAuth
        //public bool GitHubConnected { get; set; } = false;
        //public string? GitHubLogin { get; set; }
        //public string? GitHubId { get; set; } // user.id з GitHub
        //public string? GitHubAccessToken { get; set; } // короткоживучий токен OAuth
        //public string? GitHubRefreshToken { get; set; } // refresh token для оновлення
        //public DateTime GitHubTokenExpiresAt { get; set; }
    }
}
