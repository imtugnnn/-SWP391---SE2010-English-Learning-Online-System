using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningOnlineSystem.Models
{
    public class XpTransaction
    {
        [Key]
        public int TransactionId { get; set; }
        public int Amount { get; set; }

        [MaxLength(255)]
        public string Source { get; set; } // 'Quiz', 'Minigame', 'Daily Mission'
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public StudentProfile Student { get; set; }
    }
}
