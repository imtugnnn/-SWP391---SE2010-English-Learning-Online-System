using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningOnlineSystem.Models
{
    public class Class
    {
        [Key]
        public int ClassId { get; set; }

        [Required, MaxLength(255)]
        public string ClassName { get; set; }

        [MaxLength(50)]
        public string GradeLevel { get; set; }

        [MaxLength(50)]
        public string AcademicYear { get; set; }

        public int? TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        public User Teacher { get; set; }

        public int? CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course Course { get; set; }
    }
}
