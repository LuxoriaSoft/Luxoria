namespace LuxAPI.Models.DTOs
{
    public class UserReportDto
    {
        public Guid CollectionId { get; set; }
        public string ReportedUserEmail { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
