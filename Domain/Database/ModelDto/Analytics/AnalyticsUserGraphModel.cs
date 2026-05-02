namespace Domain.Database
{
    public class AnalyticsUserGraphModel
    {
        public Dictionary<string, List<int>> Stats { get; set; }
        public string[] Categories { get; set; }

    }
}
