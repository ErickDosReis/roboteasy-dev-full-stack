namespace ChatApp.DTOs
{
    public class ChatMessageDto
    {
        public Guid Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsMine { get; set; }
    }
}
