using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;

namespace EnglishLearningOnlineSystem.Models;

public class Course
{
    [Key]
    public int CourseId { get; set; }

    [Required, MaxLength(255)]
    public string CourseName { get; set; }

    [MaxLength(50)]
    public string GradeLevel { get; set; }
    public bool IsPublished { get; set; }

    public int? CreatorId { get; set; }
    [ForeignKey("CreatorId")]
    public User Creator { get; set; }

    public ICollection<Class> Classes { get; set; }
    public ICollection<Lesson> Lessons { get; set; }
    public ICollection<WeeklyAssignment> WeeklyAssignments { get; set; }
}