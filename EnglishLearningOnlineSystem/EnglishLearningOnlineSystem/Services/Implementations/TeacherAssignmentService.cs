using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;
using System.Data;
using System.Text.Json;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class TeacherAssignmentService : ITeacherAssignmentService
{
    private readonly IClassRepository _classRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TeacherAssignmentService(
        IClassRepository classRepository,
        IAssignmentRepository assignmentRepository,
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _classRepository = classRepository;
        _assignmentRepository = assignmentRepository;
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
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
        var activeStudents = await _classRepository.GetActiveStudentsByClassIdAsync(classEntity.ClassId);

        return new AssignWeeklyLessonViewModel
        {
            ClassId = classEntity.ClassId,
            ClassName = classEntity.ClassName,

            CourseId = classEntity.CourseId,
            SelectedCourseId = courseIdToLoad,
            CourseName = selectedCourse?.CourseName ?? "Chưa chọn chương trình học",
            ActiveStudentCount = activeStudents.Count,

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
                XPReward = l.XPReward,
                VocabularyCount = l.Vocabularies?.Count ?? 0,
                QuizCount = l.Quizzes?.Count ?? 0,
                MiniGameCount = l.MiniGames?.Count ?? 0,
                IncludeVocabulary = (l.Vocabularies?.Count ?? 0) > 0,
                IncludeQuiz = (l.Quizzes?.Count ?? 0) > 0,
                IncludeMiniGame = (l.MiniGames?.Count ?? 0) > 0,
                SelectedVocabularyIds = l.Vocabularies?.Select(v => v.VocabularyId).ToList() ?? new(),
                SelectedQuizIds = l.Quizzes?.Select(q => q.QuizId).ToList() ?? new(),
                SelectedMiniGameIds = l.MiniGames?.Select(g => g.GameId).ToList() ?? new(),
                Vocabularies = l.Vocabularies?
                    .OrderBy(v => v.Word)
                    .Select(v => new AssignmentVocabularyOptionViewModel
                    {
                        VocabularyId = v.VocabularyId,
                        Word = v.Word,
                        Meaning = v.Meaning
                    }).ToList() ?? new(),
                Quizzes = l.Quizzes?
                    .OrderBy(q => q.QuizId)
                    .Select(q => new AssignmentQuizOptionViewModel
                    {
                        QuizId = q.QuizId,
                        Question = q.Question,
                        QuizType = q.QuizType ?? "Quiz"
                    }).ToList() ?? new(),
                MiniGames = l.MiniGames?
                    .OrderBy(g => g.Title)
                    .Select(g => new AssignmentMiniGameOptionViewModel
                    {
                        GameId = g.GameId,
                        Title = g.Title,
                        GameType = g.GameType ?? "MiniGame"
                    }).ToList() ?? new()
            }).ToList()
        };
    }

    /// <summary>
    /// Luồng hiển thị lại form: nạp lại dữ liệu chuẩn từ database rồi ghép các lựa chọn
    /// người dùng vừa gửi để giao diện không bị mất trạng thái sau khi có lỗi.
    /// </summary>
    public async Task<AssignWeeklyLessonViewModel?> RebuildAssignWeeklyLessonsFormAsync(
        AssignWeeklyLessonViewModel postedModel,
        int teacherId)
    {
        var formModel = await GetAssignWeeklyLessonsFormAsync(
            postedModel.ClassId,
            teacherId,
            postedModel.SelectedCourseId);

        if (formModel == null)
        {
            return null;
        }

        formModel.WeekStartDate = postedModel.WeekStartDate;
        formModel.DueDate = postedModel.DueDate;
        formModel.SelectedLessonIds = postedModel.SelectedLessonIds ?? new List<int>();
        formModel.SelectedCourseId = postedModel.SelectedCourseId;
        formModel.Status = postedModel.Status;

        foreach (var postedLesson in postedModel.Lessons ?? new List<AssignLessonItemViewModel>())
        {
            var targetLesson = formModel.Lessons
                .FirstOrDefault(lesson => lesson.LessonId == postedLesson.LessonId);

            if (targetLesson == null)
            {
                continue;
            }

            targetLesson.IncludeVocabulary = postedLesson.IncludeVocabulary;
            targetLesson.IncludeQuiz = postedLesson.IncludeQuiz;
            targetLesson.IncludeMiniGame = postedLesson.IncludeMiniGame;
            targetLesson.SelectedVocabularyIds = postedLesson.SelectedVocabularyIds ?? new();
            targetLesson.SelectedQuizIds = postedLesson.SelectedQuizIds ?? new();
            targetLesson.SelectedMiniGameIds = postedLesson.SelectedMiniGameIds ?? new();
        }

        return formModel;
    }

    public async Task<TeacherLessonPreviewViewModel?> GetLessonPreviewAsync(
        int classId,
        int lessonId,
        int teacherId,
        int? selectedCourseId = null)
    {
        var classEntity = await ValidateTeacherAccessAsync(classId, teacherId);
        if (classEntity == null)
        {
            return null;
        }

        var courseId = classEntity.CourseId ?? selectedCourseId;
        if (!courseId.HasValue)
        {
            return null;
        }

        var lesson = await _assignmentRepository.GetPublishedLessonDetailAsync(
            courseId.Value,
            lessonId);

        if (lesson == null)
        {
            return null;
        }

        return new TeacherLessonPreviewViewModel
        {
            LessonId = lesson.LessonId,
            Title = lesson.Title,
            Topic = lesson.Topic ?? "Chưa cập nhật",
            EstimatedMinutes = lesson.EstimatedMinutes,
            XPReward = lesson.XPReward,
            OrderIndex = lesson.OrderIndex,
            Vocabularies = (lesson.Vocabularies ?? new List<Vocabulary>())
                .OrderBy(v => v.Word)
                .Select(v => new TeacherVocabularyPreviewViewModel
                {
                    Word = v.Word,
                    Meaning = v.Meaning,
                    ExampleSentence = v.ExampleSentence,
                    ImageUrl = v.ImageUrl,
                    AudioUrl = v.AudioUrl
                }).ToList(),
            Quizzes = (lesson.Quizzes ?? new List<Quiz>())
                .OrderBy(q => q.QuizId)
                .Select(q => new TeacherQuizPreviewViewModel
                {
                    Question = q.Question,
                    QuizType = q.QuizType ?? "Chưa cập nhật",
                    Options = ParseQuizOptions(q.Options),
                    CorrectAnswer = q.CorrectAnswer
                }).ToList(),
            MiniGames = (lesson.MiniGames ?? new List<MiniGame>())
                .OrderBy(g => g.Title)
                .Select(g => new TeacherMiniGamePreviewViewModel
                {
                    Title = g.Title,
                    GameType = g.GameType ?? "Chưa cập nhật",
                    XPReward = g.XPReward
                }).ToList()
        };
    }

    private static List<string> ParseQuizOptions(string? options)
    {
        if (string.IsNullOrWhiteSpace(options))
        {
            return new List<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(options) ?? new List<string>();
        }
        catch (JsonException)
        {
            return options
                .Split(new[] { '\r', '\n', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(option => option.Trim())
                .Where(option => option.Length > 0)
                .ToList();
        }
    }

    /// <summary>
    /// Tạo bài giao cho các bài học hợp lệ và gửi thông báo nếu giáo viên phát hành ngay.
    /// </summary>
    public async Task<TeacherAssignmentResult> AssignWeeklyLessonsAsync(
    AssignWeeklyLessonViewModel model,
    int teacherId)
    {
        // Luồng 1: kiểm tra quyền giáo viên và các quy tắc nghiệp vụ của form.
        var classEntity = await ValidateTeacherAccessAsync(model.ClassId, teacherId);

        if (classEntity == null)
        {
            return TeacherAssignmentResult.Failure(
                "Bạn không có quyền giao bài cho lớp này.");
        }

        if (!model.SelectedCourseId.HasValue)
        {
            return await CreateAssignmentFailureAsync(
                model,
                teacherId,
                "Vui lòng chọn chương trình học trước khi giao bài.");
        }

        var validationError = GetAssignmentValidationError(model);
        if (validationError != null)
        {
            return await CreateAssignmentFailureAsync(model, teacherId, validationError);
        }

        var courseId = classEntity.CourseId ?? model.SelectedCourseId.Value;
        var selectedLessonIds = model.SelectedLessonIds
            .Distinct()
            .ToList();

        var matchingLessonCount = await _assignmentRepository.CountPublishedLessonsAsync(
            courseId,
            selectedLessonIds);
        if (matchingLessonCount != selectedLessonIds.Count)
        {
            return await CreateAssignmentFailureAsync(
                model,
                teacherId,
                "Danh sách bài học không hợp lệ hoặc không thuộc chương trình đã chọn.");
        }

        // Luồng 2: khóa transaction ở mức Serializable để hai request không tạo bài giao trùng.
        await using var transaction = await _unitOfWork.BeginTransactionAsync(
            IsolationLevel.Serializable);

        // Loại bỏ những bài đã được giao trong cùng tuần để tránh tạo dữ liệu trùng.
        var existingLessonIds = await _assignmentRepository.GetAssignedLessonIdsAsync(
            model.ClassId,
            courseId,
            selectedLessonIds,
            model.WeekStartDate);

        var newLessonIds = selectedLessonIds
            .Where(id => !existingLessonIds.Contains(id))
            .ToList();

        if (!newLessonIds.Any())
        {
            await transaction.RollbackAsync();
            return await CreateAssignmentFailureAsync(
                model,
                teacherId,
                "Các bài học đã được giao trong tuần này.");
        }

        var availableLessons = await _assignmentRepository.GetPublishedLessonsByCourseIdAsync(courseId);
        var assignments = new List<WeeklyAssignment>();

        foreach (var lessonId in newLessonIds)
        {
            var availableLesson = availableLessons.FirstOrDefault(x => x.LessonId == lessonId);
            var selection = model.Lessons.FirstOrDefault(x => x.LessonId == lessonId);
            if (availableLesson == null || selection == null ||
                !TryNormalizeSelection(selection, availableLesson))
            {
                await transaction.RollbackAsync();
                return await CreateAssignmentFailureAsync(
                    model,
                    teacherId,
                    "Mỗi bài học được chọn phải có ít nhất một từ vựng, câu quiz hoặc mini game hợp lệ.");
            }

            var assignment = new WeeklyAssignment
            {
                ClassId = model.ClassId,
                CourseId = courseId,
                LessonId = lessonId,
                WeekStartDate = model.WeekStartDate,
                DueDate = model.DueDate,
                IsVisible = model.Status == AssignmentStatus.Published,
                Status = model.Status,
                IncludeVocabulary = selection.IncludeVocabulary,
                IncludeQuiz = selection.IncludeQuiz,
                IncludeMiniGame = selection.IncludeMiniGame
            };

            assignment.Vocabularies = selection.SelectedVocabularyIds
                .Select(id => new WeeklyAssignmentVocabulary { VocabularyId = id })
                .ToList();
            assignment.Quizzes = selection.SelectedQuizIds
                .Select(id => new WeeklyAssignmentQuiz { QuizId = id })
                .ToList();
            assignment.MiniGames = selection.SelectedMiniGameIds
                .Select(id => new WeeklyAssignmentMiniGame { GameId = id })
                .ToList();

            assignments.Add(assignment);
        }

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

            // Luồng 3: chỉ tạo thông báo khi giáo viên phát hành, bản nháp chưa gửi thông báo.
            if (model.Status == AssignmentStatus.Published)
            {
                var students = await _classRepository.GetActiveStudentsByClassIdAsync(model.ClassId);
                var notifications = assignments.SelectMany(assignment => students.Select(student => new Notification
                {
                    UserId = student.StudentId,
                    Assignment = assignment,
                    Type = "NEW_ASSIGNMENT",
                    Message = $"Bài {availableLessons.First(x => x.LessonId == assignment.LessonId).Title} đã được giao từ {model.WeekStartDate:dd/MM/yyyy} đến {model.DueDate:dd/MM/yyyy}.",
                    IsRead = false,
                    CreateAt = DateTime.UtcNow
                })).ToList();

                await _notificationRepository.AddRangeAsync(notifications);
            }
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return TeacherAssignmentResult.Success();
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
    private static string? GetAssignmentValidationError(AssignWeeklyLessonViewModel model)
    {
        if (model.SelectedLessonIds == null || !model.SelectedLessonIds.Any())
        {
            return "Vui lòng chọn ít nhất một bài học.";
        }

        if (model.Lessons == null || model.Lessons.Count == 0)
        {
            return "Không tìm thấy nội dung bài học để giao.";
        }

        if (model.DueDate <= model.WeekStartDate)
        {
            return "Hạn hoàn thành phải lớn hơn ngày bắt đầu.";
        }

        if (model.Status != AssignmentStatus.Draft &&
            model.Status != AssignmentStatus.Published)
        {
            return "Trạng thái bài giao không hợp lệ.";
        }

        return null;
    }

    private async Task<TeacherAssignmentResult> CreateAssignmentFailureAsync(
        AssignWeeklyLessonViewModel model,
        int teacherId,
        string errorMessage)
    {
        var formModel = await RebuildAssignWeeklyLessonsFormAsync(model, teacherId);
        return TeacherAssignmentResult.Failure(errorMessage, formModel);
    }

    private static bool TryNormalizeSelection(
        AssignLessonItemViewModel selection,
        Lesson lesson)
    {
        var vocabularyIds = (lesson.Vocabularies ?? new List<Vocabulary>())
            .Select(x => x.VocabularyId)
            .ToHashSet();
        var quizIds = (lesson.Quizzes ?? new List<Quiz>())
            .Select(x => x.QuizId)
            .ToHashSet();
        var gameIds = (lesson.MiniGames ?? new List<MiniGame>())
            .Select(x => x.GameId)
            .ToHashSet();

        selection.SelectedVocabularyIds = selection.IncludeVocabulary
            ? selection.SelectedVocabularyIds.Distinct().ToList()
            : new List<int>();
        selection.SelectedQuizIds = selection.IncludeQuiz
            ? selection.SelectedQuizIds.Distinct().ToList()
            : new List<int>();
        selection.SelectedMiniGameIds = selection.IncludeMiniGame
            ? selection.SelectedMiniGameIds.Distinct().ToList()
            : new List<int>();

        if (selection.SelectedVocabularyIds.Any(id => !vocabularyIds.Contains(id)) ||
            selection.SelectedQuizIds.Any(id => !quizIds.Contains(id)) ||
            selection.SelectedMiniGameIds.Any(id => !gameIds.Contains(id)))
        {
            return false;
        }

        if (selection.IncludeVocabulary && selection.SelectedVocabularyIds.Count == 0 ||
            selection.IncludeQuiz && selection.SelectedQuizIds.Count == 0 ||
            selection.IncludeMiniGame && selection.SelectedMiniGameIds.Count == 0)
        {
            return false;
        }

        return selection.IncludeVocabulary ||
               selection.IncludeQuiz ||
               selection.IncludeMiniGame;
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
        var selectedClass = classId.HasValue
            ? classes.FirstOrDefault(c => c.ClassId == classId.Value)
            : null;

        var classesInScope = selectedClass == null
            ? classes
            : new List<Class> { selectedClass };

        var classIds = classesInScope
            .Select(c => c.ClassId)
            .Distinct()
            .ToList();

        var assignments = await _assignmentRepository.GetAssignmentsByClassIdsAsync(classIds);

        var totalAssignments = assignments.Count;
        var draftAssignments = assignments.Count(a => a.Status == AssignmentStatus.Draft);
        var activeAssignments = assignments.Count(a => a.Status == AssignmentStatus.Published && a.DueDate >= DateTime.UtcNow);
        var expiredAssignments = assignments.Count(a => a.Status == AssignmentStatus.Published && a.DueDate < DateTime.UtcNow);

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
            ClassName = selectedClass?.ClassName ?? string.Empty,
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
                LessonId = a.LessonId ?? 0,
                LessonTitle = a.Lesson?.Title ?? "Bài học chưa xác định",
                Topic = a.Lesson?.Topic ?? "Chưa cập nhật",
                EstimatedMinutes = a.Lesson?.EstimatedMinutes ?? 0,
                XPReward = a.Lesson?.XPReward ?? 0,
                VocabularyCount = a.IncludeVocabulary ? a.Vocabularies.Count : 0,
                QuizCount = a.IncludeQuiz ? a.Quizzes.Count : 0,
                MiniGameCount = a.IncludeMiniGame ? a.MiniGames.Count : 0,
                WeekStartDate = a.WeekStartDate,
                DueDate = a.DueDate,
                Status = GetDisplayStatus(a),
                AssignedStudentCount = a.Class?.Enrollments.Count(x => x.Student.IsActive) ?? 0,
                CompletedStudentCount = a.StudentProgresses.Count(x => x.Status == AssignmentCompletionStatus.Completed),
                CompletedLateCount = a.StudentProgresses.Count(x => x.IsCompletedLate),
                CanEdit = a.Status == AssignmentStatus.Draft || a.StudentProgresses.Count == 0,
                CanCancel = a.Status == AssignmentStatus.Published,
                CanDelete = a.Status == AssignmentStatus.Draft && a.StudentProgresses.Count == 0,
                CanArchive = a.Status == AssignmentStatus.Published && a.DueDate < DateTime.UtcNow
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
            "draft" => assignments.Where(a => a.Status == AssignmentStatus.Draft).ToList(),
            "active" => assignments.Where(a => a.Status == AssignmentStatus.Published && a.DueDate >= DateTime.UtcNow).ToList(),
            "expired" => assignments.Where(a => a.Status == AssignmentStatus.Published && a.DueDate < DateTime.UtcNow).ToList(),
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

        // Luồng phát hành: kiểm tra bản nháp và chống trùng trong cùng một transaction.
        await using var transaction = await _unitOfWork.BeginTransactionAsync(
            IsolationLevel.Serializable);

        try
        {
            var assignment = await _assignmentRepository.GetForUpdateAsync(
                assignmentId,
                classId,
                classEntity.CourseId.Value);

            if (assignment == null || assignment.IsVisible)
            {
                await transaction.RollbackAsync();
                return false;
            }

            var hasPublishedDuplicate =
                await _assignmentRepository.ExistsPublishedAssignmentAsync(
                    assignment.ClassId!.Value,
                    assignment.CourseId!.Value,
                    assignment.LessonId,
                    assignment.WeekStartDate,
                    assignment.AssignmentId);

            if (hasPublishedDuplicate)
            {
                await transaction.RollbackAsync();
                return false;
            }

            assignment.IsVisible = true;
            assignment.Status = AssignmentStatus.Published;
            assignment.UpdatedAt = DateTime.UtcNow;

            var students = await _classRepository.GetActiveStudentsByClassIdAsync(classId);
            var notifications = students.Select(student => new Notification
            {
                UserId = student.StudentId,
                AssignmentId = assignment.AssignmentId,
                Type = "NEW_ASSIGNMENT",
                Message = $"Giáo viên vừa giao bài học mới từ {assignment.WeekStartDate:dd/MM/yyyy} đến {assignment.DueDate:dd/MM/yyyy}.",
                IsRead = false,
                CreateAt = DateTime.UtcNow
            }).ToList();

            await _notificationRepository.AddRangeAsync(notifications);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<TeacherAssignmentDetailsViewModel?> GetAssignmentDetailsAsync(
        int assignmentId,
        int classId,
        int teacherId)
    {
        if (await ValidateTeacherAccessAsync(classId, teacherId) == null) return null;
        var assignment = await _assignmentRepository.GetAssignmentDetailsAsync(assignmentId, classId);
        if (assignment?.Lesson == null || assignment.Class == null) return null;

        var progressByStudent = assignment.StudentProgresses.ToDictionary(x => x.StudentId);
        var students = assignment.Class.Enrollments
            .Where(x => x.Student.IsActive)
            .OrderBy(x => x.Student.Username)
            .Select(enrollment =>
            {
                progressByStudent.TryGetValue(enrollment.StudentId, out var progress);
                return new TeacherAssignmentStudentCompletionViewModel
                {
                    StudentId = enrollment.StudentId,
                    StudentName = !string.IsNullOrWhiteSpace(progress?.Student?.FullName)
                        ? progress.Student.FullName
                        : !string.IsNullOrWhiteSpace(enrollment.Student.StudentProfile?.FullName)
                            ? enrollment.Student.StudentProfile.FullName
                            : enrollment.Student.Username,
                    Email = enrollment.Student.Email,
                    CompletionStatus = progress?.Status.ToString() ?? AssignmentCompletionStatus.NotStarted.ToString(),
                    CompletedActivityCount = progress?.CompletedActivityCount ?? 0,
                    RequiredActivityCount = progress?.RequiredActivityCount ?? CountRequiredActivities(assignment),
                    QuizScore = progress?.BestQuizScore,
                    CompletedAt = progress?.CompletedAt,
                    IsCompletedLate = progress?.IsCompletedLate ?? false
                };
            }).ToList();

        var hasProgress = assignment.StudentProgresses.Count > 0;
        return new TeacherAssignmentDetailsViewModel
        {
            AssignmentId = assignment.AssignmentId,
            ClassId = classId,
            ClassName = assignment.Class.ClassName,
            LessonId = assignment.LessonId ?? 0,
            LessonTitle = assignment.Lesson.Title,
            CourseName = assignment.Course?.CourseName ?? string.Empty,
            Topic = assignment.Lesson.Topic ?? string.Empty,
            StartDate = assignment.WeekStartDate,
            DueDate = assignment.DueDate,
            Status = GetDisplayStatus(assignment),
            AssignedStudentCount = students.Count,
            CompletedStudentCount = students.Count(x => x.CompletionStatus == AssignmentCompletionStatus.Completed.ToString()),
            InProgressStudentCount = students.Count(x => x.CompletionStatus == AssignmentCompletionStatus.InProgress.ToString()),
            NotStartedStudentCount = students.Count(x => x.CompletionStatus == AssignmentCompletionStatus.NotStarted.ToString()),
            CompletedLateCount = students.Count(x => x.IsCompletedLate),
            CanEdit = assignment.Status is not (AssignmentStatus.Cancelled or AssignmentStatus.Archived) &&
                      (assignment.Status == AssignmentStatus.Draft || !hasProgress),
            CanCancel = assignment.Status == AssignmentStatus.Published,
            CanDelete = assignment.Status == AssignmentStatus.Draft && !hasProgress,
            CanArchive = assignment.Status == AssignmentStatus.Published && assignment.DueDate < DateTime.UtcNow,
            Activities = BuildActivitySummary(assignment),
            Students = students
        };
    }

    public async Task<EditTeacherAssignmentViewModel?> GetEditAssignmentAsync(
        int assignmentId,
        int classId,
        int teacherId)
    {
        if (await ValidateTeacherAccessAsync(classId, teacherId) == null) return null;
        var assignment = await _assignmentRepository.GetAssignmentDetailsAsync(assignmentId, classId);
        if (assignment?.Lesson == null || assignment.Class == null ||
            assignment.Status is AssignmentStatus.Cancelled or AssignmentStatus.Archived ||
            await _assignmentRepository.HasStudentProgressAsync(assignmentId))
        {
            return null;
        }

        return BuildEditViewModel(assignment);
    }

    public async Task<TeacherAssignmentCommandResult> UpdateAssignmentAsync(
        EditTeacherAssignmentViewModel model,
        int teacherId)
    {
        if (await ValidateTeacherAccessAsync(model.ClassId, teacherId) == null)
            return TeacherAssignmentCommandResult.Failure("Bạn không có quyền sửa bài giao này.");

        var assignment = await _assignmentRepository.GetAssignmentDetailsAsync(model.AssignmentId, model.ClassId);
        if (assignment?.Lesson == null)
            return TeacherAssignmentCommandResult.Failure("Không tìm thấy bài giao.");
        if (await _assignmentRepository.HasStudentProgressAsync(model.AssignmentId))
            return TeacherAssignmentCommandResult.Failure("Không thể đổi cấu hình sau khi học sinh đã bắt đầu làm bài.");
        if (assignment.Status is AssignmentStatus.Cancelled or AssignmentStatus.Archived)
            return TeacherAssignmentCommandResult.Failure("Bài giao đã kết thúc vòng đời và không thể chỉnh sửa.");
        if (model.DueDate <= model.StartDate)
            return TeacherAssignmentCommandResult.Failure("Hạn hoàn thành phải sau ngày bắt đầu.");

        var vocabularyIds = assignment.Lesson.Vocabularies.Select(x => x.VocabularyId).ToHashSet();
        var quizIds = assignment.Lesson.Quizzes.Select(x => x.QuizId).ToHashSet();
        var gameIds = assignment.Lesson.MiniGames.Select(x => x.GameId).ToHashSet();
        model.SelectedVocabularyIds = model.IncludeVocabulary
            ? model.SelectedVocabularyIds.Distinct().Where(vocabularyIds.Contains).ToList() : new();
        model.SelectedQuizIds = model.IncludeQuiz
            ? model.SelectedQuizIds.Distinct().Where(quizIds.Contains).ToList() : new();
        model.SelectedMiniGameIds = model.IncludeMiniGame
            ? model.SelectedMiniGameIds.Distinct().Where(gameIds.Contains).ToList() : new();
        if (model.SelectedVocabularyIds.Count + model.SelectedQuizIds.Count + model.SelectedMiniGameIds.Count == 0)
            return TeacherAssignmentCommandResult.Failure("Bài giao phải có ít nhất một hoạt động hợp lệ.");

        // Business process: thay toàn bộ cấu hình activity trong một lần SaveChanges để tránh dữ liệu nửa cũ nửa mới.
        assignment.WeekStartDate = model.StartDate;
        assignment.DueDate = model.DueDate;
        assignment.IncludeVocabulary = model.SelectedVocabularyIds.Count > 0;
        assignment.IncludeQuiz = model.SelectedQuizIds.Count > 0;
        assignment.IncludeMiniGame = model.SelectedMiniGameIds.Count > 0;
        foreach (var item in assignment.Vocabularies.Where(x => !model.SelectedVocabularyIds.Contains(x.VocabularyId)).ToList())
            assignment.Vocabularies.Remove(item);
        foreach (var id in model.SelectedVocabularyIds.Where(id => assignment.Vocabularies.All(x => x.VocabularyId != id)))
            assignment.Vocabularies.Add(new WeeklyAssignmentVocabulary { AssignmentId = assignment.AssignmentId, VocabularyId = id });
        foreach (var item in assignment.Quizzes.Where(x => !model.SelectedQuizIds.Contains(x.QuizId)).ToList())
            assignment.Quizzes.Remove(item);
        foreach (var id in model.SelectedQuizIds.Where(id => assignment.Quizzes.All(x => x.QuizId != id)))
            assignment.Quizzes.Add(new WeeklyAssignmentQuiz { AssignmentId = assignment.AssignmentId, QuizId = id });
        foreach (var item in assignment.MiniGames.Where(x => !model.SelectedMiniGameIds.Contains(x.GameId)).ToList())
            assignment.MiniGames.Remove(item);
        foreach (var id in model.SelectedMiniGameIds.Where(id => assignment.MiniGames.All(x => x.GameId != id)))
            assignment.MiniGames.Add(new WeeklyAssignmentMiniGame { AssignmentId = assignment.AssignmentId, GameId = id });
        assignment.UpdatedAt = DateTime.UtcNow;
        if (assignment.Status == AssignmentStatus.Published && assignment.Class != null)
        {
            var notifications = assignment.Class.Enrollments
                .Where(x => x.Student.IsActive)
                .Select(x => new Notification
                {
                    UserId = x.StudentId,
                    AssignmentId = assignment.AssignmentId,
                    Type = "ASSIGNMENT_UPDATED",
                    Message = $"Bài giao {assignment.Lesson.Title} đã được cập nhật. Hạn hoàn thành: {assignment.DueDate:dd/MM/yyyy}.",
                    IsRead = false,
                    CreateAt = DateTime.UtcNow
                }).ToList();
            await _notificationRepository.AddRangeAsync(notifications);
        }
        await _unitOfWork.SaveChangesAsync();
        return TeacherAssignmentCommandResult.Success("Đã cập nhật bài giao.");
    }

    public Task<TeacherAssignmentCommandResult> CancelAssignmentAsync(
        int assignmentId, int classId, int teacherId) =>
        ChangeLifecycleAsync(assignmentId, classId, teacherId, AssignmentStatus.Cancelled);

    public Task<TeacherAssignmentCommandResult> ArchiveAssignmentAsync(
        int assignmentId, int classId, int teacherId) =>
        ChangeLifecycleAsync(assignmentId, classId, teacherId, AssignmentStatus.Archived);

    public async Task<TeacherAssignmentCommandResult> DeleteAssignmentAsync(
        int assignmentId,
        int classId,
        int teacherId)
    {
        if (await ValidateTeacherAccessAsync(classId, teacherId) == null)
            return TeacherAssignmentCommandResult.Failure("Bạn không có quyền xóa bài giao này.");
        var assignment = await _assignmentRepository.GetAssignmentDetailsAsync(assignmentId, classId);
        if (assignment == null || assignment.Status != AssignmentStatus.Draft || assignment.IsVisible)
            return TeacherAssignmentCommandResult.Failure("Chỉ được xóa bài giao đang ở bản nháp.");
        if (await _assignmentRepository.HasStudentProgressAsync(assignmentId))
            return TeacherAssignmentCommandResult.Failure("Không thể xóa bài giao đã có tiến độ học sinh.");
        _assignmentRepository.RemoveAssignment(assignment);
        await _unitOfWork.SaveChangesAsync();
        return TeacherAssignmentCommandResult.Success("Đã xóa bản nháp.");
    }

    private async Task<TeacherAssignmentCommandResult> ChangeLifecycleAsync(
        int assignmentId,
        int classId,
        int teacherId,
        AssignmentStatus targetStatus)
    {
        if (await ValidateTeacherAccessAsync(classId, teacherId) == null)
            return TeacherAssignmentCommandResult.Failure("Bạn không có quyền thay đổi bài giao này.");
        var assignment = await _assignmentRepository.GetAssignmentDetailsAsync(assignmentId, classId);
        if (assignment == null || assignment.Status != AssignmentStatus.Published)
            return TeacherAssignmentCommandResult.Failure("Trạng thái bài giao không cho phép thao tác này.");
        if (targetStatus == AssignmentStatus.Archived && assignment.DueDate >= DateTime.UtcNow)
            return TeacherAssignmentCommandResult.Failure("Chỉ lưu trữ bài giao đã hết hạn.");
        assignment.Status = targetStatus;
        assignment.IsVisible = false;
        assignment.UpdatedAt = DateTime.UtcNow;
        if (targetStatus == AssignmentStatus.Cancelled && assignment.Class != null)
        {
            var notifications = assignment.Class.Enrollments
                .Where(x => x.Student.IsActive)
                .Select(x => new Notification
                {
                    UserId = x.StudentId,
                    AssignmentId = assignment.AssignmentId,
                    Type = "ASSIGNMENT_CANCELLED",
                    Message = $"Bài giao {assignment.Lesson?.Title ?? string.Empty} đã được giáo viên hủy.",
                    IsRead = false,
                    CreateAt = DateTime.UtcNow
                }).ToList();
            await _notificationRepository.AddRangeAsync(notifications);
        }
        await _unitOfWork.SaveChangesAsync();
        return TeacherAssignmentCommandResult.Success(
            targetStatus == AssignmentStatus.Cancelled ? "Đã hủy bài giao." : "Đã lưu trữ bài giao.");
    }

    private static EditTeacherAssignmentViewModel BuildEditViewModel(WeeklyAssignment assignment)
    {
        return new EditTeacherAssignmentViewModel
        {
            AssignmentId = assignment.AssignmentId,
            ClassId = assignment.ClassId ?? 0,
            ClassName = assignment.Class?.ClassName ?? string.Empty,
            LessonId = assignment.LessonId ?? 0,
            LessonTitle = assignment.Lesson?.Title ?? string.Empty,
            StartDate = assignment.WeekStartDate,
            DueDate = assignment.DueDate,
            Status = GetDisplayStatus(assignment),
            IncludeVocabulary = assignment.IncludeVocabulary,
            IncludeQuiz = assignment.IncludeQuiz,
            IncludeMiniGame = assignment.IncludeMiniGame,
            SelectedVocabularyIds = assignment.Vocabularies.Select(x => x.VocabularyId).ToList(),
            SelectedQuizIds = assignment.Quizzes.Select(x => x.QuizId).ToList(),
            SelectedMiniGameIds = assignment.MiniGames.Select(x => x.GameId).ToList(),
            Vocabularies = assignment.Lesson?.Vocabularies.Select(x => new AssignmentVocabularyOptionViewModel
                { VocabularyId = x.VocabularyId, Word = x.Word, Meaning = x.Meaning }).ToList() ?? new(),
            Quizzes = assignment.Lesson?.Quizzes.Select(x => new AssignmentQuizOptionViewModel
                { QuizId = x.QuizId, Question = x.Question, QuizType = x.QuizType ?? "Quiz" }).ToList() ?? new(),
            MiniGames = assignment.Lesson?.MiniGames.Select(x => new AssignmentMiniGameOptionViewModel
                { GameId = x.GameId, Title = x.Title, GameType = x.GameType ?? "MiniGame" }).ToList() ?? new()
        };
    }

    private static List<TeacherAssignmentActivityViewModel> BuildActivitySummary(WeeklyAssignment assignment)
    {
        var result = new List<TeacherAssignmentActivityViewModel>();
        if (assignment.IncludeVocabulary && assignment.Vocabularies.Count > 0)
            result.Add(new() { ActivityType = "Flashcard", Title = "Ôn tập từ vựng", ItemCount = assignment.Vocabularies.Count, IsRequired = true });
        if (assignment.IncludeQuiz && assignment.Quizzes.Count > 0)
            result.Add(new() { ActivityType = "Quiz", Title = "Bài kiểm tra", ItemCount = assignment.Quizzes.Count, IsRequired = true });
        if (assignment.IncludeMiniGame && assignment.MiniGames.Count > 0)
            result.Add(new() { ActivityType = "Mini-game", Title = "Trò chơi luyện tập", ItemCount = assignment.MiniGames.Count, IsRequired = true });
        return result;
    }

    private static int CountRequiredActivities(WeeklyAssignment assignment) =>
        (assignment.IncludeVocabulary && assignment.Vocabularies.Count > 0 ? 1 : 0) +
        (assignment.IncludeQuiz && assignment.Quizzes.Count > 0 ? 1 : 0) +
        (assignment.IncludeMiniGame ? assignment.MiniGames.Count : 0);

    private static string GetDisplayStatus(WeeklyAssignment assignment)
    {
        if (assignment.Status == AssignmentStatus.Published && assignment.DueDate < DateTime.UtcNow)
            return "Quá hạn";
        return assignment.Status switch
        {
            AssignmentStatus.Draft => "Bản nháp",
            AssignmentStatus.Published => "Đã giao",
            AssignmentStatus.Cancelled => "Đã hủy",
            AssignmentStatus.Archived => "Đã lưu trữ",
            _ => assignment.Status.ToString()
        };
    }
}
