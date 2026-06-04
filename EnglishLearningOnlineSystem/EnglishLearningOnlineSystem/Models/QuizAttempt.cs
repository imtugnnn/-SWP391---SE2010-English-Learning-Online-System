using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningOnlineSystem.Models
{
    public class QuizAttempt
    {
        [Key]
        public int AttemptId { get; set; }

        public int TotalQuestions { get; set; }
        public int CorrectCount { get; set; }
        public int Score { get; set; } // Percentage 0-100
        public int TimeSpentSec { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime SubmittedAt { get; set; }
        public bool XpAwarded { get; set; } // BR-14: mirrors Progress.XPEarned for display

        // Foreign keys
        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public StudentProfile Student { get; set; }

        public int LessonId { get; set; }
        [ForeignKey("LessonId")]
        public Lesson Lesson { get; set; }

        public int? WeeklyAssignmentId { get; set; }
        [ForeignKey("WeeklyAssignmentId")]
        public WeeklyAssignment? WeeklyAssignment { get; set; }

        public ICollection<QuizAttemptAnswer> Answers { get; set; } = new List<QuizAttemptAnswer>();
    }
}
