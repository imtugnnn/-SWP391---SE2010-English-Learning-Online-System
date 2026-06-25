using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces
{
    public interface ICourseRepository
    {
        Task<(List<Course> Items, int TotalCount)> GetPagedAsync(string? keyword, bool? isActive, int pageNumber, int pageSize);
        Task<Course?> GetByIdAsync(int courseId);
        Task<Course?> GetDetailByIdAsync(int courseId);
        Task<bool> ExistsByNameAsync(string courseName, int? excludeCourseId = null);
        Task AddAsync(Course course);
        void Update(Course course);
        Task<bool> HasActiveLessonsAsync(int courseId);
        Task<bool> HasEnrolledStudentsAsync(int courseId);
        Task SaveChangesAsync();
    }
}