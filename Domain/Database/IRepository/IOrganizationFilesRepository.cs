namespace Domain.Database
{
    public interface IOrganizationFilesRepository
    {
        Task<OrganizationFilesDto> Retrieve(Guid Id);
        Task<IEnumerable<LightOrganizationFiles>> RetrieveLightTaskFile(Guid taskId);
        Task<IEnumerable<LightOrganizationFiles>> RetrieveLightTaskFiles(IEnumerable<Guid> taskIds);
        Task<IEnumerable<LightOrganizationFiles>> RetrieveLightProjectFile(Guid projectId);
        Task<bool> Insert(OrganizationFilesDto file);
        Task<bool> Delete(Guid id);
        Task<bool> DeleteFiles(List<Guid> ids);
        Task<bool> DeleteByTaskId(Guid taskId);
        Task<bool> DeleteAllFileProject(Guid projectId);
        Task<bool> DeleteAllFile(Guid organizationCode);
    }
}
