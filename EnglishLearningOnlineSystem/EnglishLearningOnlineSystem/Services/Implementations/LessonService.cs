using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;
using EnglishLearningOnlineSystem.ViewModels.ContentManager.Lessons;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class LessonService : ILessonService
{
    private readonly ILessonRepository _lessonRepo;
    private readonly AppDbContext _db;

    public LessonService(ILessonRepository lessonRepo, AppDbContext db)
    {
        _lessonRepo = lessonRepo;
        _db = db;
    }

    public async Task<LessonListViewModel> GetPagedAsync(
        int? courseId,
        string? searchTitle,
        int page,
        int pageSize)
    {
        var (items, total) = await _lessonRepo.GetPagedAsync(courseId, searchTitle, page, pageSize);

        var courses = await _db.Courses!
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.CourseName)
            .Select(c => new CourseSelectItem { CourseId = c.CourseId, CourseName = c.CourseName })
            .ToListAsync();

        return new LessonListViewModel
        {
            Items = items.Select(l => new LessonListItemViewModel
            {
                LessonId = l.LessonId,
                Title = l.Title,
                Topic = l.Topic,
                OrderIndex = l.OrderIndex,
                EstimatedMinutes = l.EstimatedMinutes,
                XPReward = l.XPReward,
                IsPublished = l.IsPublished,
                CourseName = l.Course?.CourseName ?? "—",
                CourseId = l.CourseId
            }).ToList(),
            TotalCount = total,
            CurrentPage = page,
            PageSize = pageSize,
            SearchTitle = searchTitle,
            FilterCourseId = courseId,
            Courses = courses
        };
    }

    public async Task<LessonViewModel?> GetByIdAsync(int lessonId)
    {
        var lesson = await _lessonRepo.GetByIdWithCourseAsync(lessonId);
        if (lesson == null) return null;

        return MapToViewModel(lesson);
    }

    public async Task<LessonDetailsViewModel?> GetDetailsAsync(int lessonId)
    {
        var lesson = await _lessonRepo.GetByIdWithCourseAsync(lessonId);
        if (lesson == null) return null;

        return new LessonDetailsViewModel
        {
            LessonId = lesson.LessonId,
            Title = lesson.Title,
            Topic = lesson.Topic,
            OrderIndex = lesson.OrderIndex,
            EstimatedMinutes = lesson.EstimatedMinutes,
            XPReward = lesson.XPReward,
            IsPublished = lesson.IsPublished,
            CourseId = lesson.CourseId,
            CourseName = lesson.Course?.CourseName ?? "—",
            CourseGradeLevel = lesson.Course?.GradeLevel ?? "—"
        };
    }

    public async Task<CreateLessonViewModel> BuildCreateViewModelAsync(int? preselectedCourseId = null)
    {
        if (preselectedCourseId == null)
            return new CreateLessonViewModel();

        var course = await _db.Courses
            .FirstOrDefaultAsync(x => x.CourseId == preselectedCourseId);

        if (course == null)
            return new CreateLessonViewModel();

        return new CreateLessonViewModel
        {
            CourseId = course.CourseId,
            CourseName = course.CourseName
        };
    }

    public async Task<EditLessonViewModel?> BuildEditViewModelAsync(int lessonId)
    {
        var lesson = await _lessonRepo.GetByIdWithCourseAsync(lessonId);
        if (lesson == null) return null;

        var courses = await GetActiveCourseSelectItemsAsync();

        return new EditLessonViewModel
        {
            LessonId = lesson.LessonId,
            Title = lesson.Title,
            Topic = lesson.Topic,
            OrderIndex = lesson.OrderIndex,
            EstimatedMinutes = lesson.EstimatedMinutes,
            XPReward = lesson.XPReward,
            IsPublished = lesson.IsPublished,
            CourseId = lesson.CourseId,
            CourseName = lesson.Course?.CourseName ?? "—",
            Courses = courses
        };
    }

    public async Task<string?> CreateAsync(CreateLessonViewModel vm, int creatorId)
    {
        var course = await _db.Courses!
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CourseId == vm.CourseId);

        if (course == null || course.IsDeleted)
            return "The selected course does not exist or has been deleted.";

        var lesson = new Lesson
        {
            Title = vm.Title.Trim(),
            Topic = vm.Topic?.Trim() ?? string.Empty,
            OrderIndex = vm.OrderIndex,
            EstimatedMinutes = vm.EstimatedMinutes,
            XPReward = vm.XPReward,
            IsPublished = vm.IsPublished,
            CourseId = vm.CourseId
        };

        await _lessonRepo.AddAsync(lesson);
        await _lessonRepo.SaveChangesAsync();
        return null;
    }

    public async Task<string?> UpdateAsync(EditLessonViewModel vm)
    {
        // Re-fetch tracked entity
        var lesson = await _db.Lessons!.FirstOrDefaultAsync(l => l.LessonId == vm.LessonId);
        if (lesson == null) return "Lesson not found.";

        lesson.Title = vm.Title.Trim();
        lesson.Topic = vm.Topic?.Trim() ?? string.Empty;
        lesson.OrderIndex = vm.OrderIndex;
        lesson.EstimatedMinutes = vm.EstimatedMinutes;
        lesson.XPReward = vm.XPReward;
        lesson.IsPublished = vm.IsPublished;
        // CourseId is intentionally not updated after creation.

        _lessonRepo.Update(lesson);
        await _lessonRepo.SaveChangesAsync();
        return null;
    }

    public async Task<string?> DeleteAsync(int lessonId)
    {
        var lesson = await _db.Lessons!
            .FirstOrDefaultAsync(l => l.LessonId == lessonId);

        if (lesson == null)
            return "Lesson not found.";

        var hasAttempts = await _db.QuizAttempts
            .AnyAsync(x => x.LessonId == lessonId);

        if (hasAttempts)
            return "Không thể xóa bài học này vì học sinh đã làm bài kiểm tra.";

        var hasAssignments = await _db.WeeklyAssignments
            .AnyAsync(x => x.LessonId == lessonId);

        if (hasAssignments)
            return "Không thể xóa bài học này vì nó nằm trong danh sách bài tập hàng tuần.";

        _lessonRepo.Delete(lesson);
        await _lessonRepo.SaveChangesAsync();

        return null;
    }

    public async Task<string?> TogglePublishedAsync(int lessonId)
    {
        var lesson = await _db.Lessons!.FirstOrDefaultAsync(l => l.LessonId == lessonId);
        if (lesson == null) return "Lesson not found.";

        lesson.IsPublished = !lesson.IsPublished;

        _lessonRepo.Update(lesson);
        await _lessonRepo.SaveChangesAsync();
        return null;
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private async Task<List<CourseSelectItem>> GetActiveCourseSelectItemsAsync()
    {
        return await _db.Courses!
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.CourseName)
            .Select(c => new CourseSelectItem { CourseId = c.CourseId, CourseName = c.CourseName })
            .ToListAsync();
    }

    private static LessonViewModel MapToViewModel(Lesson l) => new()
    {
        LessonId = l.LessonId,
        Title = l.Title,
        Topic = l.Topic,
        OrderIndex = l.OrderIndex,
        EstimatedMinutes = l.EstimatedMinutes,
        XPReward = l.XPReward,
        IsPublished = l.IsPublished,
        CourseId = l.CourseId,
        CourseName = l.Course?.CourseName ?? "—"
    };
}