using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LuxAPI.Models
{
    [Table("CollectionAccesses")]
    public class CollectionAccess
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "CollectionID is required.")]
        public Guid CollectionId { get; set; }

        [ForeignKey(nameof(CollectionId))]
        [JsonIgnore]
        public Collection? Collection { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;
    }
}
