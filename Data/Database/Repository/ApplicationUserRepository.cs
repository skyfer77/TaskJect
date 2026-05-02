using AutoMapper;
using Data.DbContexts;
using Domain.Enums;
using Domain.Database;
using Domain.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Data.Database.Repository
{
    public class ApplicationUserRepository : IApplicationUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ITelegramTicketGenerator _ticketGenerator;

        public ApplicationUserRepository( UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, ApplicationDbContext context, IMapper mapper,
            ITelegramTicketGenerator ticketGenerator)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _mapper = mapper;
            _ticketGenerator = ticketGenerator;
        }


        public async Task<ApplicationUserLiteDto> GetUserByTelegramChatId(string chatId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.TelegramChatId == chatId);
            if (user == null)
            {
                return null;
            }
            var liteUser = new ApplicationUserLite
            {
                Id = user.Id,
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                Birthday = user.Birthday,
                PhoneNumber = user.PhoneNumber,
                CardNumber = user.CardNumber,
                RewardLevel = user.RewardLevel,
                LockoutEnd = user.LockoutEnd,
                RegistrationDate = user.RegistrationDate,
                RoleInOrganization = user.RoleInOrganization,
                OrganizationCode = user.OrganizationCode,
            };
            return _mapper.Map<ApplicationUserLiteDto>(liteUser);
        }

        public async Task<ApplicationUserLiteDto> GetTeamLead(string organizationCode)
        {
			var user = await (
                from u in _context.Users
		        join ur in _context.UserRoles on u.Id equals ur.UserId
		        join r in _context.Roles on ur.RoleId equals r.Id
		        where r.Name == SD.TeamLead
			            && u.OrganizationCode == organizationCode
		        select new ApplicationUserLiteDto
		        {
			        Id = u.Id,
			        Name = u.Name,
			        Surname = u.Surname,
			        Email = u.Email,
			        Birthday = u.Birthday,
			        PhoneNumber = u.PhoneNumber,
			        CardNumber = u.CardNumber,
			        RewardLevel = u.RewardLevel,
			        LockoutEnd = u.LockoutEnd,
			        RegistrationDate = u.RegistrationDate,
                    Culture = u.Culture,
			        OrganizationCode = u.OrganizationCode
		        }
	        ).FirstOrDefaultAsync();
			
			return user;
		}

		public async Task<List<OrganizationUserInfo>> GetOrganizationUserInfo()
        {
            var users = await _userManager.Users.ToListAsync();

            var teamLeads = await _userManager.GetUsersInRoleAsync(SD.TeamLead);

            var organizations = users
                .GroupBy(u => u.OrganizationCode)
                .Select(group =>
                {
                    var enumerable = teamLeads
                        .Where(tl => tl.OrganizationCode == group.Key)
                        .Select(tl => new ApplicationUserLiteDto
                        {
                            Id = tl.Id,
                            Name = tl.Name,
                            Surname = tl.Surname,
                            Email = tl.Email,
                            Birthday = tl.Birthday,
                            PhoneNumber = tl.PhoneNumber,
                            CardNumber = tl.CardNumber,
                            RewardLevel = tl.RewardLevel,
                            LockoutEnd = tl.LockoutEnd,
                            RegistrationDate = tl.RegistrationDate,
                            RoleInOrganization = tl.RoleInOrganization,
                        });
                    return new OrganizationUserInfo
                    {
                        OrganizationId = group.Key,
                        CountUserOrganization = group.Count(),
                        TeamLead = enumerable
                        .FirstOrDefault()
                    };
                })
                .ToList();

            return organizations;
        }
        public async Task<List<ApplicationUserLiteDto>> GetAllUsersTheOrganization(string organizationCode)
        {
            var users = await _userManager.Users.Where(u => u.OrganizationCode == organizationCode).ToListAsync();

            var liteUsers = new List<ApplicationUserLite>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault();

                liteUsers.Add(new ApplicationUserLite
                {
                    Id = user.Id,
                    Name = user.Name,
                    Surname = user.Surname,
                    Email = user.Email,
                    Birthday = user.Birthday,
                    PhoneNumber = user.PhoneNumber,
                    CardNumber = user.CardNumber,
                    RewardLevel = user.RewardLevel,
                    LockoutEnd = user.LockoutEnd,
                    RegistrationDate = user.RegistrationDate,
                    RoleInOrganization = user.RoleInOrganization,
                    Role = role
                });
            }

            return _mapper.Map<List<ApplicationUserLiteDto>>(liteUsers);
        }
        public async Task<List<ApplicationUserLiteDto>> GetAllAdmins()
        {
            var targetRoles = new[] { "Moderator", "Admin", "God" };
            var usersWithRoles = new List<ApplicationUserLite>();

            foreach (var roleName in targetRoles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
                usersWithRoles.AddRange(usersInRole.Select(user => new ApplicationUserLite
                {
                    Id = user.Id,
                    Name = user.Name,
                    Surname = user.Surname,
                    Email = user.Email,
                    Birthday = user.Birthday,
                    PhoneNumber = user.PhoneNumber,
                    CardNumber = user.CardNumber,
                    RewardLevel = user.RewardLevel,
                    TelegramUserName = user.TelegramUserName,
                    LockoutEnd = user.LockoutEnd,
                    RegistrationDate = user.RegistrationDate,
                    RoleInOrganization = user.RoleInOrganization,
                    TelegramTicket = user.TelegramTicket,
                    TelegramChatId = user.TelegramChatId,
                    Role = roleName
                }));
            }

            return _mapper.Map<List<ApplicationUserLiteDto>>(usersWithRoles);

        }
        public async Task<List<string>> GetAllUserId()
        {
            return await _userManager.Users.Select(u => u.Id).ToListAsync();
        }
        public async Task<ApplicationUserLiteDto> GetUserById(string id, string organizationCode)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id && u.OrganizationCode == organizationCode);
            if (user == null)
            {
                return null;
            }
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();

            var liteUser = new ApplicationUserLite
            {
                Id = user.Id,
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                Birthday = user.Birthday,
                PhoneNumber = user.PhoneNumber,
                CardNumber = user.CardNumber,
                RewardLevel = user.RewardLevel,
                TelegramUserName = user.TelegramUserName,
                LockoutEnd = user.LockoutEnd,
                RegistrationDate = user.RegistrationDate,
                RoleInOrganization = user.RoleInOrganization,
                TelegramTicket = user.TelegramTicket,
                TelegramChatId = user.TelegramChatId,
                Role = role
            };

            return _mapper.Map<ApplicationUserLiteDto>(liteUser);
        }
        public async Task<ApplicationUserLiteDto> GetUserById(string userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return null;
            }
            var liteUser = new ApplicationUserLite
            {
                Id = user.Id,
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                Birthday = user.Birthday,
                PhoneNumber = user.PhoneNumber,
                CardNumber = user.CardNumber,
                RewardLevel = user.RewardLevel,
                TelegramUserName = user.TelegramUserName,
                LockoutEnd = user.LockoutEnd,
                RegistrationDate = user.RegistrationDate,
                RoleInOrganization = user.RoleInOrganization,
                TelegramTicket = user.TelegramTicket,
                TelegramChatId = user.TelegramChatId,
                Culture = user.Culture
            };
            return _mapper.Map<ApplicationUserLiteDto>(liteUser);
        }
        public async Task<Dictionary<string, ApplicationUserLiteDto>> GetUsersByIds(List<string> ids, string organizationCode)
        {
            var users = await _userManager.Users.Where(u => ids.Contains(u.Id) && u.OrganizationCode == organizationCode).ToListAsync();

            var liteUsers = new Dictionary<string, ApplicationUserLiteDto>();
            foreach (var u in users)
            {
                liteUsers.Add(u.Id, new ApplicationUserLiteDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Surname = u.Surname,
                    Email = u.Email,
                    Birthday = u.Birthday,
                    PhoneNumber = u.PhoneNumber,
                    CardNumber = u.CardNumber,
                    RewardLevel = u.RewardLevel,
                    TelegramUserName = u.TelegramUserName,
                    LockoutEnd = u.LockoutEnd,
                    RegistrationDate = u.RegistrationDate,
                    RoleInOrganization = u.RoleInOrganization,
                });
            }


            return liteUsers;
        }

        public async Task<bool> UpdateUser(ApplicationUserLiteDto user, string organizationCode)
        {
            var u = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == user.Id && u.OrganizationCode == organizationCode);

            u.Name = user.Name;
            u.Surname = user.Surname;
            u.Email = user.Email;
            u.Birthday = user.Birthday;
            u.PhoneNumber = user.PhoneNumber;
            //u.CardNumber = u.CardNumber;
            u.RewardLevel = user.RewardLevel;
            u.TelegramUserName = user.TelegramUserName;
            u.RegistrationDate = user.RegistrationDate;
            u.RoleInOrganization = user.RoleInOrganization;
            var result = await _userManager.UpdateAsync(u);

            return result.Succeeded;
        }

        public async Task<bool> LockoutAllUser(DateTime? lockoutEnd, string organizationCode)
        {
            var users = await _userManager.Users.Where(u => u.OrganizationCode == organizationCode).ToListAsync();

            if (!users.Any())
            {
                return false;
            }

            foreach (var user in users)
            {
                user.LockoutEnd = lockoutEnd ?? DateTime.MaxValue;

                await _userManager.UpdateAsync(user);
            }

            return true;
        }

        public async Task<bool> UnlockoutAllUser(string organizationCode)
        {
            var users = await _userManager.Users.Where(u => u.OrganizationCode == organizationCode).ToListAsync();

            if (!users.Any())
            {
                return false;
            }

            foreach (var user in users)
            {
                user.LockoutEnd = null;
                await _userManager.UpdateAsync(user);
            }

            return true;
        }

        public async Task<List<RoleInfoModel>> GetRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();

            var rolesList = new List<RoleInfoModel>();

            foreach (var role in roles)
            {
                rolesList.Add(new RoleInfoModel()
                {
                    Id = role.Id,
                    Name = role.Name,
                });
            }

            return rolesList;
        }

		public async Task<bool> SetRoleUser(string userId, string roleId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
            if (user != null && role != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                var result = await _userManager.AddToRoleAsync(user, role.Name);

                if (result.Succeeded)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> SetRoleInOrganizationForUser(string userId, OrganizationRoles role)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                user.RoleInOrganization = role;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return true;
                }
            }

            return false;
        }
        public async Task<bool> UnconnectTelegramFromUser(string userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                user.TelegramChatId = null;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return true;
                }
            }

            return false;
        }
        public async Task<bool> LockoutUser(DateTime? lockoutEnd, string userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                user.LockoutEnd = lockoutEnd ?? DateTime.MaxValue;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return true;
                }
            }

            return false;
        }
        public async Task<bool> UnlockoutUser(string userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                user.LockoutEnd = null;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return true;
                }
            }

            return false;
        }
        public async Task<bool> LockoutUsersByIds(List<string> userIds , DateTime? lockoutEnd)
        {
            if (userIds == null || !userIds.Any())
            {
                return false;
            }

            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            if (!users.Any())
            {
                return false;
            }

            foreach (var user in users)
            {
                user.LockoutEnd = lockoutEnd ?? DateTime.MaxValue;
                await _userManager.UpdateAsync(user);
            }

            return true;
        }
        public async Task<bool> UnlockoutUsersByIds(List<string> userIds)
        {
            if (userIds == null || !userIds.Any())
            {
                return false;
            }

            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            if (!users.Any())
            {
                return false;
            }

            foreach (var user in users)
            {
                user.LockoutEnd = null;
                await _userManager.UpdateAsync(user);
            }

            return true;
        }
        public async Task<bool> DeleteUser(string userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> DeleteUsers(List<string> userIds)
        {
            if (userIds == null || userIds.Count == 0)
            {
                return false;
            }
            _context.ChangeTracker.Clear();
            try
            {
                var users = await _userManager.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();

                if (!users.Any())
                {
                    return false;
                }        

                foreach (var user in users)
                {
                    var result = await _userManager.DeleteAsync(user);
                    if (!result.Succeeded)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAllUsers(string organizationId)
        {
            var users = await _userManager.Users.Where(u => u.OrganizationCode == organizationId).ToListAsync();

            if (!users.Any())
            {
                return true;
            }

            foreach (var user in users)
            {
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<long> GetUsedStorageUsers(string organizationCode)
        {
            var totalSizeBytes = await _context.Users
                 .Where(u => u.OrganizationCode == organizationCode)
                 .SumAsync(u => (long)(
                     (
                         u.Id.Length +
                         (u.UserName != null ? u.UserName.Length : 0) +
                         (u.NormalizedUserName != null ? u.NormalizedUserName.Length : 0) +
                         (u.Email != null ? u.Email.Length : 0) +
                         (u.NormalizedEmail != null ? u.NormalizedEmail.Length : 0) +
                         (u.PasswordHash != null ? u.PasswordHash.Length : 0) +
                         (u.SecurityStamp != null ? u.SecurityStamp.Length : 0) +
                         (u.ConcurrencyStamp != null ? u.ConcurrencyStamp.Length : 0) +
                         (u.PhoneNumber != null ? u.PhoneNumber.Length : 0) +
                         (u.Name != null ? u.Name.Length : 0) +
                         (u.Surname != null ? u.Surname.Length : 0) +
                         (u.CardNumber != null ? u.CardNumber.Length : 0) +
                         (u.OrganizationCode != null ? u.OrganizationCode.Length : 0) +
                         (u.TelegramTicket != null ? u.TelegramTicket.Length : 0) +
                         (u.TelegramUserName != null ? u.TelegramUserName.Length : 0) +
                         (u.TelegramChatId != null ? u.TelegramChatId.Length : 0) +
                         (u.Culture != null ? u.Culture.Length : 0)
                     ) * sizeof(char)
                     )
                     + (u.LockoutEnd.HasValue ? 8 : 0)
                     + (u.TwoFactorEnabled ? 1 : 0)
                     + (u.EmailConfirmed ? 1 : 0)
                     + (u.PhoneNumberConfirmed ? 1 : 0)
                     + (u.LockoutEnabled ? 1 : 0)
                     + (u.AccessFailedCount > 0 ? sizeof(int) : 0)
                     + (u.RewardLevel.HasValue ? sizeof(int) : 0)
                     + sizeof(int)
                     + (u.Birthday.HasValue ? 8 : 0)
                     + (u.RegistrationDate.HasValue ? 8 : 0)
                     + (u.IsNewUser ? 1 : 0)
                 );

            return totalSizeBytes;
        }
        public async Task<bool> CreateUser(CreateUserByEmailModel model, string tempPassword)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.FirstName,
                Surname = model.Surname,
                EmailConfirmed = true,
                OrganizationCode = model.OrganizationCode,
                RegistrationDate = DateTime.Now,
                TelegramTicket = _ticketGenerator.GenerateTicket(),
                IsNewUser = true
            };
            var result = await _userManager.CreateAsync(user, tempPassword);
            if (!result.Succeeded)
            {
                return false;
            }
            await _userManager.AddToRoleAsync(user, model.RoleUser);
            return true;
        }

        public async Task<Dictionary<string, List<string>>> GetExceededUsersByOrganizations(Dictionary<string, int> maxUsersByOrganization)
        {
            var orgCodes = maxUsersByOrganization.Keys;

            var allUsers = await _userManager.Users
                .Where(u => orgCodes.Contains(u.OrganizationCode))
                .OrderBy(u => u.OrganizationCode)
                .ThenBy(u => u.RegistrationDate)
                .Select(u => new
                {
                    u.OrganizationCode,
                    u.Id
                }).ToListAsync();

            var result = allUsers
                .GroupBy(u => u.OrganizationCode)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        maxUsersByOrganization.TryGetValue(g.Key, out var maxUsers);
                        return g.Skip(maxUsers).Select(u => u.Id).ToList();
                    });

            return result;
        }



    }

}
