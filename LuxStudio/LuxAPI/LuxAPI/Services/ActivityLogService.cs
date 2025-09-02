using LuxAPI.DAL;
using LuxAPI.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace LuxAPI.Services
{
    public class ActivityLogService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ActivityLogService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string action, string details)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userIdStr = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = null;
            if (Guid.TryParse(userIdStr, out var parsed)) userId = parsed;

            // ✅ Récupère le username à partir du claim Name
            var username = httpContext?.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            var performedBy = !string.IsNullOrEmpty(username) ? username : "Unknown";

            var log = new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PerformedBy = performedBy,  
                Action = action,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
