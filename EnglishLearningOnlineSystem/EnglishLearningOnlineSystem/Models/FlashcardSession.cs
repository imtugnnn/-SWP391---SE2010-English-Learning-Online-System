using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningOnlineSystem.Models
{
    public class FlashcardSession
    {
        [Key]
        public int SessionId { get; set; }

        public int CardsReviewed { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public StudentProfile Student { get; set; }

        public int LessonId { get; set; }
        [ForeignKey("LessonId")]
        public Lesson Lesson { get; set; }

        public int? WeeklyAssignmentId { get; set; }
        [ForeignKey(nameof(WeeklyAssignmentId))]
        public WeeklyAssignment? WeeklyAssignment { get; set; }

        public ICollection<FlashcardCardResult> CardResults { get; set; } = new List<FlashcardCardResult>();
    }
}
