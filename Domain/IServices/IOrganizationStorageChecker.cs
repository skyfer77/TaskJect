namespace Domain.IServices
{
    public interface IOrganizationStorageChecker
    {
        Task<bool> CheckAsync(Guid organizationCode);
        void ClearCache();
    }
}
