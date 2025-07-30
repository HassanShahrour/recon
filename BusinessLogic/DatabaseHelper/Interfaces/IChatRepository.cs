using Reconova.BusinessLogic.Exceptions;
using Reconova.Data.Models;

namespace Reconova.BusinessLogic.DatabaseHelper.Interfaces
{
    public interface IChatRepository
    {
        Task<Result<List<User>>> GetUsers();

        Task<Result<bool>> SaveMessage(ChatMessage chatMessage);

        Task<Result<List<ChatMessage>>> GetChatHistory(string user1Id, string user2Id);

        Task<Result<bool>> DeleteMessage(string messageId);

        Task<int> GetUnreadMessageCount(string otherUserId);

        Task<Result<bool>> MarkMessagesAsRead(string senderId, string receiverId);
    }
}
