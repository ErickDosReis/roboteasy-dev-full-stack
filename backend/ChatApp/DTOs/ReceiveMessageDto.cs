namespace ChatApp.DTOs
{
    public sealed class ReceiveMessageDto
    {
        public required string FromUserId { get; init; }
        public required string FromUserName { get; init; }
        public required string ReceivedMessage { get; init; }
        public required DateTime SentAtUtc { get; init; }
    }

}
