namespace Domain.Database
{
    public interface ITaskRepository
    {
        public Task<List<TaskDto>> Retrieve(string organizationCode);
        public Task<TaskDto> Retrieve(Guid taskId, string organizationCode);
        public Task<TaskDto> Retrieve(Guid taskId);
        //public Task<List<TaskDto>> RetrieveByProject(Guid projectId, string organizationCode);
        public Task<List<TaskDto>> RetrieveByProject(Guid projectId, string organizationCode, bool? archived = null);
        public Task<List<TaskDto>> RetrieveByProject(Guid projectId, string organizationCode, DateTime? startDate);

		public Dictionary<Guid, TaskProgressDto> GetTaskProgress(IEnumerable<Guid> projectIds);
        public Task<List<TaskDto>> RetriveByUser(string userId, string organizationCode, int count = 0);
        public Task<List<TaskDto>> RetrieveComplitedTasksByUserAndDate(TaskDetailsRequest taskDetailsRequest);
        public Task<Dictionary<string, TasksCountByUser>> RetrieveCountCompletedTasksByUsers(string organizationCode);
        public Task<List<Guid>> ArchiveCompletedTasks(Guid projectId);
		public Task<bool> Insert(TaskDto projectDto);
        public Task<bool> Update(TaskDto projectDto);
        public Task<bool> Delete(Guid id);
        Task<bool> DeleteByIds(List<Guid> ids);
        public Task<bool> DeteleTasks(string organizationId);
        public Task<bool> DeleteAssigneeFromAllTasks(string userId);
        public Task<bool> DeleteAssigneeFromAllTasksByUsers(List<string> userIds);
        public Task<Dictionary<string, List<int>>> GetTotalTaskStats(TaskStatisticsRequest statisticsRequest, bool isCountStat = true);
        public Task<TaskDetails> GetDetailsTasks(TaskDetailsRequest request);
        public Task<Dictionary<string, int>> GetOverdueTasksDetails(TaskStatisticsRequest statisticsRequest);
        public Task<bool> UpdateAgreedTaskOverdue(Guid id, bool value);
        Task<Dictionary<string, TasksStatisticByPeriod>> GetStatisticByPeriod(string organizationCode, DateTime dateFrom, DateTime dateTo);
        Task<List<TasksStatsByUser>> GetStatisticByUser(string organizationCode, DateTime dateFrom, DateTime dateTo);
        Task<List<AnalyticsUserDetils>> GetTasksByPersonAndDateRange(TaskDetailsRequest request);

        //GitHub
        Task<bool> UpdateTaskStatusByBranch(GitHubUpdateStatusTask statusTask);
        Task<bool> UpdateOwnerByRepos(GitHubUpdateRepo repo);
        Task<bool> UpdateRepoNameByRepos(GitHubUpdateRepo repo);
        Task<bool> DeleteBranch(string? owner, string? repo, string? branchName);
        Task<int?> GetIssueNumberByBranch(string? branch, string? owner, string? repoName);
        Task<bool> BranchExistsForTask(Guid id);
    }
}
