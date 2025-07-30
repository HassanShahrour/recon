using Reconova.BusinessLogic.Exceptions;

namespace Reconova.BusinessLogic.DatabaseHelper.Interfaces
{
    public interface IFollowRepository
    {

        Task<Result<bool>> Follow(string followerId, string followeeId);

        Task<Result<bool>> Unfollow(string followerId, string followeeId);
    }
}
