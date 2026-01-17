using ChatApp.DTOs;

namespace ChatApp.Interfaces
{
    public interface IChatMessageService
    {
        Task PersistMessageAsync(ChatMessageCreatedDto dto, CancellationToken ct);
    }
}