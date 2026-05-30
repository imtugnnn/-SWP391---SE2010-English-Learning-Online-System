using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningOnlineSystem.Models
{
    public class TeacherFeedback
    {
        [Key]
        public int FeedbackId { get; set; }

        [Required]
        public string Content { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        public int? TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        public User Teacher { get; set; }

        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public StudentProfile Student { get; set; }
    }
}
