using Reconova.BusinessLogic.Exceptions;
using Reconova.Data.Models;

namespace Reconova.BusinessLogic.DatabaseHelper.Interfaces
{
    public interface IPostRepository
    {
        Task<Result<List<Post>>> GetAllPosts();

        Task<Result<bool>> CreatePostAsync(Post post, List<IFormFile>? mediaFiles);

    }
}
