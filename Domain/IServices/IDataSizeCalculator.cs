using Domain.Database;
namespace Domain.IServices
{
    public interface IDataSizeCalculator
    {
        Task<long> CalculateOrganizationDataSize(string organizationCode);
        Task<List<TaskWithFiles>> GetTasksWithFiles(string organizationCode);
        Task<Dictionary<Guid, long>> GetProjectFiles(string organizationCode);
    }
}
