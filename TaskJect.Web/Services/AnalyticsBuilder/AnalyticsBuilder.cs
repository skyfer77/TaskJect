using Domain.Enums;
using Domain.Database;
using TaskJect.Web.Models;
using System.Globalization;

namespace TaskJect.Web.Services.AnalyticsBuilder
{
    public static class AnalyticsBuilder
    {
        public static List<AnalyticsUserTableModel> GenerateUserAnalyticsTable(UserStatsData data)
        {
            var users = data.Users;
            var stats = data.Stats;
            var quickFilter = data.QuickFilter;
            var dateToRange = data.DateTo;

            var (startDate, endDate) = GetFilterPeriod(dateToRange, data.QuickFilter);

            var table = new List<AnalyticsUserTableModel>();

            foreach (var user in users)
            {
                var userStats = stats.FirstOrDefault(s => s.UserId == user.Id);

                if (userStats != null)
                {
                    var filteredPoints = userStats.SumPoints
                        .Where(kv => kv.Key >= startDate && (kv.Key <= endDate))
                        .Sum(kv => kv.Value);

                    var filteredCountTask = userStats.SumCountTask
                        .Where(kv => kv.Key >= startDate && (kv.Key <= endDate))
                        .Sum(kv => kv.Value);

                    var filteredTaskOverdue = userStats.SumTaskOverdue
                        .Where(kv => kv.Key >= startDate && (kv.Key <= endDate))
                        .Sum(kv => kv.Value);

                    var filteredHours = userStats.SumHours
                        .Where(kv => kv.Key >= startDate && (kv.Key <= endDate))
                        .Sum(kv => kv.Value);

                    var filteredMinutes = userStats.SumMinutes
                        .Where(kv => kv.Key >= startDate && (kv.Key <= endDate))
                        .Sum(kv => kv.Value);

                    var additionalHours = filteredMinutes / 60;
                    var remainingMinutes = filteredMinutes % 60;

                    var totalHours = filteredHours + additionalHours;
                    var totalMinutes = remainingMinutes;

                    table.Add(new AnalyticsUserTableModel
                    {
                        UserId = user.Id,
                        FirstName = user.Name,
                        Surname = user.Surname,
                        Points = filteredPoints,
                        CountTask = filteredCountTask,
                        TaskOverdue = filteredTaskOverdue,
                        ActualHours = totalHours,
                        ActualMinutes = totalMinutes
                    });
                }
            }

            return table;
        }
        public static (DateTime, DateTime) GetFilterPeriod(string dateToRange, QuickFilter quickFilter = QuickFilter.ThisMonth)
        {
            DateTime startDate = DateTime.MinValue;
            DateTime endDate = DateTime.MaxValue;

            if (dateToRange != null)
            {
                (startDate, endDate) = parseDateRange(dateToRange);
            }
            else
            {
                startDate = getStartDateByQuickFilter(quickFilter);
                endDate = getEndDateByQuickFilter(quickFilter);
            }
            return (startDate, endDate);
        }

        public static AnalyticsUserGraphModel FilterUserStatsByPeriod(List<TasksStatsByUser> stats, FilterDataRequest request)
        {
            var analysis = new AnalyticsUserGraphModel
            {
                Stats = new Dictionary<string, List<int>>(),
                Categories = Array.Empty<string>()
            };

            var quickFilter = request.QuickFilter;
            var dateToRange = request.DateTo;
            var period = request.Period;


            var (startDate, endDate) = GetFilterPeriod(dateToRange, request.QuickFilter);

            int groupCount = calculateGroupCount(quickFilter, startDate, endDate, period);

            foreach (var stat in stats)
            {
                var userId = stat.UserId;
                var taskValues = new List<int>(new int[groupCount]);

                var dictionary = request.GetStatsDictionary(stat);

                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    if (dictionary.TryGetValue(date, out int value))
                    {
                        int groupIndex = determineGroupIndex(date, quickFilter, startDate, period);
                        if (groupIndex >= 0 && groupIndex < taskValues.Count)
                        {
                            taskValues[groupIndex] += value;
                        }
                    }
                }

                analysis.Stats[userId] = taskValues;
            }

            analysis.Categories = generateCategoriesByQuickFilter(quickFilter, startDate, endDate, period);

            return analysis;
        }

        private static DateTime startOfWeek(this DateTime dt)
        {
            int diff = (7 + (dt.DayOfWeek - DayOfWeek.Monday)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        private static DateTime endOfWeek(this DateTime date)
        {
            var daysUntilSunday = ((int)DayOfWeek.Sunday - (int)date.DayOfWeek + 7) % 7;
            return date.AddDays(daysUntilSunday).Date.AddDays(1).AddTicks(-1);
        }

        private static (DateTime, DateTime) parseDateRange(string dateToRange)
        {
            DateTime startDate = DateTime.MinValue;
            DateTime endDate = DateTime.MaxValue;

            if (!string.IsNullOrEmpty(dateToRange))
            {
                if (dateToRange.Contains(" to "))
                {
                    var dateParts = dateToRange.Split(" to ");
                    if (dateParts.Length == 2)
                    {
                        if (DateTime.TryParse(dateParts[0], out var parsedStartDate))
                            startDate = parsedStartDate;
                        if (DateTime.TryParse(dateParts[1], out var parsedEndDate))
                            endDate = parsedEndDate.AddDays(1).AddSeconds(-1);
                    }
                }
                else
                {
                    if (DateTime.TryParse(dateToRange, out var parsedSingleDate))
                    {
                        startDate = parsedSingleDate;
                        endDate = parsedSingleDate.AddDays(1).AddSeconds(-1);
                    }
                }
            }

            return (startDate, endDate);
        }


        private static DateTime getStartDateByQuickFilter(QuickFilter quickFilter)
        {
            var now = DateTime.Now;
            return quickFilter switch
            {
                QuickFilter.ThisWeek => now.startOfWeek(),
                QuickFilter.ThisMonth => new DateTime(now.Year, now.Month, 1),
                QuickFilter.ThisQuarter => new DateTime(now.Year, (now.Month - 1) / 3 * 3 + 1, 1),
                QuickFilter.ThisYear => new DateTime(now.Year, 1, 1),
            };
        }

        private static DateTime getEndDateByQuickFilter(QuickFilter quickFilter)
        {
            var now = DateTime.Now;
            return quickFilter switch
            {
                QuickFilter.ThisWeek => now.endOfWeek(),
                QuickFilter.ThisMonth => new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month)).AddTicks(-1),
                QuickFilter.ThisQuarter => new DateTime(now.Year, ((now.Month - 1) / 3 + 1) * 3, 1).AddMonths(1).AddDays(-1).AddTicks(-1),
                QuickFilter.ThisYear => new DateTime(now.Year, 12, 31).AddTicks(-1),
            };
        }

        private static int determineGroupIndex(DateTime date, QuickFilter filter, DateTime startDate, Period period)
        {
            switch (period)
            {
                case Period.Day:
                    return (date - startDate).Days;
                case Period.Week:
                    return (date - startDate).Days / 7;
                case Period.Month:
                    return (date.Month - startDate.Month) + (date.Year - startDate.Year) * 12; 
                case Period.Quarter:
                    return ((date.Month - 1) / 3) - ((startDate.Month - 1) / 3);
                case Period.Year:
                    return date.Year - startDate.Year;
                default:
                    return -1;
            }
        }

        private static int calculateGroupCount(QuickFilter filter, DateTime startDate, DateTime endDate, Period period)
        {
            switch (period)
            {
                case Period.Day:
                    return (endDate - startDate).Days + 1;
                case Period.Week:
                    return (int)Math.Ceiling((endDate - startDate).TotalDays / 7);
                case Period.Month:
                    return (endDate.Month - startDate.Month) + 1 + (endDate.Year - startDate.Year) * 12;
                case Period.Quarter:
                    return (int)Math.Ceiling((endDate.Month - startDate.Month + 1) / 3.0);
                case Period.Year:
                    return endDate.Year - startDate.Year + 1;
                default:
                    return 0;
            }
        }

        private static string[] generateCategoriesByQuickFilter(QuickFilter filter, DateTime startDate, DateTime endDate, Period period)
        {
            var categories = new List<string>();

            switch (period)
            {
                case Period.Day:
                    for (int i = 0; i <= (endDate - startDate).Days; i++)
                    {
                        categories.Add(startDate.AddDays(i).ToString("yyyy-MM-dd"));
                    }
                    break;
                case Period.Week:
                    DateTime weekStart = startDate;
                    while (weekStart <= endDate)
                    {
                        int weekIndex = (int)Math.Floor((weekStart - startDate).TotalDays / 7) + 1;
                        categories.Add($"Week {weekIndex}");
                        weekStart = weekStart.AddDays(7);
                    }
                    break;
                case Period.Month:
                    for (int i = startDate.Month; i <= endDate.Month; i++)
                    {
                        categories.Add(new DateTime(startDate.Year, i, 1).ToString("MMM", CultureInfo.InvariantCulture));
                    }
                    break;
                case Period.Quarter:
                    int startQuarter = (startDate.Month - 1) / 3 + 1;
                    int endQuarter = (endDate.Month - 1) / 3 + 1;
                    int yearDiff = endDate.Year - startDate.Year;

                    for (int q = startQuarter; q <= 4 * yearDiff + endQuarter; q++)
                    {
                        int year = startDate.Year + (q - 1) / 4;
                        categories.Add($"Q{q % 4} {year}");
                    }
                    break;
                case Period.Year:
                    for (int y = startDate.Year; y <= endDate.Year; y++)
                    {
                        categories.Add(y.ToString());
                    }
                    break;
            }

            return categories.ToArray();
        }
    }
}
