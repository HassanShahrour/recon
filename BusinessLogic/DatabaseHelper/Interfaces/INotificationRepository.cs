using Reconova.BusinessLogic.Exceptions;
using Reconova.Data.Models;

namespace Reconova.BusinessLogic.DatabaseHelper.Interfaces
{
    public interface INotificationRepository
    {
        public Task<Result<List<Notification>>> GetAllNotifications();

        public Task<Result<bool>> MarkNotificationAsRead(int id);
    }
}
