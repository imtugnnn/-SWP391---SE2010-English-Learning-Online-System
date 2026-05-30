using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningOnlineSystem.Models
{
    public class Lesson
    {
        [Key]
        public int LessonId { get; set; }

        [Required, MaxLength(255)]
        public string Title { get; set; }
        public int XPReward { get; set; }

        [MaxLength(255)]
        public string Topic { get; set; }
        public int EstimatedMinutes { get; set; }
        public int OrderIndex { get; set; }
        public bool IsPublished { get; set; }

        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        public ICollection<WeeklyAssignment> WeeklyAssignments { get; set; }
        public ICollection<MiniGame> MiniGames { get; set; }
        public ICollection<Quiz> Quizzes { get; set; }
        public ICollection<Vocabulary> Vocabularies { get; set; }
        public ICollection<Progress> Progresses { get; set; }
    }
}
