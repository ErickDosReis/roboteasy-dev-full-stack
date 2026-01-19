using ChatApp.Context;
using ChatApp.Interfaces;
using ChatApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Repositories
{
    public sealed class ChatMessageRepository(AppDbContext db) : IChatMessageRepository
    {
        private readonly AppDbContext _db = db;

        public Task<bool> ExistsAsync(Guid messageId, CancellationToken ct)
        { 
            return _db.ChatMessages.AnyAsync(x => x.Id == messageId, ct);
        }

        public Task AddAsync(ChatMessage message, CancellationToken ct)
        {
            _db.ChatMessages.Add(message);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken ct)
        {
            _db.SaveChangesAsync(ct);
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<ChatMessage>> GetConversationAsync(string userA, string userB, CancellationToken ct)
        {
            return await _db.ChatMessages
                .Where(m => (m.FromUserId == userA && m.ToUserId == userB) ||
                            (m.FromUserId == userB && m.ToUserId == userA))
                .OrderBy(m => m.SentAtUtc)
                .ToListAsync(ct);
        }
    }
}
