namespace LuxAPI.Models.Messaging
{
    public class AccountCreatedMessage
    {
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
    }
}
