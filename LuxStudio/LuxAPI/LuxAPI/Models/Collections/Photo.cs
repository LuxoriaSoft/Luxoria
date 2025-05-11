using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LuxAPI.Models
{
    public enum PhotoStatus
    {
        Pending,
        ModificationReview,
        Approved
    }

    [Table("Photos")]
    public class Photo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "CollectionID is required.")]
        public Guid CollectionId { get; set; }

        [ForeignKey(nameof(CollectionId))]
        [JsonIgnore]
        public Collection? Collection { get; set; }

        public string? FilePath { get; set; }

        [Required(ErrorMessage = "Photo status is required.")]
        public PhotoStatus Status { get; set; }

        // Comments associated with the photo
        public ICollection<PhotoComment> Comments { get; set; } = new List<PhotoComment>();
    }
}
