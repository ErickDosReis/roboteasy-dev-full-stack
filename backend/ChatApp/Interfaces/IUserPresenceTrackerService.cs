using ChatApp.DTOs;

namespace ChatApp.Interfaces
{
    public interface IUserPresenceTrackerService
    {
        Task UserConnectedAsync(string userId, string connectionId, string userName);
        Task UserDisconnectedAsync(string userId, string connectionId);
        Task<IReadOnlyList<OnlineUserDto>> GetOnlineUsersAsync();
    }

}
