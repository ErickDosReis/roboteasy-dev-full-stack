using ChatApp.DTOs;

namespace ChatApp.Interfaces.RabbitMQ
{
    public interface IMessagePublisherService
    {
        Task PublishAsync(ChatMessageCreatedDto message, CancellationToken ct = default);
    }
}
