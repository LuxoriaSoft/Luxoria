namespace LuxAPI.Models.DTOs
{
    public class EmailCodeVerificationDto
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
