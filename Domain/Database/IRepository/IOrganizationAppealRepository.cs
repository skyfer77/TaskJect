namespace Domain.Database
{
    public interface IOrganizationAppealRepository
    {
        Task<IEnumerable<OrganizationAppealDto>> Retrieve();
        Task<OrganizationAppealDto> Retrieve(Guid Id);
        Task<OrganizationAppealDto> RetrieveByOrganization(Guid organizationId);
        Task<int> RetrieveCountThisMonth(Guid organizationId);
        Task<bool> Insert(OrganizationAppealDto appeal);
        Task<bool> Update(OrganizationAppealDto appeal);
        Task<bool> Delete(Guid Id);
    }
}
