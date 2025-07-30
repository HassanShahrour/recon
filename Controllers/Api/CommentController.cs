using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.Core.Utilities;
using Reconova.Data;
using Reconova.Data.Models;
using Reconova.Hubs;

namespace Reconova.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ReconovaDbContext _context;
        private readonly UserUtility _userUtility;
        private readonly IUserRepository _userRepository;
        private readonly IHubContext<NotificationHub> _hubContext;

        public CommentController(
            ReconovaDbContext context,
            UserUtility userUtility,
            IUserRepository userRepository,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userUtility = userUtility;
            _userRepository = userRepository;
            _hubContext = hubContext;
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddComment([FromBody] Comment comment)
        {
            var userId = await _userUtility.GetLoggedInUserId();
            comment.UserId = userId.ToString();
            comment.CreatedAt = DateTime.UtcNow;

            _context.Comment.Add(comment);
            await _context.SaveChangesAsync();

            var userResult = await _userRepository.GetUserById(comment.UserId);
            var user = userResult.Value;

            var post = await _context.Post.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == comment.PostId);
            if (post != null && post.UserId != comment.UserId)
            {
                var notification = new Notification
                {
                    SenderId = comment.UserId,
                    ReceiverId = post.UserId,
                    Message = $"{user.FirstName} commented on your post.",
                    Type = "Comment",
                    CreatedDate = DateTime.UtcNow
                };

                _context.Notification.Add(notification);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.User(post.UserId ?? "").SendAsync("ReceiveNotification", new
                {
                    Type = "Comment",
                    Message = $"{user.FirstName} commented on your post.",
                    SenderName = user.FirstName,
                    PostId = post.Id
                });
            }

            return Ok(new
            {
                comment.Id,
                comment.Content,
                comment.CreatedAt,
                comment.UserId,
                userFirstName = user.FirstName,
                userProfilePhotoPath = user.ProfilePhotoPath
            });
        }


        [HttpGet("Post/{postId}")]
        public async Task<IActionResult> GetComments(string postId)
        {
            var comments = await _context.Comment
                .Where(c => c.PostId == postId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return Ok(comments);
        }
    }

}
