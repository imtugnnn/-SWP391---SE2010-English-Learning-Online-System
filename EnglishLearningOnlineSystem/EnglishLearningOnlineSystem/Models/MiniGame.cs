using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningOnlineSystem.Models
{
    public class MiniGame
    {
        [Key]
        public int GameId { get; set; }

        [MaxLength(100)]
        public string GameType { get; set; }
        public int XPReward { get; set; }

        [Required, MaxLength(255)]
        public string Title { get; set; }

        public int LessonId { get; set; }
        [ForeignKey("LessonId")]
        public Lesson Lesson { get; set; }

        public ICollection<StudentGameProgress> StudentProgresses { get; set; }
    }
}
