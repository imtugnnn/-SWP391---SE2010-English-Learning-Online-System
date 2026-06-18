using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningOnlineSystem.Models
{
    public class StudentProfile
    {
        [Key, ForeignKey("User")]
        public int StudentId { get; set; }
        public User User { get; set; }

        [MaxLength(255)]
        public string Nickname { get; set; }

        [MaxLength(20)]
        public string? StudentCode { get; set; }

        public int Level { get; set; } = 1;
        public int XP { get; set; } = 0;
        public int CurrentStreakDays { get; set; } = 0;
        public DateTime? LastActiveDate { get; set; }
        [MaxLength(500)]
        public string AvatarUrl { get; set; }

        // Navigation properties
        public ICollection<StudentBadge> StudentBadges { get; set; }
        public ICollection<StudentMission> StudentMissions { get; set; }
        public ICollection<StudentGameProgress> GameProgresses { get; set; }
        public ICollection<Progress> LessonProgresses { get; set; }
        public ICollection<XpTransaction> XpTransactions { get; set; }
        public ICollection<TeacherFeedback> ReceivedFeedbacks { get; set; }
        public ICollection<QuizAttempt> QuizAttempts { get; set; }
        public ICollection<FlashcardSession> FlashcardSessions { get; set; }
    }
}
