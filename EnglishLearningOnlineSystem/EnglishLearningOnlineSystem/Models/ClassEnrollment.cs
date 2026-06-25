using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningOnlineSystem.Models;

public class ClassEnrollment
{
    public int ClassEnrollmentId { get; set; }

    public int ClassId { get; set; }

    [ForeignKey(nameof(ClassId))]
    public Class Class { get; set; } = null!;

    public int StudentId { get; set; }

    [ForeignKey(nameof(StudentId))]
    public User Student { get; set; } = null!;

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
}
