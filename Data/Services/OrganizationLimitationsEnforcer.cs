using Data.DbContexts;
using Domain.Database;
using Domain.Enums;
using Domain.IServices;

namespace Data.Services
{
    internal class OrganizationLimitationsEnforcer : IOrganizationLimitationsEnforcer
    {
        private readonly ITariffPlanHistoryRepository _planHistoryRepository;
        private readonly ITariffPlanRepository _planRepository;
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IDataSizeCalculator _dataSizeCalculator;
        private readonly ITaskRepository _taskRepository;
        private readonly IOrganizationFilesRepository _organizationFilesRepository;
        private readonly IMembershipRepository _membershipRepository;
        public OrganizationLimitationsEnforcer(
            ITariffPlanHistoryRepository planHistoryRepository,
            ITariffPlanRepository planRepository,
            IApplicationUserRepository applicationUserRepository,
            IOrganizationRepository organizationRepository,
            IDataSizeCalculator dataSizeCalculator,
            ITaskRepository taskRepository,
            IOrganizationFilesRepository organizationFilesRepository,
            IMembershipRepository membershipRepository)
        {
            _planHistoryRepository = planHistoryRepository;
            _planRepository = planRepository;
            _applicationUserRepository = applicationUserRepository;
            _organizationRepository = organizationRepository;
            _dataSizeCalculator = dataSizeCalculator;
            _taskRepository = taskRepository;
            _organizationFilesRepository = organizationFilesRepository;
            _membershipRepository = membershipRepository;
        }
        public async Task<bool> UnlockUsers(string organizationId, string planCode)
        {
            var tariff = await _planRepository.Retrieve(planCode);
            if (tariff == null)
            {
                return false; 
            } 
            var members = (await _applicationUserRepository.GetAllUsersTheOrganization(organizationId)).OrderBy(u => u.RegistrationDate).ToList();
            var lockedMembersIds = (members.Take(tariff.MaxUsers)).Where(m => m.LockoutEnd != null).Select(m => m.Id).ToList();
            if (lockedMembersIds.Any())
            {
                await _applicationUserRepository.UnlockoutUsersByIds(lockedMembersIds);
            }
            return true;
        }
        public async Task<bool> CleanupExceededLimits(List<Guid> organizationsIds)
        {
            var organizationParameters = await getOrganizationParameters(organizationsIds);
            var usersToRemove = new List<string>();
            var taskIdsToDelete = new List<Guid>();
            var fileIdsToDelete = new List<Guid>();
            foreach (var organizationParam in organizationParameters)
            {
                if(organizationParam == null || (organizationParam.Plan.MaxUsers >= organizationParam.Users.Count() && organizationParam.Plan.MaxStorageBytes >= organizationParam.UsedStorageSpace))
                {
                    continue;
                }

                var needToRemove = organizationParam.Users.Count() - organizationParam.Plan.MaxUsers;
                if (needToRemove > 0)
                {
                    var usersWithoutTeamLead = organizationParam.Users.Where(u => u.RoleInOrganization != OrganizationRoles.TeamLead).ToList();
                    usersToRemove.AddRange(usersWithoutTeamLead.Take(needToRemove).Select(u => u.Id));
                }

                var organizationUsedStorage = await _dataSizeCalculator.CalculateOrganizationDataSize(organizationParam.OrganizationId.ToString());
                if (organizationUsedStorage > organizationParam.Plan.MaxStorageBytes)
                {
                    var neededToDeleteInBytes = organizationUsedStorage - organizationParam.Plan.MaxStorageBytes;

                    var organizationTasksWithFiles = await _dataSizeCalculator.GetTasksWithFiles(organizationParam.OrganizationId.ToString());
                    if (organizationTasksWithFiles == null || !organizationTasksWithFiles.Any())
                    {
                        continue;
                    }

                    long remainingBytesToDelete = neededToDeleteInBytes;

                    foreach (var taskFiles in organizationTasksWithFiles)
                    {
                        if (remainingBytesToDelete <= 0)
                        {
                            break;
                        }

                        taskIdsToDelete.Add(taskFiles.TaskId);
                        fileIdsToDelete.AddRange(taskFiles.FilesIds);

                        remainingBytesToDelete -= taskFiles.UsedStorageSpace;
                    }

                    if (remainingBytesToDelete > 0)
                    {
                        var organizationProjectFiles = await _dataSizeCalculator.GetProjectFiles(organizationParam.OrganizationId.ToString());

                        foreach (var file in organizationProjectFiles)
                        {
                            if (remainingBytesToDelete <= 0)
                            {
                                break;
                            } 

                            fileIdsToDelete.Add(file.Key);
                            remainingBytesToDelete -= file.Value;
                        }
                    }
                }
            }

            taskIdsToDelete = taskIdsToDelete.Distinct().ToList();
            fileIdsToDelete = fileIdsToDelete.Distinct().ToList();
            usersToRemove = usersToRemove.Distinct().ToList();

            if (fileIdsToDelete.Any())
            {
                await _organizationFilesRepository.DeleteFiles(fileIdsToDelete);
            }

            if (taskIdsToDelete.Any())
            {
                await _taskRepository.DeleteByIds(taskIdsToDelete);
            }

            if (usersToRemove.Any())
            {
                    await _taskRepository.DeleteAssigneeFromAllTasksByUsers(usersToRemove);
                    await _membershipRepository.DeleteFromTeamsByIds(usersToRemove);
                    await _applicationUserRepository.DeleteUsers(usersToRemove);        
            }
            return true;
        }
        public async Task<bool> ApplyTariffPlan(TariffPlanHistoryDto tariffHistory , bool isRefund)
        {
            var tariffPlans = await _planRepository.Retrieve();
            var lastTariffPlanHistory = await _planHistoryRepository.RetrieveLatest(tariffHistory.OrganizationCode);
            var activeTariffPlanHistory = await _planHistoryRepository.RetrieveActive(tariffHistory.OrganizationCode);
            if (lastTariffPlanHistory != null && activeTariffPlanHistory != null)
            {
                tariffPlans.TryGetValue(tariffHistory.TariffPlanCode , out var newPlan);
                if (newPlan == null)
                {
                    return false;
                }  

                if (lastTariffPlanHistory.DateFrom == activeTariffPlanHistory.DateFrom)
                {
                    tariffPlans.TryGetValue(lastTariffPlanHistory.TariffPlanCode, out var lastPlan);

                    if (lastPlan == null)
                    {
                        return false;
                    }

                    if (newPlan.MaxUsers >= lastPlan.MaxUsers || isRefund)
                    {
                        lastTariffPlanHistory.DateTo = tariffHistory.DateFrom.AddSeconds(-1);
                        await _planHistoryRepository.Update(lastTariffPlanHistory);
                    }
                    else
                    {
                        tariffHistory.DateFrom = lastTariffPlanHistory.DateTo.AddSeconds(1);
                        if (tariffHistory.TariffPlanCode != SD.BasicPlanCode)
                        {
                            tariffHistory.DateTo = tariffHistory.DateFrom.AddMonths(1);
                        }
                        else
                        {
                            tariffHistory.DateTo = DateTime.MaxValue;
                        }
                    }
                }
                else
                {
                    tariffPlans.TryGetValue(activeTariffPlanHistory.TariffPlanCode, out var activePlan);

                    if (activePlan == null)
                    {
                        return false;
                    }
                    
                    await _planHistoryRepository.Delete(lastTariffPlanHistory.OrganizationCode, lastTariffPlanHistory.TariffPlanCode, lastTariffPlanHistory.DateFrom);

                    if (newPlan.MaxUsers >= activePlan.MaxUsers)
                    {
                        activeTariffPlanHistory.DateTo = tariffHistory.DateFrom.AddSeconds(-1);
                        await _planHistoryRepository.Update(activeTariffPlanHistory);
                    }
                    else
                    {
                        tariffHistory.DateFrom = activeTariffPlanHistory.DateTo.AddSeconds(1);
                        if (tariffHistory.TariffPlanCode != SD.BasicPlanCode)
                        {
                            tariffHistory.DateTo = tariffHistory.DateFrom.AddMonths(1);
                        }
                        else
                        {
                            tariffHistory.DateTo = DateTime.MaxValue;
                        }
                    }
                }
            }
            return await _planHistoryRepository.Insert(tariffHistory);
        }
            private async Task<List<OrganizationParameters>> getOrganizationParameters(List<Guid> organizationsIds)
        {
            var organizations = await _organizationRepository.GetOrganizationsByIds(organizationsIds);
            var organizationsTariffs = await _planHistoryRepository.RetrieveActiveByIds(organizationsIds);
            var tariffs = await _planRepository.Retrieve();
            var usersByOrganizations = new Dictionary<Guid, List<ApplicationUserLiteDto>>();
            foreach (var org in organizations)
            {
                var users = await _applicationUserRepository.GetAllUsersTheOrganization(org.OrganizationId.ToString());

                var sortedUsers = users.OrderByDescending(u => u.RegistrationDate).ToList();

                usersByOrganizations.Add(org.OrganizationId, sortedUsers);
            }

            var organizationsParameters = new List<OrganizationParameters>();
            foreach (var org in organizations)
            {
                organizationsTariffs.TryGetValue(org.OrganizationId, out var tariffHistory);

                var plan = new TariffPlanDto();

                if (tariffHistory != null)
                {
                    if (tariffs.TryGetValue(tariffHistory.TariffPlanCode, out var tariffDto))
                    {
                        plan = tariffDto;
                    }
                }

                organizationsParameters.Add(new OrganizationParameters
                {
                    OrganizationId = org.OrganizationId,
                    UsedStorageSpace = org.UsedStorageSpace,
                    Plan = plan,
                    Users = usersByOrganizations.ContainsKey(org.OrganizationId)
                        ? usersByOrganizations[org.OrganizationId]
                        : new List<ApplicationUserLiteDto>()
                });
            }

            return organizationsParameters;
        }
    }
}
