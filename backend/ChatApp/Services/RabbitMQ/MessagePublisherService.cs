using System.Text;
using System.Text.Json;
using ChatApp.DTOs;
using ChatApp.Interfaces.RabbitMQ;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
namespace ChatApp.Services.RabbitMQ
{
    public sealed class MessagePublisherService : IMessagePublisherService, IDisposable
    {
        private readonly RabbitMqOptions _opt;
        private readonly IConnection _conn;
        private readonly IModel _ch;
        private readonly ILogger _logger;

        public MessagePublisherService(ILogger<MessagePublisherService> logger, IOptions<RabbitMqOptions> options)
        {
            _logger = logger;

            _opt = options.Value;

            var factory = new ConnectionFactory
            {
                HostName = _opt.Host,
                Port = _opt.Port,
                UserName = _opt.UserName,
                Password = _opt.Password,
                DispatchConsumersAsync = true
            };

            _conn = factory.CreateConnection();
            _ch = _conn.CreateModel();

            _ch.ExchangeDeclare(_opt.Exchange, ExchangeType.Direct, durable: true);
            _ch.QueueDeclare(_opt.Queue, durable: true, exclusive: false, autoDelete: false);
            _ch.QueueBind(_opt.Queue, _opt.Exchange, _opt.RoutingKey);
        }

        public Task PublishAsync(ChatMessageCreatedDto message, CancellationToken ct = default)
        {
            try
            {
                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                var props = _ch.CreateBasicProperties();
                props.ContentType = "application/json";
                props.DeliveryMode = 2; // persistente

                _ch.BasicPublish(
                    exchange: _opt.Exchange,
                    routingKey: _opt.RoutingKey,
                    basicProperties: props,
                    body: body
                );

            }
            catch(Exception e)
            {
                _logger.LogError($"Erro ao publicar mensagem no RabbitMQ: {e}");
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            try { _ch?.Close(); } catch { /* ignore */ }
            try { _conn?.Close(); } catch { /* ignore */ }
        }
    }
}