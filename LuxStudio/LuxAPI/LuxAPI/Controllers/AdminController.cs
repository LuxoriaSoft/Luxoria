using System;
using System.Linq;
using System.Threading.Tasks;
using LuxAPI.DAL;
using LuxAPI.Models;
using LuxAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LuxAPI.Controllers
{
    [Authorize] // Veiller à restreindre à l'Admin uniquement
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly ILogger<AdminController> _logger;
        private readonly ActivityLogService _activityLogService;
        public AdminController(AppDbContext context, EmailService emailService, ILogger<AdminController> logger, ActivityLogService activityLogService)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
            _activityLogService = activityLogService;
        }

        // GET /api/admin/users?search=
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] string? search)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(u =>
                    u.Email.ToLower().Contains(search) ||
                    u.Username.ToLower().Contains(search));
            }

            var users = await query
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.Role,
                    IsBlocked = EF.Property<bool>(u, "IsBlocked") // Si propriété "IsBlocked"
                })
                .ToListAsync();

            return Ok(users);
        }



        // POST /api/admin/users/reset-password/{userId}
        [HttpPost("users/reset-password/{userId}")]
        public async Task<IActionResult> ResetPassword(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found");

            user.PasswordHash = "newhashedpassword"; // À implémenter
            await _context.SaveChangesAsync();

            await _activityLogService.LogAsync(
                "Reset Password",
                $"Password reset for user '{user.Username}' (ID: {user.Id})"
            );

            return Ok(new { message = "Password reset successfully" });
        }

        // POST /api/admin/users/block/{userId}
        [HttpPost("users/block/{userId}")]
        public async Task<IActionResult> BlockUser(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found");

            user.IsBlocked = true;
            await _context.SaveChangesAsync();

            await _activityLogService.LogAsync(
                "Block User",
                $"User '{user.Username}' (ID: {user.Id}) was blocked."
            );

            return Ok(new { message = "User blocked successfully" });
        }

        // POST /api/admin/users/unblock/{userId}
        [HttpPost("users/unblock/{userId}")]
        public async Task<IActionResult> UnblockUser(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found");

            user.IsBlocked = false;
            await _context.SaveChangesAsync();

            await _activityLogService.LogAsync(
                "Unblock User",
                $"User '{user.Username}' (ID: {user.Id}) was unblocked."
            );

            return Ok(new { message = "User unblocked successfully" });
        }

        // POST /api/admin/users/invite
        [HttpPost("users/invite")]
        public async Task<IActionResult> InviteUser([FromBody] InviteUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid invitation data.");
            }

            var inviteToken = Guid.NewGuid().ToString("N");
            var registrationUrl = $"{_emailService.FrontEndUrl}/register?inviteToken={inviteToken}";

            await _emailService.SendRegistrationInvitationAsync(
                dto.Email,
                "Luxoria",
                registrationUrl,
                User.Identity?.Name ?? "Admin Luxoria"
            );

            await _activityLogService.LogAsync(
                "Invite User",
                $"Invitation sent to '{dto.Email}' with role {(dto.Role == 0 ? "Client" : "Photographer")}."
            );

            _logger.LogInformation("Invitation sent to {Email}", dto.Email);

            return Ok(new { message = "Invitation sent successfully" });
        }

        // GET /api/admin/collections
        [HttpGet("collections")]
        public async Task<IActionResult> GetCollections([FromQuery] string? search = null)
        {
            var collectionsQuery = _context.Collections
                .Include(c => c.Accesses)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                collectionsQuery = collectionsQuery.Where(c =>
                    c.Name.Contains(search) ||
                    c.Accesses.Any(a => a.Email.Contains(search)) ||
                    c.Id.ToString().Contains(search));
            }

            var results = await collectionsQuery.ToListAsync();

            return Ok(results.Select(c => new {
                c.Id,
                c.Name,
                AllowedEmails = c.Accesses.Select(a => a.Email).ToList()
            }));
        }

        // GET /api/admin/reports/collections
        [HttpGet("reports/collections")]
        public async Task<IActionResult> GetCollectionReports()
        {
            var reports = await _context.CollectionReports
                .OrderByDescending(r => r.CreatedAt)
                .Take(200)
                .Select(r => new
                {
                    r.Id,
                    r.CollectionId,
                    r.CollectionName,
                    r.ReportedBy,
                    r.Reason,
                    r.CreatedAt
                })
                .ToListAsync();

            return Ok(reports);
        }

        // GET /api/admin/reports/users
        [HttpGet("reports/users")]
        public async Task<IActionResult> GetUserReports()
        {
            var reports = await _context.UserReports
                .OrderByDescending(r => r.CreatedAt)
                .Take(200) // Limite à 200 pour éviter surcharge
                .Select(r => new
                {
                    r.Id,
                    r.CollectionId,
                    CollectionName = _context.Collections
                        .Where(c => c.Id == r.CollectionId)
                        .Select(c => c.Name)
                        .FirstOrDefault(),
                    r.ReportedUserEmail,
                    r.ReportedBy,
                    r.Reason,
                    r.CreatedAt
                })
                .ToListAsync();

            return Ok(reports);
        }

        // DELETE /api/admin/reports/users/{reportId}
        [HttpDelete("reports/users/{reportId}")]
        public async Task<IActionResult> DeleteUserReport(Guid reportId)
        {
            var report = await _context.UserReports.FindAsync(reportId);
            if (report == null)
                return NotFound("User report not found.");

            _context.UserReports.Remove(report);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE /api/admin/collections/{id}
        [HttpDelete("collections/{id}")]
        public async Task<IActionResult> DeleteCollection(Guid id)
        {
            var collection = await _context.Collections.FindAsync(id);
            if (collection == null) return NotFound("Collection not found.");

            _context.Collections.Remove(collection);
            await _context.SaveChangesAsync();

            await _activityLogService.LogAsync(
                "Delete Collection",
                $"Collection '{collection.Name}' (ID: {collection.Id}) deleted."
            );

            return NoContent();
        }
        // GET /api/admin/logs
        [HttpGet("logs")]
        public async Task<IActionResult> GetActivityLogs()
        {
            var logs = await _context.ActivityLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(200) // Limite à 200 derniers logs pour éviter surcharge
                .Select(l => new
                {
                    l.Id,
                    l.Action,
                    l.PerformedBy,
                    l.Details,
                    l.Timestamp
                })
                .ToListAsync();

            return Ok(logs);
        }
    }
}
