using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.ViewModels.Blog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Controllers;

public class BlogController : Controller
{
    private readonly AppDbContext _context;

    public BlogController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? category)
    {
        var query = _context.BlogPosts
            .Include(b => b.Author)
            .Where(b => b.IsPublished);

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(b => b.Category == category);
        }

        var posts = await query
            .OrderByDescending(b => b.PublishedAt)
            .Select(b => new BlogReadListItemViewModel
            {
                BlogPostId = b.BlogPostId,
                Title = b.Title,
                Summary = b.Summary,
                Category = b.Category,
                AuthorName = b.Author != null ? b.Author.Username : "ELS",
                PublishedAt = b.PublishedAt
            })
            .AsNoTracking()
            .ToListAsync();

        ViewBag.Categories = await _context.BlogPosts
            .Where(b => b.IsPublished)
            .Select(b => b.Category)
            .Distinct()
            .ToListAsync();
        ViewBag.SelectedCategory = category;

        return View(posts);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var post = await _context.BlogPosts
            .Include(b => b.Author)
            .Where(b => b.BlogPostId == id && b.IsPublished)
            .Select(b => new BlogReadDetailViewModel
            {
                BlogPostId = b.BlogPostId,
                Title = b.Title,
                Content = b.Content,
                Category = b.Category,
                AuthorName = b.Author != null ? b.Author.Username : "ELS",
                PublishedAt = b.PublishedAt
            })
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (post == null) return NotFound();

        return View(post);
    }
}
