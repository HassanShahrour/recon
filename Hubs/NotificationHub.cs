using Microsoft.AspNetCore.SignalR;

namespace Reconova.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendNotification(string receiverId, object notification)
        {
            await Clients.User(receiverId).SendAsync("ReceiveNotification", notification);
        }
    }

}
