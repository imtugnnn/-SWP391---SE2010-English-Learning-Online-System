namespace EnglishLearningOnlineSystem.ViewModels;

public class TeacherNavigationViewModel
{
    public string TeacherName { get; set; } = "Giáo viên";
    public string CurrentAction { get; set; } = string.Empty;
    public int? SelectedClassId { get; set; }
    public List<TeacherNavigationClassViewModel> Classes { get; set; } = new();
}

public class TeacherNavigationClassViewModel
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string GradeLevel { get; set; } = string.Empty;
}
