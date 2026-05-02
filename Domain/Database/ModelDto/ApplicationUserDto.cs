using System.ComponentModel.DataAnnotations;

namespace Domain.Database
{
    public class ApplicationUserDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PasswordHash { get; set; }
        public bool SecurityStamp { get; set; }
        public bool ConcurrencyStamp { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTimeOffset LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public DateTime? Birthday { get; set; }
        public string OrganizationCode { get; set; }
        public bool IsNewUser { get; set; }
        public string? Culture { get; set; }

        [MaxLength(16)]
        public string? CardNumber { get; set; }
        public int? RewardLevel { get; set; }
        public string? TelegramUserName { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public Enums.OrganizationRoles RoleInOrganization { get; set; } = 0;
        public string? TelegramChatId { get; set; }
        public string? TelegramTicket { get; set; }
    }
}
