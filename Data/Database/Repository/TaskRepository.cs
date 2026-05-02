using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Microsoft.Data.SqlClient;
using Data.DbContexts;
using Domain.Database;

namespace Data.Database.Repository
{
    internal class TaskRepository : ITaskRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private IMapper _mapper;

        public TaskRepository(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }
        public async Task<bool> Delete(Guid id)
        {
            var task = await _dbContext.Tasks.FirstOrDefaultAsync(x => x.Id == id);
            if (task == null)
            {
                return false;
            }
            try
            {
                _dbContext.Tasks.Remove(task);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> DeleteByIds(List<Guid> ids)
        {
            if (ids == null || !ids.Any())
            {
                return true;
            }

            try
            {
                var tasks = await _dbContext.Tasks
                    .Where(t => ids.Contains(t.Id))
                    .ToListAsync();

                if (!tasks.Any())
                {
                    return true;
                }

                _dbContext.Tasks.RemoveRange(tasks);
                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        [Authorize(Roles = "Moderator, God, Admin")]
        public async Task<bool> DeteleTasks(string organizationId)
        {
            var tasks = await _dbContext.Tasks.Where(t => t.OrganizationCode == organizationId).ToListAsync();
            if (!tasks.Any())
            {
                return true;
            }
            try
            {
                _dbContext.Tasks.RemoveRange(tasks);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        //TODO rework
        public async Task<bool> DeleteAssigneeFromAllTasks(string userId)
        {
            var tasks = await _dbContext.Tasks.Where(t => t.AssigneeId == userId).ToListAsync();
            try
            {
                foreach (var task in tasks)
                {
                    task.AssigneeId = null;
                }
                _dbContext.Tasks.UpdateRange(tasks);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAssigneeFromAllTasksByUsers(List<string> userIds)
        {
            if (userIds == null || userIds.Count == 0)
            {
                return false;
            }

            try
            {
                await _dbContext.Tasks.Where(t => userIds.Contains(t.AssigneeId)).ExecuteUpdateAsync(t => t.SetProperty(x => x.AssigneeId, (string?)null));

                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<TaskDetails> GetDetailsTasks(TaskDetailsRequest request)
        {

            var taskStatuses = Enum.GetValues(typeof(Domain.Enums.TaskStatus)).Cast<Domain.Enums.TaskStatus>().Distinct().Count();
            var result = new TaskDetails()
            {
                UserId = request.UserId,
                TasksCount = Enumerable.Repeat(0, taskStatuses).ToList(),
                TasksPoint = Enumerable.Repeat(0, taskStatuses).ToList()
            };

            if (!request.StartDate.HasValue)
            {
                request.StartDate = DateTime.MinValue;
            }
            if (!request.EndDate.HasValue)
            {
                request.EndDate = DateTime.Now;
            }

            var allTasks = await _dbContext.Tasks.Where(x => x.AssigneeId == request.UserId && x.OrganizationCode == request.OrganizationCode).ToListAsync();
            var tasksByPeriod = allTasks.Where(x =>
                (x.ComplitedDate.HasValue ? x.ComplitedDate :
                    x.EndDate.HasValue ? x.EndDate : x.StartDate) >= request.StartDate
                && (x.ComplitedDate.HasValue ? x.ComplitedDate :
                    x.EndDate.HasValue ? x.EndDate : x.StartDate) <= request.EndDate).ToList();

            //var tasksWithoutDeadline = allTasks.Where(x => x.EndDate == null && x.ComplitedDate == null).ToList();

            //tasksByPeriod.AddRange(tasksWithoutDeadline);

            if (tasksByPeriod != null && tasksByPeriod.Any())
            {
                foreach (var task in tasksByPeriod)
                {
                    switch (task.Status)
                    {
                        case Domain.Enums.TaskStatus.NotStarted:
                            result.TasksCount[0]++;
                            result.TasksPoint[0] += task.Complexity;
                            break;
                        case Domain.Enums.TaskStatus.InProgress:
                            result.TasksCount[1]++;
                            result.TasksPoint[1] += task.Complexity;
                            break;
                        case Domain.Enums.TaskStatus.OnReview:
                            result.TasksCount[2]++;
                            result.TasksPoint[2] += task.Complexity;
                            break;
                        case Domain.Enums.TaskStatus.Done:
                            result.TasksCount[3]++;
                            result.TasksPoint[3] += task.Complexity;
                            break;
                        case Domain.Enums.TaskStatus.Archived:
                            result.TasksCount[4]++;
                            result.TasksPoint[4] += task.Complexity;
                            break;
                        case Domain.Enums.TaskStatus.OnHold:
                            result.TasksCount[5]++;
                            result.TasksPoint[5] += task.Complexity;
                            break;
                    }
                }
            }

            return result;
        }
        //TODO rework
        public async Task<Dictionary<string, int>> GetOverdueTasksDetails(TaskStatisticsRequest statisticsRequest)
        {
            var result = statisticsRequest.UserIds.ToDictionary(x => x, x => 0);

            statisticsRequest.StartDate ??= DateTime.MinValue;
            statisticsRequest.EndDate ??= DateTime.Now;

            var allTasks = await _dbContext.Tasks
                .Where(x => statisticsRequest.UserIds.Contains(x.AssigneeId) && x.OrganizationCode == statisticsRequest.OrganizationCode)
                .Where(x => x.Status == Domain.Enums.TaskStatus.Done || x.Status == Domain.Enums.TaskStatus.Archived)
                .ToListAsync();

            var tasks = allTasks
                .Where(x =>
                    (x.ComplitedDate.HasValue ? x.ComplitedDate.Value :
                        x.EndDate.HasValue ? x.EndDate.Value : x.StartDate) >= statisticsRequest.StartDate
                    &&
                    (x.ComplitedDate.HasValue ? x.ComplitedDate.Value :
                        x.EndDate.HasValue ? x.EndDate.Value : x.StartDate) <= statisticsRequest.EndDate)
                .ToList();

            foreach (var task in tasks)
            {
                if (result.TryGetValue(task.AssigneeId, out var assigneeOverdueValue))
                {
                    if (task.ComplitedDate.HasValue && task.EndDate.HasValue && !task.IsAgreedOverdue.Value)
                    {
                        if (task.EndDate.Value.Date < task.ComplitedDate.Value.Date)
                        {
                            result[task.AssigneeId] += 1;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Set logic (bool) value to IsAgreedOverdue field in task by id
        /// </summary>
        /// <param name="id">TaskID</param>
        /// <param name="value">Logic value for IsAgreedOverdue in Task</param>
        /// <returns></returns>

        public async Task<bool> UpdateAgreedTaskOverdue(Guid id, bool value)
        {
            var task = await _dbContext.Tasks.FirstOrDefaultAsync(x => x.Id == id);
            if (task != null)
            {
                task.IsAgreedOverdue = value;
                _dbContext.Tasks.Update(task);
                return await _dbContext.SaveChangesAsync() > 0;
            }
            return false;
        }
        //TODO Delete
        /// <summary>
        /// Returns task count statistics by default. If you want to retrieve complexity points stat, set false as a second argument
        /// </summary>
        /// <param name="statisticsRequest"></param>
        /// <param name="isCountStat"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, List<int>>> GetTotalTaskStats(TaskStatisticsRequest statisticsRequest, bool isCountStat = true)
        {
            var usersIdsWithTasks = new Dictionary<string, List<int>>();

            statisticsRequest.StartDate ??= DateTime.MinValue;
            statisticsRequest.EndDate ??= DateTime.Now;



            var allTasks = await _dbContext.Tasks
                .Where(x => statisticsRequest.UserIds.Contains(x.AssigneeId) && x.OrganizationCode == statisticsRequest.OrganizationCode)
                .ToListAsync();

            var tasksByDateRange = allTasks
                .Where(x =>
                    (x.ComplitedDate.HasValue ? x.ComplitedDate.Value :
                        x.EndDate.HasValue ? x.EndDate.Value : x.StartDate) >= statisticsRequest.StartDate
                    &&
                    (x.ComplitedDate.HasValue ? x.ComplitedDate.Value :
                        x.EndDate.HasValue ? x.EndDate.Value : x.StartDate) <= statisticsRequest.EndDate)
                .ToList();

            var tasks = tasksByDateRange
                .Where(x => x.Status == Domain.Enums.TaskStatus.Done || x.Status == Domain.Enums.TaskStatus.Archived)
                .ToList();


            foreach (var userId in statisticsRequest.UserIds)
            {
                var tasksByUser = tasks.Where(x => x.AssigneeId == userId).ToList();

                var periodStartDate = statisticsRequest.StartDate.Value;
                var periodEndDate = getPeriodEndDate(periodStartDate, statisticsRequest.TasksPeriod);


                while (periodStartDate <= statisticsRequest.EndDate.Value.Date)
                {
                    var tasksByPeriod = new List<Domain.Database.Task>();

                    if (periodEndDate < statisticsRequest.EndDate.Value.Date)
                    {
                        foreach (var task in tasksByUser)
                        {
                            DateTime dateFlag = task.ComplitedDate.HasValue ? task.ComplitedDate.Value.Date :
                                (task.EndDate.HasValue ? task.EndDate.Value.Date : task.StartDate.Value).Date;
                            if (periodStartDate <= dateFlag && dateFlag <= periodEndDate)
                            {
                                tasksByPeriod.Add(task);
                            }
                        }
                    }
                    else
                    {
                        foreach (var task in tasksByUser)
                        {
                            DateTime dateFlag = task.ComplitedDate.HasValue ? task.ComplitedDate.Value.Date :
                                (task.EndDate.HasValue ? task.EndDate.Value.Date : task.StartDate.Value).Date;
                            if (periodStartDate <= dateFlag && dateFlag <= statisticsRequest.EndDate)
                            {
                                tasksByPeriod.Add(task);
                            }
                        }
                    }

                    if (isCountStat)
                    {
                        var count = tasksByPeriod.Count;
                        if (!usersIdsWithTasks.TryGetValue(userId, out List<int> countList))
                        {
                            countList = new List<int>
                            {
                                count
                            };
                            usersIdsWithTasks[userId] = countList;
                        }
                        else
                        {
                            usersIdsWithTasks[userId].Add(count);
                        }
                    }
                    else
                    {
                        var sum = tasksByPeriod.Sum(x => x.Complexity);
                        if (!usersIdsWithTasks.TryGetValue(userId, out List<int> sumList))
                        {
                            sumList = new List<int>()
                            {
                                sum
                            };
                            usersIdsWithTasks[userId] = sumList;
                        }
                        else
                        {
                            usersIdsWithTasks[userId].Add(sum);
                        }
                    }

                    periodStartDate = periodEndDate.AddDays(1);
                    periodEndDate = getPeriodEndDate(periodStartDate, statisticsRequest.TasksPeriod);
                }
            }

            return usersIdsWithTasks;
        }
        //TODO Delete
        private DateTime getPeriodEndDate(DateTime currentDate, Period period)
        {
            switch (period)
            {
                case Period.Week:
                    DayOfWeek currentDayOfWeek = currentDate.DayOfWeek;
                    int daysUntilEndOfWeek = (DayOfWeek.Sunday - currentDayOfWeek + 7) % 7;
                    return currentDate.AddDays(daysUntilEndOfWeek).Date;
                case Period.Month:
                    return new DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month)).Date;
                case Period.Quarter:
                    int quarterNumber = (currentDate.Month - 1) / 3 + 1;
                    int lastMonthOfQuarter = quarterNumber * 3;
                    return new DateTime(currentDate.Year, lastMonthOfQuarter, DateTime.DaysInMonth(currentDate.Year, lastMonthOfQuarter)).Date;
                case Period.Year:
                    return new DateTime(currentDate.Year, 12, 31).Date;
                default: // Period.Day
                    return currentDate.Date;
            }
        }

        //TODO: переписати на SQL
        public async Task<List<TasksStatsByUser>> GetStatisticByUser(string organizationCode, DateTime dateFrom, DateTime dateTo)
        {
            var tasks = await _dbContext.Tasks
                .Where(x => x.OrganizationCode == organizationCode &&
                            (x.Status == Domain.Enums.TaskStatus.Done || x.Status == Domain.Enums.TaskStatus.Archived))
                .ToListAsync();

            var userIds = tasks
                .Select(t => t.AssigneeId)
                .Where(id => id != null)
                .Distinct()
                .ToList();

            var totalTasksStats = userIds.Select(userId => new TasksStatsByUser
            {
                UserId = userId,
                SumPoints = tasks.Where(t => t.AssigneeId == userId)
                    .GroupBy(t =>
                        t.ComplitedDate?.Date ??
                        t.EndDate?.Date ??
                        t.StartDate.Value.Date)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Complexity)),

                SumCountTask = tasks.Where(t => t.AssigneeId == userId)
                        .GroupBy(t =>
                            t.ComplitedDate?.Date ??
                            t.EndDate?.Date ??
                            t.StartDate.Value.Date)
                        .ToDictionary(g => g.Key, g => g.Count()),

                SumTaskOverdue = tasks.Where(t => t.AssigneeId == userId)
                        .GroupBy(t =>
                            t.ComplitedDate?.Date ??
                            t.EndDate?.Date ??
                            t.StartDate.Value.Date)
                        .ToDictionary(g => g.Key, g => g.Count(t =>
                            t.EndDate.HasValue &&
                            t.EndDate.Value < (t.ComplitedDate ?? DateTime.MinValue) &&
                            !t.IsAgreedOverdue.HasValue)),
                SumHours = tasks.Where(t => t.AssigneeId == userId)
                        .GroupBy(t =>
                            t.ComplitedDate?.Date ??
                            t.EndDate?.Date ??
                            t.StartDate.Value.Date)
                        .ToDictionary(g => g.Key, g =>
                        {
                            var totalMinutes = g.Sum(t => t.ActualMinutes ?? 0);
                            var totalHours = g.Sum(t => t.ActualHours ?? 0) + totalMinutes / 60;
                            return totalHours;
                        }),
                SumMinutes = tasks.Where(t => t.AssigneeId == userId)
                        .GroupBy(t =>
                            t.ComplitedDate?.Date ??
                            t.EndDate?.Date ??
                            t.StartDate.Value.Date)
                        .ToDictionary(g => g.Key, g =>
                        {
                            var totalMinutes = g.Sum(t => t.ActualMinutes ?? 0);
                            return totalMinutes % 60;
                        }),
            }).ToList();

            return totalTasksStats;
        }
        public async Task<Dictionary<string, TasksStatisticByPeriod>> GetStatisticByPeriod(string organizationCode, DateTime dateFrom, DateTime dateTo)
        {
            var sql = $@"
                SELECT 
                AssigneeID AS UserId,
                SUM(Complexity) as SumPoints,
                Count(*) as SumCountTask,
                COUNT(CASE 
                        WHEN EndDate < COALESCE(ComplitedDate, '0001-01-01')
                        AND (IsAgreedOverdue IS NULL OR IsAgreedOverdue = 0)
                        THEN 1 
                END) AS SumTaskOverdue,
                SUM(COALESCE(ActualHours, 0)) + SUM(COALESCE(ActualMinutes, 0)) / 60 AS SumHours,
                COALESCE(SUM (ActualMinutes), 0) %60 AS SumMinutes
              FROM Task
              where OrganizationCode = @organizationId AND AssigneeID <> ''
                AND Task.[Status] IN (@DoneStatus, @ArchivedStatus)
                AND CAST(COALESCE(ComplitedDate, EndDate, StartDate) AS DATE) BETWEEN CAST(@startDate AS DATE) AND CAST(@endDate AS DATE)
              Group by AssigneeID";


            var taskStatistics = _dbContext.Set<TasksStatisticByPeriod>().
                FromSqlRaw(sql,
                new SqlParameter("organizationId", organizationCode),
                new SqlParameter("DoneStatus", Domain.Enums.TaskStatus.Done),
                new SqlParameter("ArchivedStatus", Domain.Enums.TaskStatus.Archived),
                new SqlParameter("startDate", dateFrom),
                new SqlParameter("endDate", dateTo)).
                ToDictionary(k => k.UserId, v => v);
            return taskStatistics;
        }

		public async Task<List<Guid>> ArchiveCompletedTasks(Guid projectId)
		{
			var taskIds = await _dbContext.Tasks
	            .Where(t => t.ProjectId == projectId && t.Status == Domain.Enums.TaskStatus.Done)
	            .Select(t => t.Id)
	            .ToListAsync();

			if (!taskIds.Any())
            {
				return taskIds;
			}

			await _dbContext.Database.ExecuteSqlRawAsync(@"
                UPDATE Task
                SET Status = @archivedStatus,
                    ArchivedDate = @now
                WHERE ProjectId = @projectId
                  AND Status = @doneStatus",
			    new SqlParameter("@archivedStatus", (int)Domain.Enums.TaskStatus.Archived),
			    new SqlParameter("@doneStatus", (int)Domain.Enums.TaskStatus.Done),
			    new SqlParameter("@projectId", projectId),
			    new SqlParameter("@now", DateTime.UtcNow));

            return taskIds;
		}

		public async Task<bool> Insert(TaskDto taskDto)
        {
            var task = _mapper.Map<TaskDto, Domain.Database.Task>(taskDto);
            try
            {
                await _dbContext.Tasks.AddAsync(task);
                task.MarkAsCreated();
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<TaskDto>> Retrieve(string organizationCode)
        {
            var tasks = await _dbContext.Tasks.Where(x => x.OrganizationCode == organizationCode).ToListAsync();
            return _mapper.Map<List<TaskDto>>(tasks);
        }

        public async Task<TaskDto> Retrieve(Guid taskId, string organizationCode)
        {
            var task = await _dbContext.Tasks.FirstOrDefaultAsync(x => x.Id == taskId && x.OrganizationCode == organizationCode);
            return _mapper.Map<TaskDto>(task);
        }

        public async Task<TaskDto> Retrieve(Guid taskId)
        {
            var task = await _dbContext.Tasks.FirstOrDefaultAsync(x => x.Id == taskId);
            return _mapper.Map<TaskDto>(task);
        }

		public Task<List<TaskDto>> RetrieveByProject(Guid projectId, string organizationCode, bool? archived = null)
		{
			var query = _dbContext.Tasks
				.Where(x => x.ProjectId == projectId
						 && x.OrganizationCode == organizationCode);

            if (archived == false)
			{
				query = query.Where(x => x.Status != Domain.Enums.TaskStatus.Archived);
			}

			return query
				.Select(task => new TaskDto
				{
					ID = task.Id,
					ProjectID = task.ProjectId,
					Title = task.Title,
					AssigneeID = task.AssigneeId,
					Status = task.Status,
					IsAgreedOverdue = task.IsAgreedOverdue ?? false,
					StartDate = task.StartDate,
					EndDate = task.EndDate,
					ComplitedDate = task.ComplitedDate,
					ReviewDate = task.ReviewDate,
					Priority = task.Priority,
					Complexity = task.Complexity,
					ActualHours = task.ActualHours,
					ActualMinutes = task.ActualMinutes,
					OrganizationCode = task.OrganizationCode,
					DateAdd = task.DateAdd,
					DateEdit = task.DateEdit,
					CreatedByUserId = task.CreatedByUserId,
				})
				.AsNoTracking()
				.ToListAsync();
		}

		public Task<List<TaskDto>> RetrieveByProject(Guid projectId, string organizationCode, DateTime? startDate)
		{
			var query = _dbContext.Tasks
				.Where(x => x.ProjectId == projectId
						 && x.OrganizationCode == organizationCode
                         && x.Status == Domain.Enums.TaskStatus.Archived);

			if (startDate.HasValue)
			{
				query = query.Where(x => x.ArchivedDate >= startDate.Value && x.ArchivedDate <= DateTime.UtcNow);
			}

			return query
				.Select(task => new TaskDto
				{
					ID = task.Id,
					ProjectID = task.ProjectId,
					Title = task.Title,
					AssigneeID = task.AssigneeId,
					Status = task.Status,
					IsAgreedOverdue = task.IsAgreedOverdue ?? false,
					StartDate = task.StartDate,
					EndDate = task.EndDate,
					ComplitedDate = task.ComplitedDate,
					ReviewDate = task.ReviewDate,
					Priority = task.Priority,
					Complexity = task.Complexity,
					ActualHours = task.ActualHours,
					ActualMinutes = task.ActualMinutes,
					OrganizationCode = task.OrganizationCode,
					DateAdd = task.DateAdd,
					DateEdit = task.DateEdit,
					CreatedByUserId = task.CreatedByUserId,
				})
				.AsNoTracking()
				.ToListAsync();
		}
		public Dictionary<Guid, TaskProgressDto> GetTaskProgress(IEnumerable<Guid> projectIds)
        {
            var parameters = string.Join(", ", projectIds.Select((id, index) => $"@p{index}"));
            var sql = $@"
                SELECT 
                    ProjectId,
                    COUNT(Id) AS TotalTasks,
                    SUM(CASE WHEN [Status] IN (3, 4) THEN 1 ELSE 0 END) AS CompletedTasks
                FROM [TASK]
                WHERE ProjectId IN ({parameters})
                GROUP BY ProjectId";

            var sqlParameters = projectIds
                .Select((id, index) => new SqlParameter($"p{index}", id))
                .ToArray();

            var tasksProgress = _dbContext.Set<TaskProgressDto>().FromSqlRaw(sql, sqlParameters)
                .ToDictionary(k => k.ProjectId, v => v);
            return tasksProgress;
        }

        public async Task<List<TaskDto>> RetriveByUser(string userId, string organizationCode, int count = 10)
        {
            IQueryable<Domain.Database.Task> tasksQuery = _dbContext.Tasks
                .Where(x => x.AssigneeId == userId
                            && x.Status != Domain.Enums.TaskStatus.Done 
                            && x.Status != Domain.Enums.TaskStatus.Archived
                            && x.Status != Domain.Enums.TaskStatus.OnHold
                            && x.OrganizationCode == organizationCode)
                .OrderByDescending(x => x.Priority)
                .ThenBy(x => x.EndDate);

            if (count > 0)
            {
                tasksQuery = tasksQuery.Take(count);
            }

            var tasks = await tasksQuery.Select(task => new TaskDto()
            {
                ID = task.Id,
                ProjectID = task.ProjectId,
                Title = task.Title,
                //Description = task.Description,
                //PerformanceNote = task.PerformanceNote,
                AssigneeID = task.AssigneeId,
                Status = task.Status,
                IsAgreedOverdue = task.IsAgreedOverdue.HasValue ? task.IsAgreedOverdue.Value : false,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                ComplitedDate = task.ComplitedDate,
                ReviewDate = task.ReviewDate,
                Priority = task.Priority,
                Complexity = task.Complexity,
                ActualHours = task.ActualHours,
                ActualMinutes = task.ActualMinutes,
                OrganizationCode = task.OrganizationCode,
                DateAdd = task.DateAdd,
                DateEdit = task.DateEdit,
                CreatedByUserId = task.CreatedByUserId,
            }).AsNoTracking().ToListAsync();
            return tasks;
        }

        public async Task<List<TaskDto>> RetrieveComplitedTasksByUserAndDate(TaskDetailsRequest taskDetailsRequest)
        {
            if (!string.IsNullOrEmpty(taskDetailsRequest.UserId))
            {
                var tasksByUser = await _dbContext.Tasks.Where(x => x.AssigneeId == taskDetailsRequest.UserId).ToListAsync();
                var tasksByDate = tasksByUser.Where(x => x.ComplitedDate >= taskDetailsRequest.StartDate && x.ComplitedDate <= taskDetailsRequest.EndDate).ToList();
                return _mapper.Map<List<TaskDto>>(tasksByDate);
            }

            return new List<TaskDto>();
        }
        public async Task<Dictionary<string, TasksCountByUser>> RetrieveCountCompletedTasksByUsers(string organizationCode)
        {
            var sql = $@"
                    SELECT 
                        AssigneeID AS UserId,
                        Count(*) as SumCountTask
                FROM Task
                where OrganizationCode = @organizationId AND AssigneeID <> ''
                    AND Task.[Status] IN (@DoneStatus, @ArchivedStatus)
                Group by AssigneeID";


            var userStatistics = _dbContext.Set<TasksCountByUser>().
                FromSqlRaw(sql,
                new SqlParameter("organizationId", organizationCode),
                new SqlParameter("DoneStatus", Domain.Enums.TaskStatus.Done),
                new SqlParameter("ArchivedStatus", Domain.Enums.TaskStatus.Archived))
                .AsNoTracking()
                .ToDictionary(k => k.UserId, v => v);
            return userStatistics;
        }

		public async Task<bool> Update(TaskDto taskDto)
		{
			var existingTask = await _dbContext.Tasks
				.FirstOrDefaultAsync(t => t.Id == taskDto.ID && t.OrganizationCode == taskDto.OrganizationCode);

			if (existingTask == null)
			{
				return false;
			}

			//TODO: дати поміняються в existingTask.UpdateStatus але мапер їх скине, тому треба setDateByStatusTaskDto
			if (existingTask.Status != taskDto.Status)
			{
                setDateByStatusTaskDto(taskDto);
			}
			
			existingTask.UpdateStatus(taskDto.Status);

			if (taskDto.EndDate.HasValue)
			{
				existingTask.UpdateDeadline(taskDto.EndDate.Value);
			}

			if (!string.IsNullOrEmpty(taskDto.AssigneeID) && Guid.TryParse(taskDto.AssigneeID, out var assigneeId))
			{
				existingTask.UpdateAssignee(assigneeId);
			}

			_mapper.Map(taskDto, existingTask);
			//existingTask.MarkAsUpdated();

			var affectedRows = await _dbContext.SaveChangesAsync();
			return affectedRows > 0;

		}

		private void setDateByStatusTaskDto(TaskDto task)
        {
            var now = DateTime.UtcNow;

            if (task.Status == Domain.Enums.TaskStatus.Done && task.ComplitedDate == null)
            {
                task.ComplitedDate = now;

                if (task.ReviewDate == null)
                {
                    task.ReviewDate = now;
                }
            }
            if (task.Status == Domain.Enums.TaskStatus.OnReview)
            {
                task.ReviewDate = now;
            }

            if (task.Status == Domain.Enums.TaskStatus.Archived)
            {
                task.ArchivedDate = now;
				task.ComplitedDate ??= now;
			}

            if (task.Status != Domain.Enums.TaskStatus.Archived)
            {
                task.ArchivedDate = null;

                if (task.Status != Domain.Enums.TaskStatus.Done)
                {
                    task.ComplitedDate = null;
                }

                if (task.Status != Domain.Enums.TaskStatus.OnReview && task.Status != Domain.Enums.TaskStatus.Done)
                {
                    task.ReviewDate = null;
                }
            }
        }

        public async Task<List<AnalyticsUserDetils>> GetTasksByPersonAndDateRange(TaskDetailsRequest request)
        {
            var userTasks = await _dbContext.Tasks
                .Where(t =>
                    t.OrganizationCode == request.OrganizationCode &&
                    t.AssigneeId == request.UserId &&
                    (t.Status == Domain.Enums.TaskStatus.Done || t.Status == Domain.Enums.TaskStatus.Archived) &&
                    
                        (t.ComplitedDate ?? t.EndDate ?? t.StartDate) >= request.StartDate &&
                        (t.ComplitedDate ?? t.EndDate ?? t.StartDate) <= request.EndDate
                    
                )
                .Select(t => new AnalyticsUserDetils
                {
                    ID = t.Id,
                    ProjectID = t.ProjectId,
                    Title = t.Title,
                    IsAgreedOverdue = t.IsAgreedOverdue ?? false,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    ComplitedDate = t.ComplitedDate,
                    ReviewDate = t.ReviewDate,
                    Complexity = t.Complexity,
                    ActualHours = t.ActualHours,
                    ActualMinutes = t.ActualMinutes,
                })
                .ToListAsync();

            return userTasks;
        }

        #region GitHub
        public async Task<bool> UpdateTaskStatusByBranch(GitHubUpdateStatusTask statusTask)
        {
            var existingTask = await _dbContext.Tasks
                .FirstOrDefaultAsync(t => t.GitHubBranch == statusTask.Branch
                && t.GitHubOwner == statusTask.Owner
                && t.GitHubRepoName == statusTask.RepoName);

            if (existingTask == null)
            {
                return false;
            }

            if (existingTask.Status != statusTask.Status)
            {
				existingTask.UpdateStatus(statusTask.Status);

				try
                {
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            return false;
        }

        public async Task<bool> UpdateOwnerByRepos(GitHubUpdateRepo repo)
        {
            if (repo.ProjectIds == null || !repo.ProjectIds.Any())
            {
                return false;
            }

            var tasks = await _dbContext.Tasks
                .Where(t => repo.ProjectIds.Contains(t.ProjectId) && t.GitHubOwner == repo.Owner)
                .ToListAsync();

            foreach (var task in tasks)
            {
                task.GitHubOwner = repo.NewOwner;
            }

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateRepoNameByRepos(GitHubUpdateRepo repo)
        {
            if (repo.ProjectId == Guid.Empty)
            {
                return false;
            }

            var tasks = await _dbContext.Tasks
                .Where(t => t.ProjectId == repo.ProjectId && t.GitHubOwner == repo.Owner && t.GitHubRepoName == repo.RepoName)
                .ToListAsync();

            foreach (var task in tasks)
            {
                task.GitHubRepoName = repo.NewRepoName;
            }

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteBranch(string? owner, string? repo, string? branchName)
        {
            var existingTask = await _dbContext.Tasks
                .Where(t => t.GitHubOwner == owner && t.GitHubRepoName == repo && t.GitHubRepoName == branchName)
                .FirstOrDefaultAsync();

            if (existingTask == null)
            {
                return true;
            }

            existingTask.GitHubRepoName = null;
            existingTask.GitHubOwner = null;
            existingTask.GitHubBranch = null;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<int?> GetIssueNumberByBranch(string? branch, string? owner, string? repoName)
        {
            var issueNumber = await _dbContext.Tasks
                .Where(t => t.GitHubOwner == owner && t.GitHubRepoName == repoName && t.GitHubBranch == branch)
                .Select(t => t.GitHubIssueNumber)
                .FirstOrDefaultAsync();

            return issueNumber;
        }

        public async Task<bool> BranchExistsForTask(Guid id)
        {
            return await _dbContext.Tasks.AnyAsync(t => t.Id == id && t.GitHubBranch != null);
        }

		#endregion
	}
}
