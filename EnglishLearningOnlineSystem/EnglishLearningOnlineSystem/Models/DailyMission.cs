using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.Models
{
    public class DailyMission
    {
        [Key]
        public int MissionId { get; set; }

        [MaxLength(100)]
        public string Type { get; set; }
        public int XPReward { get; set; }
        public string Description { get; set; }
        public int TargetValue { get; set; }

        public ICollection<StudentMission> StudentMissions { get; set; }
    }
}
