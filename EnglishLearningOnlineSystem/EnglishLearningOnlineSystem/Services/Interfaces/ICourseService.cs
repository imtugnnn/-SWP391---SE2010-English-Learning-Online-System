using EnglishLearningOnlineSystem.ViewModels.ContentManager.Courses;

namespace EnglishLearningOnlineSystem.Services.Interfaces
{
    public interface ICourseService
    {
        Task<(List<CourseListItemViewModel> Items, int TotalCount)> GetCoursesAsync(string? keyword, bool? isActive, int pageNumber, int pageSize);
        Task<CourseDetailViewModel?> GetCourseDetailAsync(int courseId);
        Task<(CourseEditViewModel? Model, string? ErrorMessage)> GetCourseForEditAsync(int courseId);
        Task<(bool Success, string? ErrorMessage)> CreateCourseAsync(CourseCreateViewModel model, int? creatorId);
        Task<(bool Success, string? ErrorMessage)> UpdateCourseAsync(CourseEditViewModel model);
        Task<(bool Success, string? ErrorMessage)> ToggleStatusAsync(int courseId);
        Task<(bool Success, string? ErrorMessage)> DeleteCourseAsync(int courseId);
    }
}