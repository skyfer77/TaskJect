namespace Domain.Database
{
    public interface IProjectRepository
    {
        public Task<List<ProjectDto>> RetrieveByOrganization(string organizationCode);
        public Task<List<ProjectDto>> RetrieveProjectsByTeam(Guid teamId);
        public Task<List<Guid>> RetrieveProjectIdsByTeam(Guid teamId);
        public Task<ProjectDto> Retrieve(Guid id);
        public Task<Dictionary<Guid, List<ProjectDto>>> RetrieveByTeamsIDs(List<Guid> teamIds);
        public Task<List<ProjectDto>> RetrieveByProjectIDs(List<Guid> projectIds);
        public Task<Dictionary<Guid, string>> RetrieveNameProject(string organizationCode);
        public Task<Guid> RetrieveTeamId(Guid projectId);
        public Task<string> RetrieveManagerId(Guid projectId);
        public Task<bool> Insert(ProjectDto projectDto);
        public Task<bool> Update(ProjectDto projectDto);
        public Task<bool> Delete(Guid id);
        public Task<bool> DeleteByOrganization(string organizationId);

        //GitHub
        public Task<GitHubInfo> FindGitHubInfo(Guid projectId);
        public Task<bool> UpdateGitHubInfo(ProjectDto projectDto);
        public Task<Guid?> UpdateRepoName(GitHubUpdateRepo repo);
        public Task<List<Guid>> UpdateOwnerByOrganizationId(GitHubUpdateRepo repo);
        public Task<Guid> RetrieveProjectId(string nameRepo, string owner);
        public Task<string?> GetCurrentRepoFullName(Guid projectId);
    }
}
