namespace Domain.Database
{
    public class OrganizationParameters
    {
        public Guid OrganizationId { get; set; }
        public long UsedStorageSpace { get; set; }
        public TariffPlanDto Plan { get; set; }
        public List<ApplicationUserLiteDto> Users { get; set; }
    }
}
