namespace TaskJect.Web.Models
{
    public class TariffUpdateModel
    {
        public Guid OrganizationId { get; set; }
        public DateTime TariffDateTo { get; set; }
        public DateTime TariffDateFrom { get; set; }
        public string TariffName { get; set; }
    }
}
