using Domain.Enums;

namespace Domain.Database
{
    public interface IApplicationUserRepository
    {
        Task<List<ApplicationUserLiteDto>> GetAllUsersTheOrganization(string organizationCode);
        Task<List<ApplicationUserLiteDto>> GetAllAdmins();
        Task<ApplicationUserLiteDto> GetUserById(string id, string organizationCode);
        Task<ApplicationUserLiteDto> GetUserById(string userId);
        Task<ApplicationUserLiteDto> GetUserByTelegramChatId(string chatId);
        Task<ApplicationUserLiteDto> GetTeamLead(string organizationCode);
		Task<Dictionary<string, ApplicationUserLiteDto>> GetUsersByIds(List<string> ids, string organizationCode);
        Task<bool> UpdateUser(ApplicationUserLiteDto user, string organizationCode);
        Task<bool> CreateUser(CreateUserByEmailModel model, string tempPassword);
        Task<List<OrganizationUserInfo>> GetOrganizationUserInfo();
        Task<bool> LockoutAllUser(DateTime? lockoutEnd, string organizationCode);
        Task<bool> UnlockoutAllUser(string organizationCode);
        Task<List<RoleInfoModel>> GetRoles();
        Task<List<string>> GetAllUserId();
        Task<bool> SetRoleUser(string userId, string roleId);
        Task<bool> LockoutUser(DateTime? lockoutEnd, string userId);
        Task<bool> UnlockoutUser(string userId);
        Task<bool> LockoutUsersByIds(List<string> userIds, DateTime? lockoutEnd);
        Task<bool> UnlockoutUsersByIds(List<string> userIds);
        Task<bool> DeleteUser(string userId);
        Task<bool> DeleteUsers(List<string> userIds);
        Task<bool> DeleteAllUsers(string organizationId);
        Task<long> GetUsedStorageUsers(string organizationCode);
        Task<bool> SetRoleInOrganizationForUser(string userId, OrganizationRoles roleId);
        Task<bool> UnconnectTelegramFromUser(string userId);
        Task<Dictionary<string, List<string>>> GetExceededUsersByOrganizations(Dictionary<string, int> maxUsersByOrganization);

    }
}
