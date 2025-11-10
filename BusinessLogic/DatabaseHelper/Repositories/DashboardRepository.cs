using Microsoft.EntityFrameworkCore;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.Core.Utilities;
using Reconova.Data;

namespace Reconova.BusinessLogic.DatabaseHelper.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {

        private readonly ReconovaDbContext _context;
        private readonly UserUtility _userUtility;

        public DashboardRepository(ReconovaDbContext context, UserUtility userUtility)
        {
            _context = context;
            _userUtility = userUtility;
        }

        public async Task<Dictionary<string, int>> GetTopUsedToolsAsync(int count = 5)
        {
            try
            {
                var topTools = await _context.ScanResults
                    .Where(sr => sr.IsDeleted == 0 && !string.IsNullOrEmpty(sr.Tool))
                    .GroupBy(sr => sr.Tool)
                    .Select(g => new { ToolName = g.Key, UsageCount = g.Count() })
                    .OrderByDescending(g => g.UsageCount)
                    .Take(count)
                    .ToDictionaryAsync(g => g.ToolName!, g => g.UsageCount);

                return topTools;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrieve top used tools: {ex.Message}");
                return new Dictionary<string, int>();
            }
        }

        public async Task<Dictionary<string, int>> GetUserRegistrationTrendAsync()
        {
            var groupedData = await _context.Users
                //.Where(u => u.IsDeleted == 0)
                .GroupBy(u => new { u.Timestamp.Year, u.Timestamp.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count()
                })
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Month)
                .ToListAsync();

            var result = groupedData.ToDictionary(
                g => $"{g.Year}-{g.Month:D2}",
                g => g.Count);

            return result;
        }

        public async Task<Dictionary<string, int>> GetToolCategoryDistributionAsync()
        {
            var data = await _context.Tools
                .Where(t => t.IsDeleted == 0 && t.Category != null)
                .GroupBy(t => t.Category.Name ?? "")
                .Select(g => new
                {
                    CategoryName = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return data.ToDictionary(g => g.CategoryName, g => g.Count);
        }

        public async Task<Dictionary<string, int>> GetTopFollowedUsersAsync()
        {
            var data = await _context.UserFollowing
                .Where(f => f.Followee != null && f.Followee.IsDeleted == 0)
                .GroupBy(f => new { f.FolloweeId, f.Followee.FirstName, f.Followee.LastName })
                .Select(g => new
                {
                    FullName = g.Key.FirstName + " " + g.Key.LastName,
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync();

            return data.ToDictionary(g => g.FullName, g => g.Count);
        }

        public async Task<Dictionary<string, int>> GetMostActiveUsersAsync()
        {
            var users = await _context.Users
                .Where(u => u.IsDeleted == 0)
                .Select(u => new
                {
                    u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    PostsCount = u.Posts.Count(),
                    CommentsOnOthersCount = u.Posts
                        .SelectMany(p => p.Comments)
                        .Count(c => c.UserId != u.Id),
                    CommentsMade = _context.Comment.Count(c => c.UserId == u.Id && c.Post.UserId != u.Id),
                    LikesMade = _context.Like.Count(l => l.UserId == u.Id && l.Post.UserId != u.Id)
                })
                .ToListAsync();

            var result = new Dictionary<string, int>();

            foreach (var u in users)
            {
                // Weighting: posts = 3pts, comments = 2pts, likes = 1pt
                int activityScore = (u.PostsCount * 3) + (u.CommentsMade * 2) + (u.LikesMade * 1);
                if (activityScore > 0)
                {
                    result[u.FullName] = activityScore;
                }
            }

            return result
                .OrderByDescending(kvp => kvp.Value)
                .Take(5)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }


        public async Task<Dictionary<string, int>> GetUserPlanDistributionAsync()
        {
            var result = await _context.Users
                .Where(u => u.IsDeleted == 0 && u.PlanId != null)
                .GroupBy(u => u.Plan!.Name)
                .Select(g => new
                {
                    PlanName = g.Key ?? "Unknown",
                    Count = g.Count()
                })
                .ToListAsync();

            return result.ToDictionary(g => g.PlanName, g => g.Count);
        }


    }
}
