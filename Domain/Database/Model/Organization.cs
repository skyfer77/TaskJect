namespace Domain.Database
{
    public class Organization
    {
        public Guid OrganizationId { get; set; }

        public string Name { get; set; } = null!;

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Picture { get; set; }

        public DateTime RegistrationDate { get; set; }

        public bool? LockoutEnabled { get; set; }

        public DateTime? LockoutEnd { get; set; }
        public long UsedStorageSpace { get; set; }

        public long? GitHubInstallationId { get; set; } // для GitHub App
    }
}


