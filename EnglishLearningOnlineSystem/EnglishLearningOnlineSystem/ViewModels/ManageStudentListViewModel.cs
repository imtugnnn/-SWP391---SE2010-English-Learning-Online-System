namespace EnglishLearningOnlineSystem.ViewModels;

public class ManageStudentListViewModel
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;

    public string Keyword { get; set; } = string.Empty;
    public string Status { get; set; } = "all";
    public string SortBy { get; set; } = "name";

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }

    public int TotalStudents { get; set; }
    public int ActiveStudents { get; set; }
    public int InactiveStudents { get; set; }
    public int CompletedStudents { get; set; }
    public int InProgressStudents { get; set; }
    public int NotStartedStudents { get; set; }
    public int CompletedLateStudents { get; set; }
    public int StudentsNeedSupport { get; set; }

    public List<ManageStudentItemViewModel> Students { get; set; } = new();
}

public class ManageStudentItemViewModel
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string EnrollmentStatus { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
    public string LearningStatus { get; set; } = "NotStarted";
    public int CompletedAssignments { get; set; }
    public int TotalAssignments { get; set; }
    public double? AverageQuizScore { get; set; }
    public int OverdueAssignmentCount { get; set; }
    public int RiskScore { get; set; }
    public List<string> SupportReasons { get; set; } = new();
    public double CompletionRate => TotalAssignments == 0
        ? 0
        : Math.Round((double)CompletedAssignments / TotalAssignments * 100, 1);
}
