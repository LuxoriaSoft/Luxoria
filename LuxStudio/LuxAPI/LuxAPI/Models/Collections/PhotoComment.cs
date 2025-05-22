using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LuxAPI.Models
{
    [Table("PhotoComments")]
    public class PhotoComment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "PhotoID is required.")]
        public Guid PhotoId { get; set; }

        [ForeignKey(nameof(PhotoId))]
        public Photo? Photo { get; set; }

        // Optional parent comment for handling replies
        public Guid? ParentCommentId { get; set; }

        [ForeignKey("ParentCommentId")]
        public PhotoComment? ParentComment { get; set; }

        // Replies associated with this comment
        public ICollection<PhotoComment> Replies { get; set; } = new List<PhotoComment>();

        [Required(ErrorMessage = "Comment text is required.")]
        public string CommentText { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
