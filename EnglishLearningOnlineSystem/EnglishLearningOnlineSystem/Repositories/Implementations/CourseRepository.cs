using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Repositories.Implementations
{
    public class CourseRepository : ICourseRepository
    {
        private readonly AppDbContext _db;

        public CourseRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<(List<Course> Items, int TotalCount)> GetPagedAsync(string? keyword, bool? isActive, int pageNumber, int pageSize)
        {
            var query = _db.Courses!
                .AsNoTracking()
                .Include(c => c.Lessons)
                .Where(c => !c.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(c => c.CourseName.ToLower().Contains(kw));
            }

            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsPublished == isActive.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(c => c.CourseId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Course?> GetByIdAsync(int courseId)
        {
            return await _db.Courses!
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        public async Task<Course?> GetDetailByIdAsync(int courseId)
        {
            return await _db.Courses!
                .AsNoTracking()
                .Include(c => c.Creator)
                .Include(c => c.Lessons)
                .Include(c => c.Classes)
                    .ThenInclude(cl => cl.Enrollments)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        public async Task<bool> ExistsByNameAsync(string courseName, int? excludeCourseId = null)
        {
            var name = courseName.Trim().ToLower();
            var query = _db.Courses!.Where(c => c.CourseName.ToLower() == name);

            if (excludeCourseId.HasValue)
            {
                query = query.Where(c => c.CourseId != excludeCourseId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task AddAsync(Course course)
        {
            await _db.Courses!.AddAsync(course);
        }

        public void Update(Course course)
        {
            _db.Courses!.Update(course);
        }

        public async Task<bool> HasActiveLessonsAsync(int courseId)
        {
            var course = await _db.Courses!
                .AsNoTracking()
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            return course?.Lessons?.Any(l => l.IsPublished) ?? false;
        }

        public async Task<bool> HasEnrolledStudentsAsync(int courseId)
        {
            var course = await _db.Courses!
                .AsNoTracking()
                .Include(c => c.Classes)
                    .ThenInclude(cl => cl.Enrollments)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            return course?.Classes?.Any(cl => cl.Enrollments != null && cl.Enrollments.Any()) ?? false;
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}