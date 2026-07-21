//Create by TungDPL
//Create at 6/24/2026
//Last update: 7/21/2026
using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.ViewModels.Admin.AcademicYears;

public class AcademicYearCreateViewModel
{
    //BR-AY-03: YearLabel is required and cannot be empty or contain only whitespace characters.
    [Required, MaxLength(50)]
    public string YearLabel { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;
}
