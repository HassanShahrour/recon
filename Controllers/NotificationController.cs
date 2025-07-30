using Microsoft.AspNetCore.Mvc;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.Data.Models;

namespace Reconova.Controllers
{
    public class NotificationController : Controller
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationController(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var notifications = await _notificationRepository.GetAllNotifications();
                return View(notifications.Value ?? new List<Notification>());
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unexpected error: {ex.Message}";
                return View(new List<Notification>());
            }
        }

        [HttpPut]
        [Route("api/notification/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var result = await _notificationRepository.MarkNotificationAsRead(id);

            return Ok(new { message = "Notification marked as read." });
        }


    }
}
