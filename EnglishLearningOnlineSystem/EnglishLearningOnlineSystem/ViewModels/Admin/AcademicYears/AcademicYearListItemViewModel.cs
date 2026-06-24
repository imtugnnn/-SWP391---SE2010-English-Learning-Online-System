namespace EnglishLearningOnlineSystem.ViewModels.Admin.AcademicYears;

public class AcademicYearListItemViewModel
{
    public int AcademicYearId { get; set; }
    public string YearLabel { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public int ClassCount { get; set; }
}
