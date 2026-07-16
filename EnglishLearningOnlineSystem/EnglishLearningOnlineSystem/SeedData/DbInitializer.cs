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

        if (!await context.Users.AnyAsync(x => x.Email == "admin@english.com"))
        {

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
            var course = new Course
                { CourseName = "English Grade 3", GradeLevel = "3", IsPublished = true, CreatorId = teacherUser.Id };
            var course4 = new Course
                { CourseName = "English Grade 4", GradeLevel = "4", IsPublished = true, CreatorId = teacherUser.Id };
            context.Courses!.AddRange(course, course4);
            await context.SaveChangesAsync();

            // ── Lessons ───────────────────────────────────────────────────────────
            var lesson1 = new Lesson
            {
                CourseId = course.CourseId, Title = "Animals", Topic = "Vocabulary", XPReward = 50,
                EstimatedMinutes = 15, OrderIndex = 1, IsPublished = true
            };
            var lesson2 = new Lesson
            {
                CourseId = course.CourseId, Title = "Colors & Shapes", Topic = "Vocabulary", XPReward = 50,
                EstimatedMinutes = 20, OrderIndex = 2, IsPublished = true
            };
            var lesson3 = new Lesson
            {
                CourseId = course.CourseId, Title = "Numbers 1-20", Topic = "Numbers", XPReward = 40,
                EstimatedMinutes = 10, OrderIndex = 3, IsPublished = true
            };
            var lesson4 = new Lesson
            {
                CourseId = course4.CourseId, Title = "Family Members", Topic = "Family", XPReward = 60,
                EstimatedMinutes = 20, OrderIndex = 1, IsPublished = true
            };
            var lesson5 = new Lesson
            {
                CourseId = course4.CourseId, Title = "Jobs & Occupations", Topic = "Jobs", XPReward = 60,
                EstimatedMinutes = 25, OrderIndex = 2, IsPublished = true
            };
            context.Lessons!.AddRange(lesson1, lesson2, lesson3, lesson4, lesson5);
            await context.SaveChangesAsync();

            // ── Vocabulary ────────────────────────────────────────────────────────
            var vocabs = new List<Vocabulary>
            {
                new Vocabulary
                {
                    LessonId = lesson1.LessonId, Word = "Cat", Meaning = "Con mèo",
                    ImageUrl = "https://cdn-icons-png.flaticon.com/512/616/616430.png"
                },
                new Vocabulary
                {
                    LessonId = lesson1.LessonId, Word = "Dog", Meaning = "Con chó",
                    ImageUrl = "https://cdn-icons-png.flaticon.com/512/616/616408.png"
                },
                new Vocabulary
                {
                    LessonId = lesson1.LessonId, Word = "Bird", Meaning = "Con chim",
                    ImageUrl = "https://cdn-icons-png.flaticon.com/512/616/616412.png"
                },
                new Vocabulary
                {
                    LessonId = lesson1.LessonId, Word = "Fish", Meaning = "Con cá",
                    ImageUrl = "https://cdn-icons-png.flaticon.com/512/616/616554.png"
                },
                new Vocabulary
                {
                    LessonId = lesson1.LessonId, Word = "Rabbit", Meaning = "Con thỏ",
                    ImageUrl = "https://cdn-icons-png.flaticon.com/512/616/616438.png"
                },
                new Vocabulary { LessonId = lesson1.LessonId, Word = "Elephant", Meaning = "Con voi", ImageUrl = "" },
                new Vocabulary { LessonId = lesson1.LessonId, Word = "Tiger", Meaning = "Con hổ", ImageUrl = "" },
                new Vocabulary { LessonId = lesson1.LessonId, Word = "Lion", Meaning = "Con sư tử", ImageUrl = "" },

                new Vocabulary { LessonId = lesson2.LessonId, Word = "Red", Meaning = "Màu đỏ", ImageUrl = "" },
                new Vocabulary
                    { LessonId = lesson2.LessonId, Word = "Blue", Meaning = "Màu xanh dương", ImageUrl = "" },
                new Vocabulary { LessonId = lesson2.LessonId, Word = "Green", Meaning = "Màu xanh lá", ImageUrl = "" },
                new Vocabulary { LessonId = lesson2.LessonId, Word = "Yellow", Meaning = "Màu vàng", ImageUrl = "" },
                new Vocabulary { LessonId = lesson2.LessonId, Word = "Circle", Meaning = "Hình tròn", ImageUrl = "" },
                new Vocabulary { LessonId = lesson2.LessonId, Word = "Square", Meaning = "Hình vuông", ImageUrl = "" },
                new Vocabulary
                    { LessonId = lesson2.LessonId, Word = "Triangle", Meaning = "Hình tam giác", ImageUrl = "" },

                new Vocabulary { LessonId = lesson3.LessonId, Word = "One", Meaning = "Số một", ImageUrl = "" },
                new Vocabulary { LessonId = lesson3.LessonId, Word = "Two", Meaning = "Số hai", ImageUrl = "" },
                new Vocabulary { LessonId = lesson3.LessonId, Word = "Three", Meaning = "Số ba", ImageUrl = "" },
                new Vocabulary { LessonId = lesson3.LessonId, Word = "Ten", Meaning = "Số mười", ImageUrl = "" },
                new Vocabulary { LessonId = lesson3.LessonId, Word = "Twenty", Meaning = "Số hai mươi", ImageUrl = "" },

                new Vocabulary { LessonId = lesson4.LessonId, Word = "Father", Meaning = "Bố", ImageUrl = "" },
                new Vocabulary { LessonId = lesson4.LessonId, Word = "Mother", Meaning = "Mẹ", ImageUrl = "" },
                new Vocabulary
                    { LessonId = lesson4.LessonId, Word = "Brother", Meaning = "Anh/em trai", ImageUrl = "" },
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
                new Quiz
                {
                    LessonId = lesson1.LessonId, Question = "What is 'Con mèo' in English?", QuizType = "IMAGE_CHOICE",
                    Options = "[\"Cat\",\"Dog\",\"Bird\",\"Fish\"]", CorrectAnswer = "Cat"
                },
                new Quiz
                {
                    LessonId = lesson1.LessonId, Question = "What is 'Con chó' in English?", QuizType = "IMAGE_CHOICE",
                    Options = "[\"Cat\",\"Dog\",\"Rabbit\",\"Fish\"]", CorrectAnswer = "Dog"
                },
                new Quiz
                {
                    LessonId = lesson1.LessonId, Question = "What is 'Con chim' in English?", QuizType = "IMAGE_CHOICE",
                    Options = "[\"Fish\",\"Dog\",\"Bird\",\"Cat\"]", CorrectAnswer = "Bird"
                },
                new Quiz
                {
                    LessonId = lesson1.LessonId, Question = "Which animal has a long trunk?", QuizType = "IMAGE_CHOICE",
                    Options = "[\"Tiger\",\"Elephant\",\"Lion\",\"Rabbit\"]", CorrectAnswer = "Elephant"
                },

                new Quiz
                {
                    LessonId = lesson2.LessonId, Question = "What color is 'Màu đỏ'?", QuizType = "IMAGE_CHOICE",
                    Options = "[\"Red\",\"Blue\",\"Green\",\"Yellow\"]", CorrectAnswer = "Red"
                },
                new Quiz
                {
                    LessonId = lesson2.LessonId, Question = "What shape is 'Hình tròn'?", QuizType = "IMAGE_CHOICE",
                    Options = "[\"Circle\",\"Square\",\"Triangle\",\"Rectangle\"]", CorrectAnswer = "Circle"
                },

                new Quiz
                {
                    LessonId = lesson3.LessonId, Question = "How do you say 'Số ba' in English?",
                    QuizType = "IMAGE_CHOICE", Options = "[\"One\",\"Two\",\"Three\",\"Four\"]", CorrectAnswer = "Three"
                },

                new Quiz
                {
                    LessonId = lesson4.LessonId, Question = "Who is your mother's husband?", QuizType = "IMAGE_CHOICE",
                    Options = "[\"Brother\",\"Father\",\"Uncle\",\"Grandfather\"]", CorrectAnswer = "Father"
                },
                new Quiz
                {
                    LessonId = lesson4.LessonId, Question = "What is 'Chị gái' in English?", QuizType = "IMAGE_CHOICE",
                    Options = "[\"Mother\",\"Aunt\",\"Sister\",\"Grandmother\"]", CorrectAnswer = "Sister"
                },

                new Quiz
                {
                    LessonId = lesson5.LessonId, Question = "Who works in a hospital?", QuizType = "IMAGE_CHOICE",
                    Options = "[\"Teacher\",\"Farmer\",\"Doctor\",\"Police\"]", CorrectAnswer = "Doctor"
                },
                new Quiz
                {
                    LessonId = lesson5.LessonId, Question = "Who grows crops in the field?", QuizType = "IMAGE_CHOICE",
                    Options = "[\"Teacher\",\"Farmer\",\"Doctor\",\"Police\"]", CorrectAnswer = "Farmer"
                }
            );
            await context.SaveChangesAsync();

            // ── Mini Games ────────────────────────────────────────────────────────
            context.MiniGames!.AddRange(
                new MiniGame
                {
                    LessonId = lesson1.LessonId, Title = "Animal Word Match", GameType = "WORD_MATCH", XPReward = 20
                },
                new MiniGame
                {
                    LessonId = lesson1.LessonId, Title = "Animal Memory Card", GameType = "MEMORY_CARD", XPReward = 20
                },
                new MiniGame
                    { LessonId = lesson2.LessonId, Title = "Color Fill Blank", GameType = "FILL_BLANK", XPReward = 20 },
                new MiniGame
                    { LessonId = lesson3.LessonId, Title = "Number Drag Drop", GameType = "DRAG_DROP", XPReward = 20 }
            );
            await context.SaveChangesAsync();

            // ── Weekly Assignments ────────────────────────────────────────────────
            var today = DateTime.Today;
            context.WeeklyAssignments!.AddRange(
                new WeeklyAssignment
                {
                    LessonId = lesson1.LessonId, WeekStartDate = today, DueDate = today.AddDays(7), IsVisible = true
                },
                new WeeklyAssignment
                {
                    LessonId = lesson2.LessonId, WeekStartDate = today, DueDate = today.AddDays(7), IsVisible = true
                },
                new WeeklyAssignment
                {
                    LessonId = lesson3.LessonId, WeekStartDate = today, DueDate = today.AddDays(3), IsVisible = true
                },
                new WeeklyAssignment
                {
                    LessonId = lesson4.LessonId, WeekStartDate = today, DueDate = today.AddDays(2), IsVisible = true
                },
                new WeeklyAssignment
                {
                    LessonId = lesson5.LessonId, WeekStartDate = today, DueDate = today.AddDays(10), IsVisible = true
                }
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
                new DailyMission
                {
                    Type = "COMPLETE_LESSON", TargetValue = 1, XPReward = 30,
                    Description = "Hoàn thành 1 bài học hôm nay"
                },
                new DailyMission
                    { Type = "PLAY_MINIGAME", TargetValue = 1, XPReward = 20, Description = "Chơi 1 mini game" },
                new DailyMission
                    { Type = "LOGIN_STREAK", TargetValue = 1, XPReward = 10, Description = "Đăng nhập hôm nay" }
            };
            context.DailyMissions!.AddRange(missions);
            await context.SaveChangesAsync();

            // ── Student Missions (today) ───────────────────────────────────────────
            context.StudentMissions!.AddRange(
                new StudentMission
                {
                    StudentId = studentUser.Id, MissionId = missions[0].MissionId, Date = today, CurrentValue = 0,
                    IsCompleted = false
                },
                new StudentMission
                {
                    StudentId = studentUser.Id, MissionId = missions[1].MissionId, Date = today, CurrentValue = 0,
                    IsCompleted = false
                },
                new StudentMission
                {
                    StudentId = studentUser.Id, MissionId = missions[2].MissionId, Date = today, CurrentValue = 1,
                    IsCompleted = true
                }
            );
            await context.SaveChangesAsync();

            // ── Badges ────────────────────────────────────────────────────────────
            var badge1 = new Badge
                { BadgeName = "First Step", IconUrl = "", TriggerType = "LESSONS_COMPLETED", TriggerValue = 1 };
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

        await SeedTeacherSupportDemoAsync(context);
    }

    private static async Task SeedTeacherSupportDemoAsync(AppDbContext context)
    {
        var teacher = await context.Users.FirstOrDefaultAsync(u => u.Email == "teacher@english.com");
        if (teacher == null)
        {
            teacher = new User
            {
                Username = "teacher01",
                Email = "teacher@english.com",
                Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                IsActive = true,
                RoleId = 3
            };
            context.Users.Add(teacher);
            await context.SaveChangesAsync();
        }

        var academicYear = await context.AcademicYears!
            .FirstOrDefaultAsync(y => y.YearLabel == "2026-2027");

        if (academicYear == null)
        {
            academicYear = new AcademicYear
            {
                YearLabel = "2026-2027",
                StartDate = new DateTime(2026, 6, 1),
                EndDate = new DateTime(2027, 5, 31),
                IsActive = true
            };
            context.AcademicYears!.Add(academicYear);
            await context.SaveChangesAsync();
        }

        var course = await context.Courses!
            .FirstOrDefaultAsync(c => c.CourseName == "English Grade 3");

        if (course == null)
        {
            course = new Course
            {
                CourseName = "English Grade 3",
                GradeLevel = "3",
                IsPublished = true,
                CreatorId = teacher.Id
            };
            context.Courses!.Add(course);
            await context.SaveChangesAsync();
        }

        var lesson1 = await EnsureLessonAsync(context, course.CourseId, "Animals", "Vocabulary", 1, 15, 50);
        var lesson2 = await EnsureLessonAsync(context, course.CourseId, "Colors & Shapes", "Vocabulary", 2, 20, 50);
        var lesson3 = await EnsureLessonAsync(context, course.CourseId, "Numbers 1-20", "Numbers", 3, 10, 40);

        await EnsureQuizAsync(context, lesson1.LessonId, "What is 'Con mèo' in English?", "Cat");
        await EnsureQuizAsync(context, lesson2.LessonId, "What color is 'Màu đỏ'?", "Red");
        await EnsureQuizAsync(context, lesson3.LessonId, "How do you say 'Số ba' in English?", "Three");

        var demoClass = await context.Classes!
            .FirstOrDefaultAsync(c => c.ClassName == "TST-Support Demo" && c.TeacherId == teacher.Id);

        if (demoClass == null)
        {
            demoClass = new Class
            {
                ClassName = "TST-Support Demo",
                GradeLevel = "3",
                AcademicYearId = academicYear.AcademicYearId,
                TeacherId = teacher.Id,
                CourseId = course.CourseId,
                IsDeleted = false
            };
            context.Classes!.Add(demoClass);
            await context.SaveChangesAsync();
        }
        else if (demoClass.CourseId != course.CourseId || demoClass.AcademicYearId != academicYear.AcademicYearId)
        {
            demoClass.CourseId = course.CourseId;
            demoClass.AcademicYearId = academicYear.AcademicYearId;
            demoClass.IsDeleted = false;
            await context.SaveChangesAsync();
        }

        var today = DateTime.Today;
        await EnsureWeeklyAssignmentAsync(context, course.CourseId, lesson1.LessonId, today.AddDays(-14), today.AddDays(-7));
        await EnsureWeeklyAssignmentAsync(context, course.CourseId, lesson2.LessonId, today, today.AddDays(5));
        await EnsureWeeklyAssignmentAsync(context, course.CourseId, lesson3.LessonId, today.AddDays(-5), today.AddDays(-1));

        var lowScoreStudent = await EnsureStudentAsync(
            context,
            "support.low",
            "support.low@english.com",
            true,
            DateTime.UtcNow.AddDays(-1),
            "Low Score Student");

        var overdueStudent = await EnsureStudentAsync(
            context,
            "support.overdue",
            "support.overdue@english.com",
            true,
            DateTime.UtcNow.AddDays(-2),
            "Overdue Student");

        var inactiveStudent = await EnsureStudentAsync(
            context,
            "support.inactive",
            "support.inactive@english.com",
            true,
            DateTime.UtcNow.AddDays(-15),
            "Inactive Student");

        var notStartedStudent = await EnsureStudentAsync(
            context,
            "support.notstarted",
            "support.notstarted@english.com",
            true,
            DateTime.UtcNow.AddDays(-1),
            "Not Started Student");

        var goodStudent = await EnsureStudentAsync(
            context,
            "support.good",
            "support.good@english.com",
            true,
            DateTime.UtcNow,
            "Good Progress Student");

        foreach (var student in new[] { lowScoreStudent, overdueStudent, inactiveStudent, notStartedStudent, goodStudent })
        {
            await EnsureEnrollmentAsync(context, demoClass.ClassId, student.Id);
        }

        await EnsureProgressAsync(context, lowScoreStudent.Id, lesson1.LessonId, "Completed", 45, today.AddDays(-8), 20);
        await EnsureProgressAsync(context, lowScoreStudent.Id, lesson2.LessonId, "Completed", 50, today.AddDays(-1), 20);

        await EnsureProgressAsync(context, overdueStudent.Id, lesson2.LessonId, "Completed", 85, today.AddDays(-1), 30);

        await EnsureProgressAsync(context, inactiveStudent.Id, lesson1.LessonId, "Completed", 88, today.AddDays(-10), 40);
        await EnsureProgressAsync(context, inactiveStudent.Id, lesson2.LessonId, "Completed", 90, today.AddDays(-9), 40);
        await EnsureProgressAsync(context, inactiveStudent.Id, lesson3.LessonId, "Completed", 92, today.AddDays(-8), 40);

        await EnsureProgressAsync(context, goodStudent.Id, lesson1.LessonId, "Completed", 95, today.AddDays(-6), 50);
        await EnsureProgressAsync(context, goodStudent.Id, lesson2.LessonId, "In Progress", 90, null, 0);
        await EnsureProgressAsync(context, goodStudent.Id, lesson3.LessonId, "Completed", 88, today.AddDays(-1), 40);

        await EnsureQuizAttemptAsync(context, lowScoreStudent.Id, lesson1.LessonId, 45, 1, 3, today.AddDays(-8));
        await EnsureQuizAttemptAsync(context, overdueStudent.Id, lesson2.LessonId, 85, 3, 3, today.AddDays(-1));
        await EnsureQuizAttemptAsync(context, inactiveStudent.Id, lesson3.LessonId, 92, 3, 3, today.AddDays(-8));
        await EnsureQuizAttemptAsync(context, goodStudent.Id, lesson1.LessonId, 95, 3, 3, today.AddDays(-6));
    }

    private static async Task<Lesson> EnsureLessonAsync(
        AppDbContext context,
        int courseId,
        string title,
        string topic,
        int orderIndex,
        int estimatedMinutes,
        int xpReward)
    {
        var lesson = await context.Lessons!
            .FirstOrDefaultAsync(l => l.CourseId == courseId && l.Title == title);

        if (lesson != null)
        {
            return lesson;
        }

        lesson = new Lesson
        {
            CourseId = courseId,
            Title = title,
            Topic = topic,
            OrderIndex = orderIndex,
            EstimatedMinutes = estimatedMinutes,
            XPReward = xpReward,
            IsPublished = true
        };
        context.Lessons!.Add(lesson);
        await context.SaveChangesAsync();
        return lesson;
    }

    private static async Task EnsureQuizAsync(AppDbContext context, int lessonId, string question, string correctAnswer)
    {
        var exists = await context.Quizzes!
            .AnyAsync(q => q.LessonId == lessonId && q.Question == question);

        if (exists)
        {
            return;
        }

        context.Quizzes!.Add(new Quiz
        {
            LessonId = lessonId,
            Question = question,
            QuizType = "MultipleChoice",
            Options = "[\"Cat\",\"Dog\",\"Red\",\"Three\"]",
            CorrectAnswer = correctAnswer
        });
        await context.SaveChangesAsync();
    }

    private static async Task EnsureWeeklyAssignmentAsync(
        AppDbContext context,
        int courseId,
        int lessonId,
        DateTime weekStartDate,
        DateTime dueDate)
    {
        var assignment = await context.WeeklyAssignments!
            .FirstOrDefaultAsync(a => a.CourseId == courseId && a.LessonId == lessonId);

        if (assignment == null)
        {
            context.WeeklyAssignments!.Add(new WeeklyAssignment
            {
                CourseId = courseId,
                LessonId = lessonId,
                WeekStartDate = weekStartDate,
                DueDate = dueDate,
                IsVisible = true
            });
        }
        else
        {
            assignment.WeekStartDate = weekStartDate;
            assignment.DueDate = dueDate;
            assignment.IsVisible = true;
        }

        await context.SaveChangesAsync();
    }

    private static async Task<User> EnsureStudentAsync(
        AppDbContext context,
        string username,
        string email,
        bool isActive,
        DateTime? lastActiveDate,
        string nickname)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            user = new User
            {
                Username = username,
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                IsActive = isActive,
                RoleId = 1,
                LastLoginAt = lastActiveDate
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }
        else
        {
            user.Username = username;
            user.IsActive = isActive;
            user.RoleId = 1;
            user.LastLoginAt = lastActiveDate;
            await context.SaveChangesAsync();
        }

        var profile = await context.StudentProfiles!
            .FirstOrDefaultAsync(p => p.StudentId == user.Id);

        if (profile == null)
        {
            context.StudentProfiles!.Add(new StudentProfile
            {
                StudentId = user.Id,
                Nickname = nickname,
                AvatarUrl = "/images/default-avatar.png",
                Level = 1,
                XP = 100,
                CurrentStreakDays = 0,
                LastActiveDate = lastActiveDate
            });
        }
        else
        {
            profile.Nickname = nickname;
            profile.AvatarUrl = string.IsNullOrWhiteSpace(profile.AvatarUrl)
                ? "/images/default-avatar.png"
                : profile.AvatarUrl;
            profile.LastActiveDate = lastActiveDate;
        }

        await context.SaveChangesAsync();
        return user;
    }

    private static async Task EnsureEnrollmentAsync(AppDbContext context, int classId, int studentId)
    {
        var exists = await context.ClassEnrollments!
            .AnyAsync(e => e.ClassId == classId && e.StudentId == studentId);

        if (exists)
        {
            return;
        }

        context.ClassEnrollments!.Add(new ClassEnrollment
        {
            ClassId = classId,
            StudentId = studentId,
            EnrolledAt = DateTime.UtcNow.AddDays(-20)
        });
        await context.SaveChangesAsync();
    }

    private static async Task EnsureProgressAsync(
        AppDbContext context,
        int studentId,
        int lessonId,
        string status,
        int quizScore,
        DateTime? completedAt,
        int xpEarned)
    {
        var progress = await context.Progresses!
            .FirstOrDefaultAsync(p => p.StudentId == studentId && p.LessonId == lessonId);

        if (progress == null)
        {
            context.Progresses!.Add(new Progress
            {
                StudentId = studentId,
                LessonId = lessonId,
                CompletionStatus = status,
                QuizScore = quizScore,
                XPEarned = xpEarned,
                CompletedAt = completedAt,
                IsBestAttempt = true
            });
        }
        else
        {
            progress.CompletionStatus = status;
            progress.QuizScore = quizScore;
            progress.XPEarned = xpEarned;
            progress.CompletedAt = completedAt;
            progress.IsBestAttempt = true;
        }

        await context.SaveChangesAsync();
    }

    private static async Task EnsureQuizAttemptAsync(
        AppDbContext context,
        int studentId,
        int lessonId,
        int score,
        int correctCount,
        int totalQuestions,
        DateTime submittedAt)
    {
        var exists = await context.QuizAttempts!
            .AnyAsync(a => a.StudentId == studentId && a.LessonId == lessonId && a.SubmittedAt.Date == submittedAt.Date);

        if (exists)
        {
            return;
        }

        context.QuizAttempts!.Add(new QuizAttempt
        {
            StudentId = studentId,
            LessonId = lessonId,
            StartedAt = submittedAt.AddMinutes(-8),
            SubmittedAt = submittedAt,
            TimeSpentSec = 480,
            TotalQuestions = totalQuestions,
            CorrectCount = correctCount,
            Score = score,
            XpAwarded = score >= 50
        });
        await context.SaveChangesAsync();
    }
}
