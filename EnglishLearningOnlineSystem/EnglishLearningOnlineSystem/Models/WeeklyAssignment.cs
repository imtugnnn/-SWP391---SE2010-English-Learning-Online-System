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

        public int? CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        public int? LessonId { get; set; }
        [ForeignKey("LessonId")]
        public Lesson Lesson { get; set; }
    }
}
