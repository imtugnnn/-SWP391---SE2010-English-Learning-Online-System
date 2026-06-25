using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class TeacherAssignmentService : ITeacherAssignmentService
{
    private readonly IClassRepository _classRepository;
    private readonly IAssignmentRepository _assignmentRepository;

    public TeacherAssignmentService(
        IClassRepository classRepository,
        IAssignmentRepository assignmentRepository)
    {
        _classRepository = classRepository;
        _assignmentRepository = assignmentRepository;
    }

    public async Task<AssignWeeklyLessonViewModel?> GetAssignWeeklyLessonsFormAsync(
        int classId,
        int teacherId)
    {
        var classEntity = await ValidateTeacherAccessAsync(classId, teacherId);

        if (classEntity == null || classEntity.CourseId == null)
        {
            return null;
        }

        var lessons = await _assignmentRepository.GetPublishedLessonsByCourseIdAsync(
            classEntity.CourseId.Value);

        return new AssignWeeklyLessonViewModel
        {
            ClassId = classEntity.ClassId,
            ClassName = classEntity.ClassName,
            CourseId = classEntity.CourseId.Value,
            WeekStartDate = DateTime.Today,
            DueDate = DateTime.Today.AddDays(7),
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

    public async Task<bool> AssignWeeklyLessonsAsync(
        AssignWeeklyLessonViewModel model,
        int teacherId)
    {
        var classEntity = await ValidateTeacherAccessAsync(model.ClassId, teacherId);

        if (classEntity == null || classEntity.CourseId == null)
        {
            return false;
        }

        if (!ValidateAssignmentInput(model))
        {
            return false;
        }

        var selectedLessonIds = model.SelectedLessonIds.Distinct().ToList();

        var existingLessonIds = await _assignmentRepository.GetAssignedLessonIdsAsync(
            classEntity.CourseId.Value,
            selectedLessonIds,
            model.WeekStartDate);

        var newLessonIds = selectedLessonIds
            .Where(id => !existingLessonIds.Contains(id))
            .ToList();

        if (!newLessonIds.Any())
        {
            return false;
        }

        var assignments = newLessonIds.Select(lessonId => new WeeklyAssignment
        {
            CourseId = classEntity.CourseId.Value,
            LessonId = lessonId,
            WeekStartDate = model.WeekStartDate,
            DueDate = model.DueDate,
            IsVisible = true
        }).ToList();

        await _assignmentRepository.AddWeeklyAssignmentsAsync(assignments);

        return true;
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

    private static bool ValidateAssignmentInput(AssignWeeklyLessonViewModel model)
    {
        if (model.SelectedLessonIds == null || !model.SelectedLessonIds.Any())
        {
            return false;
        }

        if (model.DueDate < model.WeekStartDate)
        {
            return false;
        }

        return true;
    }
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
        var activeAssignments = assignments.Count(a => a.DueDate >= DateTime.UtcNow);
        var expiredAssignments = assignments.Count(a => a.DueDate < DateTime.UtcNow);

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
            ActiveAssignments = activeAssignments,
            ExpiredAssignments = expiredAssignments,

            Assignments = pagedAssignments.Select(a => new TeacherAssignmentItemViewModel
            {
                AssignmentId = a.AssignmentId,
                LessonTitle = a.Lesson?.Title ?? "Bài học chưa xác định",
                Topic = a.Lesson?.Topic ?? "Chưa cập nhật",
                WeekStartDate = a.WeekStartDate,
                DueDate = a.DueDate,
                Status = a.DueDate < DateTime.UtcNow ? "Quá hạn" : "Đang hoạt động"
            }).ToList()
        };
    }

    private static List<WeeklyAssignment> ApplyAssignmentStatusFilter(
        List<WeeklyAssignment> assignments,
        string? status)
    {
        var normalizedStatus = NormalizeAssignmentStatus(status);

        return normalizedStatus switch
        {
            "active" => assignments.Where(a => a.DueDate >= DateTime.UtcNow).ToList(),
            "expired" => assignments.Where(a => a.DueDate < DateTime.UtcNow).ToList(),
            _ => assignments
        };
    }

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

    private static string NormalizeAssignmentStatus(string? status)
    {
        return string.IsNullOrWhiteSpace(status)
            ? "all"
            : status.Trim().ToLower();
    }

    private static string NormalizeAssignmentSort(string? sortBy)
    {
        return string.IsNullOrWhiteSpace(sortBy)
            ? "dueDate"
            : sortBy.Trim();
    }
}