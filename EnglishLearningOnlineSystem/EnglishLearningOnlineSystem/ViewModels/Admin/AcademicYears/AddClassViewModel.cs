//Create by TungDPL
//Create: 6/24/2026
//Last update: 7/21/2026
using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.ViewModels.Admin.AcademicYears;

public class AddClassViewModel
{
    [Required, MaxLength(255)]
    public string ClassName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string GradeLevel { get; set; } = string.Empty;

    [Required]
    public int? TeacherId { get; set; }

    [Required]
    public string StudentEmails { get; set; } = string.Empty;
}
