using AutoMapper;
using Data.DbContexts;
using Domain.Database;
using Microsoft.EntityFrameworkCore;

namespace Data.Database.Repository
{
    public class PersonalTodoTaskRepository : IPersonalTodoTaskRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PersonalTodoTaskRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<bool> Insert(PersonalTodoTaskDto personalTodoTaskDto)
        {
            var personalTodoTask = _mapper.Map<PersonalTodoTaskDto, PersonalTodoTask>(personalTodoTaskDto);
            try
            {
                await _context.PersonalTodoTasks.AddAsync(personalTodoTask);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Update(PersonalTodoTaskDto personalTodoTaskDto)
        {
            var existingTodoTask = await _context.PersonalTodoTasks
                .FirstOrDefaultAsync(t => t.Id == personalTodoTaskDto.Id);

            if (existingTodoTask == null)
            {
                return false;
            }

            existingTodoTask.Text = personalTodoTaskDto.Text;

            _context.PersonalTodoTasks.Update(existingTodoTask);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleStatus(Guid id, bool isDone)
        {
            var existingTodoTask = await _context.PersonalTodoTasks
                .FirstOrDefaultAsync(t => t.Id == id);

            if (existingTodoTask == null)
            {
                return false;
            }

            existingTodoTask.IsDone = isDone;

            _context.PersonalTodoTasks.Update(existingTodoTask);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Delete(Guid id)
        {
            var personalTodoTask = await _context.PersonalTodoTasks
                .FirstOrDefaultAsync(x => x.Id == id);
            if (personalTodoTask == null)
            {
                return false;
            }
            try
            {
                _context.PersonalTodoTasks.Remove(personalTodoTask);
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
