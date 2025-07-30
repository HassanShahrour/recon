using Microsoft.AspNetCore.Mvc;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.Core.Utilities;
using Reconova.ViewModels.Chat;

namespace FMS.APP.Controllers
{
    [Route("chat")]
    public class ChatController : Controller
    {
        private readonly IChatRepository _chatRepository;
        private readonly UserUtility _userUtility;

        public ChatController(IChatRepository chatRepository, UserUtility userUtility)
        {
            _chatRepository = chatRepository;
            _userUtility = userUtility;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var usersResult = await _chatRepository.GetUsers();
                if (!usersResult.IsSuccess || usersResult.Value == null)
                {
                    TempData["Error"] = "Failed to load users.";
                    return View(new List<ChatViewModel>());
                }

                var senderId = await _userUtility.GetLoggedInUserId();
                var chatViewModels = new List<ChatViewModel>();

                foreach (var user in usersResult.Value)
                {
                    var unreadCount = await _chatRepository.GetUnreadMessageCount(user.Id);
                    chatViewModels.Add(new ChatViewModel
                    {
                        UPK = user.Id,
                        Alias = $"{user.FirstName} {user.LastName}",
                        UserProfileImage = user.ProfilePhotoPath,
                        UnreadCount = unreadCount
                    });
                }

                ViewBag.SenderId = senderId.ToString();

                var sortedList = chatViewModels
                    .OrderByDescending(c => c.UnreadCount)
                    .ToList();

                return View(sortedList);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unexpected error: {ex.Message}";
                return View(new List<ChatViewModel>());
            }
        }

    }
}
