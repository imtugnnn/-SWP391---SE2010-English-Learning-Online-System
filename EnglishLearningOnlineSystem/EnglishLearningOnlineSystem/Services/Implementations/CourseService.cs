using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels.ContentManager.Courses;

namespace EnglishLearningOnlineSystem.Services.Implementations
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;

        public CourseService(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<(List<CourseListItemViewModel> Items, int TotalCount)> GetCoursesAsync(string? keyword, bool? isActive, int pageNumber, int pageSize)
        {
            var (courses, totalCount) = await _courseRepository.GetPagedAsync(keyword, isActive, pageNumber, pageSize);

            var items = courses.Select(c => new CourseListItemViewModel
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                GradeLevel = c.GradeLevel,
                IsPublished = c.IsPublished,
                LessonCount = c.Lessons?.Count ?? 0
            }).ToList();

            return (items, totalCount);
        }

        public async Task<CourseDetailViewModel?> GetCourseDetailAsync(int courseId)
        {
            var course = await _courseRepository.GetDetailByIdAsync(courseId);
            if (course == null) return null;

            var lessons = course.Lessons ?? new List<Lesson>();

            return new CourseDetailViewModel
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                GradeLevel = course.GradeLevel,
                Description = course.Description,
                IsPublished = course.IsPublished,
                IsDeleted = course.IsDeleted,
                CreatorName = course.Creator?.Username,
                LessonCount = lessons.Count,
                TotalDurationMinutes = lessons.Sum(l => l.EstimatedMinutes),
                Lessons = lessons
                    .OrderBy(l => l.OrderIndex)
                    .Select(l => new CourseDetailViewModel.CourseLessonItem
                    {
                        LessonId = l.LessonId,
                        Title = l.Title,
                        Topic = l.Topic,
                        EstimatedMinutes = l.EstimatedMinutes,
                        IsPublished = l.IsPublished,
                        OrderIndex = l.OrderIndex
                    }).ToList()
            };
        }

        public async Task<(CourseEditViewModel? Model, string? ErrorMessage)> GetCourseForEditAsync(int courseId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null) return (null, "Không tìm thấy khoá học.");

            if (course.IsDeleted) return (null, "Khoá học đã bị xoá, không thể chỉnh sửa.");

            var lessons = course.Lessons ?? new List<Lesson>();

            return (new CourseEditViewModel
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                GradeLevel = course.GradeLevel,
                Description = course.Description,
                HasLessons = lessons.Any(),
                LessonCount = lessons.Count,
                TotalDurationMinutes = lessons.Sum(l => l.EstimatedMinutes)
            }, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateCourseAsync(CourseCreateViewModel model, int? creatorId)
        {
            if (await _courseRepository.ExistsByNameAsync(model.CourseName))
                return (false, "Tên khoá học này đã tồn tại. Vui lòng chọn tên khác.");

            // Quy tắc: khoá học mới chưa có bài học => luôn ở trạng thái chưa kích hoạt.
            var course = new Course
            {
                CourseName = model.CourseName.Trim(),
                GradeLevel = model.GradeLevel.Trim(),
                Description = model.Description?.Trim(),
                IsPublished = false,
                IsDeleted = false,
                CreatorId = creatorId
            };

            await _courseRepository.AddAsync(course);
            await _courseRepository.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateCourseAsync(CourseEditViewModel model)
        {
            var course = await _courseRepository.GetByIdAsync(model.CourseId);
            if (course == null) return (false, "Không tìm thấy khoá học.");

            // Quy tắc: không cho sửa khoá học đã xoá.
            if (course.IsDeleted) return (false, "Khoá học đã bị xoá, không thể chỉnh sửa.");

            if (await _courseRepository.ExistsByNameAsync(model.CourseName, model.CourseId))
                return (false, "Tên khoá học này đã tồn tại. Vui lòng chọn tên khác.");

            // Quy tắc: chỉ cho sửa Tên/Mô tả/Giá/Khối lớp. Duration không có trong
            // ViewModel này nên không thể bị ghi đè thủ công — nó luôn được tính lại
            // từ Lessons mỗi khi hiển thị (xem GetCourseForEditAsync / GetCourseDetailAsync).
            course.CourseName = model.CourseName.Trim();
            course.GradeLevel = model.GradeLevel.Trim();
            course.Description = model.Description?.Trim();

            _courseRepository.Update(course);
            await _courseRepository.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> ToggleStatusAsync(int courseId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null) return (false, "Không tìm thấy khoá học.");
            if (course.IsDeleted) return (false, "Khoá học đã bị xoá, không thể đổi trạng thái.");

            var lessons = course.Lessons ?? new List<Lesson>();

            if (!course.IsPublished)
            {
                // Inactive -> Active: bắt buộc phải có ít nhất 1 bài học.
                if (!lessons.Any())
                    return (false, "Không thể kích hoạt khoá học chưa có bài học nào.");

                course.IsPublished = true;
            }
            else
            {
                // Active -> Inactive: toàn bộ bài học liên quan cũng phải tắt xuất bản.
                course.IsPublished = false;
                foreach (var lesson in lessons)
                {
                    lesson.IsPublished = false;
                }
            }

            _courseRepository.Update(course);
            await _courseRepository.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteCourseAsync(int courseId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null) return (false, "Không tìm thấy khoá học.");
            if (course.IsDeleted) return (false, "Khoá học này đã được xoá trước đó.");

            if (await _courseRepository.HasActiveLessonsAsync(courseId))
                return (false, "Không thể xoá khoá học vì vẫn còn bài học đang hoạt động (đã xuất bản).");

            if (await _courseRepository.HasEnrolledStudentsAsync(courseId))
                return (false, "Không thể xoá khoá học vì đã có học sinh đăng ký.");

            // Soft delete — chỉ đánh dấu cờ, không xoá dữ liệu.
            course.IsDeleted = true;
            course.IsPublished = false;

            _courseRepository.Update(course);
            await _courseRepository.SaveChangesAsync();
            return (true, null);
        }
    }
}