namespace ChatApp.Services.RabbitMQ
{
    public sealed class RabbitMqOptions
    {
        public const string SectionName = "RabbitMQ";

        public required string Host { get; init; }
        public int Port { get; init; } = 5672;
        public string UserName { get; init; } = "guest";
        public string Password { get; init; } = "guest";

        public string Exchange { get; init; } = "chat.exchange";
        public string Queue { get; init; } = "chat.messages.created";
        public string RoutingKey { get; init; } = "chat.message.created";
    }
}
