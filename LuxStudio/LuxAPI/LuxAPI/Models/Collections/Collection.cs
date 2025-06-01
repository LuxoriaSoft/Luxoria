using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxAPI.Models
{
    [Table("Collections")]
    public class Collection
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Collection name is required.")]
        [MaxLength(100, ErrorMessage = "Collection name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;

        // List of emails authorized to access the collection
        public ICollection<CollectionAccess> Accesses { get; set; } = new List<CollectionAccess>();

        // General chat messages for the collection
        public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

        // Photos belonging to the collection
        public ICollection<Photo> Photos { get; set; } = new List<Photo>();
    }
}
