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
        var course = new Course { CourseName = "English Grade 3", GradeLevel = "3", IsPublished = true, CreatorId = teacherUser.Id };
        var course4 = new Course { CourseName = "English Grade 4", GradeLevel = "4", IsPublished = true, CreatorId = teacherUser.Id };
        context.Courses!.AddRange(course, course4);
        await context.SaveChangesAsync();

        // ── Lessons ───────────────────────────────────────────────────────────
        var lesson1 = new Lesson { CourseId = course.CourseId, Title = "Animals", Topic = "Vocabulary", XPReward = 50, EstimatedMinutes = 15, OrderIndex = 1, IsPublished = true };
        var lesson2 = new Lesson { CourseId = course.CourseId, Title = "Colors & Shapes", Topic = "Vocabulary", XPReward = 50, EstimatedMinutes = 20, OrderIndex = 2, IsPublished = true };
        var lesson3 = new Lesson { CourseId = course.CourseId, Title = "Numbers 1-20", Topic = "Numbers", XPReward = 40, EstimatedMinutes = 10, OrderIndex = 3, IsPublished = true };
        var lesson4 = new Lesson { CourseId = course4.CourseId, Title = "Family Members", Topic = "Family", XPReward = 60, EstimatedMinutes = 20, OrderIndex = 1, IsPublished = true };
        var lesson5 = new Lesson { CourseId = course4.CourseId, Title = "Jobs & Occupations", Topic = "Jobs", XPReward = 60, EstimatedMinutes = 25, OrderIndex = 2, IsPublished = true };
        context.Lessons!.AddRange(lesson1, lesson2, lesson3, lesson4, lesson5);
        await context.SaveChangesAsync();

        // ── Vocabulary ────────────────────────────────────────────────────────
        var vocabs = new List<Vocabulary>
        {
            new Vocabulary { LessonId = lesson1.LessonId, Word = "Cat", Meaning = "Con mèo", ImageUrl = "https://cdn-icons-png.flaticon.com/512/616/616430.png" },
            new Vocabulary { LessonId = lesson1.LessonId, Word = "Dog", Meaning = "Con chó", ImageUrl = "https://cdn-icons-png.flaticon.com/512/616/616408.png" },
            new Vocabulary { LessonId = lesson1.LessonId, Word = "Bird", Meaning = "Con chim", ImageUrl = "https://cdn-icons-png.flaticon.com/512/616/616412.png" },
            new Vocabulary { LessonId = lesson1.LessonId, Word = "Fish", Meaning = "Con cá", ImageUrl = "https://cdn-icons-png.flaticon.com/512/616/616554.png" },
            new Vocabulary { LessonId = lesson1.LessonId, Word = "Rabbit", Meaning = "Con thỏ", ImageUrl = "https://cdn-icons-png.flaticon.com/512/616/616438.png" },
            new Vocabulary { LessonId = lesson1.LessonId, Word = "Elephant", Meaning = "Con voi", ImageUrl = "" },
            new Vocabulary { LessonId = lesson1.LessonId, Word = "Tiger", Meaning = "Con hổ", ImageUrl = "" },
            new Vocabulary { LessonId = lesson1.LessonId, Word = "Lion", Meaning = "Con sư tử", ImageUrl = "" },
            
            new Vocabulary { LessonId = lesson2.LessonId, Word = "Red", Meaning = "Màu đỏ", ImageUrl = "" },
            new Vocabulary { LessonId = lesson2.LessonId, Word = "Blue", Meaning = "Màu xanh dương", ImageUrl = "" },
            new Vocabulary { LessonId = lesson2.LessonId, Word = "Green", Meaning = "Màu xanh lá", ImageUrl = "" },
            new Vocabulary { LessonId = lesson2.LessonId, Word = "Yellow", Meaning = "Màu vàng", ImageUrl = "" },
            new Vocabulary { LessonId = lesson2.LessonId, Word = "Circle", Meaning = "Hình tròn", ImageUrl = "" },
            new Vocabulary { LessonId = lesson2.LessonId, Word = "Square", Meaning = "Hình vuông", ImageUrl = "" },
            new Vocabulary { LessonId = lesson2.LessonId, Word = "Triangle", Meaning = "Hình tam giác", ImageUrl = "" },

            new Vocabulary { LessonId = lesson3.LessonId, Word = "One", Meaning = "Số một", ImageUrl = "" },
            new Vocabulary { LessonId = lesson3.LessonId, Word = "Two", Meaning = "Số hai", ImageUrl = "" },
            new Vocabulary { LessonId = lesson3.LessonId, Word = "Three", Meaning = "Số ba", ImageUrl = "" },
            new Vocabulary { LessonId = lesson3.LessonId, Word = "Ten", Meaning = "Số mười", ImageUrl = "" },
            new Vocabulary { LessonId = lesson3.LessonId, Word = "Twenty", Meaning = "Số hai mươi", ImageUrl = "" },

            new Vocabulary { LessonId = lesson4.LessonId, Word = "Father", Meaning = "Bố", ImageUrl = "" },
            new Vocabulary { LessonId = lesson4.LessonId, Word = "Mother", Meaning = "Mẹ", ImageUrl = "" },
            new Vocabulary { LessonId = lesson4.LessonId, Word = "Brother", Meaning = "Anh/em trai", ImageUrl = "" },
            new Vocabulary { LessonId = lesson4.LessonId, Word = "Sister", Meaning = "Chị/em gái", ImageUrl = "" },
            new Vocabulary { LessonId = lesson4.LessonId, Word = "Grandfather", Meaning = "Ông", ImageUrl = "" },
            new Vocabulary { LessonId = lesson4.LessonId, Word = "Grandmother", Meaning = "Bà", ImageUrl = "" },

            new Vocabulary { LessonId = lesson5.LessonId, Word = "Doctor", Meaning = "Bác sĩ", ImageUrl = "" },
            new Vocabulary { LessonId = lesson5.LessonId, Word = "Teacher", Meaning = "Giáo viên", ImageUrl = "" },
            new Vocabulary { LessonId = lesson5.LessonId, Word = "Farmer", Meaning = "Nông dân", ImageUrl = "" },
            new Vocabulary { LessonId = lesson5.LessonId, Word = "Police", Meaning = "Cảnh sát", ImageUrl = "" },
            new Vocabulary { LessonId = lesson5.LessonId, Word = "Nurse", Meaning = "Y tá", ImageUrl = "" }
        };
        context.Vocabularies!.AddRange(vocabs);
        await context.SaveChangesAsync();

        // ── Quizzes ───────────────────────────────────────────────────────────
        context.Quizzes!.AddRange(
            new Quiz { LessonId = lesson1.LessonId, Question = "What is 'Con mèo' in English?", QuizType = "MultipleChoice", Options = "[\"Cat\",\"Dog\",\"Bird\",\"Fish\"]", CorrectAnswer = "Cat" },
            new Quiz { LessonId = lesson1.LessonId, Question = "What is 'Con chó' in English?", QuizType = "MultipleChoice", Options = "[\"Cat\",\"Dog\",\"Rabbit\",\"Fish\"]", CorrectAnswer = "Dog" },
            new Quiz { LessonId = lesson1.LessonId, Question = "What is 'Con chim' in English?", QuizType = "MultipleChoice", Options = "[\"Fish\",\"Dog\",\"Bird\",\"Cat\"]", CorrectAnswer = "Bird" },
            new Quiz { LessonId = lesson1.LessonId, Question = "Which animal has a long trunk?", QuizType = "MultipleChoice", Options = "[\"Tiger\",\"Elephant\",\"Lion\",\"Rabbit\"]", CorrectAnswer = "Elephant" },
            
            new Quiz { LessonId = lesson2.LessonId, Question = "What color is 'Màu đỏ'?", QuizType = "MultipleChoice", Options = "[\"Red\",\"Blue\",\"Green\",\"Yellow\"]", CorrectAnswer = "Red" },
            new Quiz { LessonId = lesson2.LessonId, Question = "What shape is 'Hình tròn'?", QuizType = "MultipleChoice", Options = "[\"Circle\",\"Square\",\"Triangle\",\"Rectangle\"]", CorrectAnswer = "Circle" },
            
            new Quiz { LessonId = lesson3.LessonId, Question = "How do you say 'Số ba' in English?", QuizType = "MultipleChoice", Options = "[\"One\",\"Two\",\"Three\",\"Four\"]", CorrectAnswer = "Three" },
            
            new Quiz { LessonId = lesson4.LessonId, Question = "Who is your mother's husband?", QuizType = "MultipleChoice", Options = "[\"Brother\",\"Father\",\"Uncle\",\"Grandfather\"]", CorrectAnswer = "Father" },
            new Quiz { LessonId = lesson4.LessonId, Question = "What is 'Chị gái' in English?", QuizType = "MultipleChoice", Options = "[\"Mother\",\"Aunt\",\"Sister\",\"Grandmother\"]", CorrectAnswer = "Sister" },
            
            new Quiz { LessonId = lesson5.LessonId, Question = "Who works in a hospital?", QuizType = "MultipleChoice", Options = "[\"Teacher\",\"Farmer\",\"Doctor\",\"Police\"]", CorrectAnswer = "Doctor" },
            new Quiz { LessonId = lesson5.LessonId, Question = "Who grows crops in the field?", QuizType = "MultipleChoice", Options = "[\"Teacher\",\"Farmer\",\"Doctor\",\"Police\"]", CorrectAnswer = "Farmer" }
        );
        await context.SaveChangesAsync();

        // ── Weekly Assignments ────────────────────────────────────────────────
        var today = DateTime.Today;
        context.WeeklyAssignments!.AddRange(
            new WeeklyAssignment { LessonId = lesson1.LessonId, WeekStartDate = today, DueDate = today.AddDays(7), IsVisible = true },
            new WeeklyAssignment { LessonId = lesson2.LessonId, WeekStartDate = today, DueDate = today.AddDays(7), IsVisible = true },
            new WeeklyAssignment { LessonId = lesson3.LessonId, WeekStartDate = today, DueDate = today.AddDays(3), IsVisible = true },
            new WeeklyAssignment { LessonId = lesson4.LessonId, WeekStartDate = today, DueDate = today.AddDays(2), IsVisible = true },
            new WeeklyAssignment { LessonId = lesson5.LessonId, WeekStartDate = today, DueDate = today.AddDays(10), IsVisible = true }
        );
        await context.SaveChangesAsync();

        // ── Progress & History ────────────────────────────────────────────────
        // Seed an attempt for Lesson 1 (so history shows up)
        var attempt = new QuizAttempt
        {
            StudentId = studentUser.Id,
            LessonId = lesson1.LessonId,
            StartedAt = DateTime.Now.AddDays(-1),
            SubmittedAt = DateTime.Now.AddDays(-1).AddMinutes(5),
            TotalQuestions = 4,
            CorrectCount = 3,
            Score = 75,
            XpAwarded = true
        };
        context.QuizAttempts!.Add(attempt);
        await context.SaveChangesAsync();

        context.Progresses!.Add(new Progress
        {
            StudentId = studentUser.Id,
            LessonId = lesson1.LessonId,
            CompletionStatus = "Completed",
            QuizScore = 75,
            XPEarned = 30,
            CompletedAt = DateTime.Now.AddDays(-1),
            IsBestAttempt = true
        });

        // ── Flashcard Session ─────────────────────────────────────────────────
        // Seed a flashcard session with low recall for Lesson 2 (triggers AI recommendation)
        var fcSession = new FlashcardSession
        {
            StudentId = studentUser.Id,
            LessonId = lesson2.LessonId,
            StartedAt = DateTime.Now.AddDays(-2),
            CompletedAt = DateTime.Now.AddDays(-2).AddMinutes(10),
            CardsReviewed = 5,
            CardResults = new List<FlashcardCardResult>
            {
                // 2 knew, 3 didn't know = 40% recall
                new FlashcardCardResult { VocabularyId = vocabs[8].VocabularyId, KnewIt = true },
                new FlashcardCardResult { VocabularyId = vocabs[9].VocabularyId, KnewIt = true },
                new FlashcardCardResult { VocabularyId = vocabs[10].VocabularyId, KnewIt = false },
                new FlashcardCardResult { VocabularyId = vocabs[11].VocabularyId, KnewIt = false },
                new FlashcardCardResult { VocabularyId = vocabs[12].VocabularyId, KnewIt = false }
            }
        };
        context.FlashcardSessions!.Add(fcSession);
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