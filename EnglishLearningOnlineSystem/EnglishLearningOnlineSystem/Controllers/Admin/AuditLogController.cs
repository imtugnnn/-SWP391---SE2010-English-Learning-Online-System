using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EnglishLearningOnlineSystem.Controllers.Admin;

public class AuditLogController : Controller
{
    private readonly AppDbContext _context;

    public AuditLogController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(
        DateTime? startDate,
        DateTime? endDate,
        string? userRole,
        string? searchQuery,
        int page = 1)
    {
        var userRoleSession = HttpContext.Session.GetString("UserRole");
        if (userRoleSession != "2")
        {
            return RedirectToAction("Login", "Auth");
        }

        var query = _context.AuditLogs.AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(l => l.Timestamp >= startDate.Value);
        }
        if (endDate.HasValue)
        {
            var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(l => l.Timestamp <= endOfDay);
        }
        if (!string.IsNullOrWhiteSpace(userRole))
        {
            query = query.Where(l => l.UserRole == userRole);
        }
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            query = query.Where(l => l.Action.Contains(searchQuery) || l.Username.Contains(searchQuery));
        }

        query = query.OrderByDescending(l => l.Timestamp);

        const int pageSize = 15;
        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        if (page < 1) page = 1;
        if (page > totalPages && totalPages > 0) page = totalPages;

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
        ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
        ViewBag.UserRole = userRole;
        ViewBag.SearchQuery = searchQuery;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;

        return View("~/Views/Admin/AuditLog/Index.cshtml", items);
    }
}
