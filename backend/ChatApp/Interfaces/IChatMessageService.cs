using ChatApp.DTOs;
using ChatApp.Models;

namespace ChatApp.Interfaces
{
    public interface IChatMessageService
    {
        Task PersistMessageAsync(ChatMessageCreatedDto dto, CancellationToken ct);
        Task<IEnumerable<ChatMessage>> GetConversationHistoryAsync(string userA, string userB, CancellationToken ct);
    }
}