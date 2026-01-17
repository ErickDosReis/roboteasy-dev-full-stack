using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;


namespace ChatApp.Services
{
    public sealed class UserIdProviderService : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
            => connection.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}

