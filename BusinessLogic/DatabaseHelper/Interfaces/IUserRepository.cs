using Reconova.BusinessLogic.Exceptions;
using Reconova.Data.Models;

namespace Reconova.BusinessLogic.DatabaseHelper.Interfaces
{
    public interface IUserRepository
    {
        Task<Result<List<User>>> GetAllUsers();
        Task<Result<List<User>>> GetAllUsersExceptLoggedIn();

        Task<Result<User>> GetUserById(string id);

        //Task<Result<bool>> AddUser(User user);

        Task<Result<bool>> UpdateUser(User user);

        Task<Result<bool>> DeleteUser(string id);

        Task<string> GetLoggedInUserPhoto();

        Task<Result<List<Post>>> GetUserPosts(string userId);
    }
}
