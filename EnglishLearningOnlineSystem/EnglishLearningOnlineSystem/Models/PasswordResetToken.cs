using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningOnlineSystem.Models;

public class PasswordResetToken
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Token { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}
