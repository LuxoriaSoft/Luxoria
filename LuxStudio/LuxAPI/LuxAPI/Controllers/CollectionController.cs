using System;
using System.IO;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LuxAPI.DAL;
using LuxAPI.Models;
using LuxAPI.Models.DTOs;
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
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CollectionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly MinioService _minioService;
        private readonly IHubContext<ChatHub> _chatHub;
        private readonly EmailService _emailService;
        private readonly string _bucketName = "photos-bucket";
        private readonly string _frontEndUrl;
        private readonly string _backEndUrl;

        public CollectionController(AppDbContext context, IConfiguration configuration, MinioService minioService, IHubContext<ChatHub> chatHub, EmailService emailService)
        {
            _context = context;
            _minioService = minioService;
            _chatHub = chatHub;
            _emailService = emailService;
            _frontEndUrl = configuration["URI:FrontEnd"] ?? throw new Exception("Frontend URL is not set.");
            _backEndUrl = configuration["URI:Backend"] ?? throw new Exception("Backend URL is not set.");
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
                    m.PhotoId,
                    AvatarFileName = _context.Users
                        .Where(u => u.Email == m.SenderEmail)
                        .Select(u => u.AvatarFileName ?? "default_avatar.jpg")
                        .FirstOrDefault()
                })
            });

            return Ok(result);
        }



        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCollection(Guid id)
        {
            var currentUserEmail = User?.Identity?.Name;
            if (string.IsNullOrEmpty(currentUserEmail))
                return Unauthorized("Utilisateur non authentifié.");

            var collection = await _context.Collections
                .Include(c => c.Accesses)
                .Include(c => c.ChatMessages)
                .Include(c => c.Photos).ThenInclude(p => p.Comments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (collection == null)
                return NotFound("Collection not found");

            if (!collection.Accesses.Any(a => a.Email.Equals(currentUserEmail, StringComparison.OrdinalIgnoreCase)))
                return Unauthorized();

            var chatMessagesWithAvatars = collection.ChatMessages.Select(m => new
            {
                m.SenderUsername,
                m.SenderEmail,
                m.Message,
                m.SentAt,
                m.PhotoId,
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


        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateCollection([FromBody] CreateCollectionDto dto)
        {
            var currentUserEmail = User?.Identity?.Name;
            if (string.IsNullOrEmpty(currentUserEmail))
                return Unauthorized();

            if (dto == null)
                return BadRequest("Invalid DTO");

            var collection = new Collection
            {
                Name = dto.Name,
                Description = dto.Description
            };

            collection.Accesses.Add(new CollectionAccess
            {
                Email = currentUserEmail,
                Collection = collection
            });

            if (dto.AllowedEmails != null)
            {
                foreach (var email in dto.AllowedEmails)
                {
                    if (!string.Equals(email, currentUserEmail, StringComparison.OrdinalIgnoreCase))
                    {
                        collection.Accesses.Add(new CollectionAccess
                        {
                            Email = email,
                            Collection = collection
                        });
                    }
                }
            }

            _context.Collections.Add(collection);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCollection), new { id = collection.Id }, collection);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCollection(Guid id, [FromBody] UpdateCollectionDto dto)
        {
            // Récupération de l'email utilisateur (depuis le JWT ou HttpContext)
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail))
                return Unauthorized("Utilisateur non authentifié.");

            // Vérifie que la collection existe ET que l'utilisateur a accès
            var collection = await GetCollectionIfUserHasAccessAsync(id, userEmail);
            if (collection == null)
                return Unauthorized("Vous n'avez pas accès à cette collection.");

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

        [HttpPost("{collectionId}/mention-notification")]
        public async Task<IActionResult> SendMentionNotification(Guid collectionId, [FromBody] MentionNotificationDto dto)
        {
            var currentUserEmail = User?.Identity?.Name;
            if (string.IsNullOrEmpty(currentUserEmail))
                return Unauthorized("Utilisateur non authentifié.");

            var collection = await GetCollectionIfUserHasAccessAsync(collectionId, currentUserEmail);
            if (collection == null)
                return Unauthorized("Vous n'avez pas accès à cette collection.");

            // Vérifier que l’email mentionné a bien accès à la collection
            if (!collection.Accesses.Any(a => a.Email.Equals(dto.MentionedEmail, StringComparison.OrdinalIgnoreCase)))
                return BadRequest("L'utilisateur mentionné n'a pas accès à cette collection.");

            // Envoyer le mail de mention
            await _emailService.SendMentionEmailAsync(dto.MentionedEmail, currentUserEmail, dto.Message);

            return Ok(new { message = "Notification envoyée" });
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

            var collection = await GetCollectionIfUserHasAccessAsync(collectionId, currentUserEmail);
            if (collection == null)
                return Unauthorized("Accès refusé à cette collection.");

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

            var collectionUrl = $"{_frontEndUrl}/collections/{collectionId}";
            var registrationUrl = $"{_frontEndUrl}/register?prefilled=true&email={Uri.EscapeDataString(email)}";

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

        [HttpDelete("photo/{id}")]
        public async Task<IActionResult> DeletePhoto(Guid id)
        {
            var currentUserEmail = User?.Identity?.Name;
            if (string.IsNullOrEmpty(currentUserEmail))
                return Unauthorized("Utilisateur non authentifié.");

            var photo = await _context.Photos
                .Include(p => p.Collection)
                .ThenInclude(c => c.Accesses)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (photo == null)
                return NotFound("Image non trouvée.");

            var collection = await GetCollectionIfUserHasAccessAsync(photo.CollectionId, currentUserEmail);
            if (collection == null)
                return Unauthorized("Vous n'avez pas accès à cette collection.");

            var objectName = Path.GetFileName(new Uri(photo.FilePath).AbsolutePath);
            await _minioService.DeleteFileAsync(_bucketName, objectName);

            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("image/{filename}")]
        public async Task<IActionResult> GetImage(string filename)
        {
            var photo = await _context.Photos
                .Include(p => p.Collection)
                .ThenInclude(c => c.Accesses)
                .FirstOrDefaultAsync(p => p.FilePath.EndsWith(filename));

            if (photo == null)
                return NotFound("Fichier introuvable.");

            var currentUserEmail = User?.Identity?.Name;
            if (string.IsNullOrEmpty(currentUserEmail))
                return Unauthorized();

            var collection = await GetCollectionIfUserHasAccessAsync(photo.CollectionId, currentUserEmail);
            if (collection == null)
                return Unauthorized();

            var stream = await _minioService.GetFileAsync(_bucketName, filename);
            if (stream == null)
                return NotFound();

            return File(stream, GetContentType(filename));
        }
        private static string GetContentType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };
        }

        [Authorize]
        [HttpPost("report-user")]
        public async Task<IActionResult> ReportUser([FromBody] UserReportDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid report data");

            var reportingUserEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(reportingUserEmail))
                return Unauthorized("Utilisateur non authentifié.");

            var collection = await GetCollectionIfUserHasAccessAsync(dto.CollectionId, reportingUserEmail);
            if (collection == null)
                return Unauthorized("Vous n'avez pas accès à cette collection.");

            if (!collection.Accesses.Any(a => a.Email.Equals(dto.ReportedUserEmail, StringComparison.OrdinalIgnoreCase)))
                return BadRequest("L'utilisateur signalé n'appartient pas à cette collection");

            var report = new UserReport
            {
                CollectionId = dto.CollectionId,
                ReportedUserEmail = dto.ReportedUserEmail,
                ReportedBy = reportingUserEmail,
                Reason = dto.Reason,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserReports.Add(report);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User report submitted successfully" });
        }

        [Authorize]
        [HttpPost("report")]
        public async Task<IActionResult> ReportCollection([FromBody] CollectionReportDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid report data");

            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail))
                return Unauthorized("Utilisateur non authentifié.");

            var collection = await GetCollectionIfUserHasAccessAsync(dto.CollectionId, userEmail);
            if (collection == null)
                return Unauthorized("Vous n'avez pas accès à cette collection.");

            var report = new CollectionReport
            {
                CollectionId = collection.Id,
                CollectionName = collection.Name,
                ReportedBy = userEmail,
                Reason = dto.Reason,
                CreatedAt = DateTime.UtcNow
            };

            _context.CollectionReports.Add(report);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Report submitted successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCollection(Guid id)
        {
            var currentUserEmail = User?.Identity?.Name;
            if (string.IsNullOrEmpty(currentUserEmail))
                return Unauthorized("Utilisateur non authentifié.");

            // Vérifie si l'utilisateur a accès à la collection
            var collection = await GetCollectionIfUserHasAccessAsync(id, currentUserEmail);
            if (collection == null)
                return Unauthorized("Vous n'avez pas accès à cette collection.");

            _context.Collections.Remove(collection);
            await _context.SaveChangesAsync();
            return Ok();
        }

        public class UploadPhotoDto
        {
            public IFormFile File { get; set; }
            public Guid? PhotoId { get; set; }
        }


        [Authorize]
        [HttpPost("{collectionId}/upload")]
        public async Task<IActionResult> UploadPhoto(Guid collectionId, [FromForm] UploadPhotoDto dto)
        {
            var currentUserEmail = User?.Identity?.Name;
            if (string.IsNullOrEmpty(currentUserEmail))
                return Unauthorized("Utilisateur non authentifié.");

            var collection = await GetCollectionIfUserHasAccessAsync(collectionId, currentUserEmail);
            if (collection == null)
                return Unauthorized();

            Console.WriteLine($"photoId reçue : {dto.PhotoId}");
            var file = dto.File;
            var photoId = dto.PhotoId;

            if (file == null || file.Length == 0)
                return BadRequest("No file has been uploaded");

            var permittedExtensions = new[] { ".png", ".jpg", ".jpeg", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!permittedExtensions.Contains(ext))
                return BadRequest("File extension not supported");

            var objectName = $"{Guid.NewGuid()}{ext}";

            using (var stream = file.OpenReadStream())
            {
                await _minioService.UploadFileAsync(_bucketName, objectName, stream, file.ContentType);
            }

            var fileUrl = $"{_backEndUrl}/api/collection/image/{objectName}";

            if (photoId.HasValue)
            {
                var existingPhoto = await _context.Photos
                    .FirstOrDefaultAsync(p => p.Id == photoId.Value && p.CollectionId == collectionId);

                if (existingPhoto != null)
                {
                    var oldObjectName = Path.GetFileName(new Uri(existingPhoto.FilePath).AbsolutePath);
                    await _minioService.DeleteFileAsync(_bucketName, oldObjectName);

                    existingPhoto.FilePath = fileUrl;
                    existingPhoto.Status = PhotoStatus.Pending;

                    await _context.SaveChangesAsync();

                    return Ok(existingPhoto);
                }
            }

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

        [Authorize]
        [HttpPost("{collectionId}/chat")]
        public async Task<IActionResult> AddChatMessage(Guid collectionId, [FromBody] CreateChatMessageDto dto)
        {
            var currentUserEmail = User?.Identity?.Name;
            if (string.IsNullOrEmpty(currentUserEmail))
                return Unauthorized("Utilisateur non authentifié.");

            var collection = await GetCollectionIfUserHasAccessAsync(collectionId, currentUserEmail);
            if (collection == null)
                return Unauthorized("Accès refusé à cette collection.");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == currentUserEmail);

            var avatarFileName = user?.AvatarFileName ?? "default_avatar.jpg";

            var message = new ChatMessage
            {
                CollectionId = collectionId,
                SenderEmail = currentUserEmail,
                SenderUsername = dto.SenderUsername,
                Message = dto.Message,
                SentAt = DateTime.UtcNow,
                PhotoId = dto.PhotoId
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            await _chatHub.Clients.Group(collectionId.ToString())
                .SendAsync("ReceiveMessage",
                    message.SenderUsername,
                    message.Message,
                    avatarFileName,
                    message.SentAt,
                    message.PhotoId);

            return CreatedAtAction(nameof(GetCollection), new { id = collectionId }, message);
        }

        #region Helper privé
        private async Task<Collection?> GetCollectionIfUserHasAccessAsync(Guid collectionId, string userEmail)
        {
            var collection = await _context.Collections
                .Include(c => c.Accesses)
                .FirstOrDefaultAsync(c => c.Id == collectionId);

            if (collection == null) return null;

            if (!collection.Accesses.Any(a => a.Email.Equals(userEmail, StringComparison.OrdinalIgnoreCase)))
                return null; // Pas d'accès

            return collection;
        }
        #endregion

        [Authorize]
        [HttpPatch("photo/{photoId}/status")]
        public async Task<IActionResult> UpdatePhotoStatus(Guid photoId, [FromBody] UpdatePhotoStatusDto dto)
        {
            var currentUserEmail = User?.Identity?.Name;
            if (string.IsNullOrEmpty(currentUserEmail))
                return Unauthorized("Utilisateur non authentifié.");

            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == photoId);
            if (photo == null)
                return NotFound("Photo introuvable.");

            // 🔒 Vérifie l'accès à la collection
            var collection = await GetCollectionIfUserHasAccessAsync(photo.CollectionId, currentUserEmail);
            if (collection == null)
                return Unauthorized("Accès refusé à cette collection.");

            if (!Enum.IsDefined(typeof(PhotoStatus), dto.Status))
                return BadRequest("Statut invalide.");

            photo.Status = dto.Status;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Statut mis à jour avec succès." });
        }

        [Authorize]
        [HttpGet("photo/{photoId}/status")]
        public async Task<IActionResult> GetPhotoStatus(Guid photoId)
        {
            var currentUserEmail = User?.Identity?.Name;
            if (string.IsNullOrEmpty(currentUserEmail))
                return Unauthorized("Utilisateur non authentifié.");

            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == photoId);
            if (photo == null)
                return NotFound("Photo introuvable.");

            var collection = await GetCollectionIfUserHasAccessAsync(photo.CollectionId, currentUserEmail);
            if (collection == null)
                return Unauthorized("Accès refusé à cette collection.");

            return Ok(new { photoId = photo.Id, status = photo.Status });
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
        public Guid? PhotoId { get; set; }
    }

    public class EmailDto
    {
        public string Email { get; set; }
    }
    
    public class MentionNotificationDto
{
    public string MentionedEmail { get; set; }
    public string SenderEmail { get; set; }
    public string Message { get; set; }
}


    #endregion
}
