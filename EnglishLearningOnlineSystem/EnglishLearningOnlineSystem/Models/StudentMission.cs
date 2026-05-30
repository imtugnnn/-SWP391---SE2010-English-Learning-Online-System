using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningOnlineSystem.Models
{
    public class StudentMission
    {
        [Key]
        public int StudentMissionId { get; set; }
        public DateTime Date { get; set; }
        public int CurrentValue { get; set; }
        public bool IsCompleted { get; set; }

        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public StudentProfile Student { get; set; }

        public int MissionId { get; set; }
        [ForeignKey("MissionId")]
        public DailyMission DailyMission { get; set; }
    }
}
