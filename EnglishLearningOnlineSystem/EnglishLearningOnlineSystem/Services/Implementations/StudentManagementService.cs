using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class StudentManagementService : IStudentManagementService
{
    private const int PageSize = 10;
    private const int LowQuizScoreThreshold = 60;
    private const int InactiveDaysThreshold = 7;

    private readonly IClassRepository _classRepository;

    public StudentManagementService(IClassRepository classRepository)
    {
        _classRepository = classRepository;
    }

    public async Task<ManageStudentListViewModel?> GetManageStudentListAsync(
        int classId,
        int teacherId,
        string? keyword,
        string? status,
        string? sortBy,
        int page)
    {
        var classEntity = await ValidateTeacherAccessAsync(classId, teacherId);

        if (classEntity == null)
        {
            return null;
        }

        var enrollments = await _classRepository.GetStudentsByClassIdAsync(classId);

        var totalStudents = enrollments.Count;
        var activeStudents = enrollments.Count(e => e.Student.IsActive);
        var inactiveStudents = enrollments.Count(e => !e.Student.IsActive);

        var filteredEnrollments = ApplySearch(enrollments, keyword);
        filteredEnrollments = ApplyStatusFilter(filteredEnrollments, status);
        filteredEnrollments = ApplySorting(filteredEnrollments, sortBy);

        page = NormalizePage(page);

        var totalItems = filteredEnrollments.Count;
        var totalPages = CalculateTotalPages(totalItems, PageSize);

        if (totalPages > 0 && page > totalPages)
        {
            page = totalPages;
        }

        var pagedEnrollments = ApplyPagination(filteredEnrollments, page, PageSize);

        return BuildViewModel(
            classEntity,
            pagedEnrollments,
            keyword,
            status,
            sortBy,
            page,
            PageSize,
            totalItems,
            totalPages,
            totalStudents,
            activeStudents,
            inactiveStudents);
    }

    public async Task<TeacherStudentsNeedSupportViewModel> GetStudentsNeedSupportAsync(
        int teacherId,
        string? classFilter,
        string? reasonFilter,
        string? sortBy)
    {
        var classes = await _classRepository.GetClassesByTeacherIdAsync(teacherId);
        var selectedClassId = ParseClassFilter(classFilter);
        var classesToScan = classes
            .Where(c => !c.IsDeleted && (!selectedClassId.HasValue || c.ClassId == selectedClassId.Value))
            .OrderBy(c => c.ClassName)
            .ToList();

        var items = new List<TeacherSupportStudentItemViewModel>();

        foreach (var classEntity in classesToScan)
        {
            items.AddRange(await BuildSupportItemsForClassAsync(classEntity));
        }

        var normalizedReason = NormalizeSupportReason(reasonFilter);
        var filteredItems = ApplySupportReasonFilter(items, normalizedReason);
        filteredItems = ApplySupportSorting(filteredItems, sortBy);

        return new TeacherStudentsNeedSupportViewModel
        {
            ClassFilter = selectedClassId?.ToString() ?? "all",
            ReasonFilter = normalizedReason,
            SortBy = NormalizeSupportSort(sortBy),
            TotalNeedSupport = items.Count,
            LowScoreCount = items.Count(i => i.HasLowScore),
            OverdueCount = items.Count(i => i.HasOverdueAssignments),
            InactiveCount = items.Count(i => i.IsInactive),
            NotStartedCount = items.Count(i => i.HasNotStartedLessons),
            Classes = classes
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.ClassName)
                .Select(c => new TeacherSupportClassOptionViewModel
                {
                    ClassId = c.ClassId,
                    ClassName = c.ClassName
                })
                .ToList(),
            Students = filteredItems
        };
    }

    public async Task<int> CountStudentsNeedSupportAsync(int teacherId)
    {
        var model = await GetStudentsNeedSupportAsync(teacherId, "all", "all", "risk");
        return model.TotalNeedSupport;
    }

    private async Task<List<TeacherSupportStudentItemViewModel>> BuildSupportItemsForClassAsync(Class classEntity)
    {
        var enrollments = await _classRepository.GetStudentsByClassIdAsync(classEntity.ClassId);
        var assignments = await _classRepository.GetAssignmentsByClassCourseAsync(classEntity.CourseId);
        var lessonIds = assignments
            .Where(a => a.LessonId.HasValue)
            .Select(a => a.LessonId!.Value)
            .Distinct()
            .ToList();

        var studentIds = enrollments.Select(e => e.StudentId).Distinct().ToList();
        var progressRecords = await _classRepository.GetProgressByStudentIdsAndLessonIdsAsync(studentIds, lessonIds);
        var progressByStudent = progressRecords
            .GroupBy(p => p.StudentId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var overdueLessonIds = assignments
            .Where(a => a.DueDate < DateTime.UtcNow && a.LessonId.HasValue)
            .Select(a => a.LessonId!.Value)
            .Distinct()
            .ToList();

        var items = new List<TeacherSupportStudentItemViewModel>();

        foreach (var enrollment in enrollments)
        {
            var studentProgress = progressByStudent.GetValueOrDefault(enrollment.StudentId) ?? new List<Progress>();
            var studentProfile = await _classRepository.GetStudentProfileByIdAsync(enrollment.StudentId);
            var lastActiveDate = studentProfile?.LastActiveDate ?? enrollment.Student.LastLoginAt;

            var completedLessonIds = studentProgress
                .Where(p => string.Equals(p.CompletionStatus, "Completed", StringComparison.OrdinalIgnoreCase))
                .Select(p => p.LessonId)
                .ToHashSet();

            var startedLessonIds = studentProgress
                .Select(p => p.LessonId)
                .ToHashSet();

            var avgScore = studentProgress.Any()
                ? Math.Round(studentProgress.Average(p => p.QuizScore), 1)
                : (double?)null;

            var overdueCount = overdueLessonIds.Count(id => !completedLessonIds.Contains(id));
            var notStartedCount = lessonIds.Count(id => !startedLessonIds.Contains(id));
            var hasLowScore = avgScore.HasValue && avgScore.Value < LowQuizScoreThreshold;
            var isInactive = !enrollment.Student.IsActive ||
                !lastActiveDate.HasValue ||
                lastActiveDate.Value.Date < DateTime.UtcNow.Date.AddDays(-InactiveDaysThreshold);

            var reasons = new List<string>();
            if (hasLowScore) reasons.Add("Điểm quiz thấp");
            if (overdueCount > 0) reasons.Add("Bài quá hạn");
            if (isInactive) reasons.Add("Không hoạt động");
            if (notStartedCount > 0) reasons.Add("Chưa bắt đầu");

            if (!reasons.Any())
            {
                continue;
            }

            items.Add(new TeacherSupportStudentItemViewModel
            {
                ClassId = classEntity.ClassId,
                ClassName = classEntity.ClassName,
                StudentId = enrollment.StudentId,
                StudentName = enrollment.Student.Username,
                Email = enrollment.Student.Email,
                AverageQuizScore = avgScore,
                OverdueLessonCount = overdueCount,
                NotStartedLessonCount = notStartedCount,
                LastActiveDate = lastActiveDate,
                IsInactive = isInactive,
                HasLowScore = hasLowScore,
                HasOverdueAssignments = overdueCount > 0,
                HasNotStartedLessons = notStartedCount > 0,
                RiskScore = CalculateRiskScore(hasLowScore, overdueCount, isInactive, notStartedCount),
                Reasons = reasons
            });
        }

        return items;
    }

    private static int? ParseClassFilter(string? classFilter)
    {
        return int.TryParse(classFilter, out var classId) ? classId : null;
    }

    private static string NormalizeSupportReason(string? reason)
    {
        return string.IsNullOrWhiteSpace(reason)
            ? "all"
            : reason.Trim().ToLower();
    }

    private static string NormalizeSupportSort(string? sortBy)
    {
        return string.IsNullOrWhiteSpace(sortBy)
            ? "risk"
            : sortBy.Trim();
    }

    private static List<TeacherSupportStudentItemViewModel> ApplySupportReasonFilter(
        List<TeacherSupportStudentItemViewModel> items,
        string reason)
    {
        return reason switch
        {
            "low-score" => items.Where(i => i.HasLowScore).ToList(),
            "overdue" => items.Where(i => i.HasOverdueAssignments).ToList(),
            "inactive" => items.Where(i => i.IsInactive).ToList(),
            "not-started" => items.Where(i => i.HasNotStartedLessons).ToList(),
            _ => items
        };
    }

    private static List<TeacherSupportStudentItemViewModel> ApplySupportSorting(
        List<TeacherSupportStudentItemViewModel> items,
        string? sortBy)
    {
        return NormalizeSupportSort(sortBy) switch
        {
            "score" => items
                .OrderBy(i => i.AverageQuizScore ?? double.MaxValue)
                .ThenByDescending(i => i.RiskScore)
                .ToList(),
            "overdue" => items
                .OrderByDescending(i => i.OverdueLessonCount)
                .ThenByDescending(i => i.RiskScore)
                .ToList(),
            "last-active" => items
                .OrderBy(i => i.LastActiveDate ?? DateTime.MinValue)
                .ThenByDescending(i => i.RiskScore)
                .ToList(),
            _ => items
                .OrderByDescending(i => i.RiskScore)
                .ThenBy(i => i.StudentName)
                .ToList()
        };
    }

    private static int CalculateRiskScore(bool hasLowScore, int overdueCount, bool isInactive, int notStartedCount)
    {
        var score = 0;
        if (hasLowScore) score += 3;
        if (isInactive) score += 2;
        score += overdueCount * 2;
        score += notStartedCount;
        return score;
    }

    private async Task<Class?> ValidateTeacherAccessAsync(int classId, int teacherId)
    {
        var classEntity = await _classRepository.GetClassDetailByIdAsync(classId);

        if (classEntity == null)
        {
            return null;
        }

        if (classEntity.TeacherId != teacherId)
        {
            return null;
        }

        return classEntity;
    }

    private static List<ClassEnrollment> ApplySearch(
        List<ClassEnrollment> enrollments,
        string? keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return enrollments;
        }

        var normalizedKeyword = keyword.Trim().ToLower();

        return enrollments
            .Where(e =>
                e.Student.Username.ToLower().Contains(normalizedKeyword) ||
                e.Student.Email.ToLower().Contains(normalizedKeyword))
            .ToList();
    }

    private static List<ClassEnrollment> ApplyStatusFilter(
        List<ClassEnrollment> enrollments,
        string? status)
    {
        var normalizedStatus = NormalizeStatus(status);

        return normalizedStatus switch
        {
            "active" => enrollments.Where(e => e.Student.IsActive).ToList(),
            "inactive" => enrollments.Where(e => !e.Student.IsActive).ToList(),
            _ => enrollments
        };
    }

    private static List<ClassEnrollment> ApplySorting(
        List<ClassEnrollment> enrollments,
        string? sortBy)
    {
        var normalizedSortBy = NormalizeSortBy(sortBy);

        return normalizedSortBy switch
        {
            "email" => enrollments.OrderBy(e => e.Student.Email).ToList(),
            "date" => enrollments.OrderByDescending(e => e.EnrolledAt).ToList(),
            "status" => enrollments.OrderByDescending(e => e.Student.IsActive).ToList(),
            _ => enrollments.OrderBy(e => e.Student.Username).ToList()
        };
    }

    private static List<ClassEnrollment> ApplyPagination(
        List<ClassEnrollment> enrollments,
        int page,
        int pageSize)
    {
        return enrollments
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    private static ManageStudentListViewModel BuildViewModel(
        Class classEntity,
        List<ClassEnrollment> pagedEnrollments,
        string? keyword,
        string? status,
        string? sortBy,
        int page,
        int pageSize,
        int totalItems,
        int totalPages,
        int totalStudents,
        int activeStudents,
        int inactiveStudents)
    {
        return new ManageStudentListViewModel
        {
            ClassId = classEntity.ClassId,
            ClassName = classEntity.ClassName,

            Keyword = keyword ?? string.Empty,
            Status = NormalizeStatus(status),
            SortBy = NormalizeSortBy(sortBy),

            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,

            TotalStudents = totalStudents,
            ActiveStudents = activeStudents,
            InactiveStudents = inactiveStudents,

            Students = pagedEnrollments.Select(e => new ManageStudentItemViewModel
            {
                StudentId = e.StudentId,
                StudentName = e.Student.Username,
                Email = e.Student.Email,
                IsActive = e.Student.IsActive,
                EnrollmentStatus = e.Student.IsActive ? "Đang hoạt động" : "Không hoạt động",
                EnrolledAt = e.EnrolledAt
            }).ToList()
        };
    }

    private static int NormalizePage(int page)
    {
        return page < 1 ? 1 : page;
    }

    private static int CalculateTotalPages(int totalItems, int pageSize)
    {
        return (int)Math.Ceiling(totalItems / (double)pageSize);
    }

    private static string NormalizeStatus(string? status)
    {
        return string.IsNullOrWhiteSpace(status)
            ? "all"
            : status.Trim().ToLower();
    }

    private static string NormalizeSortBy(string? sortBy)
    {
        return string.IsNullOrWhiteSpace(sortBy)
            ? "name"
            : sortBy.Trim().ToLower();
    }
    public async Task<TeacherStudentDetailViewModel?> GetStudentDetailAsync(
    int classId,
    int studentId,
    int teacherId)
    {
        var classEntity = await ValidateTeacherAccessAsync(classId, teacherId);

        if (classEntity == null)
        {
            return null;
        }

        var belongsToClass = await ValidateStudentBelongsToClassAsync(classId, studentId);

        if (!belongsToClass)
        {
            return null;
        }

        var studentProfile = await _classRepository.GetStudentProfileByIdAsync(studentId);

        if (studentProfile == null || studentProfile.User == null)
        {
            return null;
        }

        var progressRecords = await _classRepository.GetStudentProgressByStudentIdAsync(studentId);
        var feedbacks = await _classRepository.GetTeacherFeedbackByStudentIdAsync(studentId);

        return BuildStudentDetailViewModel(
            classEntity,
            studentProfile,
            progressRecords,
            feedbacks);
    }

    private async Task<bool> ValidateStudentBelongsToClassAsync(int classId, int studentId)
    {
        var enrollments = await _classRepository.GetStudentsByClassIdAsync(classId);

        return enrollments.Any(e => e.StudentId == studentId);
    }

    private static TeacherStudentDetailViewModel BuildStudentDetailViewModel(
        EnglishLearningOnlineSystem.Models.Class classEntity,
        EnglishLearningOnlineSystem.Models.StudentProfile studentProfile,
        List<EnglishLearningOnlineSystem.Models.Progress> progressRecords,
        List<EnglishLearningOnlineSystem.Models.TeacherFeedback> feedbacks)
    {
        var completedLessons = CountCompletedLessons(progressRecords);
        var inProgressLessons = CountInProgressLessons(progressRecords);
        var averageQuizScore = CalculateAverageQuizScore(progressRecords);
        var totalXPEarned = CalculateTotalXPEarned(progressRecords);

        return new TeacherStudentDetailViewModel
        {
            ClassId = classEntity.ClassId,
            ClassName = classEntity.ClassName,

            StudentId = studentProfile.StudentId,
            StudentName = studentProfile.User.Username,
            Nickname = studentProfile.Nickname ?? studentProfile.User.Username,
            Email = studentProfile.User.Email,
            AvatarUrl = string.IsNullOrWhiteSpace(studentProfile.AvatarUrl)
                ? "/images/default-avatar.png"
                : studentProfile.AvatarUrl,
            IsActive = studentProfile.User.IsActive,
            StatusText = studentProfile.User.IsActive ? "Đang hoạt động" : "Không hoạt động",

            Level = studentProfile.Level,
            XP = studentProfile.XP,
            CurrentStreakDays = studentProfile.CurrentStreakDays,
            LastActiveDate = studentProfile.LastActiveDate,

            CompletedLessons = completedLessons,
            InProgressLessons = inProgressLessons,
            AverageQuizScore = averageQuizScore,
            TotalXPEarned = totalXPEarned,

            LessonProgresses = BuildLessonProgressViewModels(progressRecords),
            Feedbacks = BuildFeedbackViewModels(feedbacks)
        };
    }

    private static int CountCompletedLessons(List<EnglishLearningOnlineSystem.Models.Progress> progressRecords)
    {
        return progressRecords.Count(p =>
            string.Equals(p.CompletionStatus, "Completed", StringComparison.OrdinalIgnoreCase));
    }

    private static int CountInProgressLessons(List<EnglishLearningOnlineSystem.Models.Progress> progressRecords)
    {
        return progressRecords.Count(p =>
            !string.Equals(p.CompletionStatus, "Completed", StringComparison.OrdinalIgnoreCase));
    }

    private static double CalculateAverageQuizScore(List<EnglishLearningOnlineSystem.Models.Progress> progressRecords)
    {
        if (!progressRecords.Any())
        {
            return 0;
        }

        return Math.Round(progressRecords.Average(p => p.QuizScore), 2);
    }

    private static int CalculateTotalXPEarned(List<EnglishLearningOnlineSystem.Models.Progress> progressRecords)
    {
        return progressRecords.Sum(p => p.XPEarned);
    }

    private static List<TeacherStudentLessonProgressViewModel> BuildLessonProgressViewModels(
        List<EnglishLearningOnlineSystem.Models.Progress> progressRecords)
    {
        return progressRecords.Select(p => new TeacherStudentLessonProgressViewModel
        {
            LessonId = p.LessonId,
            LessonTitle = p.Lesson?.Title ?? "Bài học chưa xác định",
            Topic = p.Lesson?.Topic ?? "Chưa cập nhật",
            QuizScore = p.QuizScore,
            XPEarned = p.XPEarned,
            CompletionStatus = string.IsNullOrWhiteSpace(p.CompletionStatus)
                ? "Chưa cập nhật"
                : p.CompletionStatus,
            CompletedAt = p.CompletedAt
        }).ToList();
    }

    private static List<TeacherStudentFeedbackViewModel> BuildFeedbackViewModels(
        List<EnglishLearningOnlineSystem.Models.TeacherFeedback> feedbacks)
    {
        return feedbacks.Select(f => new TeacherStudentFeedbackViewModel
        {
            FeedbackId = f.FeedbackId,
            TeacherName = f.Teacher?.Username ?? "Giáo viên",
            Content = f.Content,
            IsRead = f.IsRead,
            ReadStatus = f.IsRead ? "Đã đọc" : "Chưa đọc",
            CreateAt = f.CreateAt
        }).ToList();
    }
    public async Task<ProvideStudentFeedbackViewModel?> GetProvideFeedbackFormAsync(
    int classId,
    int studentId,
    int teacherId)
    {
        var classEntity = await ValidateTeacherAccessAsync(classId, teacherId);

        if (classEntity == null)
        {
            return null;
        }

        var belongsToClass = await ValidateStudentBelongsToClassAsync(classId, studentId);

        if (!belongsToClass)
        {
            return null;
        }

        var studentProfile = await _classRepository.GetStudentProfileByIdAsync(studentId);

        if (studentProfile == null || studentProfile.User == null)
        {
            return null;
        }

        return new ProvideStudentFeedbackViewModel
        {
            ClassId = classEntity.ClassId,
            ClassName = classEntity.ClassName,
            StudentId = studentProfile.StudentId,
            StudentName = studentProfile.User.Username,
            StudentEmail = studentProfile.User.Email,
            AvatarUrl = string.IsNullOrWhiteSpace(studentProfile.AvatarUrl)
                ? "/images/default-avatar.png"
                : studentProfile.AvatarUrl
        };
    }

    public async Task<bool> CreateStudentFeedbackAsync(
        ProvideStudentFeedbackViewModel model,
        int teacherId)
    {
        var classEntity = await ValidateTeacherAccessAsync(model.ClassId, teacherId);

        if (classEntity == null)
        {
            return false;
        }

        var belongsToClass = await ValidateStudentBelongsToClassAsync(
            model.ClassId,
            model.StudentId);

        if (!belongsToClass)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(model.Content))
        {
            return false;
        }

        var feedback = new TeacherFeedback
        {
            Content = model.Content.Trim(),
            IsRead = false,
            CreateAt = DateTime.UtcNow,
            TeacherId = teacherId,
            StudentId = model.StudentId
        };

        await _classRepository.AddTeacherFeedbackAsync(feedback);

        return true;
    }
}
