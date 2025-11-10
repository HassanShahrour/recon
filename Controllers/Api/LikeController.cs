using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Reconova.Core.Utilities;
using Reconova.Data;
using Reconova.Data.Models;
using Reconova.Hubs;

namespace Reconova.Controllers.Api
{

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LikeController : ControllerBase
    {
        private readonly ReconovaDbContext _context;
        private readonly UserUtility _userUtility;
        private readonly IHubContext<NotificationHub> _hubContext;

        public LikeController(ReconovaDbContext context, UserUtility userUtility, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userUtility = userUtility;
            _hubContext = hubContext;
        }

        [HttpPost("Toggle")]
        public async Task<IActionResult> ToggleLike([FromBody] Like model)
        {
            var userId = await _userUtility.GetLoggedInUserId();
            model.UserId = userId.ToString();

            var existingLike = await _context.Like
                .FirstOrDefaultAsync(l => l.PostId == model.PostId && l.UserId == model.UserId);

            bool isLiked = false;

            if (existingLike != null)
            {
                _context.Like.Remove(existingLike);
            }
            else
            {
                _context.Like.Add(model);
                isLiked = true;
            }

            await _context.SaveChangesAsync();

            var likeCount = await _context.Like.CountAsync(l => l.PostId == model.PostId);

            if (isLiked)
            {
                var post = await _context.Post
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == model.PostId);

                var sender = await _context.Users.FindAsync(userId.ToString());
                var receiverId = post?.UserId;

                if (receiverId != null && receiverId != model.UserId)
                {
                    var notification = new Notification
                    {
                        SenderId = model.UserId,
                        ReceiverId = receiverId,
                        Message = $"{sender?.FirstName} liked your post.",
                        Type = "Like",
                        CreatedDate = DateTime.UtcNow
                    };
                    _context.Notification.Add(notification);
                    await _context.SaveChangesAsync();

                    await _hubContext.Clients.User(receiverId)
                        .SendAsync("ReceiveNotification", new
                        {
                            Type = "Like",
                            Message = $"{sender?.FirstName} liked your post.",
                            SenderName = sender?.FirstName,
                            PostId = model.PostId
                        });
                }
            }

            return Ok(new { success = true, likeCount });
        }


    }
}
