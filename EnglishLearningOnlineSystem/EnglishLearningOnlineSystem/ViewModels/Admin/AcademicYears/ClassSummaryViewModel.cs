namespace EnglishLearningOnlineSystem.ViewModels.Admin.AcademicYears;

public class ClassSummaryViewModel
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string GradeLevel { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string TeacherEmail { get; set; } = string.Empty;
    public List<string> StudentEmails { get; set; } = new();
    public bool IsDeleted { get; set; }
}
