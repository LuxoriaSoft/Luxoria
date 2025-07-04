namespace LuxStudio.COM.Models;

public class ChatMessage
{
    public string SenderEmail { get; set; } = string.Empty;
    public required string SenderUsername { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public string? AvatarFileName { get; set; }
}