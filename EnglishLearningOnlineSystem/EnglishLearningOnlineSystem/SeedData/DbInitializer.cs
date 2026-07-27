using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EnglishLearningOnlineSystem.SeedData;

/// <summary>
/// Khởi tạo cấu hình bắt buộc và dữ liệu mẫu phục vụ kiểm thử trong Development.
/// Seeder dùng các khóa nghiệp vụ ổn định và không tạo trùng khi chạy lại.
/// </summary>
public static class DbInitializer
{
    public const string TeacherUsername = "teacher.demo";
    public const string TeacherPassword = "Teacher@123";

    private const string DemoCourseName = "English 6 - Teacher Demo";
    private const string DemoClassName = "Lớp 6A - Teacher Demo";
    private const string DemoAcademicYear = "2026-2027";

    public static async Task SeedAsync(
        AppDbContext context,
        bool includeDemoData = false)
    {
        await SeedSystemSettingsAsync(context);

        if (!includeDemoData)
        {
            return;
        }

        var teacher = await EnsureUserAsync(
            context,
            TeacherUsername,
            "teacher.demo@els.local",
            TeacherPassword,
            roleId: 3,
            isActive: true,
            lastLoginAt: DateTime.UtcNow.AddHours(-2));

        var studentDefinitions = new[]
        {
            new DemoStudent(
                "student.an", "student.an@els.local", "Nguyễn Minh An",
                true, 4, 1280, 7, DateTime.UtcNow.AddMinutes(-18)),
            new DemoStudent(
                "student.binh", "student.binh@els.local", "Trần Gia Bình",
                true, 2, 540, 2, DateTime.UtcNow.AddDays(-2)),
            new DemoStudent(
                "student.chi", "student.chi@els.local", "Lê Khánh Chi",
                true, 1, 180, 0, DateTime.UtcNow.AddDays(-6)),
            new DemoStudent(
                "student.inactive", "student.inactive@els.local", "Học sinh ngừng hoạt động",
                false, 1, 60, 0, DateTime.UtcNow.AddMonths(-3))
        };

        var students = new List<(User User, StudentProfile Profile)>();
        foreach (var definition in studentDefinitions)
        {
            var user = await EnsureUserAsync(
                context,
                definition.Username,
                definition.Email,
                "Student@123",
                roleId: 1,
                definition.IsActive,
                definition.LastActiveAt);

            var profile = await EnsureStudentProfileAsync(context, user, definition);
            students.Add((user, profile));
        }

        var academicYear = await context.AcademicYears!
            .FirstOrDefaultAsync(x => x.YearLabel == DemoAcademicYear);
        if (academicYear == null)
        {
            academicYear = new AcademicYear
            {
                YearLabel = DemoAcademicYear,
                StartDate = new DateTime(2026, 8, 1),
                EndDate = new DateTime(2027, 5, 31),
                IsActive = true
            };
            context.AcademicYears!.Add(academicYear);
            await context.SaveChangesAsync();
        }

        var course = await context.Courses!
            .FirstOrDefaultAsync(x => x.CourseName == DemoCourseName && !x.IsDeleted);
        if (course == null)
        {
            course = new Course
            {
                CourseName = DemoCourseName,
                GradeLevel = "6",
                Description = "Khóa học mẫu để kiểm thử giao bài tuần và hỗ trợ học sinh.",
                IsPublished = true,
                IsDeleted = false
            };
            context.Courses!.Add(course);
            await context.SaveChangesAsync();
        }

        var demoClass = await context.Classes!
            .FirstOrDefaultAsync(x => x.ClassName == DemoClassName && !x.IsDeleted);
        if (demoClass == null)
        {
            demoClass = new Class
            {
                ClassName = DemoClassName,
                GradeLevel = "6",
                AcademicYearId = academicYear.AcademicYearId,
                TeacherId = teacher.Id,
                CourseId = course.CourseId,
                IsDeleted = false
            };
            context.Classes!.Add(demoClass);
            await context.SaveChangesAsync();
        }
        else
        {
            demoClass.TeacherId = teacher.Id;
            demoClass.CourseId = course.CourseId;
            demoClass.AcademicYearId = academicYear.AcademicYearId;
        }

        foreach (var student in students)
        {
            var enrolled = await context.ClassEnrollments!
                .AnyAsync(x => x.ClassId == demoClass.ClassId && x.StudentId == student.User.Id);
            if (!enrolled)
            {
                context.ClassEnrollments!.Add(new ClassEnrollment
                {
                    ClassId = demoClass.ClassId,
                    StudentId = student.User.Id,
                    EnrolledAt = DateTime.UtcNow.AddMonths(-2)
                });
            }
        }

        await context.SaveChangesAsync();

        var lessons = await EnsureLessonsAsync(context, course.CourseId);
        await EnsureLessonContentAsync(context, lessons);
        await EnsureLearningProgressAsync(context, students, lessons, teacher.Id);
        await EnsureWeeklyAssignmentsAsync(context, course.CourseId, lessons);
        await context.SaveChangesAsync();
    }

    private static async Task SeedSystemSettingsAsync(AppDbContext context)
    {
        if (!await context.SystemSettings.AnyAsync(x => x.Key == "MaintenanceModeEnabled"))
        {
            context.SystemSettings.Add(new SystemSetting
            {
                Key = "MaintenanceModeEnabled",
                Value = "false",
                Description = "Bật/tắt chế độ bảo trì hệ thống (true/false)",
                UpdatedAt = DateTime.UtcNow
            });
        }

        if (!await context.SystemSettings.AnyAsync(x => x.Key == "MaintenanceStartAt"))
        {
            context.SystemSettings.Add(new SystemSetting
            {
                Key = "MaintenanceStartAt",
                Value = string.Empty,
                Description = "Thời gian bắt đầu bảo trì hệ thống (định dạng yyyy-MM-dd HH:mm:ss)",
                UpdatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task<User> EnsureUserAsync(
        AppDbContext context,
        string username,
        string email,
        string password,
        int roleId,
        bool isActive,
        DateTime? lastLoginAt)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.Username == username);
        if (user != null)
        {
            return user;
        }

        user = new User
        {
            Username = username,
            Email = email,
            Password = BCrypt.Net.BCrypt.HashPassword(password),
            RoleId = roleId,
            IsActive = isActive,
            LastLoginAt = lastLoginAt,
            BirthDate = roleId == 1 ? new DateTime(2014, 5, 15) : new DateTime(1992, 8, 20)
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private static async Task<StudentProfile> EnsureStudentProfileAsync(
        AppDbContext context,
        User user,
        DemoStudent definition)
    {
        var profile = await context.StudentProfiles!
            .FirstOrDefaultAsync(x => x.StudentId == user.Id);
        if (profile != null)
        {
            return profile;
        }

        profile = new StudentProfile
        {
            StudentId = user.Id,
            Nickname = definition.DisplayName,
            Level = definition.Level,
            XP = definition.Xp,
            CurrentStreakDays = definition.StreakDays,
            LastActiveDate = definition.LastActiveAt,
            AvatarUrl = string.Empty
        };
        context.StudentProfiles!.Add(profile);
        await context.SaveChangesAsync();
        return profile;
    }

    private static async Task<List<Lesson>> EnsureLessonsAsync(
        AppDbContext context,
        int courseId)
    {
        var definitions = new[]
        {
            new DemoLesson("Greetings and Introductions", "Daily communication", 1, 20, 100),
            new DemoLesson("My Family", "Family members", 2, 25, 120),
            new DemoLesson("School Things", "School vocabulary", 3, 30, 140),
            new DemoLesson("Daily Routines", "Daily activities", 4, 30, 150)
        };

        var lessons = new List<Lesson>();
        foreach (var definition in definitions)
        {
            var lesson = await context.Lessons!
                .FirstOrDefaultAsync(x =>
                    x.CourseId == courseId &&
                    x.Title == definition.Title);

            if (lesson == null)
            {
                lesson = new Lesson
                {
                    CourseId = courseId,
                    Title = definition.Title,
                    Topic = definition.Topic,
                    OrderIndex = definition.OrderIndex,
                    EstimatedMinutes = definition.EstimatedMinutes,
                    XPReward = definition.XpReward,
                    IsPublished = true
                };
                context.Lessons!.Add(lesson);
                await context.SaveChangesAsync();
            }

            lessons.Add(lesson);
        }

        return lessons;
    }

    private static async Task EnsureLessonContentAsync(
        AppDbContext context,
        IReadOnlyList<Lesson> lessons)
    {
        var vocabularyByLesson = new[]
        {
            new[]
            {
                ("hello", "xin chào", "Hello, my name is An."),
                ("name", "tên", "What is your name?"),
                ("friend", "bạn bè", "This is my friend.")
            },
            new[]
            {
                ("father", "bố", "My father is a doctor."),
                ("mother", "mẹ", "My mother is a teacher."),
                ("sister", "chị/em gái", "I have one sister."),
                ("brother", "anh/em trai", "My brother is ten.")
            },
            new[]
            {
                ("notebook", "vở", "This is my notebook."),
                ("pencil", "bút chì", "I write with a pencil."),
                ("schoolbag", "cặp sách", "My schoolbag is blue.")
            },
            new[]
            {
                ("wake up", "thức dậy", "I wake up at six."),
                ("have breakfast", "ăn sáng", "We have breakfast together."),
                ("go to school", "đi học", "I go to school by bus.")
            }
        };

        var quizByLesson = new[]
        {
            new[]
            {
                ("Choose the correct greeting.", new[] { "Hello", "Goodbye", "Sorry" }, "Hello"),
                ("Complete: My ___ is Lan.", new[] { "name", "friend", "school" }, "name")
            },
            new[]
            {
                ("Who is your mother's son?", new[] { "Brother", "Father", "Sister" }, "Brother"),
                ("Choose the family member.", new[] { "Mother", "Pencil", "Teacher" }, "Mother")
            },
            new[]
            {
                ("What do you write with?", new[] { "Pencil", "Schoolbag", "Desk" }, "Pencil"),
                ("Where do you keep your books?", new[] { "Schoolbag", "Ruler", "Board" }, "Schoolbag")
            },
            new[]
            {
                ("What do you do in the morning?", new[] { "Wake up", "Go to bed", "Have dinner" }, "Wake up"),
                ("Complete: I ___ to school.", new[] { "go", "goes", "going" }, "go")
            }
        };

        var gameDefinitions = new[]
        {
            ("Greeting Match", "Matching", 30),
            ("Family Word Race", "WordRace", 35),
            ("School Bag Sort", "Sorting", 40),
            ("Routine Sequence", "Ordering", 45)
        };

        for (var index = 0; index < lessons.Count; index++)
        {
            var lesson = lessons[index];

            foreach (var vocabulary in vocabularyByLesson[index])
            {
                var exists = await context.Vocabularies!
                    .AnyAsync(x => x.LessonId == lesson.LessonId && x.Word == vocabulary.Item1);
                if (!exists)
                {
                    context.Vocabularies!.Add(new Vocabulary
                    {
                        LessonId = lesson.LessonId,
                        Word = vocabulary.Item1,
                        Meaning = vocabulary.Item2,
                        ExampleSentence = vocabulary.Item3,
                        ImageUrl = string.Empty,
                        AudioUrl = string.Empty
                    });
                }
            }

            foreach (var quiz in quizByLesson[index])
            {
                var exists = await context.Quizzes!
                    .AnyAsync(x => x.LessonId == lesson.LessonId && x.Question == quiz.Item1);
                if (!exists)
                {
                    context.Quizzes!.Add(new Quiz
                    {
                        LessonId = lesson.LessonId,
                        Question = quiz.Item1,
                        QuizType = "Multiple Choice",
                        Options = JsonSerializer.Serialize(quiz.Item2),
                        CorrectAnswer = quiz.Item3
                    });
                }
            }

            var game = gameDefinitions[index];
            var gameExists = await context.MiniGames!
                .AnyAsync(x => x.LessonId == lesson.LessonId && x.Title == game.Item1);
            if (!gameExists)
            {
                context.MiniGames!.Add(new MiniGame
                {
                    LessonId = lesson.LessonId,
                    Title = game.Item1,
                    GameType = game.Item2,
                    XPReward = game.Item3
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task EnsureLearningProgressAsync(
        AppDbContext context,
        IReadOnlyList<(User User, StudentProfile Profile)> students,
        IReadOnlyList<Lesson> lessons,
        int teacherId)
    {
        var progressDefinitions = new[]
        {
            new DemoProgress(0, 0, 92, 100, "Completed", true, DateTime.UtcNow.AddDays(-3)),
            new DemoProgress(1, 0, 58, 45, "In Progress", true, null),
            new DemoProgress(0, 1, 76, 120, "Completed", true, DateTime.UtcNow.AddDays(-1))
        };

        foreach (var definition in progressDefinitions)
        {
            var student = students[definition.StudentIndex].Profile;
            var lesson = lessons[definition.LessonIndex];
            var exists = await context.Progresses!
                .AnyAsync(x => x.StudentId == student.StudentId && x.LessonId == lesson.LessonId);
            if (!exists)
            {
                context.Progresses!.Add(new Progress
                {
                    StudentId = student.StudentId,
                    LessonId = lesson.LessonId,
                    QuizScore = definition.Score,
                    XPEarned = definition.XpEarned,
                    CompletionStatus = definition.Status,
                    IsBestAttempt = definition.IsBestAttempt,
                    CompletedAt = definition.CompletedAt
                });
            }
        }

        var supportStudent = students[1].Profile;
        var feedbackExists = await context.TeacherFeedbacks!
            .AnyAsync(x =>
                x.TeacherId == teacherId &&
                x.StudentId == supportStudent.StudentId &&
                x.Content == "Em cần ôn lại từ vựng chủ đề gia đình và hoàn thành bài luyện tập.");
        if (!feedbackExists)
        {
            context.TeacherFeedbacks!.Add(new TeacherFeedback
            {
                TeacherId = teacherId,
                StudentId = supportStudent.StudentId,
                Content = "Em cần ôn lại từ vựng chủ đề gia đình và hoàn thành bài luyện tập.",
                IsRead = false,
                CreateAt = DateTime.UtcNow.AddDays(-1)
            });
        }
    }

    private static async Task EnsureWeeklyAssignmentsAsync(
        AppDbContext context,
        int courseId,
        IReadOnlyList<Lesson> lessons)
    {
        var today = DateTime.UtcNow.Date;
        var currentMonday = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));

        var definitions = new[]
        {
            new DemoAssignment(0, currentMonday.AddDays(-7), currentMonday.AddTicks(-1), true),
            new DemoAssignment(1, currentMonday, currentMonday.AddDays(6).AddHours(23).AddMinutes(59), true),
            new DemoAssignment(2, currentMonday.AddDays(7), currentMonday.AddDays(13).AddHours(23).AddMinutes(59), false)
        };

        foreach (var definition in definitions)
        {
            var lessonId = lessons[definition.LessonIndex].LessonId;
            var assignment = await context.WeeklyAssignments!
                .FirstOrDefaultAsync(x =>
                    x.CourseId == courseId &&
                    x.LessonId == lessonId);

            if (assignment == null)
            {
                context.WeeklyAssignments!.Add(new WeeklyAssignment
                {
                    CourseId = courseId,
                    LessonId = lessonId,
                    WeekStartDate = definition.WeekStart,
                    DueDate = definition.DueDate,
                    IsVisible = definition.IsVisible
                });
            }
            else
            {
                assignment.WeekStartDate = definition.WeekStart;
                assignment.DueDate = definition.DueDate;
                assignment.IsVisible = definition.IsVisible;
            }
        }
    }

    private sealed record DemoStudent(
        string Username,
        string Email,
        string DisplayName,
        bool IsActive,
        int Level,
        int Xp,
        int StreakDays,
        DateTime LastActiveAt);

    private sealed record DemoLesson(
        string Title,
        string Topic,
        int OrderIndex,
        int EstimatedMinutes,
        int XpReward);

    private sealed record DemoProgress(
        int StudentIndex,
        int LessonIndex,
        int Score,
        int XpEarned,
        string Status,
        bool IsBestAttempt,
        DateTime? CompletedAt);

    private sealed record DemoAssignment(
        int LessonIndex,
        DateTime WeekStart,
        DateTime DueDate,
        bool IsVisible);
}
