using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxAPI.Models;

[Table("Tokens")]
public class Token
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    [Column(TypeName = "TEXT")] // Permet de stocker un token de longueur variable
    public string AccessToken { get; set; }

    [Required]
    [Column(TypeName = "TEXT")] // Permet de stocker un refresh token de longueur variable
    public string RefreshToken { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    [Required]
    public DateTime Expiry { get; set; }
}
