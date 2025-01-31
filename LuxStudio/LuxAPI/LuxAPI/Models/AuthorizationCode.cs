using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxAPI.Models;

[Table("AuthorizationCodes")]
public class AuthorizationCode
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Code { get; set; }

    [Required]
    public Guid ClientId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public DateTime Expiry { get; set; }
}