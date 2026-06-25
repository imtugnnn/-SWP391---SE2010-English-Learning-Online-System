using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.ViewModels.Admin.AcademicYears;

public class AcademicYearCreateViewModel
{
    [Required, MaxLength(50)]
    public string YearLabel { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;
}
