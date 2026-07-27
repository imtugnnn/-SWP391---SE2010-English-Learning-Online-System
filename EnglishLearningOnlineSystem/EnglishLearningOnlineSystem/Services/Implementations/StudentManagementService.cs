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

    /// <summary>
    /// Lấy danh sách học sinh trong lớp với tìm kiếm, lọc trạng thái, sắp xếp và phân trang.
    /// </summary>
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

        var enrollments = await _classRepository.GetActiveStudentsByClassIdAsync(classId);

        var totalStudents = enrollments.Count;
        var activeStudents = totalStudents;
        const int inactiveStudents = 0;

        var filteredEnrollments = ApplySearch(enrollments, keyword);
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
            "active",
            sortBy,
            page,
            PageSize,
            totalItems,
            totalPages,
            totalStudents,
            activeStudents,
            inactiveStudents);
    }

    /// <summary>
    /// Tổng hợp các học sinh có dấu hiệu cần hỗ trợ trong những lớp giáo viên phụ trách.
    /// </summary>
    public async Task<TeacherStudentsNeedSupportViewModel> GetStudentsNeedSupportAsync(
        int teacherId,
        string? classFilter,
        string? reasonFilter,
        string? sortBy)
    {
        var classes = await _classRepository.GetClassesByTeacherIdAsync(teacherId);
        var selectedClassId = ParseClassFilter(classFilter);
        var selectedClass = selectedClassId.HasValue
            ? classes.FirstOrDefault(c => c.ClassId == selectedClassId.Value && !c.IsDeleted)
            : null;
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
            ClassId = selectedClass?.ClassId ?? 0,
            ClassName = selectedClass?.ClassName ?? string.Empty,
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

    /// <summary>
    /// Đếm số học sinh cần hỗ trợ để hiển thị trên dashboard giáo viên.
    /// </summary>
    public async Task<int> CountStudentsNeedSupportAsync(int teacherId)
    {
        var model = await GetStudentsNeedSupportAsync(teacherId, "all", "all", "risk");
        return model.TotalNeedSupport;
    }

    /// <summary>
    /// Phân tích tiến độ của từng học sinh trong lớp và tạo danh sách các trường hợp có rủi ro.
    /// </summary>
    private async Task<List<TeacherSupportStudentItemViewModel>> BuildSupportItemsForClassAsync(Class classEntity)
    {
        var enrollments = await _classRepository.GetActiveStudentsByClassIdAsync(classEntity.ClassId);
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
            var isInactive = !lastActiveDate.HasValue ||
                lastActiveDate.Value.Date < DateTime.UtcNow.Date.AddDays(-InactiveDaysThreshold);

            var reasons = new List<string>();
            if (hasLowScore) reasons.Add("Điểm quiz thấp");
            if (overdueCount > 0) reasons.Add("Bài quá hạn");
            if (isInactive) reasons.Add("Ít hoạt động học tập");
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

    // Chuyển mã lớp từ query string sang số; null tương ứng với tất cả lớp.
    private static int? ParseClassFilter(string? classFilter)
    {
        return int.TryParse(classFilter, out var classId) ? classId : null;
    }

    // Chuẩn hóa nguyên nhân cần hỗ trợ để áp dụng bộ lọc nhất quán.
    private static string NormalizeSupportReason(string? reason)
    {
        return string.IsNullOrWhiteSpace(reason)
            ? "all"
            : reason.Trim().ToLower();
    }

    // Chuẩn hóa tiêu chí sắp xếp danh sách học sinh cần hỗ trợ.
    private static string NormalizeSupportSort(string? sortBy)
    {
        return string.IsNullOrWhiteSpace(sortBy)
            ? "risk"
            : sortBy.Trim();
    }

    /// <summary>
    /// Lọc học sinh theo dấu hiệu rủi ro mà giáo viên lựa chọn.
    /// </summary>
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

    /// <summary>
    /// Sắp xếp học sinh cần hỗ trợ theo mức độ ưu tiên hoặc tiêu chí được chọn.
    /// </summary>
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

    // Điểm rủi ro càng cao thì học sinh càng cần được giáo viên ưu tiên hỗ trợ.
    private static int CalculateRiskScore(bool hasLowScore, int overdueCount, bool isInactive, int notStartedCount)
    {
        var score = 0;
        if (hasLowScore) score += 3;
        if (isInactive) score += 2;
        score += overdueCount * 2;
        score += notStartedCount;
        return score;
    }

    /// <summary>
    /// Bảo đảm giáo viên chỉ có thể truy cập lớp được phân công cho mình.
    /// </summary>
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

    // Tìm học sinh theo tên hoặc email, không phân biệt hoa/thường.
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

    // Sắp xếp danh sách ghi danh theo tiêu chí giáo viên lựa chọn.
    private static List<ClassEnrollment> ApplySorting(
        List<ClassEnrollment> enrollments,
        string? sortBy)
    {
        var normalizedSortBy = NormalizeSortBy(sortBy);

        return normalizedSortBy switch
        {
            "email" => enrollments.OrderBy(e => e.Student.Email).ToList(),
            "date" => enrollments.OrderByDescending(e => e.EnrolledAt).ToList(),
            _ => enrollments.OrderBy(e => e.Student.Username).ToList()
        };
    }

    // Chỉ lấy các bản ghi thuộc trang hiện tại.
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

    /// <summary>
    /// Ánh xạ dữ liệu lớp và ghi danh sang model dùng bởi màn hình quản lý học sinh.
    /// </summary>
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

    // Không cho phép chỉ số trang nhỏ hơn 1.
    private static int NormalizePage(int page)
    {
        return page < 1 ? 1 : page;
    }

    // Tính tổng số trang và làm tròn lên khi trang cuối không đủ số bản ghi.
    private static int CalculateTotalPages(int totalItems, int pageSize)
    {
        return (int)Math.Ceiling(totalItems / (double)pageSize);
    }

    // Chuẩn hóa bộ lọc trạng thái, mặc định hiển thị tất cả.
    private static string NormalizeStatus(string? status)
    {
        return string.IsNullOrWhiteSpace(status)
            ? "all"
            : status.Trim().ToLower();
    }

    // Chuẩn hóa tiêu chí sắp xếp, mặc định theo tên học sinh.
    private static string NormalizeSortBy(string? sortBy)
    {
        return string.IsNullOrWhiteSpace(sortBy)
            ? "name"
            : sortBy.Trim().ToLower();
    }
    /// <summary>
    /// Lấy hồ sơ, tiến độ bài học và lịch sử phản hồi của một học sinh trong lớp.
    /// </summary>
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

    // Xác nhận học sinh có bản ghi ghi danh trong lớp trước khi đọc hoặc ghi dữ liệu.
    private async Task<bool> ValidateStudentBelongsToClassAsync(int classId, int studentId)
    {
        var enrollments = await _classRepository.GetActiveStudentsByClassIdAsync(classId);

        return enrollments.Any(e => e.StudentId == studentId);
    }

    /// <summary>
    /// Kết hợp hồ sơ, tiến độ và phản hồi thành dữ liệu cho trang chi tiết học sinh.
    /// </summary>
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

            Level = studentProfile.Level,
            XP = studentProfile.XP,
            CurrentStreakDays = studentProfile.CurrentStreakDays,
            LastActiveDate = studentProfile.LastActiveDate,

            CompletedLessons = completedLessons,
            InProgressLessons = inProgressLessons,
            AverageQuizScore = averageQuizScore,
            TotalXPEarned = totalXPEarned,
            StudyDurationMinutes = progressRecords
                .Where(p => string.Equals(p.CompletionStatus, "Completed", StringComparison.OrdinalIgnoreCase))
                .Sum(p => p.Lesson?.EstimatedMinutes ?? 0),

            LessonProgresses = BuildLessonProgressViewModels(progressRecords),
            Feedbacks = BuildFeedbackViewModels(feedbacks)
        };
    }

    // Đếm các bài học đã hoàn thành.
    private static int CountCompletedLessons(List<EnglishLearningOnlineSystem.Models.Progress> progressRecords)
    {
        return progressRecords.Count(p =>
            string.Equals(p.CompletionStatus, "Completed", StringComparison.OrdinalIgnoreCase));
    }

    // Đếm các bài học chưa hoàn thành.
    private static int CountInProgressLessons(List<EnglishLearningOnlineSystem.Models.Progress> progressRecords)
    {
        return progressRecords.Count(p =>
            !string.Equals(p.CompletionStatus, "Completed", StringComparison.OrdinalIgnoreCase));
    }

    // Tính điểm quiz trung bình và làm tròn đến hai chữ số thập phân.
    private static double CalculateAverageQuizScore(List<EnglishLearningOnlineSystem.Models.Progress> progressRecords)
    {
        if (!progressRecords.Any())
        {
            return 0;
        }

        return Math.Round(progressRecords.Average(p => p.QuizScore), 2);
    }

    // Tính tổng XP học sinh đã nhận từ các bài học.
    private static int CalculateTotalXPEarned(List<EnglishLearningOnlineSystem.Models.Progress> progressRecords)
    {
        return progressRecords.Sum(p => p.XPEarned);
    }

    // Chuyển các bản ghi tiến độ sang dữ liệu hiển thị cho giáo viên.
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

    // Chuyển lịch sử phản hồi sang dữ liệu hiển thị và bổ sung nhãn đã đọc/chưa đọc.
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
    /// <summary>
    /// Kiểm tra quyền truy cập và chuẩn bị thông tin học sinh cho biểu mẫu phản hồi.
    /// </summary>
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

    /// <summary>
    /// Lưu phản hồi của giáo viên và tạo thông báo mới cho học sinh.
    /// </summary>
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
        await _classRepository.AddNotificationAsync(new Notification
        {
            UserId = model.StudentId,
            Type = "TEACHER_FEEDBACK",
            Message = "Giáo viên vừa gửi phản hồi cho bạn. Hãy kiểm tra ngay!",
            IsRead = false,
            CreateAt = DateTime.UtcNow
        });

        return true;
    }
}
