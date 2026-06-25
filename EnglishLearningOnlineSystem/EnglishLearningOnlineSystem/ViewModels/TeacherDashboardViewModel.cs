namespace EnglishLearningOnlineSystem.ViewModels;

public class TeacherDashboardViewModel
{
    public string TeacherName { get; set; } = string.Empty;

    public int TotalClasses { get; set; }
    public int TotalStudents { get; set; }
    public int TotalAssignments { get; set; }
    public int ActiveAssignments { get; set; }
    public int ExpiredAssignments { get; set; }
    public int StudentsNeedAttention { get; set; }

    public List<TeacherDashboardClassViewModel> Classes { get; set; } = new();
}

public class TeacherDashboardClassViewModel
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string GradeLevel { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;

    public int StudentCount { get; set; }
    public int AssignmentCount { get; set; }
    public int ExpiredAssignmentCount { get; set; }
}