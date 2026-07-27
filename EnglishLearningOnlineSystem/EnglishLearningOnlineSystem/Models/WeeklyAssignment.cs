using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningOnlineSystem.Models
{
    public class WeeklyAssignment
    {
        [Key]
        public int AssignmentId { get; set; }
        public DateTime WeekStartDate { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsVisible { get; set; }

        public int? ClassId { get; set; }
        [ForeignKey(nameof(ClassId))]
        public Class? Class { get; set; }

        public int? CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        public int? LessonId { get; set; }
        [ForeignKey("LessonId")]
        public Lesson Lesson { get; set; }

        public bool IncludeVocabulary { get; set; }
        public bool IncludeQuiz { get; set; }
        public bool IncludeMiniGame { get; set; }

        public ICollection<WeeklyAssignmentVocabulary> Vocabularies { get; set; }
            = new List<WeeklyAssignmentVocabulary>();
        public ICollection<WeeklyAssignmentQuiz> Quizzes { get; set; }
            = new List<WeeklyAssignmentQuiz>();
        public ICollection<WeeklyAssignmentMiniGame> MiniGames { get; set; }
            = new List<WeeklyAssignmentMiniGame>();
    }
}
