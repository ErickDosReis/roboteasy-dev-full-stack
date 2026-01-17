using System.Collections.Concurrent;
using ChatApp.DTOs;
using ChatApp.Interfaces;

namespace ChatApp.Services
{
    public sealed class UserPresenceTrackerService : IUserPresenceTrackerService
    {
        private sealed class UserConnections
        {
            public string UserName { get; set; } = "";
            public HashSet<string> ConnectionIds { get; } = [];
        }

        private readonly ConcurrentDictionary<string, UserConnections> _users = new();

        public Task UserConnectedAsync(string userId, string connectionId, string userName)
        {
            var entry = _users.GetOrAdd(userId, _ => new UserConnections());

            lock (entry)
            {
                entry.UserName = userName;
                entry.ConnectionIds.Add(connectionId);
            }

            return Task.CompletedTask;
        }

        public Task UserDisconnectedAsync(string userId, string connectionId)
        {
            if (!_users.TryGetValue(userId, out var entry))
                return Task.CompletedTask;

            var removeUser = false;

            lock (entry)
            {
                entry.ConnectionIds.Remove(connectionId);
                removeUser = entry.ConnectionIds.Count == 0;
            }

            if (removeUser)
                _users.TryRemove(userId, out _);

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<OnlineUserDto>> GetOnlineUsersAsync()
        {
            var list = _users.Select(record => new OnlineUserDto(record.Key, record.Value.UserName))
                             .OrderBy(x => x.UserName)
                             .ToList()
                             .AsReadOnly();

            return Task.FromResult<IReadOnlyList<OnlineUserDto>>(list);
        }
    }
}