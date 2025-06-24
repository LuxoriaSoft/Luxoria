using System;
using System.Linq;
using System.Threading.Tasks;
using LuxAPI.DAL;
using LuxAPI.Models;
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

        public AdminController(AppDbContext context)
        {
            _context = context;
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

            // Ici tu mets ta logique de reset du mot de passe
            user.PasswordHash = "newhashedpassword"; // À implémenter
            await _context.SaveChangesAsync();

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

            return Ok(new { message = "User unblocked successfully" });
        }

        //[Authorize(Roles = "Admin")]
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

        //[Authorize(Roles = "Admin")]
        [HttpDelete("collections/{id}")]
        public async Task<IActionResult> DeleteCollection(Guid id)
        {
            var collection = await _context.Collections.FindAsync(id);
            if (collection == null) return NotFound("Collection not found.");
            _context.Collections.Remove(collection);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
