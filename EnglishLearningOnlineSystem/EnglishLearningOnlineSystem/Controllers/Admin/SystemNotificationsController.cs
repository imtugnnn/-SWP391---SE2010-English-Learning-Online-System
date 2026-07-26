//Create by TungDPL
//Create at 6/26/2026
//Last update: 7/23/2026
using Microsoft.AspNetCore.Mvc;
using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.ViewModels.Admin;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EnglishLearningOnlineSystem.Controllers.Admin
{
    // BR-NOTI-02: Only users with the Admin role are authorized to create, edit, publish, archive, or deactivate system notifications. (TODO: Implement controller-level authorization checks)
    public class SystemNotificationsController : Controller
    {
        private readonly AppDbContext _context;

        public SystemNotificationsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var notifications = await _context.SystemNotifications!
                .Include(n => n.User)
                .AsNoTracking()
                .OrderByDescending(n => n.PublishTime ?? n.CreatedAt)
                .ToListAsync();
            ViewBag.Roles = await _context.Roles.AsNoTracking().ToListAsync();
            return View("~/Views/Admin/SystemNotifications/Index.cshtml", notifications);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SystemNotificationViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin" || u.RoleId == 2);
            int adminId = adminUser != null ? adminUser.Id : 1;

            var notification = new SystemNotification
            {
                Title = vm.Title,
                Content = vm.Content,
                Recipient = vm.Recipient,
                UserType = vm.UserType,
                Status = vm.Status,
                // BR-NOTI-05: A notification cannot be published if PublishTime is earlier than the current system time. (TODO: Implement validation block for past PublishTime)
                PublishTime = vm.Status == "Bản nháp" ? null : (vm.PublishTime ?? DateTime.Now),
                // BR-NOTI-10: The system automatically records the creator (UserId) and creation timestamp (CreatedAt) when a notification is created.
                UserId = adminId,
                CreatedAt = DateTime.Now
            };

            _context.SystemNotifications!.Add(notification);
            await _context.SaveChangesAsync();

            var currentAdminId = GetCurrentUserId() ?? adminId;
            await LogActivityAsync(currentAdminId, $"Tạo thông báo hệ thống: '{notification.Title}' ({notification.Status})");

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] SystemNotificationViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var notification = await _context.SystemNotifications!.FindAsync(vm.Id);
            if (notification == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông báo." });
            }

            // BR-NOTI-08: Once a notification has been published, only its title, content, or publish time may be updated. Changes are reflected immediately to all recipients. (Note: Currently, editing is fully blocked once published)
            if (notification.Status == "Đã phát hành")
            {
                return Json(new { success = false, message = "Thông báo đã phát hành không thể chỉnh sửa." });
            }

            notification.Title = vm.Title;
            notification.Content = vm.Content;
            notification.Recipient = vm.Recipient;
            notification.UserType = vm.UserType;
            notification.Status = vm.Status;
            // BR-NOTI-05: A notification cannot be published if PublishTime is earlier than the current system time. (TODO: Implement validation block for past PublishTime)
            notification.PublishTime = vm.Status == "Bản nháp" ? null : (vm.PublishTime ?? DateTime.Now);
            // BR-NOTI-11: The system automatically updates UpdatedAt whenever a notification is modified.
            notification.UpdatedAt = DateTime.Now;

            _context.SystemNotifications.Update(notification);
            await _context.SaveChangesAsync();

            var currentAdminId = GetCurrentUserId();
            if (currentAdminId.HasValue)
            {
                await LogActivityAsync(currentAdminId.Value, $"Chỉnh sửa thông báo hệ thống: '{notification.Title}' (ID: {notification.Id})");
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var notification = await _context.SystemNotifications!.FindAsync(id);
            if (notification == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông báo." });
            }

            //Nếu trạng thái là "Đã phát hành" -> không cho xóa
            if (notification.Status == "Đã phát hành")
            {
                return Json(new { success = false, message = "Không thể hủy thông báo đã phát hành." });
            }

            // BR-NOTI-09: Published notifications cannot be permanently deleted. They can only be archived or deactivated to preserve the system audit history. (Soft delete by updating status to "Đã hủy")
            notification.Status = "Đã hủy";
            // BR-NOTI-11: The system automatically updates UpdatedAt whenever a notification is modified.
            notification.UpdatedAt = DateTime.Now;

            _context.SystemNotifications.Update(notification);
            await _context.SaveChangesAsync();

            //Lưu Audit Log
            var currentAdminId = GetCurrentUserId();
            if (currentAdminId.HasValue)
            {
                await LogActivityAsync(currentAdminId.Value, $"Hủy (xóa mềm) thông báo hệ thống: '{notification.Title}' (ID: {notification.Id})");
            }

            return Json(new { success = true });
        }

        [HttpGet("/admin/notifications/api")]
        public async Task<IActionResult> GetNotificationsApi()
        {
            var now = DateTime.Now;
            // BR-NOTI-03: A notification with the status Draft is visible only to its creator and cannot be viewed by recipients. (Only "Đã phát hành" status is retrieved for recipients here)
            // BR-NOTI-04: A notification becomes visible to recipients only when its status is Published and the current system time is equal to or later than PublishTime.
            var notifications = await _context.SystemNotifications!
                .AsNoTracking()
                .Where(n => n.Status == "Đã phát hành" && (n.PublishTime == null || n.PublishTime <= now))
                .OrderByDescending(n => n.PublishTime ?? n.CreatedAt)
                .ToListAsync();

            // BR-NOTI-06: When Recipient is set to All Users, the notification is delivered to every active user in the system.
            // BR-NOTI-07: When Recipient is set to a specific user group, only users whose role matches the selected UserType receive the notification. (Currently filtering for Admin role recipients here)
            var adminNotifications = notifications
                .Where(n => n.Recipient != null && (
                    n.Recipient.Equals("Tất cả người dùng", StringComparison.OrdinalIgnoreCase) ||
                    n.Recipient.Equals("Tất cả", StringComparison.OrdinalIgnoreCase) ||
                    n.Recipient.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim().ToLower())
                        .Any(s => s == "2" || s == "admin")
                ))
                .Select(n => new
                {
                    id = n.Id,
                    title = n.Title,
                    content = n.Content,
                    publishTime = n.PublishTime ?? n.CreatedAt
                })
                .ToList();

            return Json(adminNotifications);
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
                await _context.SaveChangesAsync();
            }
        }
    }
}
