using ChatApp.Models;

namespace ChatApp.Interfaces
{
    public interface IChatMessageRepository
    {
        Task<bool> ExistsAsync(Guid messageId, CancellationToken ct);
        Task AddAsync(ChatMessage message, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
    }
}
