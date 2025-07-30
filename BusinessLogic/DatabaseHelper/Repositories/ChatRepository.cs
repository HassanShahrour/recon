using Microsoft.EntityFrameworkCore;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.BusinessLogic.Exceptions;
using Reconova.Core.Utilities;
using Reconova.Data;
using Reconova.Data.Models;

namespace Reconova.BusinessLogic.DatabaseHelper.Repositories
{
    public class ChatRepository : IChatRepository
    {

        private readonly ReconovaDbContext _context;
        private readonly UserUtility _userUtility;

        public ChatRepository(ReconovaDbContext context, UserUtility userUtility)
        {
            _context = context;
            _userUtility = userUtility;
        }

        public async Task<Result<List<User>>> GetUsers()
        {
            try
            {
                var loggedInUserId = await _userUtility.GetLoggedInUserId();

                var users = await _context.Users
                     .Include(u => u.ScanResults)
                     .Include(u => u.AIResults)
                     .Where(u => u.IsDeleted == 0 && u.Id != loggedInUserId.ToString())
                     .ToListAsync();

                if (users is null || !users.Any())
                {
                    return Result<List<User>>.Failure("No users found.");
                }

                return Result<List<User>>.Success(users);
            }
            catch (InvalidDataException ex)
            {
                return Result<List<User>>.Failure($"Invalid data: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching users: " + ex.Message);
                return Result<List<User>>.Failure("An error occurred while fetching users.");
            }
        }

        public async Task<Result<bool>> SaveMessage(ChatMessage chatMessage)
        {
            try
            {
                _context.ChatMessage.Add(chatMessage);
                await _context.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Failed to save message: {ex.Message}");
            }
        }

        public async Task<Result<List<ChatMessage>>> GetChatHistory(string user1Id, string user2Id)
        {
            try
            {
                var chatHistory = await _context.ChatMessage
                    .AsNoTracking()
                    .Where(m =>
                        ((m.SenderId == user1Id && m.RecipientId == user2Id) ||
                        (m.SenderId == user2Id && m.RecipientId == user1Id)) &&
                        m.IsDeleted == 0)
                    .OrderBy(m => m.Time)
                    .ToListAsync();

                if (chatHistory == null || !chatHistory.Any())
                {
                    return Result<List<ChatMessage>>.Failure("No Messages Found.");
                }

                return Result<List<ChatMessage>>.Success(chatHistory);
            }
            catch (Exception ex)
            {
                return Result<List<ChatMessage>>.Failure($"Failed to retrieve chat history: {ex.Message}");
            }
        }


        public async Task<Result<bool>> DeleteMessage(string messageId)
        {
            try
            {
                var message = await _context.ChatMessage.FindAsync(messageId);

                if (message == null)
                {
                    return Result<bool>.Failure("Message not found.");
                }

                message.IsDeleted = 1;
                _context.ChatMessage.Update(message);
                var rowsAffected = await _context.SaveChangesAsync();
                return Result<bool>.Success(rowsAffected > 0);
            }
            catch (InvalidDataException ex)
            {
                return Result<bool>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error marking message as deleted: " + ex.Message);
                return Result<bool>.Failure("An error occurred while marking the message as deleted.");
            }

        }


        public async Task<Result<bool>> MarkMessagesAsRead(string senderId, string receiverId)
        {

            var unreadMessages = await _context.ChatMessage
            .Where(msg => msg.SenderId == senderId && msg.RecipientId == receiverId && msg.IsRead == 0)
            .ToListAsync();

            if (unreadMessages.Count != 0)
            {
                foreach (var message in unreadMessages)
                {
                    message.IsRead = 1;
                }

                await _context.SaveChangesAsync();
            }

            return Result<bool>.Success(true);
        }

        public async Task<int> GetUnreadMessageCount(string otherUserId)
        {
            var LoggedInUserId = await _userUtility.GetLoggedInUserId();

            return await _context.ChatMessage
                .Where(m => m.RecipientId == LoggedInUserId.ToString() && m.SenderId == otherUserId && m.IsRead == 0)
                .CountAsync();
        }

    }
}
