using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class LessonService : ILessonService
{
    private readonly ILessonRepository _lessonRepo;

    public LessonService(ILessonRepository lessonRepo)
    {
        _lessonRepo = lessonRepo;
    }

    public async Task<List<Lesson>> GetAllLessonsAsync()
    {
        return await _lessonRepo.GetAllLessonsWithCourseAsync();
    }

    public async Task<(List<EnglishLearningOnlineSystem.ViewModels.ContentManager.Lessons.LessonListItemViewModel> Items, int TotalCount)> GetLessonsAsync(string? keyword, int? courseId, int page, int pageSize)
    {
        var (lessons, totalCount) = await _lessonRepo.GetLessonsPaginatedAsync(keyword, courseId, page, pageSize);
        var items = lessons.Select(l => new EnglishLearningOnlineSystem.ViewModels.ContentManager.Lessons.LessonListItemViewModel
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

    public async Task<(EnglishLearningOnlineSystem.ViewModels.ContentManager.Lessons.LessonEditViewModel? Model, string? ErrorMessage)> GetLessonForEditAsync(int id)
    {
        var l = await _lessonRepo.GetLessonByIdAsync(id);
        if (l == null) return (null, "Không tìm thấy bài học.");

        var model = new EnglishLearningOnlineSystem.ViewModels.ContentManager.Lessons.LessonEditViewModel
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

    public async Task<(bool Success, string? ErrorMessage)> CreateLessonAsync(EnglishLearningOnlineSystem.ViewModels.ContentManager.Lessons.LessonCreateViewModel model)
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

    public async Task<(bool Success, string? ErrorMessage)> UpdateLessonAsync(EnglishLearningOnlineSystem.ViewModels.ContentManager.Lessons.LessonEditViewModel model)
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
}
