using Microsoft.EntityFrameworkCore;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.BusinessLogic.Exceptions;
using Reconova.Core.Utilities;
using Reconova.Data;
using Reconova.Data.Models;

namespace Reconova.BusinessLogic.DatabaseHelper.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly ReconovaDbContext _context;
        private readonly UserUtility _userUtility;

        public PostRepository(ReconovaDbContext context, UserUtility userUtility)
        {
            _context = context;
            _userUtility = userUtility;
        }

        public async Task<Result<List<Post>>> GetAllPosts()
        {
            try
            {
                var posts = await _context.Post
                    .Include(p => p.User)
                    .Include(p => p.Media)
                    .Include(p => p.Likes)
                    .Include(p => p.Comments)
                        .ThenInclude(c => c.User)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                if (posts == null || !posts.Any())
                {
                    return Result<List<Post>>.Failure("No posts found.");
                }

                return Result<List<Post>>.Success(posts);
            }
            catch (Exception ex)
            {
                return Result<List<Post>>.Failure($"An error occurred while retrieving posts: {ex.Message}");
            }
        }

        public async Task<Result<bool>> CreatePostAsync(Post post, List<IFormFile>? mediaFiles)
        {
            try
            {
                var userId = await _userUtility.GetLoggedInUserId();
                post.UserId = userId.ToString();
                post.Title = "Title";

                if (mediaFiles != null && mediaFiles.Count > 0)
                {
                    foreach (var file in mediaFiles)
                    {
                        if (file.Length > 0)
                        {
                            var ext = Path.GetExtension(file.FileName).ToLower();
                            var fileType = ext switch
                            {
                                ".jpg" or ".jpeg" or ".png" or ".gif" => "image",
                                ".mp4" or ".avi" or ".mov" or ".webm" => "video",
                                _ => null
                            };

                            if (fileType == null)
                                continue;

                            var fileName = Guid.NewGuid() + ext;
                            var relativePath = "/uploads/" + fileName;
                            var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

                            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

                            using (var stream = new FileStream(absolutePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            post.Media.Add(new PostMedia
                            {
                                FilePath = relativePath,
                                FileType = fileType
                            });
                        }
                    }
                }

                _context.Post.Add(post);
                await _context.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving post with media: " + ex.Message);
                return Result<bool>.Failure("An error occurred while saving the post and media.");
            }
        }

    }
}
