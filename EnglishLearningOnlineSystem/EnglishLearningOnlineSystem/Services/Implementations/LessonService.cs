using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels.ContentManager.Lessons;
using EnglishLearningOnlineSystem.ViewModels.ContentManager.Minigames;
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

    public async Task<List<Lesson>> GetAllLessonsAsync()
    {
        return await _lessonRepo.GetAllLessonsWithCourseAsync();
    }

    public async Task<(List<LessonListItemViewModel> Items, int TotalCount)> GetLessonsAsync(
        string? keyword, int? courseId, int page, int pageSize)
    {
        var (lessons, totalCount) = await _lessonRepo.GetLessonsPaginatedAsync(keyword, courseId, page, pageSize);

        var items = lessons.Select(l => new LessonListItemViewModel
        {
            LessonId = l.LessonId,
            Title = l.Title,
            Topic = l.Topic,
            EstimatedMinutes = l.EstimatedMinutes,
            IsPublished = l.IsPublished,
            CourseName = l.Course?.CourseName ?? ""
        }).ToList();

        return (items, totalCount);
    }

    public async Task<LessonDetailsViewModel?> GetDetailsAsync(int lessonId)
    {
        var lesson = await _lessonRepo.GetLessonByIdAsync(lessonId);
        if (lesson == null) return null;

        var miniGames = await _db.MiniGames!
            .AsNoTracking()
            .Where(g => g.LessonId == lessonId)
            .OrderBy(g => g.Title)
            .Select(g => new MiniGameListItemViewModel
            {
                GameId = g.GameId,
                Title = g.Title,
                GameType = g.GameType,
                XPReward = g.XPReward,
                LessonId = g.LessonId,
                LessonTitle = lesson.Title
            })
            .ToListAsync();

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
            CourseGradeLevel = lesson.Course?.GradeLevel ?? "—",
            MiniGames = miniGames
        };
    }

    public async Task<(LessonEditViewModel? Model, string? ErrorMessage)> GetLessonForEditAsync(int id)
    {
        var l = await _lessonRepo.GetLessonByIdAsync(id);
        if (l == null) return (null, "Không tìm thấy bài học.");

        var model = new LessonEditViewModel
        {
            LessonId = l.LessonId,
            CourseId = l.CourseId,
            Title = l.Title,
            Topic = l.Topic,
            XPReward = l.XPReward,
            EstimatedMinutes = l.EstimatedMinutes,
            OrderIndex = l.OrderIndex,
            IsPublished = l.IsPublished
        };

        return (model, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> CreateLessonAsync(LessonCreateViewModel model)
    {
        var lesson = new Lesson
        {
            CourseId = model.CourseId,
            Title = model.Title,
            Topic = model.Topic,
            XPReward = model.XPReward,
            EstimatedMinutes = model.EstimatedMinutes,
            OrderIndex = model.OrderIndex,
            IsPublished = model.IsPublished
        };

        await _lessonRepo.AddLessonAsync(lesson);
        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateLessonAsync(LessonEditViewModel model)
    {
        var lesson = await _lessonRepo.GetLessonByIdAsync(model.LessonId);
        if (lesson == null) return (false, "Không tìm thấy bài học.");

        lesson.CourseId = model.CourseId;
        lesson.Title = model.Title;
        lesson.Topic = model.Topic;
        lesson.XPReward = model.XPReward;
        lesson.EstimatedMinutes = model.EstimatedMinutes;
        lesson.OrderIndex = model.OrderIndex;
        lesson.IsPublished = model.IsPublished;

        await _lessonRepo.UpdateLessonAsync(lesson);
        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteLessonAsync(int id)
    {
        var lesson = await _lessonRepo.GetLessonByIdAsync(id);
        if (lesson == null) return (false, "Không tìm thấy bài học.");

        await _lessonRepo.DeleteLessonAsync(lesson);
        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> TogglePublishedAsync(int lessonId)
    {
        var lesson = await _lessonRepo.GetLessonByIdAsync(lessonId);
        if (lesson == null) return (false, "Không tìm thấy bài học.");

        lesson.IsPublished = !lesson.IsPublished;
        await _lessonRepo.UpdateLessonAsync(lesson);
        return (true, null);
    }
}