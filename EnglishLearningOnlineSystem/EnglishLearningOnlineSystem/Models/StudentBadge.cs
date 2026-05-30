namespace EnglishLearningOnlineSystem.Models
{
    public class StudentBadge
    {
        public int StudentId { get; set; }
        public StudentProfile Student { get; set; }

        public int BadgeId { get; set; }
        public Badge Badge { get; set; }

        public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
    }
}
