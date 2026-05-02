using Domain.Database;
using Domain.Enums;
using TaskJect.Web.Models;
using TaskJect.Web.Resources;
using TaskJect.Web.Services.AnalyticsBuilder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace TaskJect.Web.Controllers
{
    [Authorize]
    public class AnalyticsController : Controller
    {
        ITaskRepository _taskRepository;
        IProjectRepository _projectRepository;
        IHttpClientFactory _httpClientFactory;
        IApplicationUserRepository _applicationUserRepository;
        private readonly IStringLocalizer<ErrorResources> _localizer;
        private readonly IStringLocalizer<SharedResources> _sharedLocalizer;

        public AnalyticsController(IHttpClientFactory httpClientFactory, IApplicationUserRepository applicationUserRepository,
            IProjectRepository projectRepository, ITaskRepository taskRepository, IStringLocalizer<ErrorResources> localizer
            , IStringLocalizer<SharedResources> sharedLocalizer)
        {
            _httpClientFactory = httpClientFactory;
            _applicationUserRepository = applicationUserRepository;
            _projectRepository = projectRepository;
            _taskRepository = taskRepository;
            _localizer = localizer;
            _sharedLocalizer = sharedLocalizer;
        }
        public async Task<ActionResult> IndexAsync(QuickFilter quickFilter, string dateTo)
        {
            var organizationCode = User.Claims.FirstOrDefault(c => c.Type == "organization_code")?.Value;

            var users = await _applicationUserRepository.GetAllUsersTheOrganization(organizationCode);

            if (users == null)
            {
                return redirectToErrorPage(_localizer["NoFoundUsersTitle"], _localizer["NoFoundUsersMessage"]);
            }
            var (startDate, endDate) = AnalyticsBuilder.GetFilterPeriod(dateTo, quickFilter);
            var statsByUsers = await _taskRepository.GetStatisticByPeriod(organizationCode, startDate, endDate);
            if (statsByUsers == null)
            {
                return redirectToErrorPage(_localizer["Error"], _localizer["ErrorCalculatingAnalyticsMessage"]);
            }
            var resultTable = new List<AnalyticsUserTableModel>();
            foreach(var user in users)
            {
                var userRow = new AnalyticsUserTableModel();
                userRow.UserId = user.Id;
                userRow.FirstName = user.Name;
                userRow.Surname = user.Surname;
                if(statsByUsers.TryGetValue(user.Id, out var statistic))
                {
                    userRow.Points = statistic.SumPoints;
                    userRow.CountTask = statistic.SumCountTask;
                    userRow.TaskOverdue = statistic.SumTaskOverdue;
                    userRow.ActualHours = statistic.SumHours;
                    userRow.ActualMinutes = statistic.SumMinutes;
                }
                resultTable.Add(userRow);
            }

            return View(resultTable);
        }

        [HttpPost]
        public async Task<IActionResult> GetTableData(QuickFilter quickFilter, string dateTo)
        {
            var organizationCode = User.Claims.FirstOrDefault(c => c.Type == "organization_code")?.Value;

            var users = await _applicationUserRepository.GetAllUsersTheOrganization(organizationCode);
            if (users == null)
            {
                return redirectToErrorPage(_localizer["NoFoundUsersTitle"], _localizer["NoFoundUsersMessage"]);
            }
            var (startDate, endDate) = AnalyticsBuilder.GetFilterPeriod(dateTo, quickFilter);
            var statsByUsers = await _taskRepository.GetStatisticByPeriod(organizationCode, startDate, endDate);
            if (statsByUsers == null)
            {
                return redirectToErrorPage(_localizer["Error"], _localizer["ErrorCalculatingAnalyticsMessage"]);
            }
            var resultTable = new List<AnalyticsUserTableModel>();
            foreach (var user in users)
            {
                var userRow = new AnalyticsUserTableModel();
                userRow.UserId = user.Id;
                userRow.FirstName = user.Name;
                userRow.Surname = user.Surname;
                if (statsByUsers.TryGetValue(user.Id, out var statistic))
                {
                    userRow.Points = statistic.SumPoints;
                    userRow.CountTask = statistic.SumCountTask;
                    userRow.TaskOverdue = statistic.SumTaskOverdue;
                    userRow.ActualHours = statistic.SumHours;
                    userRow.ActualMinutes = statistic.SumMinutes;
                }
                resultTable.Add(userRow);
            }

            return Json(resultTable);
        }

        [HttpPost]
        public async Task<ActionResult> MoreDetails(string id, string dateTo, Period period, QuickFilter quickFilter)
        {
            int CountTaskOverdue = 0;

            var organizationCode = User.Claims.FirstOrDefault(c => c.Type == "organization_code")?.Value;
            var user = await _applicationUserRepository.GetUserById(id, organizationCode);

            if (user == null)
            {
                return redirectToErrorPage(_localizer["NoFoundDetails"], _localizer["NoFoundCurrentUser"]);
            }

            var idUser = new List<string>() { id };
            var taskStatisticsRequest = new TaskStatisticsRequest();
            taskStatisticsRequest.UserIds = idUser;
            taskStatisticsRequest.TasksPeriod = period;
            taskStatisticsRequest.QuickFilter = quickFilter;

            var requestData = setStartEndDate(taskStatisticsRequest, dateTo);
            requestData.OrganizationCode = organizationCode;

            var overdueResponse = await _taskRepository.GetOverdueTasksDetails(requestData);
            if (overdueResponse == null)
            {
                return redirectToErrorPage(_localizer["overdueTasksLoadErrorTitle"], _localizer["overdueTasksLoadErrorMessage"]);
            }
            overdueResponse.TryGetValue(user.Id, out CountTaskOverdue);

            var taskResponse = await _taskRepository.GetTotalTaskStats(requestData);
            if (taskResponse == null)
            {
                return redirectToErrorPage(_localizer["totalTasksStatsLoadErrorTitle"], _localizer["totalTasksStatsLoadErrorMessage"]);
            }

            var pointResponse = await _taskRepository.GetTotalTaskStats(requestData, false);
            if (pointResponse == null)
            {
                return redirectToErrorPage(_localizer["totalPointsStatsLoadErrorTitle"], _localizer["totalPointsStatsLoadErrorMessage"]);
            }

            int sumTask = 0, sumPoint = 0;
            foreach (var point in pointResponse) {
                if (user.Id.Equals(point.Key))
                {
                    sumPoint = point.Value.Sum();
                }
            }

            foreach (var task in taskResponse)
            {
                if (user.Id.Equals(task.Key))
                {
                    sumTask = task.Value.Sum();
                }
            }

            var requestDetailsDate = new TaskDetailsRequest();
            requestDetailsDate.UserId = requestData.UserIds[0];
            requestDetailsDate.StartDate = requestData.StartDate;
            requestDetailsDate.EndDate = requestData.EndDate;
            requestDetailsDate.OrganizationCode = organizationCode;
            var detailsResponse = await _taskRepository.GetDetailsTasks(requestDetailsDate);
            if (detailsResponse == null)
            {
                return redirectToErrorPage(_localizer["taskDetailsLoadErrorTitle"], _localizer["taskDetailsLoadErrorMessage"]);
            }

            var anlalysis = new AnalysisOverviewUserModel()
            {
                User = user,
                TaskCount = sumTask,
                PointSum= sumPoint,
                Tasks = detailsResponse.TasksCount,
                Points = detailsResponse.TasksPoint,
                TasksPeriod = taskStatisticsRequest.TasksPeriod,
                QuickFilter = taskStatisticsRequest.QuickFilter,
                DateTo = dateTo,
                CountTaskOverdue = CountTaskOverdue
            };

            return PartialView("_MoreDetails", anlalysis);
        }

        [HttpGet("Analytics/UserDetails/{userId}")]
        public async Task<ActionResult> UserDetails(string userId, [FromQuery] string dateTo)
        {
            var organizationCode = User.Claims.FirstOrDefault(c => c.Type == "organization_code")?.Value;
            var user = await _applicationUserRepository.GetUserById(userId, organizationCode);

            if (user == null)
            {
                return redirectToErrorPage(_localizer["NoFoundDetails"], _localizer["NoFoundCurrentUser"]);
            }

            var (startDate, endDate) = AnalyticsBuilder.GetFilterPeriod(dateTo);

            var taskDetailsRequest = new TaskDetailsRequest()
            {
                StartDate = startDate,
                EndDate = endDate,
                UserId = userId,
                OrganizationCode = organizationCode,
            };

            var tasks = await _taskRepository.GetTasksByPersonAndDateRange(taskDetailsRequest);
            if (tasks == null)
            {
                return redirectToErrorPage(_localizer["userTasksLoadErrorTitle"], _localizer["userTasksLoadErrorMessage"]);
            }

            var projects = await _projectRepository.RetrieveNameProject(organizationCode);
            if (projects == null)
            {
                return redirectToErrorPage(_localizer["projectsLoadErrorTitle"], _localizer["projectsLoadErrorMessage"]);
            }

            ViewBag.TotalTasks = tasks.Count;
            ViewBag.TotalPoints = tasks.Sum(t => t.Complexity);
            ViewBag.TotalTimes = getFormattedTotalTime(tasks);

            var userAnalytics = new AnalysisUserDetails
            {
                UserId = user.Id,
                UserName = user.Name,
                UserSurname = user.Surname,
                Tasks = tasks,
                Projects = projects,
                DateTo = dateTo,
            };
            return View(userAnalytics);
        }

        [HttpPost]
        public async Task<ActionResult> GetUserDetailsFiltered(string userId, string dateTo)
        {
		    var organizationCode = User.Claims.FirstOrDefault(c => c.Type == "organization_code")?.Value;

            var (startDate, endDate) = AnalyticsBuilder.GetFilterPeriod(dateTo);
            var taskDetailsRequest = new TaskDetailsRequest()
            {
                StartDate = startDate,
                EndDate = endDate,
                UserId = userId,
                OrganizationCode = organizationCode
            };

            var tasks = await _taskRepository.GetTasksByPersonAndDateRange(taskDetailsRequest);
            if (tasks == null)
            {
                return redirectToErrorPage(_localizer["userTasksLoadErrorTitle"], _localizer["userTasksLoadErrorMessage"]);
            }

            var projects = await _projectRepository.RetrieveNameProject(organizationCode);
            if (projects == null)
            {
                return redirectToErrorPage(_localizer["projectsLoadErrorTitle"], _localizer["projectsLoadErrorMessage"]);
            }

            ViewData["Projects"] = projects;

            TempData["TotalTasks"] = tasks.Count;
            TempData["TotalPoints"] = tasks.Sum(t => t.Complexity);
            TempData["TotalTimes"] = getFormattedTotalTime(tasks);
            TempData["DateTo"] = dateTo;

            return PartialView("_UserTasksTablePartial", tasks);
        }

        private string getFormattedTotalTime(List<AnalyticsUserDetils> analyticsUsers)
        {
            int totalHours = 0;
            int totalMinutes = 0;

            foreach (var user in analyticsUsers)
            {
                totalHours += user.ActualHours ?? 0;
                totalMinutes += user.ActualMinutes ?? 0;
            }

            totalHours += totalMinutes / 60;
            totalMinutes = totalMinutes % 60;

            return $"{totalHours}{_sharedLocalizer["h"]} {totalMinutes}{_sharedLocalizer["m"]}";
        }

        private ActionResult redirectToErrorPage(string errorTitle, string errorMsg)
        {
            TempData["ErrorTitle"] = errorTitle;
            TempData["ErrorMessage"] = errorMsg;
            return RedirectToAction("Error", "Analytics");
        }

        //Error view Analysis
        public IActionResult Error()
        {
            if (TempData["ErrorTitle"] != null)
            {
                ViewBag.ErrorTitle = TempData["ErrorTitle"];
            }
            else
            {
                ViewBag.ErrorTitle = _localizer["PageNotFound"];
            }
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View();
        }
        //TODO Delete
        private TaskStatisticsRequest setStartEndDate(TaskStatisticsRequest taskStatisticsRequest, string dateTo)
        {
            if (dateTo != null)
            {
                var dateList = dateTo.Split(' ').ToList();
                var start = DateTime.Parse(dateList[0]);
                var end = DateTime.Parse(dateList[2]);
                taskStatisticsRequest.StartDate = start;
                taskStatisticsRequest.EndDate = end;
            }
            else
            {
                DateTime baseDate = DateTime.Now.Date;

                var thisWeekStart = baseDate.AddDays(-(int)baseDate.DayOfWeek + 1).Date;
                var thisMonthStart = baseDate.AddDays(1 - baseDate.Day);
                switch (taskStatisticsRequest.QuickFilter)
                {
                    case QuickFilter.ThisWeek:
                        taskStatisticsRequest.StartDate = thisWeekStart;
                        taskStatisticsRequest.EndDate = thisWeekStart.AddDays(7).AddSeconds(-1);
                        taskStatisticsRequest.TasksPeriod = Period.Day;
                        break;
                    case QuickFilter.ThisMonth:
                        taskStatisticsRequest.StartDate = thisMonthStart;
                        taskStatisticsRequest.EndDate = thisMonthStart.AddMonths(1).AddSeconds(-1);
                        taskStatisticsRequest.TasksPeriod = Period.Week;
                        break;
                    case QuickFilter.ThisQuarter:
                        int currQuarter = (baseDate.Month - 1) / 3 + 1;
                        taskStatisticsRequest.StartDate = new DateTime(baseDate.Year, 3 * currQuarter - 2, 1);
                        taskStatisticsRequest.EndDate = new DateTime(baseDate.Year, 3 * currQuarter, DateTime.DaysInMonth(baseDate.Year, 3 * currQuarter));
                        taskStatisticsRequest.TasksPeriod = Period.Week;
                        break;
                    case QuickFilter.ThisYear:
                        taskStatisticsRequest.StartDate = new DateTime(baseDate.Year, 1, 1);
                        taskStatisticsRequest.EndDate = new DateTime(baseDate.Year, 12, 31);
                        taskStatisticsRequest.TasksPeriod = Period.Month;
                        break;
                }
            }
            return taskStatisticsRequest;
        }
    }
}
