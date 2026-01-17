using ChatApp.Context;
using ChatApp.DTOs;
using ChatApp.Interfaces;
using ChatApp.Models;
using ChatApp.Repositories;

namespace ChatApp.Services;

public sealed class ChatMessageService(IChatMessageRepository repo) : IChatMessageService
{
    private readonly IChatMessageRepository _repo = repo;

    public async Task PersistMessageAsync(ChatMessageCreatedDto dto, CancellationToken ct)
    {
        if (await _repo.ExistsAsync(dto.MessageId, ct))
            return;

        var entity = new ChatMessage
        {
            Id = dto.MessageId,
            FromUserId = dto.FromUserId,
            FromUserName = dto.FromUserName,
            ToUserId = dto.ToUserId,
            Content = dto.Content,
            SentAtUtc = dto.SentAtUtc
        };

        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);
    }
}
