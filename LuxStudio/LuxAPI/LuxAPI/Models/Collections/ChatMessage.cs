using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LuxAPI.Models
{
    [Table("ChatMessages")]
    public class ChatMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "CollectionID is required.")]
        public Guid CollectionId { get; set; }

        [ForeignKey(nameof(CollectionId))]
        [JsonIgnore]
        public Collection? Collection { get; set; }

        [Required(ErrorMessage = "Sender email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string SenderEmail { get; set; } = string.Empty;

        public string SenderUsername { get; set; } // 👈 nouveau champ

        [Required(ErrorMessage = "Message is required.")]
        public string Message { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public Guid? PhotoId { get; set; }

        [ForeignKey(nameof(PhotoId))]
        [JsonIgnore]
        public Photo? Photo { get; set; }
    }
}
