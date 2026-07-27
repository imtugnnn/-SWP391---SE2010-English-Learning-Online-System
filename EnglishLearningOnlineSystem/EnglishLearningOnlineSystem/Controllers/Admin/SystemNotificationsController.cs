//Create by TungDPL
//Create at 6/26/2026
//Last update: 7/28/2026
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels.Admin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearningOnlineSystem.Controllers.Admin
{
    // BR-NOTI-02: Only users with the Admin role are authorized to create, edit, publish, archive, or deactivate system notifications. (TODO: Implement controller-level authorization checks)
    public class SystemNotificationsController : Controller
    {
        private readonly ISystemNotificationService _systemNotificationService;

        public SystemNotificationsController(ISystemNotificationService systemNotificationService)
        {
            _systemNotificationService = systemNotificationService;
        }

        public async Task<IActionResult> Index()
        {
            var userRoleSession = HttpContext.Session.GetString("UserRole");
            //Nếu người dùng khoông phải là admin, không cho đăng nhập
            if (userRoleSession != "2")
            {
                return RedirectToAction("Login", "Auth");
            }
            var result = await _systemNotificationService.GetIndexDataAsync();
            if (!result.Succeeded || result.Data == null)
            {
                ViewBag.Roles = new List<EnglishLearningOnlineSystem.Models.Role>();
                return View("~/Views/Admin/SystemNotifications/Index.cshtml", new List<EnglishLearningOnlineSystem.Models.SystemNotification>());
            }

            ViewBag.Roles = result.Data.Roles;
            return View("~/Views/Admin/SystemNotifications/Index.cshtml", result.Data.Notifications);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SystemNotificationViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var result = await _systemNotificationService.CreateAsync(vm, GetCurrentUserId() ?? 1);
            return result.Succeeded
                ? Json(new { success = true })
                : Json(new { success = false, message = result.ErrorMessage });
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] SystemNotificationViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var result = await _systemNotificationService.UpdateAsync(vm, GetCurrentUserId());
            return result.Succeeded
                ? Json(new { success = true })
                : Json(new { success = false, message = result.ErrorMessage });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _systemNotificationService.DeleteAsync(id, GetCurrentUserId());
            return result.Succeeded
                ? Json(new { success = true })
                : Json(new { success = false, message = result.ErrorMessage });
        }

        [HttpGet("/admin/notifications/api")]
        public async Task<IActionResult> GetNotificationsApi()
        {
            var result = await _systemNotificationService.GetAdminNotificationsAsync();
            if (!result.Succeeded || result.Data == null)
            {
                return Json(Array.Empty<object>());
            }

            var adminNotifications = result.Data
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
    }
}
