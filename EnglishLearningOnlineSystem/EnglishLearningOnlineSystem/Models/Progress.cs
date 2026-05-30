using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningOnlineSystem.Models
{
    public class Progress
    {
        [Key]
        public int ProgressId { get; set; }
        public int QuizScore { get; set; }
        public int XPEarned { get; set; }

        [MaxLength(50)]
        public string CompletionStatus { get; set; } // 'In Progress', 'Completed'
        public bool IsBestAttempt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public StudentProfile Student { get; set; }

        public int LessonId { get; set; }
        [ForeignKey("LessonId")]
        public Lesson Lesson { get; set; }
    }
}
