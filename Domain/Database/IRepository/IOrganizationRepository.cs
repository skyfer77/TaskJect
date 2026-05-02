namespace Domain.Database
{
    public interface IOrganizationRepository
    {
        Task<IEnumerable<OrganizationDto>> Retrieve();
        Task<OrganizationDto> GetOrganizationById(Guid organizetionId);
        Task<List<OrganizationDto>> GetOrganizationsByIds(List<Guid> organizationIds);
        Task<OrganizationDto> GetOrganizationByName(string organizetionName);
        Task<bool> Insert(OrganizationDto organizationDto);
        Task<bool> Update(OrganizationDto organizationDto);
        Task<bool> LockoutUnlockout(bool isLockout, Guid organizationId);
        Task<bool> DeleteOrganization(string organizationId);
        Task<bool> Delete(Guid organizationId);

        //GitHub
        Task<long?> FindGitHubInstallationId(Guid organizetionId);
        Task<bool> SetGitHubInstallationId(Guid organizetionId, long installationId);
        Task<Guid?> GetIdByInstallationId(long installationId);
    }
}
