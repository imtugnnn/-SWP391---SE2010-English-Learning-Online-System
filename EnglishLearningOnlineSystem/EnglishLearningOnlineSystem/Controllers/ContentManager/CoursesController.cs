using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels.ContentManager.Courses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnglishLearningOnlineSystem.Models;
using System;

namespace EnglishLearningOnlineSystem.Controllers.ContentManager;

[Route("contentmanager/courses")]
public class CoursesController : BaseContentManagerController
{
    private const int DefaultPageSize = 10;
    private readonly ICourseService _courseService;

    public CoursesController(AppDbContext db, ICourseService courseService) : base(db)
    {
        _courseService = courseService;
    }

    // =========================
    // INDEX
    // =========================
    [HttpGet]
    public async Task<IActionResult> Index(string? keyword, bool? isActive, int page = 1)
    {
        if (page < 1) page = 1;

        var (items, totalCount) =
            await _courseService.GetCoursesAsync(keyword, isActive, page, DefaultPageSize);

        ViewBag.Keyword = keyword;
        ViewBag.IsActive = isActive;
        ViewBag.PageNumber = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)DefaultPageSize);

        return View("~/Views/ContentManager/Courses/Index.cshtml", items);
    }

    // =========================
    // CREATE
    // =========================
    [HttpGet("create")]
    public IActionResult Create()
        => View("~/Views/ContentManager/Courses/Create.cshtml", new CourseCreateViewModel());

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View("~/Views/ContentManager/Courses/Create.cshtml", model);

        var creatorId = GetCurrentUserId();
        var (success, errorMessage) = await _courseService.CreateCourseAsync(model, creatorId);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Không thể tạo khoá học.");
            return View("~/Views/ContentManager/Courses/Create.cshtml", model);
        }

        if (creatorId.HasValue)
        {
            var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == creatorId.Value);
            if (user != null)
            {
                _db.AuditLogs.Add(new AuditLog
                {
                    UserId = creatorId,
                    Username = user.Username,
                    UserRole = user.Role?.Name ?? "Content Manager",
                    Action = $"Tạo khóa học '{model.CourseName}' (Khối: {model.GradeLevel})",
                    Timestamp = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
            }
        }

        TempData["SuccessMessage"] = "Tạo khoá học thành công.";
        return RedirectToAction(nameof(Index));
    }

    // =========================
    // DETAILS
    // =========================
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var detail = await _courseService.GetCourseDetailAsync(id);
        if (detail == null) return NotFound();

        return View("~/Views/ContentManager/Courses/Details.cshtml", detail);
    }

    // =========================
    // EDIT
    // =========================
    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var (model, errorMessage) = await _courseService.GetCourseForEditAsync(id);

        if (model == null)
        {
            TempData["ErrorMessage"] = errorMessage;
            return RedirectToAction(nameof(Index));
        }

        return View("~/Views/ContentManager/Courses/Edit.cshtml", model);
    }

    [HttpPost("{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CourseEditViewModel model)
    {
        if (id != model.CourseId) return BadRequest();

        if (!ModelState.IsValid)
            return View("~/Views/ContentManager/Courses/Edit.cshtml", model);

        var (success, errorMessage) = await _courseService.UpdateCourseAsync(model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Không thể cập nhật khoá học.");
            return View("~/Views/ContentManager/Courses/Edit.cshtml", model);
        }

        TempData["SuccessMessage"] = "Cập nhật khoá học thành công.";
        return RedirectToAction(nameof(Index));
    }

    // =========================
    // TOGGLE STATUS
    // =========================
    [HttpPost("{id:int}/toggle-status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var (success, errorMessage) = await _courseService.ToggleStatusAsync(id);

        TempData[success ? "SuccessMessage" : "ErrorMessage"]
            = success ? "Đã đổi trạng thái khoá học." : errorMessage;

        return RedirectToAction(nameof(Index));
    }

    // =========================
    // DELETE
    // =========================
    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, errorMessage) = await _courseService.DeleteCourseAsync(id);

        TempData[success ? "SuccessMessage" : "ErrorMessage"]
            = success ? "Đã xoá khoá học." : errorMessage;

        return RedirectToAction(nameof(Index));
    }

    // =========================
    // HELPER
    // =========================
    private int? GetCurrentUserId()
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        return int.TryParse(userIdStr, out var userId) ? userId : null;
    }
}