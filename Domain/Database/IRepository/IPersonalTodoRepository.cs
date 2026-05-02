namespace Domain.Database
{
    public interface IPersonalTodoRepository
    {
        Task<PersonalTodoDto> Retrieve(Guid Id);
        Task<IEnumerable<PersonalTodoDto>> Retrieve(string userId);
        Task<bool> Insert(PersonalTodoDto personalTodoDto);
        Task<bool> Update(PersonalTodoDto personalTodoDto);
        Task<DateTime?> SetUpdatedAtNow(Guid id);
        Task<bool> Delete(Guid id);
    }
}
