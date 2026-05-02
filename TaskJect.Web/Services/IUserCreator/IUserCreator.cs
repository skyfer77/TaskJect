
using Domain.Database;
namespace TaskJect.Web.Services
{
    public interface IUserCreator
    {
        Task<bool> CreateUser(CreateUserByEmailModel model);
    
    }
}
