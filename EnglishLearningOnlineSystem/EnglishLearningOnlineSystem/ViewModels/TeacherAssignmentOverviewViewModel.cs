namespace EnglishLearningOnlineSystem.ViewModels;

public class TeacherAssignmentOverviewViewModel
{
    public int? ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string Status { get; set; } = "all";
    public string SortBy { get; set; } = "dueDate";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }

    public int TotalAssignments { get; set; }
    public int DraftAssignments { get; set; }
    public int ActiveAssignments { get; set; }
    public int ExpiredAssignments { get; set; }

    public List<TeacherAssignmentItemViewModel> Assignments { get; set; } = new();
}

public class TeacherAssignmentItemViewModel
{
    public int AssignmentId { get; set; }
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public int EstimatedMinutes { get; set; }
    public int XPReward { get; set; }
    public int VocabularyCount { get; set; }
    public int QuizCount { get; set; }
    public int MiniGameCount { get; set; }
    public DateTime WeekStartDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int AssignedStudentCount { get; set; }
    public int CompletedStudentCount { get; set; }
    public int CompletedLateCount { get; set; }
    public double CompletionRate => AssignedStudentCount == 0
        ? 0
        : Math.Round((double)CompletedStudentCount / AssignedStudentCount * 100, 1);
    public bool CanEdit { get; set; }
    public bool CanCancel { get; set; }
    public bool CanDelete { get; set; }
    public bool CanArchive { get; set; }
}
