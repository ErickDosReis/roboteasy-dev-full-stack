namespace ChatApp.DTOs
{
    public class SendMessageDto
    {
        public required string ToUserId { get; set; }
        public required string SentMessage { get; set; }
    }
}
