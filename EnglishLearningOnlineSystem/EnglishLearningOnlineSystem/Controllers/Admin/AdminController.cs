//Create by TungDPL
//Last update: 7/21/2026
using Microsoft.AspNetCore.Mvc;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;
using EnglishLearningOnlineSystem.ViewModels.Admin;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EnglishLearningOnlineSystem.Controllers.Admin;

public class AdminController : Controller
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public AdminController(IUserService userService, IRoleService roleService, AppDbContext context, IAuditLogService auditLogService)
    {
        _userService = userService;
        _roleService = roleService;
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<IActionResult> Dashboard()
    {
        // Lấy năm học đang hoạt động trước
        var activeAcademicYear = await _context.AcademicYears!
            .AsNoTracking()
            .FirstOrDefaultAsync(y => y.IsActive);

        // 1. Stats grid counts (dữ liệu thực từ database)
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        // Lọc người dùng đang hoạt động có đăng nhập (LastLoginAt) trong tháng này
        var totalUsersAll = await _context.Users.CountAsync();
        var totalUsersActive = await _context.Users.CountAsync(u => u.IsActive && u.LastLoginAt >= startOfMonth);
        var studentCountActive = await _context.Users.CountAsync(u => u.RoleId == 1 && u.IsActive && u.LastLoginAt >= startOfMonth);
        var teacherCountActive = await _context.Users.CountAsync(u => u.RoleId == 3 && u.IsActive && u.LastLoginAt >= startOfMonth);
        
        // Lọc lớp học đang hoạt động dựa vào active academic year
        var activeClassesCount = 0;
        if (activeAcademicYear != null)
        {
            activeClassesCount = await _context.Classes!
                .CountAsync(c => !c.IsDeleted && c.AcademicYearId == activeAcademicYear.AcademicYearId);
        }
        else
        {
            activeClassesCount = await _context.Classes!.CountAsync(c => !c.IsDeleted);
        }
        
        var publishedNotifsThisMonth = await _context.SystemNotifications!
            .CountAsync(n => n.Status == "Đã phát hành" && n.CreatedAt >= startOfMonth);
        var scheduledNotifsThisMonth = await _context.SystemNotifications!
            .CountAsync(n => n.Status == "Đã lên lịch" && n.CreatedAt >= startOfMonth);

        // 2. Phân bố vai trò người dùng (cho biểu đồ Pie)
        var studentCountAll = await _context.Users.CountAsync(u => u.RoleId == 1);
        var teacherCountAll = await _context.Users.CountAsync(u => u.RoleId == 3);
        var parentCountAll = await _context.Users.CountAsync(u => u.RoleId == 4);
        var contentManagerCountAll = await _context.Users.CountAsync(u => u.RoleId == 5);

        // Gán dữ liệu vào ViewBag cho Top Grid Cards
        ViewBag.TotalUsersAll = totalUsersAll;
        ViewBag.TotalUsersActive = totalUsersActive;
        ViewBag.StudentCountActive = studentCountActive;
        ViewBag.TeacherCountActive = teacherCountActive;
        ViewBag.ActiveClassesCount = activeClassesCount;
        ViewBag.ActiveAcademicYear = activeAcademicYear?.YearLabel ?? "Chưa kích hoạt";
        ViewBag.PublishedNotifsThisMonth = publishedNotifsThisMonth;
        ViewBag.ScheduledNotifsThisMonth = scheduledNotifsThisMonth;

        ViewBag.StudentCountAll = studentCountAll;
        ViewBag.TeacherCountAll = teacherCountAll;
        ViewBag.ParentCountAll = parentCountAll;
        ViewBag.ContentManagerCountAll = contentManagerCountAll;
        ViewBag.TotalUsersForChart = studentCountAll + teacherCountAll + parentCountAll + contentManagerCountAll;

        // 3. Thống kê học tập tổng quan (Overview Learning Stats) và so sánh tháng trước
        var startOfLastMonth = startOfMonth.AddMonths(-1);

        // A. Bài học hoàn thành
        var lessonsCompletedThisMonth = _context.Progresses != null
            ? await _context.Progresses.CountAsync(p => p.CompletionStatus == "Completed" && p.CompletedAt >= startOfMonth)
            : 0;
        var lessonsCompletedLastMonth = _context.Progresses != null
            ? await _context.Progresses.CountAsync(p => p.CompletionStatus == "Completed" && p.CompletedAt >= startOfLastMonth && p.CompletedAt < startOfMonth)
            : 0;
        int lessonsTrendPct = 0;
        if (lessonsCompletedLastMonth > 0)
        {
            lessonsTrendPct = (int)Math.Round((double)(lessonsCompletedThisMonth - lessonsCompletedLastMonth) / lessonsCompletedLastMonth * 100);
        }
        else if (lessonsCompletedThisMonth > 0)
        {
            lessonsTrendPct = 100;
        }

        // B. Lượt làm bài quiz
        var quizAttemptsThisMonth = _context.QuizAttempts != null
            ? await _context.QuizAttempts.CountAsync(qa => qa.SubmittedAt >= startOfMonth)
            : 0;
        var quizAttemptsLastMonth = _context.QuizAttempts != null
            ? await _context.QuizAttempts.CountAsync(qa => qa.SubmittedAt >= startOfLastMonth && qa.SubmittedAt < startOfMonth)
            : 0;
        int quizAttemptsTrendPct = 0;
        if (quizAttemptsLastMonth > 0)
        {
            quizAttemptsTrendPct = (int)Math.Round((double)(quizAttemptsThisMonth - quizAttemptsLastMonth) / quizAttemptsLastMonth * 100);
        }
        else if (quizAttemptsThisMonth > 0)
        {
            quizAttemptsTrendPct = 100;
        }

        // C. Điểm trung bình quiz
        var avgQuizScoreThisMonth = _context.QuizAttempts != null && await _context.QuizAttempts.AnyAsync(qa => qa.SubmittedAt >= startOfMonth)
            ? (int)Math.Round(await _context.QuizAttempts.Where(qa => qa.SubmittedAt >= startOfMonth).AverageAsync(qa => qa.Score))
            : 0;
        var avgQuizScoreLastMonth = _context.QuizAttempts != null && await _context.QuizAttempts.AnyAsync(qa => qa.SubmittedAt >= startOfLastMonth && qa.SubmittedAt < startOfMonth)
            ? (int)Math.Round(await _context.QuizAttempts.Where(qa => qa.SubmittedAt >= startOfLastMonth && qa.SubmittedAt < startOfMonth).AverageAsync(qa => qa.Score))
            : 0;
        int quizScoreTrendPct = avgQuizScoreThisMonth - avgQuizScoreLastMonth;

        // D. Học sinh hoạt động (Đại diện cho hoạt động học tập / Chuỗi ngày học)
        var activeStudentIdsThisMonth = new List<int>();
        var activeStudentIdsLastMonth = new List<int>();
        if (_context.XpTransactions != null)
        {
            activeStudentIdsThisMonth.AddRange(await _context.XpTransactions
                .Where(x => x.CreatedAt >= startOfMonth)
                .Select(x => x.StudentId)
                .Distinct()
                .ToListAsync());

            activeStudentIdsLastMonth.AddRange(await _context.XpTransactions
                .Where(x => x.CreatedAt >= startOfLastMonth && x.CreatedAt < startOfMonth)
                .Select(x => x.StudentId)
                .Distinct()
                .ToListAsync());
        }
        if (_context.QuizAttempts != null)
        {
            activeStudentIdsThisMonth.AddRange(await _context.QuizAttempts
                .Where(qa => qa.SubmittedAt >= startOfMonth)
                .Select(qa => qa.StudentId)
                .Distinct()
                .ToListAsync());

            activeStudentIdsLastMonth.AddRange(await _context.QuizAttempts
                .Where(qa => qa.SubmittedAt >= startOfLastMonth && qa.SubmittedAt < startOfMonth)
                .Select(qa => qa.StudentId)
                .Distinct()
                .ToListAsync());
        }
        var activeStudentsThisMonth = activeStudentIdsThisMonth.Distinct().Count();
        var activeStudentsLastMonth = activeStudentIdsLastMonth.Distinct().Count();
        int activeStudentsTrendPct = 0;
        if (activeStudentsLastMonth > 0)
        {
            activeStudentsTrendPct = (int)Math.Round((double)(activeStudentsThisMonth - activeStudentsLastMonth) / activeStudentsLastMonth * 100);
        }
        else if (activeStudentsThisMonth > 0)
        {
            activeStudentsTrendPct = 100;
        }

        var completedLessonsCount = _context.Progresses != null
            ? await _context.Progresses.CountAsync(p => p.CompletionStatus == "Completed")
            : 0;
        var totalQuizAttempts = _context.QuizAttempts != null
            ? await _context.QuizAttempts.CountAsync()
            : 0;
        var avgQuizScore = _context.QuizAttempts != null && await _context.QuizAttempts.AnyAsync()
            ? (int)Math.Round(await _context.QuizAttempts.AverageAsync(qa => qa.Score))
            : 0;
        var streakUsersCount = _context.StudentProfiles != null
            ? await _context.StudentProfiles.CountAsync(sp => sp.CurrentStreakDays > 0)
            : 0;

        ViewBag.CompletedLessonsCount = lessonsCompletedThisMonth;
        ViewBag.TotalQuizAttempts = quizAttemptsThisMonth;
        ViewBag.AvgQuizScore = avgQuizScoreThisMonth;
        ViewBag.StreakUsersCount = activeStudentsThisMonth;

        ViewBag.LessonsTrendPct = lessonsTrendPct;
        ViewBag.QuizAttemptsTrendPct = quizAttemptsTrendPct;
        ViewBag.QuizScoreTrendPct = quizScoreTrendPct;
        ViewBag.ActiveStudentsTrendPct = activeStudentsTrendPct;

        // 4. Xuương hoàn thành (6 tháng gần nhất)
        var months = new List<string>();
        var lessonTrend = new List<int>();
        var quizTrend = new List<int>();
        for (int i = 5; i >= 0; i--)
        {
            var targetMonth = now.AddMonths(-i);
            months.Add($"Tháng {targetMonth.Month}");
            
            var start = new DateTime(targetMonth.Year, targetMonth.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1);
            
            var lessonsVal = _context.Progresses != null
                ? await _context.Progresses.CountAsync(p => p.CompletionStatus == "Completed" && p.CompletedAt >= start && p.CompletedAt < end)
                : 0;
            var quizzesVal = _context.QuizAttempts != null
                ? await _context.QuizAttempts.CountAsync(qa => qa.SubmittedAt >= start && qa.SubmittedAt < end)
                : 0;

            lessonTrend.Add(lessonsVal);
            quizTrend.Add(quizzesVal);
        }
        ViewBag.TrendMonths = months;
        ViewBag.LessonTrend = lessonTrend;
        ViewBag.QuizTrend = quizTrend;

        // 5. Tỷ lệ hoàn thành bài học (Donut Chart)
        var totalStudents = await _context.Users.CountAsync(u => u.RoleId == 1);
        var totalLessons = _context.Lessons != null ? await _context.Lessons.CountAsync() : 0;
        var totalPossibleProgress = totalStudents * totalLessons;

        var completedProgressCount = _context.Progresses != null
            ? await _context.Progresses.CountAsync(p => p.CompletionStatus == "Completed")
            : 0;
        var inProgressProgressCount = _context.Progresses != null
            ? await _context.Progresses.CountAsync(p => p.CompletionStatus == "In Progress")
            : 0;
        var notStartedProgressCount = Math.Max(0, totalPossibleProgress - completedProgressCount - inProgressProgressCount);

        double pctCompleted = 0;
        double pctInProgress = 0;
        double pctNotStarted = 100; // Mặc định nếu không có bài học/học sinh nào

        if (totalPossibleProgress > 0)
        {
            pctCompleted = (double)completedProgressCount / totalPossibleProgress * 100;
            pctInProgress = (double)inProgressProgressCount / totalPossibleProgress * 100;
            pctNotStarted = (double)notStartedProgressCount / totalPossibleProgress * 100;
        }

        ViewBag.PctCompleted = (int)Math.Round(pctCompleted);
        ViewBag.PctInProgress = (int)Math.Round(pctInProgress);
        ViewBag.PctNotStarted = (int)Math.Round(pctNotStarted);

        // 6. Hiệu suất lớp học (Class Performance Table - Top 5) - lọc theo năm học đang hoạt động
        var classPerformanceList = new List<ClassPerformanceViewModel>();
        if (_context.Classes != null)
        {
            var classesQuery = _context.Classes
                .Include(c => c.Course)
                .ThenInclude(co => co.Lessons)
                .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student)
                .Where(c => !c.IsDeleted);

            if (activeAcademicYear != null)
            {
                classesQuery = classesQuery.Where(c => c.AcademicYearId == activeAcademicYear.AcademicYearId);
            }
            else
            {
                classesQuery = classesQuery.Where(c => false);
            }

            var classes = await classesQuery.ToListAsync();

            foreach (var c in classes)
            {
                var studentIds = c.Enrollments.Select(e => e.StudentId).ToList();
                var studentCount = studentIds.Count;
                var classTotalLessons = c.Course?.Lessons?.Count ?? 0;
                
                double completionRate = 0;
                double classAvgQuizScore = 0;
                int activeStudents = 0;

                if (studentCount > 0)
                {
                    var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                    activeStudents = c.Enrollments.Count(e => e.Student.LastLoginAt >= thirtyDaysAgo);

                    var lessonIds = classTotalLessons > 0 ? c.Course.Lessons.Select(l => l.LessonId).ToList() : new List<int>();

                    if (classTotalLessons > 0)
                    {
                        var completedCount = _context.Progresses != null
                            ? await _context.Progresses
                                .CountAsync(p => studentIds.Contains(p.StudentId) && lessonIds.Contains(p.LessonId) && p.CompletionStatus == "Completed")
                            : 0;

                        completionRate = (double)completedCount / (studentCount * classTotalLessons) * 100;
                    }

                    if (lessonIds.Any())
                    {
                        var quizAttempts = _context.QuizAttempts != null
                            ? await _context.QuizAttempts
                                .Where(qa => studentIds.Contains(qa.StudentId) && lessonIds.Contains(qa.LessonId))
                                .Select(qa => (double?)qa.Score)
                                .ToListAsync()
                            : new List<double?>();
                        
                        if (quizAttempts.Any(q => q.HasValue))
                        {
                            classAvgQuizScore = quizAttempts.Where(q => q.HasValue).Average(q => q.Value);
                        }
                    }
                }

                classPerformanceList.Add(new ClassPerformanceViewModel
                {
                    ClassName = c.ClassName,
                    CompletionRate = (int)Math.Round(completionRate),
                    AvgQuizScore = (int)Math.Round(classAvgQuizScore),
                    ActiveStudentsText = $"{activeStudents}/{studentCount}"
                });
            }
        }
        ViewBag.TopClassesPerformance = classPerformanceList.OrderByDescending(cp => cp.CompletionRate).Take(5).ToList();

        // 7. Nội dung học tập (Content Stats & Top Courses)
        var totalCoursesCount = _context.Courses != null ? await _context.Courses.CountAsync(c => !c.IsDeleted) : 0;
        var activeCoursesCount = _context.Courses != null ? await _context.Courses.CountAsync(c => !c.IsDeleted && c.IsPublished) : 0;
        var totalLessonsCount = _context.Lessons != null ? await _context.Lessons.CountAsync() : 0;
        var activeLessonsCount = _context.Lessons != null ? await _context.Lessons.CountAsync(l => l.IsPublished) : 0;
        var totalQuizzesCount = _context.Quizzes != null ? await _context.Quizzes.CountAsync() : 0;
        var activeQuizzesCount = _context.Quizzes != null ? await _context.Quizzes.CountAsync(q => q.Lesson.IsPublished) : 0;
        var totalMinigamesCount = _context.MiniGames != null ? await _context.MiniGames.CountAsync() : 0;
        var activeMinigamesCount = _context.MiniGames != null ? await _context.MiniGames.CountAsync(g => g.Lesson.IsPublished) : 0;

        ViewBag.TotalCoursesCount = totalCoursesCount;
        ViewBag.ActiveCoursesCount = activeCoursesCount;
        ViewBag.TotalLessonsCount = totalLessonsCount;
        ViewBag.ActiveLessonsCount = activeLessonsCount;
        ViewBag.TotalQuizzesCount = totalQuizzesCount;
        ViewBag.ActiveQuizzesCount = activeQuizzesCount;
        ViewBag.TotalMinigamesCount = totalMinigamesCount;
        ViewBag.ActiveMinigamesCount = activeMinigamesCount;

        var topCoursesByUse = new List<CourseUseViewModel>();
        if (_context.Courses != null)
        {
            var courses = await _context.Courses
                .Where(co => !co.IsDeleted)
                .Include(co => co.Classes)
                .ThenInclude(cl => cl.Enrollments)
                .ToListAsync();

            foreach (var co in courses)
            {
                var enrolledStudents = co.Classes
                    .Where(cl => !cl.IsDeleted)
                    .SelectMany(cl => cl.Enrollments)
                    .Select(e => e.StudentId)
                    .Distinct()
                    .Count();

                double pct = totalStudents > 0 ? (double)enrolledStudents / totalStudents * 100 : 0;

                topCoursesByUse.Add(new CourseUseViewModel
                {
                    CourseName = co.CourseName,
                    StudentCount = enrolledStudents,
                    UsagePercentage = (int)Math.Round(pct)
                });
            }
        }
        ViewBag.TopCoursesByUse = topCoursesByUse.OrderByDescending(t => t.StudentCount).Take(3).ToList();

        // 8. Gamification & trends
        var totalXP = _context.StudentProfiles != null && await _context.StudentProfiles.AnyAsync()
            ? await _context.StudentProfiles.SumAsync(sp => sp.XP)
            : 0;
        var unlockedBadgesCount = _context.StudentBadges != null
            ? await _context.StudentBadges.CountAsync()
            : 0;
        var leveledUpStudentsCount = _context.StudentProfiles != null
            ? await _context.StudentProfiles.CountAsync(sp => sp.Level > 1)
            : 0;
        var activeTodayStudentsCount = _context.StudentProfiles != null
            ? await _context.StudentProfiles.CountAsync(sp => sp.LastActiveDate.HasValue && sp.LastActiveDate.Value.Date == now.Date)
            : 0;

        ViewBag.TotalXP = totalXP;
        ViewBag.UnlockedBadgesCount = unlockedBadgesCount;
        ViewBag.LeveledUpStudentsCount = leveledUpStudentsCount;
        ViewBag.ActiveTodayStudentsCount = activeTodayStudentsCount;

        // XP Trend
        var xpThisMonth = _context.XpTransactions != null
            ? await _context.XpTransactions.Where(x => x.CreatedAt >= startOfMonth).SumAsync(x => (int?)x.Amount) ?? 0
            : 0;
        var xpLastMonth = _context.XpTransactions != null
            ? await _context.XpTransactions.Where(x => x.CreatedAt >= startOfLastMonth && x.CreatedAt < startOfMonth).SumAsync(x => (int?)x.Amount) ?? 0
            : 0;
        int xpTrendPct = 0;
        if (xpLastMonth > 0)
        {
            xpTrendPct = (int)Math.Round((double)(xpThisMonth - xpLastMonth) / xpLastMonth * 100);
        }
        else if (xpThisMonth > 0)
        {
            xpTrendPct = 100;
        }
        ViewBag.XpTrendPct = xpTrendPct;

        // Unlocked Badges Trend
        var badgesThisMonth = _context.StudentBadges != null
            ? await _context.StudentBadges.CountAsync(sb => sb.EarnedAt >= startOfMonth)
            : 0;
        var badgesLastMonth = _context.StudentBadges != null
            ? await _context.StudentBadges.CountAsync(sb => sb.EarnedAt >= startOfLastMonth && sb.EarnedAt < startOfMonth)
            : 0;
        int badgesTrendPct = 0;
        if (badgesLastMonth > 0)
        {
            badgesTrendPct = (int)Math.Round((double)(badgesThisMonth - badgesLastMonth) / badgesLastMonth * 100);
        }
        else if (badgesThisMonth > 0)
        {
            badgesTrendPct = 100;
        }
        ViewBag.BadgesTrendPct = badgesTrendPct;

        // Level Up Trend (correlated with active student growth)
        ViewBag.LevelUpTrendPct = activeStudentsTrendPct;

        // Active Today vs Yesterday
        var todayDate = now.Date;
        var yesterdayDate = now.AddDays(-1).Date;

        var activeTodayIds = new List<int>();
        var activeYesterdayIds = new List<int>();

        if (_context.XpTransactions != null)
        {
            activeTodayIds.AddRange(await _context.XpTransactions
                .Where(x => x.CreatedAt.Date == todayDate)
                .Select(x => x.StudentId)
                .Distinct()
                .ToListAsync());

            activeYesterdayIds.AddRange(await _context.XpTransactions
                .Where(x => x.CreatedAt.Date == yesterdayDate)
                .Select(x => x.StudentId)
                .Distinct()
                .ToListAsync());
        }
        if (_context.QuizAttempts != null)
        {
            activeTodayIds.AddRange(await _context.QuizAttempts
                .Where(qa => qa.SubmittedAt.Date == todayDate)
                .Select(qa => qa.StudentId)
                .Distinct()
                .ToListAsync());

            activeYesterdayIds.AddRange(await _context.QuizAttempts
                .Where(qa => qa.SubmittedAt.Date == yesterdayDate)
                .Select(qa => qa.StudentId)
                .Distinct()
                .ToListAsync());
        }
        var activeTodayCount = activeTodayIds.Distinct().Count();
        var activeYesterdayCount = activeYesterdayIds.Distinct().Count();
        int activeTodayTrendPct = 0;
        if (activeYesterdayCount > 0)
        {
            activeTodayTrendPct = (int)Math.Round((double)(activeTodayCount - activeYesterdayCount) / activeYesterdayCount * 100);
        }
        else if (activeTodayCount > 0)
        {
            activeTodayTrendPct = 100;
        }
        ViewBag.ActiveTodayTrendPct = activeTodayTrendPct;

        // Lấy 5 system audit log gần nhất
        var latestAuditLogs = _context.AuditLogs != null
            ? await _context.AuditLogs
                .OrderByDescending(al => al.Timestamp)
                .Take(5)
                .ToListAsync()
            : new List<AuditLog>();
        ViewBag.LatestAuditLogs = latestAuditLogs;

        // Lấy danh sách lớp học theo năm học đang hoạt động
        var activeClassesList = new List<Class>();
        if (activeAcademicYear != null)
        {
            activeClassesList = await _context.Classes!
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                .Where(c => !c.IsDeleted && c.AcademicYearId == activeAcademicYear.AcademicYearId)
                .OrderBy(c => c.ClassName)
                .ToListAsync();
        }
        ViewBag.ActiveClassesList = activeClassesList;

        return View("AdminDashboard");
    }
    
    public async Task<IActionResult> UserManagement()
    {
        var result = await _userService.GetUserManagementDataAsync();
        var roles = await _roleService.GetAllAsync();

        var activeYear = await _context.AcademicYears!
            .AsNoTracking()
            .FirstOrDefaultAsync(y => y.IsActive);

        ViewBag.Roles = roles;
        ViewBag.ActiveAcademicYearId = activeYear?.AcademicYearId;
        var vm = result.Succeeded ? result.Data : new UserManagementViewModel();
        return View("~/Views/Admin/UserManagement/Index.cshtml", vm);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
        }

        var result = await _userService.CreateAsync(vm);
        if (!result.Succeeded)
        {
            return Json(new { success = false, message = result.ErrorMessage ?? "Lỗi khi tạo người dùng." });
        }

        var adminId = GetCurrentUserId();
        if (adminId.HasValue)
        {
            await _auditLogService.LogActivityAsync(adminId.Value, $"Tạo người dùng mới: {vm.Username} ({vm.Email}) với vai trò ID {vm.RoleId}");
        }

        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> EditUser([FromBody] UserEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
        }

        var result = await _userService.UpdateAsync(vm);
        if (!result.Succeeded)
        {
            return Json(new { success = false, message = result.ErrorMessage ?? "Lỗi khi cập nhật người dùng." });
        }

        var adminId = GetCurrentUserId();
        if (adminId.HasValue)
        {
            await _auditLogService.LogActivityAsync(adminId.Value, $"Cập nhật thông tin người dùng: {vm.Username} (ID: {vm.Id})");
        }

        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> ToggleUserStatus(int id)
    {
        var userResult = await _userService.GetByIdAsync(id);
        if (!userResult.Succeeded || userResult.Data == null)
        {
            return Json(new { success = false, message = "Không tìm thấy người dùng." });
        }

        var user = userResult.Data;
        var vm = new UserEditViewModel
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            BirthDate = user.BirthDate,
            IsActive = !user.IsActive, // Toggle
            RoleId = user.RoleId,
            Password = null
        };

        var updateResult = await _userService.UpdateAsync(vm);
        if (!updateResult.Succeeded)
        {
            return Json(new { success = false, message = updateResult.ErrorMessage ?? "Lỗi khi cập nhật trạng thái." });
        }

        var adminId = GetCurrentUserId();
        if (adminId.HasValue)
        {
            var statusStr = vm.IsActive ? "kích hoạt" : "vô hiệu hóa";
            await _auditLogService.LogActivityAsync(adminId.Value, $"Thay đổi trạng thái của người dùng (ID: {id}) thành {statusStr}");
        }

        return Json(new { success = true, isActive = vm.IsActive });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var result = await _userService.DeleteAsync(id);
        if (!result.Succeeded)
        {
            return Json(new { success = false, message = result.ErrorMessage ?? "Lỗi khi xóa người dùng." });
        }

        var adminId = GetCurrentUserId();
        if (adminId.HasValue)
        {
            await _auditLogService.LogActivityAsync(adminId.Value, $"Xóa người dùng (ID: {id})");
        }

        return Json(new { success = true });
    }

    private int? GetCurrentUserId()
    {
        var raw = HttpContext.Session.GetString("UserId");
        return int.TryParse(raw, out var id) ? id : null;
    }
}

public class ClassPerformanceViewModel
{
    public string ClassName { get; set; }
    public int CompletionRate { get; set; }
    public int AvgQuizScore { get; set; }
    public string ActiveStudentsText { get; set; }
}

public class CourseUseViewModel
{
    public string CourseName { get; set; }
    public int StudentCount { get; set; }
    public int UsagePercentage { get; set; }
}   
