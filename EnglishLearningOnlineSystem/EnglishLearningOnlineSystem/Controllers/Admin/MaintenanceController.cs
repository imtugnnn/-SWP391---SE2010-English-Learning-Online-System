using Microsoft.AspNetCore.Mvc;
using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EnglishLearningOnlineSystem.Controllers.Admin;

public class MaintenanceController : Controller
{
    private readonly AppDbContext _context;

    public MaintenanceController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("/Admin/Maintenance")]
    public async Task<IActionResult> Index()
    {
        var userRoleSession = HttpContext.Session.GetString("UserRole");
        if (userRoleSession != "2")
        {
            return RedirectToAction("Login", "Auth");
        }

        var enabledSetting = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == "MaintenanceModeEnabled");
        
        var startAtSetting = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == "MaintenanceStartAt");

        bool isEnabled = enabledSetting != null && string.Equals(enabledSetting.Value, "true", StringComparison.OrdinalIgnoreCase);
        DateTime? startAt = null;

        if (startAtSetting != null && !string.IsNullOrWhiteSpace(startAtSetting.Value))
        {
            if (DateTime.TryParse(startAtSetting.Value, out var dt))
            {
                startAt = dt;
            }
        }

        bool isMaintenanceActive = isEnabled || (startAt.HasValue && DateTime.Now >= startAt.Value);
        bool isScheduled = startAt.HasValue && startAt.Value > DateTime.Now;

        var logs = await _context.AuditLogs
            .Where(l => l.Action.Contains("bảo trì") || l.Action.Contains("Maintenance"))
            .OrderByDescending(l => l.Timestamp)
            .Take(10)
            .ToListAsync();

        ViewBag.IsEnabled = isEnabled;
        ViewBag.StartAt = startAt;
        ViewBag.IsScheduled = isScheduled;
        ViewBag.IsMaintenanceActive = isMaintenanceActive;
        ViewBag.AuditLogs = logs;

        return View("~/Views/Admin/Maintenance/Index.cshtml");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Schedule(DateTime startAt)
    {
        var userRoleSession = HttpContext.Session.GetString("UserRole");
        if (userRoleSession != "2")
        {
            return RedirectToAction("Login", "Auth");
        }

        if (startAt <= DateTime.Now)
        {
            TempData["ErrorMessage"] = "Thời gian bắt đầu bảo trì phải lớn hơn thời gian hiện tại.";
            return RedirectToAction(nameof(Index));
        }

        var enabledSetting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == "MaintenanceModeEnabled");
        var startAtSetting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == "MaintenanceStartAt");

        if (enabledSetting == null)
        {
            enabledSetting = new SystemSetting { Key = "MaintenanceModeEnabled", Value = "false", Description = "Bật/Tắt chế độ bảo trì hệ thống" };
            _context.SystemSettings.Add(enabledSetting);
        }
        else
        {
            enabledSetting.Value = "false";
            enabledSetting.UpdatedAt = DateTime.UtcNow;
        }

        if (startAtSetting == null)
        {
            startAtSetting = new SystemSetting { Key = "MaintenanceStartAt", Value = startAt.ToString("yyyy-MM-dd HH:mm:ss"), Description = "Thời gian bắt đầu bảo trì hệ thống" };
            _context.SystemSettings.Add(startAtSetting);
        }
        else
        {
            startAtSetting.Value = startAt.ToString("yyyy-MM-dd HH:mm:ss");
            startAtSetting.UpdatedAt = DateTime.UtcNow;
        }

        // Tự động tạo System Notification gửi tới người dùng
        var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin" || u.RoleId == 2);
        int adminId = adminUser != null ? adminUser.Id : 1;
        var currentAdminId = GetCurrentUserId() ?? adminId;

        var notification = new SystemNotification
        {
            Title = "Thông báo bảo trì hệ thống",
            Content = $"Hệ thống English Learning Online System sẽ tiến hành bảo trì định kỳ bắt đầu từ lúc {startAt:dd/MM/yyyy HH:mm}. Trong thời gian này, các chức năng học tập và giảng dạy sẽ tạm thời không thể truy cập. Rất mong quý người học, giáo viên thông cảm vì sự bất tiện này.",
            Recipient = "Tất cả người dùng",
            UserType = "Tất cả",
            Status = "Đã phát hành",
            PublishTime = DateTime.Now,
            UserId = currentAdminId,
            CreatedAt = DateTime.Now
        };

        _context.SystemNotifications!.Add(notification);

        // Ghi AuditLog
        await LogActivityAsync(currentAdminId, $"Lên lịch bảo trì hệ thống từ lúc {startAt:dd/MM/yyyy HH:mm} và phát thông báo");

        await _context.SaveChangesAsync();

        TempData["Message"] = $"Lên lịch bảo trì thành công từ lúc {startAt:dd/MM/yyyy HH:mm}. Hệ thống đã tự động tạo thông báo gửi tới người dùng.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelSchedule()
    {
        var userRoleSession = HttpContext.Session.GetString("UserRole");
        if (userRoleSession != "2")
        {
            return RedirectToAction("Login", "Auth");
        }

        var startAtSetting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == "MaintenanceStartAt");
        if (startAtSetting != null)
        {
            startAtSetting.Value = "";
            startAtSetting.UpdatedAt = DateTime.UtcNow;
            _context.SystemSettings.Update(startAtSetting);
        }

        var currentAdminId = GetCurrentUserId() ?? 1;
        await LogActivityAsync(currentAdminId, "Hủy lịch bảo trì hệ thống");
        await _context.SaveChangesAsync();

        TempData["Message"] = "Đã hủy lịch bảo trì hệ thống thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleImmediate(bool enable)
    {
        var userRoleSession = HttpContext.Session.GetString("UserRole");
        if (userRoleSession != "2")
        {
            return RedirectToAction("Login", "Auth");
        }

        var enabledSetting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == "MaintenanceModeEnabled");
        var startAtSetting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == "MaintenanceStartAt");

        if (enabledSetting == null)
        {
            enabledSetting = new SystemSetting { Key = "MaintenanceModeEnabled", Value = enable ? "true" : "false" };
            _context.SystemSettings.Add(enabledSetting);
        }
        else
        {
            enabledSetting.Value = enable ? "true" : "false";
            enabledSetting.UpdatedAt = DateTime.UtcNow;
            _context.SystemSettings.Update(enabledSetting);
        }

        // Khi bật ngay hoặc tắt bảo trì, xóa trường lịch hẹn để tránh xung đột
        if (startAtSetting != null)
        {
            startAtSetting.Value = "";
            startAtSetting.UpdatedAt = DateTime.UtcNow;
            _context.SystemSettings.Update(startAtSetting);
        }

        var currentAdminId = GetCurrentUserId() ?? 1;
        if (enable)
        {
            await LogActivityAsync(currentAdminId, "Bắt đầu bảo trì hệ thống ngay lập tức");
            TempData["Message"] = "Hệ thống đã chuyển sang chế độ bảo trì.";
        }
        else
        {
            await LogActivityAsync(currentAdminId, "Kết thúc bảo trì hệ thống, khôi phục hoạt động bình thường");
            TempData["Message"] = "Đã tắt chế độ bảo trì. Hệ thống hoạt động bình thường.";
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/maintenance")]
    public async Task<IActionResult> UnderMaintenance()
    {
        // Kiểm tra xem thực tế hệ thống có đang bảo trì không
        var enabledSetting = await _context.SystemSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == "MaintenanceModeEnabled");
        var startAtSetting = await _context.SystemSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == "MaintenanceStartAt");

        bool isEnabled = enabledSetting != null && string.Equals(enabledSetting.Value, "true", StringComparison.OrdinalIgnoreCase);
        DateTime? startAt = null;

        if (startAtSetting != null && !string.IsNullOrWhiteSpace(startAtSetting.Value))
        {
            if (DateTime.TryParse(startAtSetting.Value, out var dt))
            {
                startAt = dt;
            }
        }

        bool isMaintenanceActive = isEnabled || (startAt.HasValue && DateTime.Now >= startAt.Value);

        if (!isMaintenanceActive)
        {
            return Redirect("/");
        }

        return View("~/Views/Admin/Maintenance/UnderMaintenance.cshtml");
    }

    private int? GetCurrentUserId()
    {
        var raw = HttpContext.Session.GetString("UserId");
        return int.TryParse(raw, out var id) ? id : null;
    }

    private async Task LogActivityAsync(int userId, string action)
    {
        var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
        if (user != null)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                Username = user.Username,
                UserRole = user.Role?.Name ?? "Admin",
                Action = action,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
