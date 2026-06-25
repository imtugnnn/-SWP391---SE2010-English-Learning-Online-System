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

        public int AcademicYearId { get; set; }
        [ForeignKey("AcademicYearId")]
        public AcademicYear AcademicYear { get; set; }

        public int? TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        public User Teacher { get; set; }

        public int? CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        public bool IsDeleted { get; set; } = false;

        public ICollection<ClassEnrollment> Enrollments { get; set; } = new List<ClassEnrollment>();
    }
}
