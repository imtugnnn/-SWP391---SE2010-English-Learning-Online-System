using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.ViewModels.Blog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Controllers;

public class BlogManagementController : Controller
{
    private const int ContentManagerRoleId = 5;

    private readonly AppDbContext _context;

    public BlogManagementController(AppDbContext context)
    {
        _context = context;
    }

    public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
    {
        var roleStr = HttpContext.Session.GetString("UserRole");
        if (!int.TryParse(roleStr, out var roleId) || roleId != ContentManagerRoleId)
        {
            context.Result = RedirectToAction("Login", "Auth");
        }

        base.OnActionExecuting(context);
    }

    public async Task<IActionResult> Index()
    {
        var posts = await _context.BlogPosts
            .Include(b => b.Author)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new BlogPostListItemViewModel
            {
                BlogPostId = b.BlogPostId,
                Title = b.Title,
                Category = b.Category,
                IsPublished = b.IsPublished,
                AuthorName = b.Author != null ? b.Author.Username : "N/A",
                CreatedAt = b.CreatedAt,
                PublishedAt = b.PublishedAt
            })
            .AsNoTracking()
            .ToListAsync();

        return View(posts);
    }

    public IActionResult Create()
    {
        return View(new BlogPostEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BlogPostEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var authorId = GetCurrentUserId();
        if (authorId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var post = new BlogPost
        {
            Title = vm.Title.Trim(),
            Summary = string.IsNullOrWhiteSpace(vm.Summary) ? null : vm.Summary.Trim(),
            Content = vm.Content,
            Category = vm.Category,
            IsPublished = vm.IsPublished,
            AuthorId = authorId.Value,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            PublishedAt = vm.IsPublished ? DateTime.UtcNow : null
        };

        _context.BlogPosts.Add(post);
        await _context.SaveChangesAsync();

        TempData["Message"] = "Đã tạo bài viết.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var post = await _context.BlogPosts.FirstOrDefaultAsync(b => b.BlogPostId == id);
        if (post == null) return NotFound();

        return View(new BlogPostEditViewModel
        {
            BlogPostId = post.BlogPostId,
            Title = post.Title,
            Summary = post.Summary,
            Content = post.Content,
            Category = post.Category,
            IsPublished = post.IsPublished
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BlogPostEditViewModel vm)
    {
        if (id != vm.BlogPostId) return NotFound();

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var post = await _context.BlogPosts.FirstOrDefaultAsync(b => b.BlogPostId == id);
        if (post == null) return NotFound();

        post.Title = vm.Title.Trim();
        post.Summary = string.IsNullOrWhiteSpace(vm.Summary) ? null : vm.Summary.Trim();
        post.Content = vm.Content;
        post.Category = vm.Category;
        post.UpdatedAt = DateTime.UtcNow;

        if (vm.IsPublished && !post.IsPublished)
        {
            post.PublishedAt = DateTime.UtcNow;
        }
        post.IsPublished = vm.IsPublished;

        await _context.SaveChangesAsync();

        TempData["Message"] = "Đã cập nhật bài viết.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var post = await _context.BlogPosts.FirstOrDefaultAsync(b => b.BlogPostId == id);
        if (post == null) return NotFound();

        _context.BlogPosts.Remove(post);
        await _context.SaveChangesAsync();

        TempData["Message"] = "Đã xóa bài viết.";
        return RedirectToAction(nameof(Index));
    }

    private int? GetCurrentUserId()
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        return int.TryParse(userIdStr, out var userId) ? userId : null;
    }
}
