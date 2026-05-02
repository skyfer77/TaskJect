using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Domain.Database;
using Microsoft.Data.SqlClient;
using Data.DbContexts;

namespace Data.Database.Repository
{
    public class MembershipRepository : IMembershipRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private IMapper _mapper;

        public MembershipRepository(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<bool> Delete(Guid id)
        {
            try
            {
                var rows = await _dbContext.Database.ExecuteSqlRawAsync(@"
                    DELETE FROM [Membership]
                    WHERE [MembershipId] = @p0",
                    id);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> Delete(MembershipDto teamDto)
        {
            var memberShip = _mapper.Map<MembershipDto, Membership>(teamDto);
            _dbContext.Memberships.Remove(memberShip);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Delete(TeamWithTeamMembersSelectDto model)
        {
            try
            {
                if (model.SelectedUsersId != null && model.SelectedUsersId.Length > 0)
                {
                    //var memberships = await _dbContext.Memberships
                    //    .Where(x => x.TeamId == model.TeamId && model.SelectedUsersId.Contains(x.UserId))
                    //    .ToListAsync();

                    //if (memberships.Count > 0)
                    //{
                    //    _dbContext.Memberships.RemoveRange(memberships);
                    //    await _dbContext.SaveChangesAsync();
                    //    return true;
                    //}
                    var userParams = model.SelectedUsersId
                        .Select((userId, index) => new SqlParameter($"@userId{index}", userId))
                        .ToArray();

                    var inClause = string.Join(", ", userParams.Select(p => p.ParameterName));
                    var sql = $"DELETE FROM Membership WHERE TeamId = @teamId AND UserId IN ({inClause})";

                    var teamIdParam = new SqlParameter("@teamId", model.TeamId);
                    var parameters = new List<SqlParameter> { teamIdParam };
                    parameters.AddRange(userParams);

                    var affectedRows = await _dbContext.Database.ExecuteSqlRawAsync(sql, parameters.ToArray());

                    return affectedRows > 0;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteMembersByTeamId(Guid teamId)
        {
            return await DeleteMembersByTeamIds(new List<Guid> { teamId });
        }

        public async Task<bool> DeleteMembersByTeamIds(List<Guid> teamIds)
        {
            //try
            //{
            //    var memberships = await _dbContext.Memberships.Where(m => teamIds.Contains(m.TeamId)).ToListAsync();

            //    if (memberships.Any())
            //    {
            //        _dbContext.Memberships.RemoveRange(memberships);
            //        await _dbContext.SaveChangesAsync();
            //    }

            //    return true;
            //}
            //catch (Exception ex)
            //{
            //    return false;
            //}
            try
            {
                if (teamIds == null || teamIds.Count == 0)
                    return true;

                var parameters = teamIds.Select((id, i) => new SqlParameter($"@p{i}", id)).ToArray();
                var inClause = string.Join(", ", parameters.Select(p => p.ParameterName));

                var sql = $"DELETE FROM Membership WHERE TeamId IN ({inClause})";

                await _dbContext.Database.ExecuteSqlRawAsync(sql, parameters);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IEnumerable<MembershipDto>> GetMemberships()
        {
            var memberships = await _dbContext.Memberships.ToListAsync();
            return _mapper.Map<List<MembershipDto>>(memberships);
        }

        public async Task<MembershipDto> GetMembership(Guid membershipId)
        {
            var membership = await _dbContext.Memberships.FirstOrDefaultAsync(x => x.MembershipId == membershipId);
            return _mapper.Map<MembershipDto>(membership);
        }

        public async Task<bool> IsUserOnTeam(Guid teamId, string userId)
        {
            return await _dbContext.Memberships.AnyAsync(m => m.TeamId == teamId && m.UserId == userId);
        }

        public async Task<List<MembershipDto>> GetMembershipsByUser(string userId)
        {
            var membership = _dbContext.Memberships.Where(x => x.UserId == userId).ToList();
            return _mapper.Map<List<Membership>, List<MembershipDto>>(membership);
        }

        public async Task<List<MembershipDto>> GetMembershipByTeam(Guid teamId)
        {
            var membership = await _dbContext.Memberships.Where(x => x.TeamId == teamId).ToListAsync();
            return _mapper.Map<List<Membership>, List<MembershipDto>>(membership);
        }

        public async Task<bool> Add(MembershipDto membershipDto)
        {
            var membership = _mapper.Map<Membership>(membershipDto);
            if (membership.TeamId != null && membership.UserId != null)
            {
                _dbContext.Memberships.Add(membership);

                await _dbContext.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> Add(TeamWithTeamMembersSelectDto model)
        {
            if (model.TeamId != null)
            {
                var team = await _dbContext.Teams.FirstOrDefaultAsync(x => x.Id == model.TeamId);
                if (team.Id != null)
                {
                    foreach (var userId in model.SelectedUsersId)
                    {
                        var membership = new Membership
                        {
                            MembershipId = Guid.NewGuid(),
                            TeamId = team.Id,
                            UserId = userId
                        };
                        await _dbContext.Memberships.AddAsync(membership);
                    }
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> DeleteByUserAndTeam(string userId, Guid teamId)
        {
            try
            {
                //if (userId != null && teamId != null)
                //{
                //	var membership = await _dbContext.Memberships.Where(x => x.UserId == userId && x.TeamId == teamId).FirstOrDefaultAsync();
                //	if (membership != null)
                //	{
                //		_dbContext.Memberships.Remove(membership);
                //		await _dbContext.SaveChangesAsync();
                //		return true;
                //	}
                //}
                if (!string.IsNullOrEmpty(userId))
                {
                    var sql = "DELETE FROM Membership WHERE UserId = @userId AND TeamId = @teamId";

                    var parameters = new[]
                    {
                        new SqlParameter("@userId", userId),
                        new SqlParameter("@teamId", teamId)
                    };

                    var rowsAffected = await _dbContext.Database.ExecuteSqlRawAsync(sql, parameters);

                    return rowsAffected > 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<bool> DeleteFromAllTeam(string userId)
        {
            try
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    var rows = await _dbContext.Database.ExecuteSqlRawAsync(@"
                        DELETE FROM [Membership]
                        WHERE [UserId] = @p0",
                        userId);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public async Task<bool> DeleteFromTeamsByIds(List<string> userIds)
        {
            if (userIds == null || userIds.Count == 0)
            {
                return false;
            }

            try
            {
                await _dbContext.Memberships.Where(m => userIds.Contains(m.UserId)).ExecuteDeleteAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
