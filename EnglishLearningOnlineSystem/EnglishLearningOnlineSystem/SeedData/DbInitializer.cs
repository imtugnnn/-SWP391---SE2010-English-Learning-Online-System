using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.SeedData;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync(x => x.Email == "admin@english.com"))
            return;

        // ── Admin ─────────────────────────────────────────────────────────────
        var admin = new User
        {
            Username = "admin",
            Email = "admin@english.com",
            Password = BCrypt.Net.BCrypt.HashPassword("123456"),
            IsActive = true,
            RoleId = 2
        };
        context.Users.Add(admin);
        await context.SaveChangesAsync();

        // ── Student ───────────────────────────────────────────────────────────
        var studentUser = new User
        {
            Username = "student01",
            Email = "student@english.com",
            Password = BCrypt.Net.BCrypt.HashPassword("123456"),
            IsActive = true,
            RoleId = 1
        };
        context.Users.Add(studentUser);
        await context.SaveChangesAsync();

        context.StudentProfiles!.Add(new StudentProfile
        {
            StudentId = studentUser.Id,
            Nickname = "Student 01",
            AvatarUrl = "/images/default-avatar.png",
            Level = 3,
            XP = 420,
            CurrentStreakDays = 5,
            LastActiveDate = DateTime.Now
        });
        await context.SaveChangesAsync();

        // ── Teacher ───────────────────────────────────────────────────────────
        var teacherUser = new User
        {
            Username = "teacher01",
            Email = "teacher@english.com",
            Password = BCrypt.Net.BCrypt.HashPassword("123456"),
            IsActive = true,
            RoleId = 3
        };
        context.Users.Add(teacherUser);
        await context.SaveChangesAsync();

        // ── Course ────────────────────────────────────────────────────────────
        var course = new Course
        {
            CourseName = "English Grade 3",
            GradeLevel = "3",
            IsPublished = true,
            CreatorId = teacherUser.Id
        };
        context.Courses!.Add(course);
        await context.SaveChangesAsync();

        // ── Lessons ───────────────────────────────────────────────────────────
        var lesson1 = new Lesson
        {
            CourseId = course.CourseId,
            Title = "Animals",
            Topic = "Vocabulary",
            XPReward = 50,
            EstimatedMinutes = 15,
            OrderIndex = 1,
            IsPublished = true
        };
        var lesson2 = new Lesson
        {
            CourseId = course.CourseId,
            Title = "Colors & Shapes",
            Topic = "Vocabulary",
            XPReward = 50,
            EstimatedMinutes = 20,
            OrderIndex = 2,
            IsPublished = true
        };
        var lesson3 = new Lesson
        {
            CourseId = course.CourseId,
            Title = "Numbers 1-20",
            Topic = "Numbers",
            XPReward = 40,
            EstimatedMinutes = 10,
            OrderIndex = 3,
            IsPublished = true
        };
        context.Lessons!.AddRange(lesson1, lesson2, lesson3);
        await context.SaveChangesAsync();

        // ── Vocabulary ────────────────────────────────────────────────────────
        context.Vocabularies!.AddRange(
            new Vocabulary { LessonId = lesson1.LessonId, Word = "Cat", Meaning = "Con mèo", ImageUrl = "https://cdn-icons-png.flaticon.com/512/616/616430.png" },
            new Vocabulary { LessonId = lesson1.LessonId, Word = "Dog", Meaning = "Con chó", ImageUrl = "https://cdn-icons-png.flaticon.com/512/616/616408.png" },
            new Vocabulary { LessonId = lesson1.LessonId, Word = "Bird", Meaning = "Con chim", ImageUrl = "https://cdn-icons-png.flaticon.com/512/616/616412.png" },
            new Vocabulary { LessonId = lesson1.LessonId, Word = "Fish", Meaning = "Con cá", ImageUrl = "https://cdn-icons-png.flaticon.com/512/616/616554.png" },
            new Vocabulary { LessonId = lesson1.LessonId, Word = "Rabbit", Meaning = "Con thỏ", ImageUrl = "https://cdn-icons-png.flaticon.com/512/616/616438.png" },
            new Vocabulary { LessonId = lesson2.LessonId, Word = "Red", Meaning = "Màu đỏ", ImageUrl = "" },
            new Vocabulary { LessonId = lesson2.LessonId, Word = "Blue", Meaning = "Màu xanh", ImageUrl = "" },
            new Vocabulary { LessonId = lesson2.LessonId, Word = "Circle", Meaning = "Hình tròn", ImageUrl = "" },
            new Vocabulary { LessonId = lesson2.LessonId, Word = "Square", Meaning = "Hình vuông", ImageUrl = "" },
            new Vocabulary { LessonId = lesson3.LessonId, Word = "One", Meaning = "Số một", ImageUrl = "" },
            new Vocabulary { LessonId = lesson3.LessonId, Word = "Two", Meaning = "Số hai", ImageUrl = "" },
            new Vocabulary { LessonId = lesson3.LessonId, Word = "Three", Meaning = "Số ba", ImageUrl = "" }
        );
        await context.SaveChangesAsync();

        // ── Quizzes ───────────────────────────────────────────────────────────
        context.Quizzes!.AddRange(
            new Quiz
            {
                LessonId = lesson1.LessonId,
                Question = "What is 'Con mèo' in English?",
                QuizType = "IMAGE_CHOICE",
                Options = "[\"Cat\",\"Dog\",\"Bird\",\"Fish\"]",
                CorrectAnswer = "Cat"
            },
            new Quiz
            {
                LessonId = lesson1.LessonId,
                Question = "What is 'Con chó' in English?",
                QuizType = "IMAGE_CHOICE",
                Options = "[\"Cat\",\"Dog\",\"Rabbit\",\"Fish\"]",
                CorrectAnswer = "Dog"
            },
            new Quiz
            {
                LessonId = lesson1.LessonId,
                Question = "What is 'Con chim' in English?",
                QuizType = "IMAGE_CHOICE",
                Options = "[\"Fish\",\"Dog\",\"Bird\",\"Cat\"]",
                CorrectAnswer = "Bird"
            },
            new Quiz
            {
                LessonId = lesson2.LessonId,
                Question = "What color is 'Màu đỏ'?",
                QuizType = "IMAGE_CHOICE",
                Options = "[\"Red\",\"Blue\",\"Green\",\"Yellow\"]",
                CorrectAnswer = "Red"
            },
            new Quiz
            {
                LessonId = lesson2.LessonId,
                Question = "What shape is 'Hình tròn'?",
                QuizType = "IMAGE_CHOICE",
                Options = "[\"Circle\",\"Square\",\"Triangle\",\"Rectangle\"]",
                CorrectAnswer = "Circle"
            },
            new Quiz
            {
                LessonId = lesson3.LessonId,
                Question = "How do you say 'Số ba' in English?",
                QuizType = "IMAGE_CHOICE",
                Options = "[\"One\",\"Two\",\"Three\",\"Four\"]",
                CorrectAnswer = "Three"
            }
        );
        await context.SaveChangesAsync();

        // ── Mini Games ────────────────────────────────────────────────────────
        context.MiniGames!.AddRange(
            new MiniGame { LessonId = lesson1.LessonId, Title = "Animal Word Match", GameType = "WORD_MATCH", XPReward = 20 },
            new MiniGame { LessonId = lesson1.LessonId, Title = "Animal Memory Card", GameType = "MEMORY_CARD", XPReward = 20 },
            new MiniGame { LessonId = lesson2.LessonId, Title = "Color Fill Blank", GameType = "FILL_BLANK", XPReward = 20 },
            new MiniGame { LessonId = lesson3.LessonId, Title = "Number Drag Drop", GameType = "DRAG_DROP", XPReward = 20 }
        );
        await context.SaveChangesAsync();

        // ── Weekly Assignments ────────────────────────────────────────────────
        var today = DateTime.Today;
        context.WeeklyAssignments!.AddRange(
            new WeeklyAssignment
            {
                LessonId = lesson1.LessonId,
                WeekStartDate = today,
                DueDate = today.AddDays(7),
                IsVisible = true
            },
            new WeeklyAssignment
            {
                LessonId = lesson2.LessonId,
                WeekStartDate = today,
                DueDate = today.AddDays(7),
                IsVisible = true
            },
            new WeeklyAssignment
            {
                LessonId = lesson3.LessonId,
                WeekStartDate = today,
                DueDate = today.AddDays(3),
                IsVisible = true
            }
        );
        await context.SaveChangesAsync();

        // ── Daily Missions ────────────────────────────────────────────────────
        var missions = new List<DailyMission>
        {
            new DailyMission { Type = "COMPLETE_LESSON", TargetValue = 1, XPReward = 30, Description = "Hoàn thành 1 bài học hôm nay" },
            new DailyMission { Type = "PLAY_MINIGAME",   TargetValue = 1, XPReward = 20, Description = "Chơi 1 mini game" },
            new DailyMission { Type = "LOGIN_STREAK",    TargetValue = 1, XPReward = 10, Description = "Đăng nhập hôm nay" }
        };
        context.DailyMissions!.AddRange(missions);
        await context.SaveChangesAsync();

        // ── Student Missions (today) ───────────────────────────────────────────
        context.StudentMissions!.AddRange(
            new StudentMission { StudentId = studentUser.Id, MissionId = missions[0].MissionId, Date = today, CurrentValue = 0, IsCompleted = false },
            new StudentMission { StudentId = studentUser.Id, MissionId = missions[1].MissionId, Date = today, CurrentValue = 0, IsCompleted = false },
            new StudentMission { StudentId = studentUser.Id, MissionId = missions[2].MissionId, Date = today, CurrentValue = 1, IsCompleted = true}
        );
        await context.SaveChangesAsync();

        // ── Badges ────────────────────────────────────────────────────────────
        var badge1 = new Badge { BadgeName = "First Step", IconUrl = "", TriggerType = "LESSONS_COMPLETED", TriggerValue = 1 };
        var badge2 = new Badge { BadgeName = "Streak 5", IconUrl = "", TriggerType = "STREAK", TriggerValue = 5 };
        context.Badges!.AddRange(badge1, badge2);
        await context.SaveChangesAsync();

        // Give streak badge to student
        context.StudentBadges!.Add(new StudentBadge
        {
            StudentId = studentUser.Id,
            BadgeId = badge2.BadgeId,
            EarnedAt = DateTime.Now
        });
        await context.SaveChangesAsync();
    }
}