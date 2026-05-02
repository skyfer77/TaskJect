using AutoMapper;
using Data.DbContexts;
using Domain.Database;
using Microsoft.EntityFrameworkCore;

namespace Data.Database.Repository
{
    public class PersonalTodoRepository : IPersonalTodoRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PersonalTodoRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PersonalTodoDto> Retrieve(Guid Id)
        {
            var personalTodo = await _context.PersonalTodos
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == Id);
            return _mapper.Map<PersonalTodoDto>(personalTodo);
        }

        public async Task<IEnumerable<PersonalTodoDto>> Retrieve(string userId)
        {
            var personalTodos = await _context.PersonalTodos
                .Where(p => p.UserId == userId)
                .Include(p => p.Tasks)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return _mapper.Map<List<PersonalTodoDto>>(personalTodos);
        }

        public async Task<bool> Insert(PersonalTodoDto personalTodoDto)
        {
            var personalTodo = _mapper.Map<PersonalTodoDto, PersonalTodo>(personalTodoDto);
            try
            {
                //personalTodo.CreatedAt = DateTime.UtcNow;
                //personalTodo.UpdatedAt = personalTodo.CreatedAt;
                await _context.PersonalTodos.AddAsync(personalTodo);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Update(PersonalTodoDto personalTodoDto)
        {
            var existingTodo = await _context.PersonalTodos
                .FirstOrDefaultAsync(t => t.Id == personalTodoDto.Id);

            if (existingTodo == null)
            {
                return false;
            }

            existingTodo.UpdatedAt = personalTodoDto.UpdatedAt;
            existingTodo.Title = personalTodoDto.Title;

            _context.PersonalTodos.Update(existingTodo);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<DateTime?> SetUpdatedAtNow(Guid id)
        {
            var existingTodo = await _context.PersonalTodos
                .FirstOrDefaultAsync(t => t.Id == id);

            if (existingTodo == null)
            {
                return null;
            }

            existingTodo.UpdatedAt = DateTime.UtcNow;

            _context.PersonalTodos.Update(existingTodo);
            await _context.SaveChangesAsync();

            return existingTodo.UpdatedAt;
        }

        public async Task<bool> Delete(Guid id)
        {
            var personalTodo = await _context.PersonalTodos
                .FirstOrDefaultAsync(x => x.Id == id);
            if (personalTodo == null)
            {
                return false;
            }
            try
            {
                _context.PersonalTodos.Remove(personalTodo);
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
