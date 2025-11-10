using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.Core.Utilities;
using Reconova.Data;
using Reconova.Data.Models;
using Reconova.Hubs;

namespace Reconova.Controllers.Api
{

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FollowController : ControllerBase
    {

        private readonly ReconovaDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IFollowRepository _followRepository;
        private readonly UserUtility _userUtility;
        private readonly IUserRepository _userRepository;

        public FollowController(
            IUserRepository userRepository,
            IFollowRepository followRepository,
            UserUtility userUtility,
            ReconovaDbContext context,
            IHubContext<NotificationHub> hubContext)
        {
            _followRepository = followRepository;
            _userUtility = userUtility;
            _context = context;
            _hubContext = hubContext;
            _userRepository = userRepository;
        }

        [HttpPost("follow")]
        public async Task<IActionResult> Follow([FromBody] string userIdToFollow)
        {
            try
            {
                var currentUserId = await _userUtility.GetLoggedInUserId();
                var result = await _followRepository.Follow(currentUserId.ToString(), userIdToFollow);

                var senderResult = await _userRepository.GetUserById(currentUserId.ToString());
                var sender = senderResult.Value;

                if (currentUserId.ToString() != userIdToFollow)
                {
                    var notification = new Notification
                    {
                        SenderId = currentUserId.ToString(),
                        ReceiverId = userIdToFollow,
                        Message = $"{sender.FirstName} started following you.",
                        Type = "Follow",
                        CreatedDate = DateTime.UtcNow
                    };

                    _context.Notification.Add(notification);
                    await _context.SaveChangesAsync();

                    await _hubContext.Clients.User(userIdToFollow).SendAsync("ReceiveNotification", new
                    {
                        Type = "Follow",
                        Message = notification.Message,
                        SenderName = sender.FirstName
                    });
                }

                return Ok(new { success = true, message = "Followed user successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


        [HttpPost("unfollow")]
        public async Task<IActionResult> Unfollow([FromBody] string userIdToUnfollow)
        {
            try
            {
                var currentUserId = await _userUtility.GetLoggedInUserId();
                var result = await _followRepository.Unfollow(currentUserId.ToString(), userIdToUnfollow);

                return Ok(new { success = true, message = "Unfollowed user successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

}
