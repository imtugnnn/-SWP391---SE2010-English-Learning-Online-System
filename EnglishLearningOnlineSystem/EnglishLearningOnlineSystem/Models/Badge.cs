using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.Models
{
    public class Badge
    {
        [Key]
        public int BadgeId { get; set; }

        [Required, MaxLength(255)]
        public string BadgeName { get; set; }

        [MaxLength(100)]
        public string TriggerType { get; set; }

        [MaxLength(500)]
        public string IconUrl { get; set; }
        public int TriggerValue { get; set; }

        public ICollection<StudentBadge> StudentBadges { get; set; }
    }
}
