using AutoMapper;
using Data.DbContexts;
using Domain.Database;
using Microsoft.EntityFrameworkCore;

namespace Data.Database.Repository
{
    public class PersonalNoteRepository : IPersonalNoteRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PersonalNoteRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PersonalNoteDto> Retrieve(Guid Id)
        {
            var personalNote = await _context.PersonalNotes
                .Where(p => p.Id == Id)
                .FirstOrDefaultAsync();
            return _mapper.Map<PersonalNoteDto>(personalNote);
        }

        public async Task<IEnumerable<PersonalNoteDto>> Retrieve(string userId)
        {
            var personalNotes = await _context.PersonalNotes
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return _mapper.Map<List<PersonalNoteDto>>(personalNotes);
        }

        public async Task<bool> Insert(PersonalNoteDto personalNoteDto)
        {
            var personalNote = _mapper.Map<PersonalNoteDto, PersonalNote>(personalNoteDto);
            try
            {
                personalNote.CreatedAt = DateTime.UtcNow;
                await _context.PersonalNotes.AddAsync(personalNote);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Update(PersonalNoteDto personalNoteDto)
        {
            var existingNote = await _context.PersonalNotes
                .FirstOrDefaultAsync(t => t.Id == personalNoteDto.Id);

            if (existingNote == null)
            {
                return false;
            }

            existingNote.UpdatedAt = DateTime.UtcNow;
            existingNote.Title = personalNoteDto.Title != null ? personalNoteDto.Title : existingNote.Title;
            existingNote.Text = personalNoteDto.Text != null ? personalNoteDto.Text : existingNote.Text;

            _context.PersonalNotes.Update(existingNote);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Delete(Guid id)
        {
            var personalNote = await _context.PersonalNotes
                .FirstOrDefaultAsync(x => x.Id == id);
            if (personalNote == null)
            {
                return false;
            }
            try
            {
                _context.PersonalNotes.Remove(personalNote);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
