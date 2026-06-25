namespace EnglishLearningOnlineSystem.ViewModels;

public class TeacherClassDetailViewModel
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string GradeLevel { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;

    public int StudentCount { get; set; }
    public int AssignmentCount { get; set; }
    public double CompletionRate { get; set; }
    public int StudentsBehindSchedule { get; set; }

    public List<TeacherClassStudentViewModel> Students { get; set; } = new();
    public List<TeacherClassAssignmentViewModel> Assignments { get; set; } = new();
}

public class TeacherClassStudentViewModel
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string EnrollmentStatus { get; set; } = string.Empty;
}

public class TeacherClassAssignmentViewModel
{
    public int AssignmentId { get; set; }
    public string AssignmentTitle { get; set; } = string.Empty;
    public string LessonTitle { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
}