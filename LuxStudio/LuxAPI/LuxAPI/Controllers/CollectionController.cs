using System;
using System.IO;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LuxAPI.DAL;
using LuxAPI.Models;
using LuxAPI.Hubs;
using LuxAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace LuxAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CollectionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly MinioService _minioService;
        private readonly IHubContext<ChatHub> _chatHub;
        private readonly string _bucketName = "photos-bucket";

        public CollectionController(AppDbContext context, MinioService minioService, IHubContext<ChatHub> chatHub)
        {
            _context = context;
            _minioService = minioService;
            _chatHub = chatHub;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetCollections()
        {
            var currentUserEmail = User?.Identity?.Name;
            if (string.IsNullOrEmpty(currentUserEmail))
                return Unauthorized("Utilisateur non authentifié.");

            var collections = await _context.Collections
                .Include(c => c.Accesses)
                .Include(c => c.Photos)
                    .ThenInclude(p => p.Comments)
                .Include(c => c.ChatMessages)
                .Where(c => c.Accesses.Any(a => a.Email == currentUserEmail))
                .ToListAsync();

            var result = collections.Select(c => new
            {
                c.Id,
                c.Name,
                c.Description,
                AllowedEmails = c.Accesses.Select(a => a.Email),
                Photos = c.Photos.Select(p => new
                {
                    p.Id,
                    p.FilePath,
                    p.Status,
                    Comments = p.Comments.Select(cm => new
                    {
                        cm.Id,
                        cm.CommentText,
                        cm.CreatedAt
                    })
                }),
                ChatMessages = c.ChatMessages.Select(m => new
                {
                    m.SenderUsername,
                    m.SenderEmail,
                    m.Message,
                    m.SentAt,
                    AvatarFileName = _context.Users
                        .Where(u => u.Email == m.SenderEmail)
                        .Select(u => u.AvatarFileName ?? "default_avatar.jpg")
                        .FirstOrDefault()
                })
            });

            return Ok(result);
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetCollection(Guid id)
        {
            var collection = await _context.Collections
                .Include(c => c.Accesses)
                .Include(c => c.ChatMessages)
                .Include(c => c.Photos).ThenInclude(p => p.Comments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (collection == null)
                return NotFound("Collection non trouvée.");

            // Manuellement mapper les messages avec les avatars
            var chatMessagesWithAvatars = collection.ChatMessages.Select(m => new
            {
                m.SenderUsername,
                m.SenderEmail,
                m.Message,
                m.SentAt,
                AvatarFileName = _context.Users
                    .Where(u => u.Email == m.SenderEmail)
                    .Select(u => u.AvatarFileName)
                    .FirstOrDefault()
            });

            var result = new
            {
                collection.Id,
                collection.Name,
                collection.Description,
                AllowedEmails = collection.Accesses.Select(a => a.Email),
                Photos = collection.Photos.Select(p => new
                {
                    p.Id,
                    p.FilePath,
                    p.Status,
                    Comments = p.Comments.Select(c => new
                    {
                        c.Id,
                        c.CommentText,
                        c.CreatedAt
                    })
                }),
                ChatMessages = chatMessagesWithAvatars
            };

            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> CreateCollection([FromBody] CreateCollectionDto dto)
        {
            if (dto == null)
                return BadRequest("Données invalides.");

            var collection = new Collection
            {
                Name = dto.Name,
                Description = dto.Description
            };

            if (dto.AllowedEmails != null)
            {
                foreach (var email in dto.AllowedEmails)
                {
                    collection.Accesses.Add(new CollectionAccess
                    {
                        Email = email,
                        Collection = collection
                    });
                }
            }

            _context.Collections.Add(collection);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCollection), new { id = collection.Id }, collection);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCollection(Guid id, [FromBody] UpdateCollectionDto dto)
        {
            var collection = await _context.Collections.Include(c => c.Accesses).FirstOrDefaultAsync(c => c.Id == id);
            if (collection == null)
                return NotFound("Collection non trouvée.");

            collection.Name = dto.Name;
            collection.Description = dto.Description;

            collection.Accesses.Clear();
            if (dto.AllowedEmails != null)
            {
                foreach (var email in dto.AllowedEmails)
                {
                    collection.Accesses.Add(new CollectionAccess
                    {
                        Email = email,
                        Collection = collection
                    });
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{collectionId}/allowedEmails")]
        [Authorize]
        public async Task<IActionResult> AddAllowedEmail(
            Guid collectionId,
            [FromBody] EmailDto dto,
            [FromServices] EmailService emailService)
        {
            var currentUserEmail = User?.Identity?.Name;
            if (string.IsNullOrEmpty(currentUserEmail))
                return Unauthorized("Utilisateur non authentifié.");

            var collection = await _context.Collections
                .Include(c => c.Accesses)
                .FirstOrDefaultAsync(c => c.Id == collectionId);

            if (collection == null)
                return NotFound("Collection non trouvée.");

            if (!collection.Accesses.Any(a => a.Email.Equals(currentUserEmail, StringComparison.OrdinalIgnoreCase)))
                return Forbid("Vous n'avez pas accès à cette collection.");

            if (!new EmailAddressAttribute().IsValid(dto.Email))
                return BadRequest("Format d'email invalide.");

            var email = dto.Email.Trim().ToLowerInvariant();

            if (collection.Accesses.Any(a => a.Email == email))
            {
                return Ok(new { message = "Cet utilisateur a déjà accès à la collection." });
            }

            collection.Accesses.Add(new CollectionAccess
            {
                Email = email,
                CollectionId = collectionId
            });
            await _context.SaveChangesAsync();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            var collectionUrl = $"http://localhost:5173/collections/{collectionId}";
            var registrationUrl = $"http://localhost:5173/register?prefilled=true&email={Uri.EscapeDataString(email)}";

            if (user != null)
            {
                await emailService.SendAccessGrantedNotificationAsync(email, collection.Name, collectionUrl, currentUserEmail);
                return Ok(new { message = "Utilisateur existant ajouté et notifié." });
            }
            else
            {
                await emailService.SendRegistrationInvitationAsync(email, collection.Name, registrationUrl, currentUserEmail);
                return Ok(new { message = "Utilisateur inconnu ajouté et invitation envoyée.", redirectUrl = registrationUrl });
            }
        }

        [Authorize]
        [HttpDelete("photo/{id}")]
        public async Task<IActionResult> DeletePhoto(Guid id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);
            if (photo == null)
                return NotFound("Image non trouvée.");

            var objectName = Path.GetFileName(new Uri(photo.FilePath).AbsolutePath);
            await _minioService.DeleteFileAsync(_bucketName, objectName);

            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("image/{filename}")]
        public async Task<IActionResult> GetImage(string filename)
        {
            var stream = await _minioService.GetFileAsync(_bucketName, filename);
            if (stream == null) return NotFound("Fichier introuvable.");
            return File(stream, GetContentType(filename));
        }

        private static string GetContentType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCollection(Guid id)
        {
            var collection = await _context.Collections.FindAsync(id);
            if (collection == null)
                return NotFound("Collection non trouvée.");

            _context.Collections.Remove(collection);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{collectionId}/upload")]
        public async Task<IActionResult> UploadPhoto(Guid collectionId, [FromForm] IFormFile file)
        {
            var collection = await _context.Collections.FindAsync(collectionId);
            if (collection == null)
                return NotFound("Collection non trouvée.");

            if (file == null || file.Length == 0)
                return BadRequest("Aucun fichier sélectionné.");

            var permittedExtensions = new[] { ".png", ".jpg", ".jpeg" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!permittedExtensions.Contains(ext))
                return BadRequest("Type de fichier non supporté.");

            var objectName = $"{Guid.NewGuid()}{ext}";

            using (var stream = file.OpenReadStream())
            {
                await _minioService.UploadFileAsync(_bucketName, objectName, stream, file.ContentType);
            }

            var fileUrl = $"http://localhost:5269/api/collection/image/{objectName}";

            var photo = new Photo
            {
                CollectionId = collectionId,
                FilePath = fileUrl,
                Status = PhotoStatus.Pending
            };

            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCollection), new { id = collectionId }, photo);
        }

        [HttpPost("{collectionId}/chat")]
        public async Task<IActionResult> AddChatMessage(Guid collectionId, [FromBody] CreateChatMessageDto dto)
        {
            var collection = await _context.Collections.FindAsync(collectionId);
            if (collection == null)
                return NotFound("Collection non trouvée.");

            // Récupère l'utilisateur pour son avatar
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.SenderEmail);
            var avatarFileName = user?.AvatarFileName ?? "default_avatar.jpg";

            var message = new ChatMessage
            {
                CollectionId = collectionId,
                SenderEmail = dto.SenderEmail,
                SenderUsername = dto.SenderUsername,
                Message = dto.Message,
                SentAt = DateTime.UtcNow
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            // ✅ Envoie le message avec avatar et date
            await _chatHub.Clients.Group(collectionId.ToString())
                .SendAsync("ReceiveMessage", dto.SenderUsername, dto.Message, avatarFileName, message.SentAt);

            return CreatedAtAction(nameof(GetCollection), new { id = collectionId }, message);
        }
        
        [HttpPatch("photo/{photoId}/status")]
        [Authorize]
        public async Task<IActionResult> UpdatePhotoStatus(Guid photoId, [FromBody] UpdatePhotoStatusDto dto)
        {
            var photo = await _context.Photos.FindAsync(photoId);
            if (photo == null) return NotFound("Photo introuvable.");

            if (!Enum.IsDefined(typeof(PhotoStatus), dto.Status))
                return BadRequest("Statut invalide.");

            photo.Status = dto.Status;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Statut de la photo mis à jour." });
        }

        public class UpdatePhotoStatusDto
        {
            public PhotoStatus Status { get; set; }
        }

    }

    #region DTOs

    public class CreateCollectionDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> AllowedEmails { get; set; }
    }

    public class UpdateCollectionDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<string>? AllowedEmails { get; set; }
    }

    public class CreateChatMessageDto
    {
        public string SenderEmail { get; set; }
        public string SenderUsername { get; set; }
        public string Message { get; set; }
    }

    public class EmailDto
    {
        public string Email { get; set; }
    }

    #endregion
}
