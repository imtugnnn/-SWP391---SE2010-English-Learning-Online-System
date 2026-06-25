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
            StudentCode = "STU-DEMO01",
            AvatarUrl = "/images/default-avatar.png",
            Level = 3,
            XP = 420,
            CurrentStreakDays = 5,
            LastActiveDate = DateTime.Now
        });
        await context.SaveChangesAsync();

        var parentUser = new User
        {
            Username = "parent01",
            Email = "parent@english.com",
            Password = BCrypt.Net.BCrypt.HashPassword("123456"),
            IsActive = true,
            RoleId = 4
        };
        context.Users.Add(parentUser);
        await context.SaveChangesAsync();

        var contentManager = new User
        {
            Username = "content01",
            Email = "content@english.com",
            Password = BCrypt.Net.BCrypt.HashPassword("123456"),
            IsActive = true,
            RoleId = 5
        };
        context.Users.Add(contentManager);
        await context.SaveChangesAsync();

        context.BlogPosts.AddRange(
            new BlogPost
            {
                Title = "5 mẹo ghi nhớ từ vựng tiếng Anh hiệu quả",
                Summary = "Những phương pháp đơn giản giúp bé ghi nhớ từ vựng lâu hơn.",
                Content = "1. Học từ vựng theo chủ đề.\n2. Dùng flashcard mỗi ngày.\n3. Đặt câu với từ mới.\n4. Ôn lại sau 1 ngày, 1 tuần.\n5. Chơi mini game từ vựng để ôn tập vui hơn.",
                Category = "Learning Tips",
                IsPublished = true,
                AuthorId = contentManager.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3),
                PublishedAt = DateTime.UtcNow.AddDays(-3)
            },
            new BlogPost
            {
                Title = "Phân biệt 'a' và 'an' trong tiếng Anh",
                Summary = "Quy tắc dùng mạo từ a/an cho người mới bắt đầu.",
                Content = "Dùng 'a' trước từ bắt đầu bằng phụ âm: a cat, a dog.\nDùng 'an' trước từ bắt đầu bằng nguyên âm: an apple, an orange.\nLưu ý: dựa vào âm đọc, không phải chữ cái.",
                Category = "Grammar",
                IsPublished = true,
                AuthorId = contentManager.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                PublishedAt = DateTime.UtcNow.AddDays(-1)
            },
            new BlogPost
            {
                Title = "Thông báo: Cập nhật khoá học mới tháng này",
                Summary = "Hệ thống vừa thêm khoá học và bài học mới.",
                Content = "Chúng tôi vừa bổ sung các bài học mới về chủ đề Gia đình và Nghề nghiệp. Các bé hãy vào học và nhận thêm XP nhé!",
                Category = "Announcement",
                IsPublished = false,
                AuthorId = contentManager.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
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
            new Quiz { LessonId = lesson1.LessonId, Question = "What is 'Con mèo' in English?", QuizType = "IMAGE_CHOICE", Options = "[\"Cat\",\"Dog\",\"Bird\",\"Fish\"]", CorrectAnswer = "Cat" },
            new Quiz { LessonId = lesson1.LessonId, Question = "What is 'Con chó' in English?", QuizType = "IMAGE_CHOICE", Options = "[\"Cat\",\"Dog\",\"Rabbit\",\"Fish\"]", CorrectAnswer = "Dog" },
            new Quiz { LessonId = lesson1.LessonId, Question = "What is 'Con chim' in English?", QuizType = "IMAGE_CHOICE", Options = "[\"Fish\",\"Dog\",\"Bird\",\"Cat\"]", CorrectAnswer = "Bird" },
            new Quiz { LessonId = lesson1.LessonId, Question = "Which animal has a long trunk?", QuizType = "IMAGE_CHOICE", Options = "[\"Tiger\",\"Elephant\",\"Lion\",\"Rabbit\"]", CorrectAnswer = "Elephant" },
            
            new Quiz { LessonId = lesson2.LessonId, Question = "What color is 'Màu đỏ'?", QuizType = "IMAGE_CHOICE", Options = "[\"Red\",\"Blue\",\"Green\",\"Yellow\"]", CorrectAnswer = "Red" },
            new Quiz { LessonId = lesson2.LessonId, Question = "What shape is 'Hình tròn'?", QuizType = "IMAGE_CHOICE", Options = "[\"Circle\",\"Square\",\"Triangle\",\"Rectangle\"]", CorrectAnswer = "Circle" },
            
            new Quiz { LessonId = lesson3.LessonId, Question = "How do you say 'Số ba' in English?", QuizType = "IMAGE_CHOICE", Options = "[\"One\",\"Two\",\"Three\",\"Four\"]", CorrectAnswer = "Three" },
            
            new Quiz { LessonId = lesson4.LessonId, Question = "Who is your mother's husband?", QuizType = "IMAGE_CHOICE", Options = "[\"Brother\",\"Father\",\"Uncle\",\"Grandfather\"]", CorrectAnswer = "Father" },
            new Quiz { LessonId = lesson4.LessonId, Question = "What is 'Chị gái' in English?", QuizType = "IMAGE_CHOICE", Options = "[\"Mother\",\"Aunt\",\"Sister\",\"Grandmother\"]", CorrectAnswer = "Sister" },
            
            new Quiz { LessonId = lesson5.LessonId, Question = "Who works in a hospital?", QuizType = "IMAGE_CHOICE", Options = "[\"Teacher\",\"Farmer\",\"Doctor\",\"Police\"]", CorrectAnswer = "Doctor" },
            new Quiz { LessonId = lesson5.LessonId, Question = "Who grows crops in the field?", QuizType = "IMAGE_CHOICE", Options = "[\"Teacher\",\"Farmer\",\"Doctor\",\"Police\"]", CorrectAnswer = "Farmer" }
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
        await context.SaveChangesAsync();

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

        context.QuizAttempts!.AddRange(
            new QuizAttempt { StudentId = studentUser.Id, LessonId = lesson2.LessonId, StartedAt = DateTime.Now.AddDays(-2), SubmittedAt = DateTime.Now.AddDays(-2).AddMinutes(6), TotalQuestions = 5, CorrectCount = 4, Score = 80, TimeSpentSec = 360, XpAwarded = true },
            new QuizAttempt { StudentId = studentUser.Id, LessonId = lesson3.LessonId, StartedAt = DateTime.Now.AddDays(-3), SubmittedAt = DateTime.Now.AddDays(-3).AddMinutes(4), TotalQuestions = 5, CorrectCount = 3, Score = 60, TimeSpentSec = 240, XpAwarded = true },
            new QuizAttempt { StudentId = studentUser.Id, LessonId = lesson4.LessonId, StartedAt = DateTime.Now.AddDays(-1), SubmittedAt = DateTime.Now.AddDays(-1).AddMinutes(7), TotalQuestions = 6, CorrectCount = 5, Score = 83, TimeSpentSec = 420, XpAwarded = true }
        );
        await context.SaveChangesAsync();

        context.Progresses!.AddRange(
            new Progress { StudentId = studentUser.Id, LessonId = lesson2.LessonId, CompletionStatus = "Completed", QuizScore = 80, XPEarned = 50, CompletedAt = DateTime.Now.AddDays(-2), IsBestAttempt = true },
            new Progress { StudentId = studentUser.Id, LessonId = lesson4.LessonId, CompletionStatus = "Completed", QuizScore = 83, XPEarned = 60, CompletedAt = DateTime.Now.AddDays(-1), IsBestAttempt = true }
        );
        await context.SaveChangesAsync();

        context.TeacherFeedbacks!.AddRange(
            new TeacherFeedback { TeacherId = teacherUser.Id, StudentId = studentUser.Id, Content = "Con học rất chăm chỉ và tiến bộ tốt ở phần từ vựng động vật. Hãy tiếp tục phát huy nhé!", IsRead = false, CreateAt = DateTime.Now.AddDays(-2) },
            new TeacherFeedback { TeacherId = teacherUser.Id, StudentId = studentUser.Id, Content = "Phần Numbers cần luyện thêm. Con nên ôn lại các số từ 11-20 để làm bài tốt hơn.", IsRead = false, CreateAt = DateTime.Now.AddDays(-1) }
        );
        await context.SaveChangesAsync();
    }
}