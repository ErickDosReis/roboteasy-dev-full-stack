using ChatApp.DTOs;

namespace ChatApp.Interfaces
{
    public interface IUserService
    {
        Task UserConnectedAsync(string userId, string connectionId, string userName);
        Task UserDisconnectedAsync(string userId, string connectionId);
        Task<IReadOnlyList<OnlineUserDto>> GetOnlineUsersAsync();
    }

}
