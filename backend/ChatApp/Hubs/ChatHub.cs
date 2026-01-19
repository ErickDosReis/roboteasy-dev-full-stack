using System.Security.Claims;
using ChatApp.DTOs;
using ChatApp.Interfaces;
using ChatApp.Interfaces.RabbitMQ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Hubs;

[Authorize]
public class ChatHub(IUserService presenceTracker,
                     IMessagePublisherService chatPublisher) : Hub
{
    private readonly IUserService _presenceTracker = presenceTracker;
    private readonly IMessagePublisherService _chatPublisher = chatPublisher;

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new HubException("Usuário sem UserId definido.");
        var userName = Context.User?.Identity?.Name ?? "Anonymous";

        await _presenceTracker.UserConnectedAsync(userId, Context.ConnectionId, userName);
        await Clients.All.SendAsync("UserOnline", userId, userName);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != null)
        {
            await _presenceTracker.UserDisconnectedAsync(userId, Context.ConnectionId);
            await Clients.All.SendAsync("UserOffline", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessageToUser(SendMessageDto request)
    {
        var fromUserId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new HubException("Usuário sem UserId definido.");
        var fromUserName = Context.User?.Identity?.Name ?? "Anonymous";

        var sentAt = DateTime.UtcNow;

        // Envia mensagem em tempo real para o usuário de destino
        await Clients.User(request.ToUserId).SendAsync("ReceiveMessage", new ReceiveMessageDto
        {
            FromUserId = fromUserId,
            FromUserName = fromUserName,
            ReceivedMessage = request.SentMessage,
            SentAtUtc = sentAt
        });

        // Evento para o RabbitMQ persistir a mensagem no histórico
        await _chatPublisher.PublishAsync(new ChatMessageCreatedDto
        {
            MessageId = Guid.NewGuid(),
            FromUserId = fromUserId,
            FromUserName = fromUserName,
            ToUserId = request.ToUserId,
            Content = request.SentMessage,
            SentAtUtc = sentAt
        });
    }

    public async Task<IEnumerable<OnlineUserDto>> GetOnlineUsers()
    {
        return await _presenceTracker.GetOnlineUsersAsync();
    }
}
