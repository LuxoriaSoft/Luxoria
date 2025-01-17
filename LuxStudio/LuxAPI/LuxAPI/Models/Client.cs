using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxAPI.Models;

[Table("Clients")]
public class Client
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    public Guid ClientId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ClientSecret { get; set; }

    [Required]
    [MaxLength(200)]
    public string RedirectUri { get; set; }
}