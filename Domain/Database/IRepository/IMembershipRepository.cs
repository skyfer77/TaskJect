namespace Domain.Database
{
    public interface IMembershipRepository
    {
        Task<IEnumerable<MembershipDto>> GetMemberships();
        Task<List<MembershipDto>> GetMembershipByTeam(Guid teamId);
        Task<List<MembershipDto>> GetMembershipsByUser(string userId);
        Task<MembershipDto> GetMembership(Guid membershipId);
        Task<bool> IsUserOnTeam(Guid teamId, string userId);
        Task<bool> Add(MembershipDto teamDto);
        Task<bool> Add(TeamWithTeamMembersSelectDto model);
        Task<bool> Delete(Guid id);
        Task<bool> Delete(MembershipDto teamDto);
        Task<bool> Delete(TeamWithTeamMembersSelectDto model);
        Task<bool> DeleteByUserAndTeam(string userId, Guid teamId);
        Task<bool> DeleteFromAllTeam(string userId);
        Task<bool> DeleteFromTeamsByIds(List<string> userIds);
        Task<bool> DeleteMembersByTeamId(Guid teamId);
        Task<bool> DeleteMembersByTeamIds(List<Guid> teamIds);

    }
}
