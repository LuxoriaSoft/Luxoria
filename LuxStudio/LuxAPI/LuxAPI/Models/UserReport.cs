using System;

namespace LuxAPI.Models
{
    public class UserReport
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CollectionId { get; set; }
        public string ReportedUserEmail { get; set; } = string.Empty;
        public string ReportedBy { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
