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
                .AsNoTracking()
                .OrderByDescending(n => n.PublishTime ?? n.CreatedAt)
                .ToListAsync();
            return View("~/Views/Admin/SystemNotifications/Index.cshtml", notifications);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SystemNotificationViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var notification = new SystemNotification
            {
                Title = vm.Title,
                Content = vm.Content,
                Recipient = vm.Recipient,
                UserType = vm.UserType,
                Status = vm.Status,
                PublishTime = vm.Status == "Bản nháp" ? null : (vm.PublishTime ?? DateTime.Now),
                Creator = "Administrator",
                CreatedAt = DateTime.Now
            };

            _context.SystemNotifications!.Add(notification);
            await _context.SaveChangesAsync();

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

            // Refuse edits for already published notifications
            if (notification.Status == "Đã phát hành")
            {
                return Json(new { success = false, message = "Thông báo đã phát hành không thể chỉnh sửa." });
            }

            notification.Title = vm.Title;
            notification.Content = vm.Content;
            notification.Recipient = vm.Recipient;
            notification.UserType = vm.UserType;
            notification.Status = vm.Status;
            notification.PublishTime = vm.Status == "Bản nháp" ? null : (vm.PublishTime ?? DateTime.Now);
            notification.UpdatedAt = DateTime.Now;

            _context.SystemNotifications.Update(notification);
            await _context.SaveChangesAsync();

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

            // Soft delete by updating status to "Đã hủy"
            notification.Status = "Đã hủy";
            notification.UpdatedAt = DateTime.Now;

            _context.SystemNotifications.Update(notification);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
