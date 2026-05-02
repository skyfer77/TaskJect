namespace Domain.Database
{
    public interface IPersonalNoteRepository
    {
        Task<PersonalNoteDto> Retrieve(Guid Id);
        Task<IEnumerable<PersonalNoteDto>> Retrieve(string userId);
        Task<bool> Insert(PersonalNoteDto personalNoteDto);
        Task<bool> Update(PersonalNoteDto personalNoteDto);
        Task<bool> Delete(Guid id);
    }
}
