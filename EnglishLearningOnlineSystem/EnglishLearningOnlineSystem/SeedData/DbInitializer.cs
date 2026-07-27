using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.SeedData;

/// <summary>
/// Seeds required configuration and a coherent demo data set for every application area.
/// Demo data is only created when <paramref name="includeDemoData"/> is true.
/// Every lookup uses a stable business key so the initializer is safe to run repeatedly.
/// </summary>
public static class DbInitializer
{
    private const string DemoPassword = "Demo@123";

    public static async Task SeedAsync(
        AppDbContext context,
        bool includeDemoData = false)
    {
        await SeedSystemSettingsAsync(context);

        if (!includeDemoData)
        {
            return;
        }

        await using var transaction = await context.Database.BeginTransactionAsync();

        await SeedDemoDataAsync(context);

        await transaction.CommitAsync();
    }

    private static async Task SeedSystemSettingsAsync(AppDbContext context)
    {
        var settings = new[]
        {
            new SystemSetting
            {
                Key = "MaintenanceModeEnabled",
                Value = "false",
                Description = "Bật/tắt chế độ bảo trì hệ thống (true/false)",
                UpdatedAt = DateTime.UtcNow
            },
            new SystemSetting
            {
                Key = "MaintenanceStartAt",
                Value = string.Empty,
                Description = "Thời gian bắt đầu bảo trì hệ thống (định dạng yyyy-MM-dd HH:mm:ss)",
                UpdatedAt = DateTime.UtcNow
            }
        };

        foreach (var setting in settings)
        {
            if (!await context.SystemSettings.AnyAsync(x => x.Key == setting.Key))
            {
                context.SystemSettings.Add(setting);
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedDemoDataAsync(AppDbContext context)
    {
        var roles = await context.Roles
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Name, x => x.Id);

        var requiredRoles = new[] { "Student", "Admin", "Teacher", "Content Manager" };
        var missingRoles = requiredRoles.Where(x => !roles.ContainsKey(x)).ToList();
        if (missingRoles.Count > 0)
        {
            throw new InvalidOperationException(
                $"Cannot seed demo data because these roles are missing: {string.Join(", ", missingRoles)}.");
        }

        var users = await SeedUsersAsync(context, roles);
        var students = await SeedStudentProfilesAsync(context, users);
        var academicYear = await SeedAcademicYearAsync(context);
        var courses = await SeedCoursesAsync(context, users["content"]);
        var lessons = await SeedLessonsAsync(context, courses);

        await SeedVocabulariesAsync(context, lessons);
        await SeedQuizzesAsync(context, lessons);
        await SeedMiniGamesAsync(context, lessons);

        var classes = await SeedClassesAsync(
            context,
            academicYear,
            users["teacher"],
            courses);

        await SeedEnrollmentsAsync(context, classes, users);
        var assignments = await SeedAssignmentsAsync(context, lessons, courses, classes);
        var missions = await SeedDailyMissionsAsync(context);
        var badges = await SeedBadgesAsync(context);

        await SeedLearningActivityAsync(
            context,
            students,
            lessons,
            assignments,
            missions,
            badges);

        await SeedCommunicationsAsync(context, users, students);
        await SeedAuditLogsAsync(context, users);
    }

    private static async Task<Dictionary<string, User>> SeedUsersAsync(
        AppDbContext context,
        IReadOnlyDictionary<string, int> roles)
    {
        var now = DateTime.UtcNow;
        var definitions = new[]
        {
            new DemoUser("admin", "admin.demo", "admin.demo@elos.local", "Admin", new DateTime(1990, 3, 15), now.AddHours(-2)),
            new DemoUser("content", "content.demo", "content.demo@elos.local", "Content Manager", new DateTime(1993, 7, 21), now.AddDays(-1)),
            new DemoUser("teacher", "teacher.demo", "teacher.demo@elos.local", "Teacher", new DateTime(1991, 11, 8), now.AddHours(-4)),
            new DemoUser("student1", "minh.anh", "minh.anh@elos.local", "Student", new DateTime(2013, 5, 12), now.AddHours(-1)),
            new DemoUser("student2", "gia.huy", "gia.huy@elos.local", "Student", new DateTime(2013, 9, 3), now.AddDays(-2)),
            new DemoUser("student3", "bao.ngoc", "bao.ngoc@elos.local", "Student", new DateTime(2014, 1, 20), now.AddDays(-12)),
            new DemoUser("student4", "duc.anh", "duc.anh@elos.local", "Student", new DateTime(2014, 6, 17), now.AddDays(-20))
        };

        var result = new Dictionary<string, User>(StringComparer.OrdinalIgnoreCase);

        foreach (var definition in definitions)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Email == definition.Email);
            if (user == null)
            {
                user = new User
                {
                    Username = definition.Username,
                    Email = definition.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(DemoPassword),
                    BirthDate = definition.BirthDate,
                    IsActive = true,
                    RoleId = roles[definition.Role],
                    CreateAt = now.AddMonths(-3),
                    UpdateAt = now,
                    LastLoginAt = definition.LastLoginAt
                };
                context.Users.Add(user);
            }

            result[definition.Key] = user;
        }

        await context.SaveChangesAsync();
        return result;
    }

    private static async Task<Dictionary<string, StudentProfile>> SeedStudentProfilesAsync(
        AppDbContext context,
        IReadOnlyDictionary<string, User> users)
    {
        var definitions = new[]
        {
            new DemoStudent("student1", "Minh Anh", 4, 780, 12, DateTime.Today),
            new DemoStudent("student2", "Gia Huy", 3, 460, 5, DateTime.Today.AddDays(-2)),
            new DemoStudent("student3", "Bảo Ngọc", 2, 230, 0, DateTime.Today.AddDays(-12)),
            new DemoStudent("student4", "Đức Anh", 1, 80, 0, DateTime.Today.AddDays(-20))
        };

        var result = new Dictionary<string, StudentProfile>(StringComparer.OrdinalIgnoreCase);

        foreach (var definition in definitions)
        {
            var user = users[definition.Key];
            var profile = await context.StudentProfiles!
                .FirstOrDefaultAsync(x => x.StudentId == user.Id);

            if (profile == null)
            {
                profile = new StudentProfile
                {
                    StudentId = user.Id,
                    Nickname = definition.Nickname,
                    Level = definition.Level,
                    XP = definition.Xp,
                    CurrentStreakDays = definition.Streak,
                    LastActiveDate = definition.LastActiveDate,
                    AvatarUrl = "/images/default-avatar.png"
                };
                context.StudentProfiles!.Add(profile);
            }

            result[definition.Key] = profile;
        }

        await context.SaveChangesAsync();
        return result;
    }

    private static async Task<AcademicYear> SeedAcademicYearAsync(AppDbContext context)
    {
        const string label = "2026-2027";
        var year = await context.AcademicYears!
            .FirstOrDefaultAsync(x => x.YearLabel == label);

        if (year == null)
        {
            year = new AcademicYear
            {
                YearLabel = label,
                StartDate = new DateTime(2026, 8, 17),
                EndDate = new DateTime(2027, 5, 28),
                IsActive = true
            };
            context.AcademicYears!.Add(year);
            await context.SaveChangesAsync();
        }

        return year;
    }

    private static async Task<Dictionary<string, Course>> SeedCoursesAsync(
        AppDbContext context,
        User creator)
    {
        var definitions = new[]
        {
            new DemoCourse(
                "grade6",
                "English Foundations 6",
                "6",
                "Xây dựng nền tảng từ vựng, ngữ pháp và giao tiếp tiếng Anh cho học sinh lớp 6.",
                true),
            new DemoCourse(
                "grade7",
                "Everyday English 7",
                "7",
                "Luyện tiếng Anh qua các chủ đề đời sống, trường học và cộng đồng.",
                true),
            new DemoCourse(
                "draft",
                "English Discovery 8 (Draft)",
                "8",
                "Khóa học mẫu ở trạng thái nháp để Content Manager kiểm thử quy trình xuất bản.",
                false)
        };

        var result = new Dictionary<string, Course>(StringComparer.OrdinalIgnoreCase);

        foreach (var definition in definitions)
        {
            var course = await context.Courses!
                .FirstOrDefaultAsync(x => x.CourseName == definition.Name);

            if (course == null)
            {
                course = new Course
                {
                    CourseName = definition.Name,
                    GradeLevel = definition.Grade,
                    Description = definition.Description,
                    IsPublished = definition.IsPublished,
                    IsDeleted = false,
                    CreatorId = creator.Id
                };
                context.Courses!.Add(course);
            }

            result[definition.Key] = course;
        }

        await context.SaveChangesAsync();
        return result;
    }

    private static async Task<Dictionary<string, Lesson>> SeedLessonsAsync(
        AppDbContext context,
        IReadOnlyDictionary<string, Course> courses)
    {
        var definitions = new[]
        {
            new DemoLesson("greetings", "Greetings and Introductions", "Communication", 15, 1, 50, true, "grade6"),
            new DemoLesson("family", "My Family", "Family", 20, 2, 60, true, "grade6"),
            new DemoLesson("school", "At School", "School", 20, 3, 60, true, "grade6"),
            new DemoLesson("hobbies", "Hobbies and Free Time", "Hobbies", 25, 1, 70, true, "grade7"),
            new DemoLesson("community", "Around the Community", "Community", 25, 2, 70, true, "grade7"),
            new DemoLesson("draft", "Future Adventures (Draft)", "Travel", 25, 1, 80, false, "draft")
        };

        var result = new Dictionary<string, Lesson>(StringComparer.OrdinalIgnoreCase);

        foreach (var definition in definitions)
        {
            var course = courses[definition.CourseKey];
            var lesson = await context.Lessons!
                .FirstOrDefaultAsync(x => x.CourseId == course.CourseId && x.Title == definition.Title);

            if (lesson == null)
            {
                lesson = new Lesson
                {
                    Title = definition.Title,
                    Topic = definition.Topic,
                    EstimatedMinutes = definition.Minutes,
                    OrderIndex = definition.Order,
                    XPReward = definition.XpReward,
                    IsPublished = definition.IsPublished,
                    CourseId = course.CourseId
                };
                context.Lessons!.Add(lesson);
            }

            result[definition.Key] = lesson;
        }

        await context.SaveChangesAsync();
        return result;
    }

    private static async Task SeedVocabulariesAsync(
        AppDbContext context,
        IReadOnlyDictionary<string, Lesson> lessons)
    {
        var definitions = new[]
        {
            new DemoVocabulary("greetings", "hello", "xin chào", "Hello, my name is Minh."),
            new DemoVocabulary("greetings", "welcome", "chào mừng", "Welcome to our English class."),
            new DemoVocabulary("greetings", "introduce", "giới thiệu", "Let me introduce my best friend."),
            new DemoVocabulary("greetings", "classmate", "bạn cùng lớp", "Lan is my new classmate."),
            new DemoVocabulary("family", "parents", "bố mẹ", "My parents work at a hospital."),
            new DemoVocabulary("family", "brother", "anh/em trai", "My brother likes football."),
            new DemoVocabulary("family", "sister", "chị/em gái", "Her sister is twelve years old."),
            new DemoVocabulary("family", "grandparents", "ông bà", "We visit our grandparents on Sunday."),
            new DemoVocabulary("school", "library", "thư viện", "I read books in the library."),
            new DemoVocabulary("school", "science", "khoa học", "Science is my favorite subject."),
            new DemoVocabulary("school", "homework", "bài tập về nhà", "I finish my homework after dinner."),
            new DemoVocabulary("school", "playground", "sân chơi", "The students are in the playground."),
            new DemoVocabulary("hobbies", "painting", "vẽ tranh", "Painting helps me relax."),
            new DemoVocabulary("hobbies", "cycling", "đạp xe", "We go cycling every weekend."),
            new DemoVocabulary("hobbies", "collect", "sưu tầm", "I collect postcards from many countries."),
            new DemoVocabulary("hobbies", "gardening", "làm vườn", "Gardening is my grandfather's hobby."),
            new DemoVocabulary("community", "bakery", "tiệm bánh", "The bakery is next to the bank."),
            new DemoVocabulary("community", "museum", "bảo tàng", "The museum opens at nine o'clock."),
            new DemoVocabulary("community", "crosswalk", "vạch qua đường", "Use the crosswalk to cross the street."),
            new DemoVocabulary("community", "neighborhood", "khu phố", "Our neighborhood is quiet and friendly.")
        };

        foreach (var definition in definitions)
        {
            var lesson = lessons[definition.LessonKey];
            if (await context.Vocabularies!
                .AnyAsync(x => x.LessonId == lesson.LessonId && x.Word == definition.Word))
            {
                continue;
            }

            context.Vocabularies!.Add(new Vocabulary
            {
                LessonId = lesson.LessonId,
                Word = definition.Word,
                Meaning = definition.Meaning,
                ExampleSentence = definition.Example,
                ImageUrl = "/images/vocabulary/default.png",
                AudioUrl = null
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedQuizzesAsync(
        AppDbContext context,
        IReadOnlyDictionary<string, Lesson> lessons)
    {
        var definitions = new[]
        {
            new DemoQuiz("greetings", "Which phrase is used when meeting someone for the first time?", new[] { "Nice to meet you.", "Good night.", "See you yesterday.", "Never mind." }, "Nice to meet you."),
            new DemoQuiz("greetings", "Choose the correct introduction.", new[] { "I am name Minh.", "My name is Minh.", "Me name Minh.", "Mine name Minh." }, "My name is Minh."),
            new DemoQuiz("greetings", "What does “classmate” mean?", new[] { "A teacher", "A family member", "A student in the same class", "A neighbor" }, "A student in the same class"),
            new DemoQuiz("family", "My mother's son is my ____.", new[] { "brother", "uncle", "grandfather", "cousin" }, "brother"),
            new DemoQuiz("family", "Choose the correct sentence.", new[] { "She have two sisters.", "She has two sisters.", "She having two sisters.", "She is have two sisters." }, "She has two sisters."),
            new DemoQuiz("family", "Who are your grandparents?", new[] { "Your parents' parents", "Your parents' friends", "Your classmates", "Your cousins" }, "Your parents' parents"),
            new DemoQuiz("school", "Where can you borrow books?", new[] { "The playground", "The library", "The laboratory", "The cafeteria" }, "The library"),
            new DemoQuiz("school", "I ____ my homework every evening.", new[] { "do", "does", "doing", "didn't" }, "do"),
            new DemoQuiz("school", "Science, English and Math are school ____.", new[] { "subjects", "buildings", "uniforms", "breaks" }, "subjects"),
            new DemoQuiz("hobbies", "Which activity usually needs a bicycle?", new[] { "Cycling", "Painting", "Reading", "Gardening" }, "Cycling"),
            new DemoQuiz("hobbies", "She enjoys ____ pictures.", new[] { "paint", "painting", "paints", "painted" }, "painting"),
            new DemoQuiz("hobbies", "A hobby is an activity you do in your ____ time.", new[] { "free", "late", "busy", "school" }, "free"),
            new DemoQuiz("community", "Where can you buy bread?", new[] { "A museum", "A bakery", "A library", "A hospital" }, "A bakery"),
            new DemoQuiz("community", "The bank is ____ the post office.", new[] { "next to", "under", "inside of", "during" }, "next to"),
            new DemoQuiz("community", "What should pedestrians use to cross a busy street?", new[] { "A crosswalk", "A museum", "A bakery", "A playground" }, "A crosswalk")
        };

        foreach (var definition in definitions)
        {
            var lesson = lessons[definition.LessonKey];
            if (await context.Quizzes!
                .AnyAsync(x => x.LessonId == lesson.LessonId && x.Question == definition.Question))
            {
                continue;
            }

            context.Quizzes!.Add(new Quiz
            {
                LessonId = lesson.LessonId,
                Question = definition.Question,
                QuizType = "MultipleChoice",
                Options = System.Text.Json.JsonSerializer.Serialize(definition.Options),
                CorrectAnswer = definition.CorrectAnswer
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedMiniGamesAsync(
        AppDbContext context,
        IReadOnlyDictionary<string, Lesson> lessons)
    {
        foreach (var lessonKey in new[] { "greetings", "family", "school", "hobbies", "community" })
        {
            var lesson = lessons[lessonKey];
            var games = new[]
            {
                new MiniGame
                {
                    LessonId = lesson.LessonId,
                    GameType = "WordScramble",
                    Title = $"{lesson.Title} - Word Scramble",
                    XPReward = 15
                },
                new MiniGame
                {
                    LessonId = lesson.LessonId,
                    GameType = "Matching",
                    Title = $"{lesson.Title} - Matching",
                    XPReward = 20
                }
            };

            foreach (var game in games)
            {
                if (!await context.MiniGames!.AnyAsync(x =>
                    x.LessonId == game.LessonId &&
                    x.GameType == game.GameType &&
                    x.Title == game.Title))
                {
                    context.MiniGames!.Add(game);
                }
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task<Dictionary<string, Class>> SeedClassesAsync(
        AppDbContext context,
        AcademicYear academicYear,
        User teacher,
        IReadOnlyDictionary<string, Course> courses)
    {
        var definitions = new[]
        {
            new DemoClass("class6", "6A - English Foundations", "6", "grade6"),
            new DemoClass("class7", "7A - Everyday English", "7", "grade7")
        };

        var result = new Dictionary<string, Class>(StringComparer.OrdinalIgnoreCase);

        foreach (var definition in definitions)
        {
            var course = courses[definition.CourseKey];
            var classEntity = await context.Classes!.FirstOrDefaultAsync(x =>
                x.AcademicYearId == academicYear.AcademicYearId &&
                x.ClassName == definition.Name);

            if (classEntity == null)
            {
                classEntity = new Class
                {
                    ClassName = definition.Name,
                    GradeLevel = definition.Grade,
                    AcademicYearId = academicYear.AcademicYearId,
                    TeacherId = teacher.Id,
                    CourseId = course.CourseId,
                    IsDeleted = false
                };
                context.Classes!.Add(classEntity);
            }

            result[definition.Key] = classEntity;
        }

        await context.SaveChangesAsync();
        return result;
    }

    private static async Task SeedEnrollmentsAsync(
        AppDbContext context,
        IReadOnlyDictionary<string, Class> classes,
        IReadOnlyDictionary<string, User> users)
    {
        var definitions = new[]
        {
            ("class6", "student1"),
            ("class6", "student2"),
            ("class6", "student3"),
            ("class6", "student4"),
            ("class7", "student1"),
            ("class7", "student2")
        };

        foreach (var (classKey, studentKey) in definitions)
        {
            var classEntity = classes[classKey];
            var student = users[studentKey];

            if (!await context.ClassEnrollments!.AnyAsync(x =>
                x.ClassId == classEntity.ClassId && x.StudentId == student.Id))
            {
                context.ClassEnrollments!.Add(new ClassEnrollment
                {
                    ClassId = classEntity.ClassId,
                    StudentId = student.Id,
                    EnrolledAt = DateTime.UtcNow.AddMonths(-2)
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task<Dictionary<string, WeeklyAssignment>> SeedAssignmentsAsync(
        AppDbContext context,
        IReadOnlyDictionary<string, Lesson> lessons,
        IReadOnlyDictionary<string, Course> courses,
        IReadOnlyDictionary<string, Class> classes)
    {
        var today = DateTime.Today;
        var monday = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
        var definitions = new[]
        {
            new DemoAssignment("current1", "greetings", "grade6", "class6", monday, today.AddDays(2), true),
            new DemoAssignment("current2", "family", "grade6", "class6", monday, today.AddDays(5), true),
            new DemoAssignment("overdue", "school", "grade6", "class6", monday.AddDays(-7), today.AddDays(-2), true),
            new DemoAssignment("grade7", "hobbies", "grade7", "class7", monday, today.AddDays(3), true),
            new DemoAssignment("draft", "community", "grade7", "class7", monday, today.AddDays(7), false)
        };

        var result = new Dictionary<string, WeeklyAssignment>(StringComparer.OrdinalIgnoreCase);

        foreach (var definition in definitions)
        {
            var lesson = lessons[definition.LessonKey];
            var course = courses[definition.CourseKey];
            var classEntity = classes[definition.ClassKey];
            var assignment = await context.WeeklyAssignments!
                .FirstOrDefaultAsync(x =>
                    x.ClassId == classEntity.ClassId &&
                    x.CourseId == course.CourseId &&
                    x.LessonId == lesson.LessonId);

            if (assignment == null)
            {
                assignment = new WeeklyAssignment
                {
                    ClassId = classEntity.ClassId,
                    CourseId = course.CourseId,
                    LessonId = lesson.LessonId
                };
                context.WeeklyAssignments!.Add(assignment);
            }

            // Keep relative demo deadlines useful whenever the application is started.
            assignment.WeekStartDate = definition.WeekStart;
            assignment.DueDate = definition.DueDate;
            assignment.IsVisible = definition.IsVisible;
            assignment.IncludeVocabulary = true;
            assignment.IncludeQuiz = true;
            assignment.IncludeMiniGame = true;
            result[definition.Key] = assignment;
        }

        await context.SaveChangesAsync();

        foreach (var definition in definitions)
        {
            var assignment = result[definition.Key];
            var lesson = lessons[definition.LessonKey];

            var vocabularyIds = await context.Vocabularies!
                .Where(x => x.LessonId == lesson.LessonId)
                .Select(x => x.VocabularyId)
                .ToListAsync();
            var quizIds = await context.Quizzes!
                .Where(x => x.LessonId == lesson.LessonId)
                .Select(x => x.QuizId)
                .ToListAsync();
            var gameIds = await context.MiniGames!
                .Where(x => x.LessonId == lesson.LessonId)
                .Select(x => x.GameId)
                .ToListAsync();

            foreach (var vocabularyId in vocabularyIds)
            {
                if (!await context.WeeklyAssignmentVocabularies.AnyAsync(x =>
                    x.AssignmentId == assignment.AssignmentId &&
                    x.VocabularyId == vocabularyId))
                {
                    context.WeeklyAssignmentVocabularies.Add(new WeeklyAssignmentVocabulary
                    {
                        AssignmentId = assignment.AssignmentId,
                        VocabularyId = vocabularyId
                    });
                }
            }

            foreach (var quizId in quizIds)
            {
                if (!await context.WeeklyAssignmentQuizzes.AnyAsync(x =>
                    x.AssignmentId == assignment.AssignmentId && x.QuizId == quizId))
                {
                    context.WeeklyAssignmentQuizzes.Add(new WeeklyAssignmentQuiz
                    {
                        AssignmentId = assignment.AssignmentId,
                        QuizId = quizId
                    });
                }
            }

            foreach (var gameId in gameIds)
            {
                if (!await context.WeeklyAssignmentMiniGames.AnyAsync(x =>
                    x.AssignmentId == assignment.AssignmentId && x.GameId == gameId))
                {
                    context.WeeklyAssignmentMiniGames.Add(new WeeklyAssignmentMiniGame
                    {
                        AssignmentId = assignment.AssignmentId,
                        GameId = gameId
                    });
                }
            }
        }

        await context.SaveChangesAsync();
        return result;
    }

    private static async Task<Dictionary<string, DailyMission>> SeedDailyMissionsAsync(AppDbContext context)
    {
        var definitions = new[]
        {
            new DemoMission("lesson", "CompleteLesson", "Hoàn thành 1 bài học hôm nay", 1, 25),
            new DemoMission("flashcard", "ReviewFlashcards", "Ôn tập 5 thẻ từ vựng", 5, 15),
            new DemoMission("game", "PlayMiniGame", "Chơi 1 trò chơi từ vựng", 1, 15)
        };

        var result = new Dictionary<string, DailyMission>(StringComparer.OrdinalIgnoreCase);

        foreach (var definition in definitions)
        {
            var mission = await context.DailyMissions!
                .FirstOrDefaultAsync(x => x.Type == definition.Type);

            if (mission == null)
            {
                mission = new DailyMission
                {
                    Type = definition.Type,
                    Description = definition.Description,
                    TargetValue = definition.Target,
                    XPReward = definition.XpReward
                };
                context.DailyMissions!.Add(mission);
            }

            result[definition.Key] = mission;
        }

        await context.SaveChangesAsync();
        return result;
    }

    private static async Task<Dictionary<string, Badge>> SeedBadgesAsync(AppDbContext context)
    {
        var definitions = new[]
        {
            new DemoBadge("first", "First Step", "LessonsCompleted", 1, "/images/badges/first-step.png"),
            new DemoBadge("streak", "Seven Day Streak", "StreakDays", 7, "/images/badges/seven-day-streak.png"),
            new DemoBadge("xp", "XP Explorer", "TotalXP", 500, "/images/badges/xp-explorer.png")
        };

        var result = new Dictionary<string, Badge>(StringComparer.OrdinalIgnoreCase);

        foreach (var definition in definitions)
        {
            var badge = await context.Badges!
                .FirstOrDefaultAsync(x => x.BadgeName == definition.Name);

            if (badge == null)
            {
                badge = new Badge
                {
                    BadgeName = definition.Name,
                    TriggerType = definition.TriggerType,
                    TriggerValue = definition.TriggerValue,
                    IconUrl = definition.IconUrl
                };
                context.Badges!.Add(badge);
            }

            result[definition.Key] = badge;
        }

        await context.SaveChangesAsync();
        return result;
    }

    private static async Task SeedLearningActivityAsync(
        AppDbContext context,
        IReadOnlyDictionary<string, StudentProfile> students,
        IReadOnlyDictionary<string, Lesson> lessons,
        IReadOnlyDictionary<string, WeeklyAssignment> assignments,
        IReadOnlyDictionary<string, DailyMission> missions,
        IReadOnlyDictionary<string, Badge> badges)
    {
        await SeedProgressAsync(context, students, lessons);
        await SeedQuizAttemptsAsync(context, students, lessons, assignments);
        await SeedFlashcardSessionsAsync(context, students, lessons);
        await SeedGameProgressAsync(context, students, lessons);
        await SeedXpTransactionsAsync(context, students);
        await SeedStudentMissionsAsync(context, students, missions);
        await SeedStudentBadgesAsync(context, students, badges);
    }

    private static async Task SeedProgressAsync(
        AppDbContext context,
        IReadOnlyDictionary<string, StudentProfile> students,
        IReadOnlyDictionary<string, Lesson> lessons)
    {
        var definitions = new[]
        {
            new DemoProgress("student1", "greetings", 92, 50, "Completed", true, DateTime.UtcNow.AddDays(-8)),
            new DemoProgress("student1", "family", 84, 60, "Completed", true, DateTime.UtcNow.AddDays(-3)),
            new DemoProgress("student1", "hobbies", 76, 70, "Completed", true, DateTime.UtcNow.AddDays(-1)),
            new DemoProgress("student2", "greetings", 58, 50, "Completed", true, DateTime.UtcNow.AddDays(-5)),
            new DemoProgress("student2", "family", 42, 0, "In Progress", true, DateTime.UtcNow.AddDays(-2)),
            new DemoProgress("student3", "greetings", 35, 0, "In Progress", true, DateTime.UtcNow.AddDays(-13))
        };

        foreach (var definition in definitions)
        {
            var student = students[definition.StudentKey];
            var lesson = lessons[definition.LessonKey];
            if (await context.Progresses!.AnyAsync(x =>
                x.StudentId == student.StudentId &&
                x.LessonId == lesson.LessonId &&
                x.IsBestAttempt))
            {
                continue;
            }

            context.Progresses!.Add(new Progress
            {
                StudentId = student.StudentId,
                LessonId = lesson.LessonId,
                QuizScore = definition.Score,
                XPEarned = definition.XpEarned,
                CompletionStatus = definition.Status,
                IsBestAttempt = definition.IsBest,
                CompletedAt = definition.CompletedAt
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedQuizAttemptsAsync(
        AppDbContext context,
        IReadOnlyDictionary<string, StudentProfile> students,
        IReadOnlyDictionary<string, Lesson> lessons,
        IReadOnlyDictionary<string, WeeklyAssignment> assignments)
    {
        var definitions = new[]
        {
            new DemoAttempt("student1", "greetings", "current1", 3, 3, 92, 125, true, DateTime.UtcNow.AddDays(-8)),
            new DemoAttempt("student1", "family", "current2", 3, 3, 84, 150, true, DateTime.UtcNow.AddDays(-3)),
            new DemoAttempt("student2", "greetings", "current1", 3, 2, 58, 190, true, DateTime.UtcNow.AddDays(-5)),
            new DemoAttempt("student3", "greetings", "current1", 3, 1, 35, 240, false, DateTime.UtcNow.AddDays(-13))
        };

        foreach (var definition in definitions)
        {
            var student = students[definition.StudentKey];
            var lesson = lessons[definition.LessonKey];
            if (await context.QuizAttempts.AnyAsync(x =>
                x.StudentId == student.StudentId && x.LessonId == lesson.LessonId))
            {
                continue;
            }

            var quizzes = await context.Quizzes!
                .Where(x => x.LessonId == lesson.LessonId)
                .OrderBy(x => x.QuizId)
                .Take(definition.TotalQuestions)
                .ToListAsync();

            var attempt = new QuizAttempt
            {
                StudentId = student.StudentId,
                LessonId = lesson.LessonId,
                WeeklyAssignmentId = assignments[definition.AssignmentKey].AssignmentId,
                TotalQuestions = quizzes.Count,
                CorrectCount = Math.Min(definition.CorrectCount, quizzes.Count),
                Score = definition.Score,
                TimeSpentSec = definition.TimeSpentSeconds,
                StartedAt = definition.SubmittedAt.AddSeconds(-definition.TimeSpentSeconds),
                SubmittedAt = definition.SubmittedAt,
                XpAwarded = definition.XpAwarded
            };

            for (var index = 0; index < quizzes.Count; index++)
            {
                var quiz = quizzes[index];
                var isCorrect = index < attempt.CorrectCount;
                attempt.Answers.Add(new QuizAttemptAnswer
                {
                    QuizId = quiz.QuizId,
                    SelectedAnswer = isCorrect ? quiz.CorrectAnswer : GetFirstIncorrectOption(quiz),
                    IsCorrect = isCorrect
                });
            }

            context.QuizAttempts.Add(attempt);
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedFlashcardSessionsAsync(
        AppDbContext context,
        IReadOnlyDictionary<string, StudentProfile> students,
        IReadOnlyDictionary<string, Lesson> lessons)
    {
        var definitions = new[]
        {
            new DemoFlashcardSession("student1", "family", 4, 3, DateTime.UtcNow.AddDays(-2)),
            new DemoFlashcardSession("student2", "greetings", 4, 2, DateTime.UtcNow.AddDays(-1)),
            new DemoFlashcardSession("student3", "greetings", 4, 1, DateTime.UtcNow.AddDays(-12))
        };

        foreach (var definition in definitions)
        {
            var student = students[definition.StudentKey];
            var lesson = lessons[definition.LessonKey];
            if (await context.FlashcardSessions.AnyAsync(x =>
                x.StudentId == student.StudentId && x.LessonId == lesson.LessonId))
            {
                continue;
            }

            var vocabularies = await context.Vocabularies!
                .Where(x => x.LessonId == lesson.LessonId)
                .OrderBy(x => x.VocabularyId)
                .Take(definition.CardsReviewed)
                .ToListAsync();

            var session = new FlashcardSession
            {
                StudentId = student.StudentId,
                LessonId = lesson.LessonId,
                CardsReviewed = vocabularies.Count,
                StartedAt = definition.CompletedAt.AddMinutes(-5),
                CompletedAt = definition.CompletedAt
            };

            for (var index = 0; index < vocabularies.Count; index++)
            {
                session.CardResults.Add(new FlashcardCardResult
                {
                    VocabularyId = vocabularies[index].VocabularyId,
                    KnewIt = index < definition.KnownCards
                });
            }

            context.FlashcardSessions.Add(session);
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedGameProgressAsync(
        AppDbContext context,
        IReadOnlyDictionary<string, StudentProfile> students,
        IReadOnlyDictionary<string, Lesson> lessons)
    {
        var definitions = new[]
        {
            new DemoGameProgress("student1", "greetings", "Matching", 100, 20, DateTime.UtcNow.AddDays(-7)),
            new DemoGameProgress("student1", "family", "WordScramble", 100, 15, DateTime.UtcNow.AddDays(-2)),
            new DemoGameProgress("student2", "greetings", "WordScramble", 100, 15, DateTime.UtcNow.AddDays(-1)),
            new DemoGameProgress("student3", "greetings", "Matching", 50, 10, DateTime.UtcNow.AddDays(-12))
        };

        foreach (var definition in definitions)
        {
            var student = students[definition.StudentKey];
            var lesson = lessons[definition.LessonKey];
            var game = await context.MiniGames!.FirstAsync(x =>
                x.LessonId == lesson.LessonId && x.GameType == definition.GameType);

            if (!await context.StudentGameProgresses!.AnyAsync(x =>
                x.StudentId == student.StudentId && x.GameId == game.GameId))
            {
                context.StudentGameProgresses!.Add(new StudentGameProgress
                {
                    StudentId = student.StudentId,
                    GameId = game.GameId,
                    Score = definition.Score,
                    XPEarned = definition.XpEarned,
                    CompletedAt = definition.CompletedAt
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedXpTransactionsAsync(
        AppDbContext context,
        IReadOnlyDictionary<string, StudentProfile> students)
    {
        var definitions = new[]
        {
            new DemoXp("student1", 50, "Quiz - Greetings and Introductions", DateTime.UtcNow.AddDays(-8)),
            new DemoXp("student1", 20, "MiniGame - Greetings Matching", DateTime.UtcNow.AddDays(-7)),
            new DemoXp("student1", 60, "Quiz - My Family", DateTime.UtcNow.AddDays(-3)),
            new DemoXp("student2", 50, "Quiz - Greetings and Introductions", DateTime.UtcNow.AddDays(-5)),
            new DemoXp("student2", 15, "MiniGame - Greetings WordScramble", DateTime.UtcNow.AddDays(-1)),
            new DemoXp("student3", 10, "MiniGame - Greetings Matching", DateTime.UtcNow.AddDays(-12))
        };

        foreach (var definition in definitions)
        {
            var student = students[definition.StudentKey];
            if (!await context.XpTransactions!.AnyAsync(x =>
                x.StudentId == student.StudentId && x.Source == definition.Source))
            {
                context.XpTransactions!.Add(new XpTransaction
                {
                    StudentId = student.StudentId,
                    Amount = definition.Amount,
                    Source = definition.Source,
                    CreatedAt = definition.CreatedAt
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedStudentMissionsAsync(
        AppDbContext context,
        IReadOnlyDictionary<string, StudentProfile> students,
        IReadOnlyDictionary<string, DailyMission> missions)
    {
        var today = DateTime.Today;
        var definitions = new[]
        {
            new DemoStudentMission("student1", "lesson", 1, true),
            new DemoStudentMission("student1", "flashcard", 3, false),
            new DemoStudentMission("student1", "game", 1, true),
            new DemoStudentMission("student2", "lesson", 0, false),
            new DemoStudentMission("student2", "flashcard", 5, true),
            new DemoStudentMission("student2", "game", 0, false),
            new DemoStudentMission("student3", "lesson", 0, false),
            new DemoStudentMission("student3", "flashcard", 1, false),
            new DemoStudentMission("student3", "game", 0, false),
            new DemoStudentMission("student4", "lesson", 0, false),
            new DemoStudentMission("student4", "flashcard", 0, false),
            new DemoStudentMission("student4", "game", 0, false)
        };

        foreach (var definition in definitions)
        {
            var student = students[definition.StudentKey];
            var mission = missions[definition.MissionKey];
            if (!await context.StudentMissions!.AnyAsync(x =>
                x.StudentId == student.StudentId &&
                x.MissionId == mission.MissionId &&
                x.Date.Date == today))
            {
                context.StudentMissions!.Add(new StudentMission
                {
                    StudentId = student.StudentId,
                    MissionId = mission.MissionId,
                    Date = today,
                    CurrentValue = definition.CurrentValue,
                    IsCompleted = definition.IsCompleted
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedStudentBadgesAsync(
        AppDbContext context,
        IReadOnlyDictionary<string, StudentProfile> students,
        IReadOnlyDictionary<string, Badge> badges)
    {
        var definitions = new[]
        {
            new DemoStudentBadge("student1", "first", DateTime.UtcNow.AddDays(-20)),
            new DemoStudentBadge("student1", "streak", DateTime.UtcNow.AddDays(-5)),
            new DemoStudentBadge("student1", "xp", DateTime.UtcNow.AddDays(-2)),
            new DemoStudentBadge("student2", "first", DateTime.UtcNow.AddDays(-6))
        };

        foreach (var definition in definitions)
        {
            var student = students[definition.StudentKey];
            var badge = badges[definition.BadgeKey];
            if (!await context.StudentBadges!.AnyAsync(x =>
                x.StudentId == student.StudentId && x.BadgeId == badge.BadgeId))
            {
                context.StudentBadges!.Add(new StudentBadge
                {
                    StudentId = student.StudentId,
                    BadgeId = badge.BadgeId,
                    EarnedAt = definition.EarnedAt
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedCommunicationsAsync(
        AppDbContext context,
        IReadOnlyDictionary<string, User> users,
        IReadOnlyDictionary<string, StudentProfile> students)
    {
        var notifications = new[]
        {
            new Notification
            {
                UserId = users["student1"].Id,
                Type = "Assignment",
                Message = "Bài học My Family đã được giao và sẽ hết hạn trong tuần này.",
                IsRead = false,
                CreateAt = DateTime.UtcNow.AddHours(-6)
            },
            new Notification
            {
                UserId = users["student2"].Id,
                Type = "Achievement",
                Message = "Bạn đã nhận huy hiệu First Step.",
                IsRead = true,
                CreateAt = DateTime.UtcNow.AddDays(-5)
            }
        };

        foreach (var notification in notifications)
        {
            if (!await context.Notifications!.AnyAsync(x =>
                x.UserId == notification.UserId && x.Message == notification.Message))
            {
                context.Notifications!.Add(notification);
            }
        }

        var feedbacks = new[]
        {
            new TeacherFeedback
            {
                TeacherId = users["teacher"].Id,
                StudentId = students["student2"].StudentId,
                Content = "Em cần ôn lại từ vựng bài My Family và thử làm quiz thêm một lần.",
                IsRead = false,
                CreateAt = DateTime.UtcNow.AddDays(-1)
            },
            new TeacherFeedback
            {
                TeacherId = users["teacher"].Id,
                StudentId = students["student3"].StudentId,
                Content = "Cô ghi nhận nỗ lực của em. Hãy duy trì học 10 phút mỗi ngày để cải thiện điểm.",
                IsRead = true,
                CreateAt = DateTime.UtcNow.AddDays(-4)
            }
        };

        foreach (var feedback in feedbacks)
        {
            if (!await context.TeacherFeedbacks!.AnyAsync(x =>
                x.TeacherId == feedback.TeacherId &&
                x.StudentId == feedback.StudentId &&
                x.Content == feedback.Content))
            {
                context.TeacherFeedbacks!.Add(feedback);
            }
        }

        var systemNotifications = new[]
        {
            new SystemNotification
            {
                Title = "Chào mừng năm học 2026-2027",
                Content = "Chúc giáo viên và học sinh một năm học hiệu quả cùng English Learning Online System.",
                Recipient = "Tất cả người dùng",
                UserType = "Tất cả",
                Status = "Đã phát hành",
                PublishTime = DateTime.Now.AddDays(-7),
                UserId = users["admin"].Id,
                CreatedAt = DateTime.Now.AddDays(-7)
            },
            new SystemNotification
            {
                Title = "Cập nhật nội dung học tập tuần này",
                Content = "Các bài học và hoạt động từ vựng mới đã sẵn sàng để giáo viên giao cho lớp.",
                Recipient = "Giáo viên",
                UserType = "Giáo viên",
                Status = "Đã phát hành",
                PublishTime = DateTime.Now.AddDays(-2),
                UserId = users["admin"].Id,
                CreatedAt = DateTime.Now.AddDays(-2)
            },
            new SystemNotification
            {
                Title = "Thông báo bảo trì dự kiến",
                Content = "Đây là bản nháp thông báo bảo trì để kiểm thử chức năng quản trị.",
                Recipient = "Tất cả người dùng",
                UserType = "Tất cả",
                Status = "Bản nháp",
                PublishTime = null,
                UserId = users["admin"].Id,
                CreatedAt = DateTime.Now
            }
        };

        foreach (var notification in systemNotifications)
        {
            if (!await context.SystemNotifications!.AnyAsync(x => x.Title == notification.Title))
            {
                context.SystemNotifications!.Add(notification);
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedAuditLogsAsync(
        AppDbContext context,
        IReadOnlyDictionary<string, User> users)
    {
        var definitions = new[]
        {
            new AuditLog
            {
                UserId = users["admin"].Id,
                Username = users["admin"].Username,
                UserRole = "Admin",
                Action = "Khởi tạo dữ liệu mẫu hệ thống",
                Timestamp = DateTime.UtcNow.AddDays(-7)
            },
            new AuditLog
            {
                UserId = users["content"].Id,
                Username = users["content"].Username,
                UserRole = "Content Manager",
                Action = "Xuất bản khóa học English Foundations 6",
                Timestamp = DateTime.UtcNow.AddDays(-6)
            },
            new AuditLog
            {
                UserId = users["teacher"].Id,
                Username = users["teacher"].Username,
                UserRole = "Teacher",
                Action = "Giao bài học My Family cho lớp 6A",
                Timestamp = DateTime.UtcNow.AddDays(-2)
            }
        };

        foreach (var auditLog in definitions)
        {
            if (!await context.AuditLogs.AnyAsync(x =>
                x.UserId == auditLog.UserId && x.Action == auditLog.Action))
            {
                context.AuditLogs.Add(auditLog);
            }
        }

        await context.SaveChangesAsync();
    }

    private static string GetFirstIncorrectOption(Quiz quiz)
    {
        try
        {
            var options = System.Text.Json.JsonSerializer.Deserialize<List<string>>(quiz.Options) ?? [];
            return options.FirstOrDefault(x =>
                       !string.Equals(x, quiz.CorrectAnswer, StringComparison.OrdinalIgnoreCase))
                   ?? "Incorrect answer";
        }
        catch (System.Text.Json.JsonException)
        {
            return "Incorrect answer";
        }
    }

    private sealed record DemoUser(
        string Key,
        string Username,
        string Email,
        string Role,
        DateTime BirthDate,
        DateTime LastLoginAt);

    private sealed record DemoStudent(
        string Key,
        string Nickname,
        int Level,
        int Xp,
        int Streak,
        DateTime LastActiveDate);

    private sealed record DemoCourse(
        string Key,
        string Name,
        string Grade,
        string Description,
        bool IsPublished);

    private sealed record DemoLesson(
        string Key,
        string Title,
        string Topic,
        int Minutes,
        int Order,
        int XpReward,
        bool IsPublished,
        string CourseKey);

    private sealed record DemoVocabulary(
        string LessonKey,
        string Word,
        string Meaning,
        string Example);

    private sealed record DemoQuiz(
        string LessonKey,
        string Question,
        string[] Options,
        string CorrectAnswer);

    private sealed record DemoClass(
        string Key,
        string Name,
        string Grade,
        string CourseKey);

    private sealed record DemoAssignment(
        string Key,
        string LessonKey,
        string CourseKey,
        string ClassKey,
        DateTime WeekStart,
        DateTime DueDate,
        bool IsVisible);

    private sealed record DemoMission(
        string Key,
        string Type,
        string Description,
        int Target,
        int XpReward);

    private sealed record DemoBadge(
        string Key,
        string Name,
        string TriggerType,
        int TriggerValue,
        string IconUrl);

    private sealed record DemoProgress(
        string StudentKey,
        string LessonKey,
        int Score,
        int XpEarned,
        string Status,
        bool IsBest,
        DateTime CompletedAt);

    private sealed record DemoAttempt(
        string StudentKey,
        string LessonKey,
        string AssignmentKey,
        int TotalQuestions,
        int CorrectCount,
        int Score,
        int TimeSpentSeconds,
        bool XpAwarded,
        DateTime SubmittedAt);

    private sealed record DemoFlashcardSession(
        string StudentKey,
        string LessonKey,
        int CardsReviewed,
        int KnownCards,
        DateTime CompletedAt);

    private sealed record DemoGameProgress(
        string StudentKey,
        string LessonKey,
        string GameType,
        int Score,
        int XpEarned,
        DateTime CompletedAt);

    private sealed record DemoXp(
        string StudentKey,
        int Amount,
        string Source,
        DateTime CreatedAt);

    private sealed record DemoStudentMission(
        string StudentKey,
        string MissionKey,
        int CurrentValue,
        bool IsCompleted);

    private sealed record DemoStudentBadge(
        string StudentKey,
        string BadgeKey,
        DateTime EarnedAt);
}
