using Microsoft.AspNetCore.SignalR;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.Core.Utilities;
using Reconova.Data;
using Reconova.Data.Models;

namespace Reconova.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatRepository _chatRepository;
        private readonly UserUtility _userUtility;
        private readonly ReconovaDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly IHubContext<NotificationHub> _hubContext;

        private static readonly HashSet<string> OnlineUserIds = new();

        public ChatHub(IChatRepository chatRepository, UserUtility userUtility, ReconovaDbContext context, IUserRepository userRepository, IHubContext<NotificationHub> notificationHubContext)
        {
            _chatRepository = chatRepository;
            _userUtility = userUtility;
            _context = context;
            _userRepository = userRepository;
            _hubContext = notificationHubContext;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = await _userUtility.GetLoggedInUserId();
            if (!string.IsNullOrEmpty(userId.ToString()))
            {
                OnlineUserIds.Add(userId.ToString());
                await Clients.All.SendAsync("UserOnline", userId.ToString());
            }
            await base.OnConnectedAsync();
        }


        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = await _userUtility.GetLoggedInUserId();
            if (!string.IsNullOrEmpty(userId.ToString()))
            {
                OnlineUserIds.Remove(userId.ToString());
                await Clients.All.SendAsync("UserOffline", userId.ToString());
            }
            await base.OnDisconnectedAsync(exception);
        }

        public Task<List<string>> GetOnlineUsers()
        {
            return Task.FromResult(OnlineUserIds.ToList());
        }

        public async Task<string> SendMessageToUser(string recipientUserId, string senderUserId, string message)
        {
            if (string.IsNullOrWhiteSpace(recipientUserId) || string.IsNullOrWhiteSpace(senderUserId))
                throw new HubException("Sender or recipient ID cannot be empty.");

            if (string.IsNullOrWhiteSpace(message))
                throw new HubException("Message cannot be empty.");

            var chatMessage = new ChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                SenderId = senderUserId,
                RecipientId = recipientUserId,
                Message = message,
                Time = DateTime.UtcNow
            };

            try
            {
                var saveResult = await _chatRepository.SaveMessage(chatMessage);
                if (!saveResult.IsSuccess)
                    throw new HubException($"Failed to save the message: {saveResult.Error}");

                await Clients.User(recipientUserId).SendAsync("ReceiveMessage", senderUserId, message, chatMessage.Id);

                var sender = await _userRepository.GetUserById(senderUserId);
                var notification = new Notification
                {
                    SenderId = senderUserId,
                    ReceiverId = recipientUserId,
                    Type = "Message",
                    Message = $"{sender.Value.FirstName} sent you a message.",
                    CreatedDate = DateTime.UtcNow,
                    IsRead = 0
                };

                await _context.Notification.AddAsync(notification);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.User(recipientUserId).SendAsync("ReceiveNotification", new
                {
                    Type = "Message",
                    Message = notification.Message,
                    SenderName = sender.Value.FirstName
                });

                return chatMessage.Id;
            }
            catch (Exception ex)
            {
                throw new HubException($"Error sending message: {ex.InnerException?.Message ?? ex.Message}");
            }
        }


        public async Task<List<ChatMessage>> GetChatHistory(string user1Id, string user2Id)
        {
            try
            {
                var historyResult = await _chatRepository.GetChatHistory(user1Id, user2Id);

                if (!historyResult.IsSuccess)
                    throw new HubException($"Failed to retrieve chat history: {historyResult.Error}");

                return historyResult.Value;
            }
            catch (Exception ex)
            {
                throw new HubException($"Error retrieving chat history: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        public async Task DeleteMessage(string messageId, string senderId, string receiverId)
        {
            try
            {
                var deleteResult = await _chatRepository.DeleteMessage(messageId);

                if (!deleteResult.IsSuccess)
                    throw new HubException($"Failed to delete message: {deleteResult.Error}");

                await Clients.User(senderId).SendAsync("MessageDeleted", messageId);
                await Clients.User(receiverId).SendAsync("MessageDeleted", messageId);
            }
            catch (Exception ex)
            {
                throw new HubException($"Error deleting message: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        public async Task MarkMessageAsRead(string senderId, string receiverId)
        {
            try
            {
                var readResult = await _chatRepository.MarkMessagesAsRead(senderId, receiverId);

                if (!readResult.IsSuccess)
                    throw new HubException($"Failed to mark messages as read: {readResult.Error}");

                await Clients.User(receiverId).SendAsync("MessageRead");
            }
            catch (Exception ex)
            {
                throw new HubException($"Error marking messages as read: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        public async Task UserTyping(string senderId, string recipientId)
        {
            if (string.IsNullOrWhiteSpace(senderId) || string.IsNullOrWhiteSpace(recipientId))
                return;

            try
            {
                await Clients.User(recipientId).SendAsync("UserTyping", senderId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
