using System;
using System.IO;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LuxAPI.DAL;
using LuxAPI.Models;
using LuxAPI.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace LuxAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CollectionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMinioClient _minioClient;
        private readonly string _bucketName = "photos-bucket";
        private readonly string _minioEndpoint = string.Empty;

        private readonly IHubContext<ChatHub> _chatHub;

        public CollectionController(AppDbContext context, IConfiguration configuration, IHubContext<ChatHub> chatHub)
        {
            _context = context;
            _chatHub = chatHub;
            // Init MinIO client from config (compatible with MinIO v6.0.4)
            _minioClient = new MinioClient()
                                .WithEndpoint(configuration["Minio:Endpoint"])
                                .WithCredentials(configuration["Minio:AccessKey"], configuration["Minio:SecretKey"])
                                .Build();
            _minioEndpoint = configuration["Minio:Endpoint"] ?? string.Empty;
        }

        // GET: api/collection
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetCollections()
        {

            var currentUserEmail = User?.Identity?.Name;
            if (string.IsNullOrEmpty(currentUserEmail))
                return Unauthorized("Utilisateur non authentifié.");

            var collections = await _context.Collections
                .Include(c => c.AllowedEmails)
                .Include(c => c.ChatMessages)
                .Include(c => c.Photos)
                    .ThenInclude(p => p.Comments)
                .Where(c => c.AllowedEmails.Any(a => a.Email == currentUserEmail))
                .ToListAsync();

            return Ok(collections);
        }

        // GET: api/collection/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCollection(Guid id)
        {
            var collection = await _context.Collections
                .Include(c => c.AllowedEmails)
                .Include(c => c.ChatMessages)
                .Include(c => c.Photos)
                    .ThenInclude(p => p.Comments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (collection == null)
                return NotFound("Collection non trouvée.");

            return Ok(collection);
        }

        // POST: api/collection
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
                    collection.AllowedEmails.Add(new CollectionAccess
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

        // PUT: api/collection/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCollection(Guid id, [FromBody] UpdateCollectionDto dto)
        {
            var collection = await _context.Collections
                .Include(c => c.AllowedEmails)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (collection == null)
                return NotFound("Collection non trouvée.");

            collection.Name = dto.Name;
            collection.Description = dto.Description;

            collection.AllowedEmails.Clear();
            if (dto.AllowedEmails != null)
            {
                foreach (var email in dto.AllowedEmails)
                {
                    collection.AllowedEmails.Add(new CollectionAccess
                    {
                        Email = email,
                        Collection = collection
                    });
                }
            }

            _context.Collections.Update(collection);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/collection/{collectionId}/allowedEmails
        [HttpPatch("{collectionId}/allowedEmails")]
        public async Task<IActionResult> AddAllowedEmail(Guid collectionId, [FromBody] EmailDto dto)
        {
            var currentUserEmail = User?.Identity?.Name;
            if (string.IsNullOrEmpty(currentUserEmail))
                return Unauthorized("Utilisateur non authentifié.");

            var collection = await _context.Collections
                .Include(c => c.AllowedEmails)
                .FirstOrDefaultAsync(c => c.Id == collectionId);

            if (collection == null)
                return NotFound("Collection non trouvée.");

            if (!collection.AllowedEmails.Any(a => a.Email == currentUserEmail))
                return Forbid("Vous n'avez pas accès à cette collection.");

            if (!new EmailAddressAttribute().IsValid(dto.Email))
                return BadRequest("Format d'email invalide.");

            if (collection.AllowedEmails.Any(a => a.Email.Equals(dto.Email, StringComparison.OrdinalIgnoreCase)))
                return BadRequest("Cet email est déjà autorisé.");

            collection.AllowedEmails.Add(new CollectionAccess
            {
                Email = dto.Email,
                CollectionId = collectionId
            });

            await _context.SaveChangesAsync();

            return Ok(collection);
        }



                // DELETE: api/photo/{id}
        [Authorize]
        [HttpDelete("photo/{id}")]
        public async Task<IActionResult> DeletePhoto(Guid id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);
            if (photo == null)
                return NotFound("Image non trouvée.");

            // Extraire l’objet MinIO depuis l’URL
            var objectName = Path.GetFileName(new Uri(photo.FilePath).AbsolutePath);

            // Supprimer dans MinIO
            await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName));

            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("image/{filename}")]
        public async Task<IActionResult> GetImage(string filename)
        {
            try
            {
                var memoryStream = new MemoryStream();

                var getObjectArgs = new GetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(filename);

                await _minioClient.GetObjectAsync(getObjectArgs.WithCallbackStream(async stream =>
                {
                    await stream.CopyToAsync(memoryStream);
                }));

                memoryStream.Position = 0;

                var contentType = GetContentType(filename);
                return File(memoryStream, contentType);
            }
            catch (Minio.Exceptions.MinioException ex)
            {
                return NotFound($"Erreur MinIO : {ex.Message}");
            }
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


        // DELETE: api/collection/{id}
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

        // POST: api/collection/{collectionId}/upload
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
            if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
                return BadRequest("Type de fichier non supporté. Seuls les fichiers PNG et JPG sont autorisés.");

            var objectName = $"{Guid.NewGuid()}{ext}";

            bool bucketExists = await _minioClient.BucketExistsAsync(new Minio.DataModel.Args.BucketExistsArgs().WithBucket(_bucketName));
            if (!bucketExists)
            {
                await _minioClient.MakeBucketAsync(new Minio.DataModel.Args.MakeBucketArgs().WithBucket(_bucketName));
            }

            using (var stream = file.OpenReadStream())
            {
                var putObjectArgs = new Minio.DataModel.Args.PutObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(objectName)
                    .WithStreamData(stream)
                    .WithObjectSize(file.Length)
                    .WithContentType(file.ContentType);
                await _minioClient.PutObjectAsync(putObjectArgs);
            }

            //var fileUrl = $"https://{_minioEndpoint}/{_bucketName}/{objectName}";
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

        // POST: api/collection/{collectionId}/chat
        [HttpPost("{collectionId}/chat")]
        public async Task<IActionResult> AddChatMessage(Guid collectionId, [FromBody] CreateChatMessageDto dto)
        {
            var collection = await _context.Collections.FindAsync(collectionId);
            if (collection == null)
                return NotFound("Collection non trouvée.");

            var message = new ChatMessage
            {
                CollectionId = collectionId,
                SenderEmail = dto.SenderEmail,
                Message = dto.Message,
                SentAt = DateTime.UtcNow,
                SenderUsername = dto.SenderUsername
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            await _chatHub.Clients.Group(collectionId.ToString())
                .SendAsync("ReceiveMessage", dto.SenderUsername, dto.Message);

            return CreatedAtAction(nameof(GetCollection), new { id = collectionId }, message);
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
