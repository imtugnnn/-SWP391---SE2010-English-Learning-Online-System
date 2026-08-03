using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningOnlineSystem.Models
{
    public class StudentGameProgress
    {
        [Key]
        public int GameProgressId { get; set; }
        public int Score { get; set; }
        public int XPEarned { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public StudentProfile Student { get; set; }

        public int GameId { get; set; }
        [ForeignKey("GameId")]
        public MiniGame MiniGame { get; set; }

        public int? WeeklyAssignmentId { get; set; }
        [ForeignKey(nameof(WeeklyAssignmentId))]
        public WeeklyAssignment? WeeklyAssignment { get; set; }
    }
}
