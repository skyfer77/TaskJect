namespace Domain.Database
{
    public class TaskWithFiles
    {
        public Guid TaskId { get; set; }
        public List<Guid> FilesIds { get; set; } = new();
        public long UsedStorageSpace { get; set; }
    }
}
