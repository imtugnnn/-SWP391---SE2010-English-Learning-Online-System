using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningOnlineSystem.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [MaxLength(100)]
        public string Type { get; set; }

        [Required]
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
