namespace ChatApp.DTOs
{
    public sealed record ChatMessageCreatedDto
    {
        public required Guid MessageId { get; init; }
        public required string FromUserId { get; init; }
        public required string FromUserName { get; init; }
        public required string ToUserId { get; init; }
        public required string Content { get; init; }
        public required DateTime SentAtUtc { get; init; }
    }
}
