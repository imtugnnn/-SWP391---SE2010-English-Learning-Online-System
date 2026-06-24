using Microsoft.AspNetCore.Http;

namespace EnglishLearningOnlineSystem.ViewModels.Admin.AcademicYears;

public class AcademicYearEditViewModel
{
    public int AcademicYearId { get; set; }
    public string YearLabel { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public List<ClassSummaryViewModel> Classes { get; set; } = new();
    public AddClassViewModel NewClass { get; set; } = new();
    public IFormFile? ImportFile { get; set; }
    public int? SelectedClassId { get; set; }
    public ClassSummaryViewModel? SelectedClass { get; set; }
}
