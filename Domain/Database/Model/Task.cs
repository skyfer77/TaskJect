using Domain.DomainEvents;
using Domain.Handlers;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Database;

[Index("ProjectId", Name = "IX_Tasks_ProjectId")]
[Index(nameof(AssigneeId), nameof(Status), nameof(OrganizationCode), Name = "IX_Tasks_Assignee_Status_Organization")]
public partial class Task : BaseEntity
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public string? AssigneeId { get; set; }
    public string? CreatedByUserId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public Enums.TaskStatus Status { get; set; }

    public Enums.Priority Priority { get; set; }

    public int Complexity { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? ComplitedDate { get; set; }

    public DateTime? ArchivedDate { get; set; }

	public bool? IsAgreedOverdue { get; set; }

    public string? PerformanceNote { get; set; }

    public DateTime? ReviewDate { get; set; }

    public int? ActualHours { get; set; }

    public int? ActualMinutes { get; set; }

    public string OrganizationCode { get; set; } = null!;
    public DateTime? DateAdd { get; set; }
    public DateTime? DateEdit { get; set; }

    // GitHub
    public bool IsGitHubIntegration { get; set; }
    public string? GitHubBranch { get; set; }
    public string? GitHubOwner { get; set; } // org або user
    public string? GitHubRepoName { get; set; }
    public int? GitHubIssueNumber { get; set; } //key Issue

    public void MarkAsCreated()
    {
        AddDomainEvent(new TaskCreatedDomainEvent(Id));
    }

    public void MarkAsUpdated()
    {
        AddDomainEvent(new TaskUpdatedDomainEvent(Id));
    }

    public void UpdateStatus(Enums.TaskStatus newStatus)
    {
        if (Status != newStatus)
        {
			var oldStatus = Status;
            Status = newStatus;

			var now = DateTime.UtcNow;
			applyStatusDates(newStatus, now);

			AddDomainEvent(new TaskStatusChangedDomainEvent(this.Id, oldStatus, newStatus));
        }
    }

	private void applyStatusDates(Enums.TaskStatus newStatus, DateTime now)
	{
		switch (newStatus)
		{
			case Enums.TaskStatus.OnReview:
				ReviewDate = now;
				ComplitedDate = null;
				ArchivedDate = null;
				break;

			case Enums.TaskStatus.Done:
				ComplitedDate ??= now;
				ReviewDate ??= now;
				ArchivedDate = null;
				break;

			case Enums.TaskStatus.Archived:
				ArchivedDate = now;
				ComplitedDate ??= now;
				break;

			default:
				// NotStarted / InProgress / OnHold
				ComplitedDate = null;
				ReviewDate = null;
				ArchivedDate = null;
				break;
		}
	}

	public void UpdateDeadline(DateTime newDeadline)
    {
        if ((EndDate.HasValue && EndDate.Value != newDeadline) || (!EndDate.HasValue && newDeadline != default))
        {
            var oldDeadline = EndDate;
            EndDate = newDeadline;

            AddDomainEvent(new TaskDeadlineChangedDomainEvent(this.Id, oldDeadline, newDeadline));
        }
    }

    public void UpdateAssignee(Guid newAssigneeId)
    {
        if (AssigneeId != newAssigneeId.ToString())
        {
            var oldAssignee =  string.IsNullOrEmpty(AssigneeId) ? Guid.Empty : Guid.Parse(AssigneeId);
            AssigneeId = newAssigneeId.ToString();

            AddDomainEvent(new TaskAssigneeChangedDomainEvent(this.Id, oldAssignee, newAssigneeId));
        }
    }

}

[NotMapped]
public class TaskProgressDto
{
    public Guid ProjectId { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
}

[NotMapped]
public class TasksCountByUser
{
    public string UserId { get; set; }
    public int SumCountTask { get; set; }

}
