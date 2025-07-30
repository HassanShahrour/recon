using Microsoft.EntityFrameworkCore;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.BusinessLogic.Exceptions;
using Reconova.Core.Utilities;
using Reconova.Data;
using Reconova.Data.Models;

namespace Reconova.BusinessLogic.DatabaseHelper.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ReconovaDbContext _context;
        private readonly UserUtility _userUtility;

        public NotificationRepository(ReconovaDbContext context, UserUtility userUtility)
        {
            _context = context;
            _userUtility = userUtility;
        }

        public async Task<Result<List<Notification>>> GetAllNotifications()
        {
            try
            {
                var userId = await _userUtility.GetLoggedInUserId();

                var notifications = await _context.Notification
                    .Where(n => n.ReceiverId == userId.ToString())
                    .Include(n => n.Sender)
                    .Include(n => n.Receiver)
                    .OrderByDescending(n => n.CreatedDate)
                    .ToListAsync();

                if (notifications == null || !notifications.Any())
                {
                    return Result<List<Notification>>.Failure("No notifications found.");
                }

                return Result<List<Notification>>.Success(notifications);
            }
            catch (Exception ex)
            {
                return Result<List<Notification>>.Failure($"An error occurred while retrieving notifications: {ex.Message}");
            }
        }

        public async Task<Result<bool>> MarkNotificationAsRead(int id)
        {
            try
            {
                var notification = await _context.Notification.FindAsync(id);
                if (notification == null || notification.IsRead == 1)
                    return Result<bool>.Failure("Notification not found.");


                notification.IsRead = 1;
                _context.Notification.Update(notification);
                await _context.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"An error occurred while deleting the notification: {ex.Message}");
            }
        }

    }
}
