using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.SeedData;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // ── Content Manager ───────────────────────────────────────────────────
        if (!await context.Users.AnyAsync(x => x.Email == "contentmanager@english.com"))
        {
            var cmUser = new User
            {
                Username = "contentmanager",
                Email = "contentmanager@english.com",
                Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                IsActive = true,
                RoleId = 5
            };
            context.Users.Add(cmUser);
            await context.SaveChangesAsync();
        }

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

        var contentManager = new User
        {
            Username = "contentmanager01",
            Email = "content@english.com",
            Password = BCrypt.Net.BCrypt.HashPassword("123456"),
            IsActive = true,
            RoleId = 5
        };
        context.Users.Add(contentManager);
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

        // ── System Notifications ──────────────────────────────────────────────
        if (context.SystemNotifications != null && !await context.SystemNotifications.AnyAsync())
        {
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
            string creatorName = adminUser != null ? "Administrator" : "Admin";

            // Main 6 notifications from the screenshot
            var mainList = new List<SystemNotification>
            {
                new SystemNotification
                {
                    Title = "Bảo trì hệ thống định kỳ",
                    Content = "Hệ thống sẽ được bảo trì từ 22:00 ngày 25/05/2025 đến 04:00 ngày 26/05/2025 để nâng cấp hệ thống và tối ưu trải nghiệm học tập.",
                    Recipient = "Tất cả người dùng",
                    UserType = "Tất cả",
                    Status = "Đã phát hành",
                    PublishTime = new DateTime(2025, 05, 24, 20, 0, 0),
                    Creator = creatorName
                },
                new SystemNotification
                {
                    Title = "Cập nhật tính năng mới",
                    Content = "Chúng tôi vừa cập nhật một số tính năng mới bao gồm giao diện học từ vựng trực quan hơn, tối ưu tốc độ tải trang bài học.",
                    Recipient = "Giáo viên",
                    UserType = "Giáo viên",
                    Status = "Đã phát hành",
                    PublishTime = new DateTime(2025, 05, 22, 10, 30, 0),
                    Creator = creatorName
                },
                new SystemNotification
                {
                    Title = "Thông báo cuộc thi tiếng Anh",
                    Content = "Cuộc thi English Challenge 2025 sẽ bắt đầu từ ngày 01/06. Vui lòng đăng ký tham gia trước hạn chót.",
                    Recipient = "Học sinh",
                    UserType = "Học sinh",
                    Status = "Đã lên lịch",
                    PublishTime = new DateTime(2025, 05, 28, 8, 0, 0),
                    Creator = creatorName
                },
                new SystemNotification
                {
                    Title = "Nhắc nhở hoàn thành bài học",
                    Content = "Hãy hoàn thành các bài học trong tuần này để duy trì chuỗi Streak và đạt thứ hạng cao trên bảng xếp hạng.",
                    Recipient = "Học sinh, Phụ huynh",
                    UserType = "Nhiều vai trò",
                    Status = "Đã phát hành",
                    PublishTime = new DateTime(2025, 05, 20, 9, 15, 0),
                    Creator = creatorName
                },
                new SystemNotification
                {
                    Title = "Lịch nghỉ lễ 30/4 - 1/5",
                    Content = "Trung tâm sẽ nghỉ lễ từ ngày 30/04 đến hết ngày 03/05. Chúc mọi người một kỳ nghỉ vui vẻ!",
                    Recipient = "Phụ huynh, Giáo viên",
                    UserType = "Nhiều vai trò",
                    Status = "Bản nháp",
                    PublishTime = null,
                    Creator = creatorName
                },
                new SystemNotification
                {
                    Title = "Nội quy lớp học trực tuyến",
                    Content = "Vui lòng đọc và tuân thủ nội quy lớp học trực tuyến mới để đảm bảo lớp học diễn ra nghiêm túc và hiệu quả.",
                    Recipient = "Học sinh",
                    UserType = "Học sinh",
                    Status = "Đã hủy",
                    PublishTime = new DateTime(2025, 05, 18, 14, 45, 0),
                    Creator = creatorName
                }
            };

            context.SystemNotifications.AddRange(mainList);

            // Add additional items to match the stats in the screenshot:
            // Need 9 more "Đã phát hành" (total 12)
            for (int i = 1; i <= 9; i++)
            {
                context.SystemNotifications.Add(new SystemNotification
                {
                    Title = $"Thông báo phát hành bổ sung {i}",
                    Content = $"Nội dung thông báo bổ sung {i} tự động tạo để hiển thị đủ số lượng thống kê mẫu.",
                    Recipient = i % 2 == 0 ? "Tất cả người dùng" : "Giáo viên",
                    UserType = i % 2 == 0 ? "Tất cả" : "Giáo viên",
                    Status = "Đã phát hành",
                    PublishTime = DateTime.Now.AddDays(-i),
                    Creator = creatorName
                });
            }

            // Need 4 more "Đã lên lịch" (total 5)
            for (int i = 1; i <= 4; i++)
            {
                context.SystemNotifications.Add(new SystemNotification
                {
                    Title = $"Thông báo lên lịch bổ sung {i}",
                    Content = $"Nội dung thông báo lên lịch bổ sung {i} tự động tạo để hiển thị đủ số lượng thống kê mẫu.",
                    Recipient = "Học sinh",
                    UserType = "Học sinh",
                    Status = "Đã lên lịch",
                    PublishTime = DateTime.Now.AddDays(i),
                    Creator = creatorName
                });
            }

            // Need 3 more "Bản nháp" (total 4)
            for (int i = 1; i <= 3; i++)
            {
                context.SystemNotifications.Add(new SystemNotification
                {
                    Title = $"Thông báo nháp bổ sung {i}",
                    Content = $"Nội dung thông báo nháp bổ sung {i} tự động tạo để hiển thị đủ số lượng thống kê mẫu.",
                    Recipient = "Phụ huynh",
                    UserType = "Nhiều vai trò",
                    Status = "Bản nháp",
                    PublishTime = null,
                    Creator = creatorName
                });
            }

            // Need 2 more "Đã hủy" (total 3)
            for (int i = 1; i <= 2; i++)
            {
                context.SystemNotifications.Add(new SystemNotification
                {
                    Title = $"Thông báo hủy bổ sung {i}",
                    Content = $"Nội dung thông báo hủy bổ sung {i} tự động tạo để hiển thị đủ số lượng thống kê mẫu.",
                    Recipient = "Tất cả người dùng",
                    UserType = "Tất cả",
                    Status = "Đã hủy",
                    PublishTime = DateTime.Now.AddDays(-10 - i),
                    Creator = creatorName
                });
            }

            await context.SaveChangesAsync();
        }




        // ── Analytics test data ────────────────────────────────────────────────────
        // Seed thêm students và quiz attempts trải đều 30 ngày để test biểu đồ
        var random = new Random(42);
        var students = new List<User>();

        for (int i = 2; i <= 8; i++)
        {
            var u = new User
            {
                Username = $"student0{i}",
                Email = $"student0{i}@english.com",
                Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                IsActive = true,
                RoleId = 1
            };
            students.Add(u);
            context.Users.Add(u);
        }
        await context.SaveChangesAsync();

        foreach (var s in students)
        {
            context.StudentProfiles!.Add(new StudentProfile
            {
                StudentId = s.Id,
                Nickname = s.Username,
                AvatarUrl = "/images/default-avatar.png",
                Level = random.Next(1, 5),
                XP = random.Next(100, 800),
                CurrentStreakDays = random.Next(0, 14),
                LastActiveDate = DateTime.Now.AddDays(-random.Next(0, 5))
            });
        }
        await context.SaveChangesAsync();

        var allLessons = new[] { lesson1, lesson2, lesson3, lesson4, lesson5 };
        var allStudentIds = students.Select(s => s.Id).Append(studentUser.Id).ToList();

        // Tạo attempts trải đều 30 ngày, mỗi ngày 1-4 lượt
        for (int daysAgo = 29; daysAgo >= 0; daysAgo--)
        {
            var day = DateTime.UtcNow.AddDays(-daysAgo).Date;
            int attemptsToday = random.Next(1, 5);

            for (int a = 0; a < attemptsToday; a++)
            {
                var lesson = allLessons[random.Next(allLessons.Length)];
                var studentId = allStudentIds[random.Next(allStudentIds.Count)];
                var score = random.Next(40, 101);
                var timeSpent = random.Next(300, 900); // 5-15 phút

                context.QuizAttempts!.Add(new QuizAttempt
                {
                    StudentId = studentId,
                    LessonId = lesson.LessonId,
                    StartedAt = day.AddHours(random.Next(7, 22)),
                    SubmittedAt = day.AddHours(random.Next(7, 22)).AddSeconds(timeSpent),
                    TotalQuestions = 4,
                    CorrectCount = (int)Math.Round(score / 100.0 * 4),
                    Score = score,
                    TimeSpentSec = timeSpent,
                    XpAwarded = score >= 50
                });
            }
        }
        await context.SaveChangesAsync();

        // Seed thêm flashcard sessions cho các lesson
        foreach (var lesson in allLessons)
        {
            for (int i = 0; i < 5; i++)
            {
                var studentId = allStudentIds[random.Next(allStudentIds.Count)];
                var startAt = DateTime.UtcNow.AddDays(-random.Next(0, 30));
                bool allKnew = random.Next(0, 2) == 1;

                context.FlashcardSessions!.Add(new FlashcardSession
                {
                    StudentId = studentId,
                    LessonId = lesson.LessonId,
                    StartedAt = startAt,
                    CompletedAt = startAt.AddMinutes(random.Next(5, 20)),
                    CardsReviewed = 5,
                    CardResults = Enumerable.Range(0, 5).Select(ci => new FlashcardCardResult
                    {
                        VocabularyId = vocabs.Where(v => v.LessonId == lesson.LessonId)
                                             .ElementAtOrDefault(ci % vocabs.Count(v => v.LessonId == lesson.LessonId))?.VocabularyId ?? vocabs[0].VocabularyId,
                        KnewIt = allKnew || random.Next(0, 2) == 1
                    }).ToList()
                });
            }
        }
        await context.SaveChangesAsync();
    }



}