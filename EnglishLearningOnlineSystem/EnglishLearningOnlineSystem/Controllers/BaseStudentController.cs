using EnglishLearningOnlineSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Controllers;

/// <summary>
/// Controller nền của khu vực Student.
/// Cung cấp dữ liệu hồ sơ, tiến độ học tập và thông tin hiển thị
/// dùng chung cho các trang học sinh.
/// </summary>
public class BaseStudentController : Controller
{
    protected readonly AppDbContext _db;

    public BaseStudentController(AppDbContext db)
    {
        _db = db;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Lấy UserId của học sinh từ Session
        var userIdStr = HttpContext.Session.GetString("UserId");

        if (int.TryParse(userIdStr, out var userId))
        {
            // Lấy thông tin hồ sơ cần hiển thị trên layout
            var profile = _db.StudentProfiles!
                .AsNoTracking()
                .Where(s => s.StudentId == userId)
                .Select(s => new
                {
                    s.Nickname,
                    s.AvatarUrl,
                    s.Level,
                    s.XP,
                    s.CurrentStreakDays
                })
                .FirstOrDefault();

            if (profile != null)
            {
                // Gán dữ liệu hồ sơ vào ViewBag để dùng trong _StudentLayout
                ViewBag.Nickname = profile.Nickname ?? "Học sinh";
                ViewBag.AvatarUrl = profile.AvatarUrl ?? "/images/default-avatar.png";
                ViewBag.Level = profile.Level;
                ViewBag.XP = profile.XP;
                ViewBag.StreakDays = profile.CurrentStreakDays;

                // Đếm số bài học chưa bắt đầu để hiển thị badge thông báo
                ViewBag.PendingLessons = _db.Progresses!
                    .AsNoTracking()
                    .Count(p => p.StudentId == userId
                             && p.CompletionStatus == "NOT_STARTED");
            }
        }

        base.OnActionExecuting(context);
    }
}