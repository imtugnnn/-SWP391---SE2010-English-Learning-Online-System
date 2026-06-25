using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.Models;

public class AcademicYear
{
    public int AcademicYearId { get; set; }

    [Required, MaxLength(50)]
    public string YearLabel { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; }

    public ICollection<Class> Classes { get; set; } = new List<Class>();
}
