using TaskJect.Web.Attributes;

namespace TaskJect.Web.Enums
{
    public enum NotificationType
    {
        //Global notification
        [DisplayNotification("SystemTitleNewUpdate", "NewSystemUpdateMessage")]
        NewSystemUpdate,

        //Task notification
        [DisplayNotification("TaskTitleAssigneeChanged", "YouSetExecutorForTheTask")]
        TaskAssigneeChanged,

        [DisplayNotification("TaskTitleDedlineChanged", "TaskDeadlineChanged")]
        TaskDedlineChanged,
        [DisplayNotification("TaskTitleDedlineSet", "TaskDeadlineSet")]
        TaskDedlineSet,

        [DisplayNotification("TaskTitleStatusOnReview", "TheTaskTransferredStatusOnReview")]
        TaskStatusOnReview,
        [DisplayNotification("TaskTitleStatusCompleted", "YourTaskHasStatusCompleted")]
        TaskStatusCompleted,

        [DisplayNotification("TaskTitleCreated", "ForYouWasCreatedTask")]
        TaskCreated,

        [DisplayNotification("TaskTitleUpdated", "YourTaskWasUpdated")]
        TaskUpdated,

        [DisplayNotification("YouGotRequest", "OrganizationRequestRecieved")]
        OrganizationAppealSent
    }
}