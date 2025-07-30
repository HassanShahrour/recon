using Microsoft.EntityFrameworkCore;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.BusinessLogic.Exceptions;
using Reconova.Data;
using Reconova.Data.Models;

namespace Reconova.BusinessLogic.DatabaseHelper.Repositories
{
    public class FollowRepository : IFollowRepository
    {

        private readonly ReconovaDbContext _context;

        public FollowRepository(ReconovaDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Follow(string followerId, string followeeId)
        {
            try
            {
                if (followerId == followeeId)
                    return Result<bool>.Failure("Can not follow.");

                var alreadyFollowing = await _context.UserFollowing
                    .AnyAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);

                if (alreadyFollowing)
                    return Result<bool>.Failure("Already Following.");

                var follow = new UserFollowing
                {
                    Id = Guid.NewGuid(),
                    FollowerId = followerId,
                    FolloweeId = followeeId,
                    FollowedAt = DateTime.UtcNow
                };

                _context.UserFollowing.Add(follow);
                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error following user: " + ex.Message);
                return Result<bool>.Failure("An error occurred while following user.");
            }
        }

        public async Task<Result<bool>> Unfollow(string followerId, string followeeId)
        {
            try
            {
                var follow = await _context.UserFollowing
                    .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);

                if (follow == null)
                    return Result<bool>.Failure("Not following.");

                _context.UserFollowing.Remove(follow);
                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error unfollowing user: " + ex.Message);
                return Result<bool>.Failure("An error occurred while unfollowing user.");
            }
        }


    }
}
