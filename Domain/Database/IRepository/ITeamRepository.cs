
namespace Domain.Database
{
    public interface ITeamRepository
    {
        Task<IEnumerable<TeamDto>> GetAllTeams();
        Task<IEnumerable<TeamDto>> GetTeamsByOrganization(string organizationCode);
        Task<TeamDto> GetTeamById(Guid teamId);
        Task<Dictionary<Guid, TeamDto>> GetTeamByIds(IEnumerable<Guid> teamIds);
        Task<TeamDto> Modify(TeamDto teamDto);
        Task<bool> Delete(Guid teamId);
        Task<bool> DeleteTeams(string organizationCode);
    }
}
