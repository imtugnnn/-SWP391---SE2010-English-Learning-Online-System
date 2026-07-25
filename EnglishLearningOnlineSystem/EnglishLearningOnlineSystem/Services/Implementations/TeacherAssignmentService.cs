using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class TeacherAssignmentService : ITeacherAssignmentService
{
    private readonly IClassRepository _classRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly AppDbContext _context;

    public TeacherAssignmentService(
        IClassRepository classRepository,
        IAssignmentRepository assignmentRepository,
        AppDbContext context)
    {
        _classRepository = classRepository;
        _assignmentRepository = assignmentRepository;
        _context = context;
    }

    /// <summary>
    /// Nạp lớp, khóa học và danh sách bài học đã phát hành để tạo biểu mẫu giao bài tuần.
    /// </summary>
    public async Task<AssignWeeklyLessonViewModel?> GetAssignWeeklyLessonsFormAsync(
    int classId,
    int teacherId,
    int? selectedCourseId = null)
    {
        var classEntity = await ValidateTeacherAccessAsync(classId, teacherId);

        if (classEntity == null)
        {
            return null;
        }

        var courses = await _assignmentRepository.GetPublishedCoursesAsync();

        var courseIdToLoad = classEntity.CourseId ?? selectedCourseId;

        var lessons = courseIdToLoad.HasValue
            ? await _assignmentRepository.GetPublishedLessonsByCourseIdAsync(courseIdToLoad.Value)
            : new List<Lesson>();

        var selectedCourse = courses.FirstOrDefault(c => c.CourseId == courseIdToLoad);

        return new AssignWeeklyLessonViewModel
        {
            ClassId = classEntity.ClassId,
            ClassName = classEntity.ClassName,

            CourseId = classEntity.CourseId,
            SelectedCourseId = courseIdToLoad,
            CourseName = selectedCourse?.CourseName ?? "Chưa chọn chương trình học",

            WeekStartDate = DateTime.Today,
            DueDate = DateTime.Today.AddDays(7),

            Courses = courses.Select(c => new CourseOptionViewModel
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                GradeLevel = c.GradeLevel ?? "Chưa cập nhật"
            }).ToList(),

            Lessons = lessons.Select(l => new AssignLessonItemViewModel
            {
                LessonId = l.LessonId,
                Title = l.Title,
                Topic = l.Topic ?? "Chưa cập nhật",
                EstimatedMinutes = l.EstimatedMinutes,
                XPReward = l.XPReward
            }).ToList()
        };
    }

    /// <summary>
    /// Tạo bài giao cho các bài học hợp lệ và gửi thông báo nếu giáo viên phát hành ngay.
    /// </summary>
    public async Task<bool> AssignWeeklyLessonsAsync(
    AssignWeeklyLessonViewModel model,
    int teacherId)
    {
        var classEntity = await ValidateTeacherAccessAsync(model.ClassId, teacherId);

        if (classEntity == null)
        {
            return false;
        }

        if (!model.SelectedCourseId.HasValue)
        {
            return false;
        }

        if (!ValidateAssignmentInput(model))
        {
            return false;
        }

        var courseId = classEntity.CourseId ?? model.SelectedCourseId.Value;
        var selectedLessonIds = model.SelectedLessonIds
            .Distinct()
            .ToList();

        if (!await _assignmentRepository.ValidateLessonsBelongToCourseAsync(
                courseId,
                selectedLessonIds))
        {
            return false;
        }

        // Serializable giúp hai request đồng thời không cùng vượt qua bước kiểm tra trùng.
        using var transaction = await _context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable);

        // Loại bỏ những bài đã được giao trong cùng tuần để tránh tạo dữ liệu trùng.
        var existingLessonIds = await _assignmentRepository.GetAssignedLessonIdsAsync(
            courseId,
            selectedLessonIds,
            model.WeekStartDate);

        var newLessonIds = selectedLessonIds
            .Where(id => !existingLessonIds.Contains(id))
            .ToList();

        if (!newLessonIds.Any())
        {
            await transaction.RollbackAsync();
            return false;
        }

        var assignments = newLessonIds.Select(lessonId => new WeeklyAssignment
        {
            CourseId = courseId,
            LessonId = lessonId,
            WeekStartDate = model.WeekStartDate,
            DueDate = model.DueDate,
            IsVisible = model.Status == AssignmentStatus.Published
        }).ToList();

        // Việc cập nhật khóa học, tạo bài giao và thông báo phải thành công như một đơn vị.
        try
        {
            if (classEntity.CourseId == null)
            {
                await _classRepository.UpdateClassCourseAsync(
                    classEntity.ClassId,
                    courseId);
            }

            await _assignmentRepository.AddWeeklyAssignmentsAsync(assignments);

            if (model.Status == AssignmentStatus.Published)
            {
                var students = await _classRepository.GetActiveStudentsByClassIdAsync(model.ClassId);
                var notifications = students.Select(student => new Notification
                {
                    UserId = student.StudentId,
                    Type = "NEW_ASSIGNMENT",
                    Message = $"Giáo viên vừa giao bài học mới từ {model.WeekStartDate:dd/MM/yyyy} đến {model.DueDate:dd/MM/yyyy}.",
                    IsRead = false,
                    CreateAt = DateTime.UtcNow
                }).ToList();

                await _classRepository.AddNotificationsAsync(notifications);
            }
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Xác nhận lớp tồn tại và thuộc quyền quản lý của giáo viên hiện tại.
    /// </summary>
    private async Task<Class?> ValidateTeacherAccessAsync(int classId, int teacherId)
    {
        var classEntity = await _classRepository.GetClassDetailByIdAsync(classId);

        if (classEntity == null || classEntity.IsDeleted)
        {
            return null;
        }

        if (classEntity.TeacherId != teacherId)
        {
            return null;
        }

        return classEntity;
    }

    /// <summary>
    /// Kiểm tra các điều kiện nghiệp vụ cơ bản trước khi lưu bài giao.
    /// </summary>
    private static bool ValidateAssignmentInput(AssignWeeklyLessonViewModel model)
    {
        if (model.SelectedLessonIds == null || !model.SelectedLessonIds.Any())
        {
            return false;
        }

        if (model.DueDate <= model.WeekStartDate)
        {
            return false;
        }

        if (model.Status != AssignmentStatus.Draft &&
            model.Status != AssignmentStatus.Published)
        {
            return false;
        }

        return true;
    }
    /// <summary>
    /// Lấy danh sách bài giao thuộc các lớp của giáo viên, áp dụng bộ lọc và phân trang.
    /// </summary>
    public async Task<TeacherAssignmentOverviewViewModel> GetAssignmentOverviewAsync(
    int? classId,
    int teacherId,
    string? status,
    string? sortBy,
    int page)
    {
        var classes = await _classRepository.GetClassesByTeacherIdAsync(teacherId);

        var courseIds = classes
            .Where(c => c.CourseId.HasValue)
            .Select(c => c.CourseId!.Value)
            .Distinct()
            .ToList();

        var assignments = await _assignmentRepository.GetAssignmentsByCourseIdsAsync(courseIds);

        var totalAssignments = assignments.Count;
        var draftAssignments = assignments.Count(a => !a.IsVisible);
        var activeAssignments = assignments.Count(a => a.IsVisible && a.DueDate >= DateTime.UtcNow);
        var expiredAssignments = assignments.Count(a => a.IsVisible && a.DueDate < DateTime.UtcNow);

        var filteredAssignments = ApplyAssignmentStatusFilter(assignments, status);
        filteredAssignments = ApplyAssignmentSorting(filteredAssignments, sortBy);

        const int pageSize = 10;
        page = page < 1 ? 1 : page;

        var totalItems = filteredAssignments.Count;
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        if (totalPages > 0 && page > totalPages)
        {
            page = totalPages;
        }

        var pagedAssignments = filteredAssignments
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new TeacherAssignmentOverviewViewModel
        {
            ClassId = classId,
            Status = NormalizeAssignmentStatus(status),
            SortBy = NormalizeAssignmentSort(sortBy),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,

            TotalAssignments = totalAssignments,
            DraftAssignments = draftAssignments,
            ActiveAssignments = activeAssignments,
            ExpiredAssignments = expiredAssignments,

            Assignments = pagedAssignments.Select(a => new TeacherAssignmentItemViewModel
            {
                AssignmentId = a.AssignmentId,
                LessonTitle = a.Lesson?.Title ?? "Bài học chưa xác định",
                Topic = a.Lesson?.Topic ?? "Chưa cập nhật",
                WeekStartDate = a.WeekStartDate,
                DueDate = a.DueDate,
                Status = !a.IsVisible
                    ? "Bản nháp"
                    : a.DueDate < DateTime.UtcNow ? "Quá hạn" : "Đang hoạt động"
            }).ToList()
        };
    }

    /// <summary>
    /// Lọc bài giao theo trạng thái nháp, đang hoạt động hoặc quá hạn.
    /// </summary>
    private static List<WeeklyAssignment> ApplyAssignmentStatusFilter(
        List<WeeklyAssignment> assignments,
        string? status)
    {
        var normalizedStatus = NormalizeAssignmentStatus(status);

        return normalizedStatus switch
        {
            "draft" => assignments.Where(a => !a.IsVisible).ToList(),
            "active" => assignments.Where(a => a.IsVisible && a.DueDate >= DateTime.UtcNow).ToList(),
            "expired" => assignments.Where(a => a.IsVisible && a.DueDate < DateTime.UtcNow).ToList(),
            _ => assignments
        };
    }

    /// <summary>
    /// Sắp xếp bài giao theo lựa chọn trên màn hình tổng quan.
    /// </summary>
    private static List<WeeklyAssignment> ApplyAssignmentSorting(
        List<WeeklyAssignment> assignments,
        string? sortBy)
    {
        var normalizedSort = NormalizeAssignmentSort(sortBy);

        return normalizedSort switch
        {
            "startDate" => assignments.OrderByDescending(a => a.WeekStartDate).ToList(),
            "lesson" => assignments.OrderBy(a => a.Lesson != null ? a.Lesson.Title : "").ToList(),
            _ => assignments.OrderBy(a => a.DueDate).ToList()
        };
    }

    // Chuẩn hóa giá trị bộ lọc để việc so sánh không phụ thuộc hoa/thường.
    private static string NormalizeAssignmentStatus(string? status)
    {
        return string.IsNullOrWhiteSpace(status)
            ? "all"
            : status.Trim().ToLower();
    }

    // Chuẩn hóa tiêu chí sắp xếp và dùng giá trị mặc định khi đầu vào rỗng.
    private static string NormalizeAssignmentSort(string? sortBy)
    {
        return string.IsNullOrWhiteSpace(sortBy)
            ? "dueDate"
            : sortBy.Trim();
    }

    /// <summary>
    /// Phát hành bản nháp thuộc chương trình học của lớp và thông báo cho học sinh.
    /// </summary>
    public async Task<bool> PublishDraftAsync(int assignmentId, int classId, int teacherId)
    {
        var classEntity = await ValidateTeacherAccessAsync(classId, teacherId);
        if (classEntity?.CourseId == null)
        {
            return false;
        }

        using var transaction = await _context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable);

        try
        {
            var assignment = await _context.WeeklyAssignments!
                .FirstOrDefaultAsync(a =>
                    a.AssignmentId == assignmentId &&
                    a.CourseId == classEntity.CourseId);

            if (assignment == null || assignment.IsVisible)
            {
                await transaction.RollbackAsync();
                return false;
            }

            var hasPublishedDuplicate = await _context.WeeklyAssignments!
                .AnyAsync(a =>
                    a.AssignmentId != assignment.AssignmentId &&
                    a.CourseId == assignment.CourseId &&
                    a.LessonId == assignment.LessonId &&
                    a.WeekStartDate.Date == assignment.WeekStartDate.Date &&
                    a.IsVisible);

            if (hasPublishedDuplicate)
            {
                await transaction.RollbackAsync();
                return false;
            }

            assignment.IsVisible = true;

            var students = await _classRepository.GetActiveStudentsByClassIdAsync(classId);
            var notifications = students.Select(student => new Notification
            {
                UserId = student.StudentId,
                Type = "NEW_ASSIGNMENT",
                Message = $"Giáo viên vừa giao bài học mới từ {assignment.WeekStartDate:dd/MM/yyyy} đến {assignment.DueDate:dd/MM/yyyy}.",
                IsRead = false,
                CreateAt = DateTime.UtcNow
            }).ToList();

            await _classRepository.AddNotificationsAsync(notifications);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
