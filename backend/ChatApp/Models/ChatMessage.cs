namespace ChatApp.Models
{
    public sealed class ChatMessage
    {
        public Guid Id { get; set; }
        public required string FromUserId { get; set; }
        public required string FromUserName { get; set; }
        public required string ToUserId { get; set; }
        public required string Content { get; set; }
        public DateTime SentAtUtc { get; set; }
    }
}
