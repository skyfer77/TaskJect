using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Domain.Database;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Data.DbContexts;

namespace Data.Database.Repository
{
    public class TeamRepository : ITeamRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private IMapper _mapper;
        public TeamRepository(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        [Authorize(Roles = "Moderator, Admin, God, TeamLead")]
        public async Task<bool> Delete(Guid teamId)
        {
            try
            {
                var team = await _dbContext.Teams.FirstOrDefaultAsync(x => x.Id == teamId);
                if (team == null)
                {
                    return false;
                }
                _dbContext.Teams.Remove(team);
                await _dbContext.SaveChangesAsync();
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [Authorize(Roles = "Moderator, Admin, God")]
        public async Task<bool> DeleteTeams(string organizationCode)
        {
            try
            {
                var teams = await _dbContext.Teams.Where(x => x.OrganizationCode == organizationCode).ToListAsync();

                if (teams.Any())
                {
                    _dbContext.Teams.RemoveRange(teams);
                    await _dbContext.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<TeamDto> GetTeamById(Guid teamId)
        {
            var team = await _dbContext.Teams.Where(x => x.Id == teamId).FirstOrDefaultAsync();
            return _mapper.Map<TeamDto>(team);
        }


        public async Task<Dictionary<Guid, TeamDto>> GetTeamByIds(IEnumerable<Guid> teamIds)
        {
            if (!teamIds.Any())
            {
                return new Dictionary<Guid, TeamDto>();
            }
            var teams = await _dbContext.Teams.Where(x => teamIds.Contains(x.Id)).ToListAsync();
            var teamsDto = _mapper.Map<List<TeamDto>>(teams);
            return teamsDto.ToDictionary(x => x.Id, x => x);
        }

        [Authorize(Roles = "Admin, God")]
        public async Task<IEnumerable<TeamDto>> GetAllTeams()
        {
            var teams = await _dbContext.Teams.ToListAsync();
            return _mapper.Map<List<TeamDto>>(teams);
        }

        public async Task<IEnumerable<TeamDto>> GetTeamsByOrganization(string organizationCode)
        {
            var teams = await _dbContext.Teams.Where(t => t.OrganizationCode == organizationCode).ToListAsync();
            return _mapper.Map<List<TeamDto>>(teams);
        }

        public async Task<TeamDto> Modify(TeamDto teamDto)
        {
            var team = _mapper.Map<TeamDto, Team>(teamDto);
            if (team.Id != Guid.Empty)
            {
                var existingTeam = await _dbContext.Teams.FindAsync(teamDto.Id);
                if (existingTeam == null)
                {
                    return null;
                }

                _mapper.Map(teamDto, existingTeam);

                await _dbContext.SaveChangesAsync();

                return _mapper.Map<Team, TeamDto>(existingTeam);

            }
            else
            {
                _dbContext.Teams.Add(team);
            }
            await _dbContext.SaveChangesAsync();
            return _mapper.Map<Team, TeamDto>(team);
        }
    }
}
