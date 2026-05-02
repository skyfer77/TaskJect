namespace Domain.Database;

public partial class Project
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? ShortDescription { get; set; }

    public string? ManagerId { get; set; }

    public string? Client { get; set; }

    public int? Status { get; set; }

    public int? Priority { get; set; }

    public Guid? TeamId { get; set; }

    public string? File { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string OrganizationCode { get; set; } = null!;

    public DateTime? DateAdd { get; set; }
    public DateTime? DateEdit { get; set; }

    // GitHub
    public string? GitHubOwner { get; set; } // org або user
    public string? GitHubRepoName { get; set; }
}
