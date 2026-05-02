namespace Domain.Database
{
    public interface IPersonalTodoTaskRepository
    {
        Task<bool> Insert(PersonalTodoTaskDto personalTodoTaskDto);
        Task<bool> Update(PersonalTodoTaskDto personalTodoTaskDto);
        Task<bool> ToggleStatus(Guid id, bool isDone);
        Task<bool> Delete(Guid id);
    }
}
