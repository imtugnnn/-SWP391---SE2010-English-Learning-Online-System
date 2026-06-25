using EnglishLearningOnlineSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Controllers.ContentManager;

public class BaseContentManagerController : Controller
{
    protected readonly AppDbContext _db;

    public BaseContentManagerController(AppDbContext db)
    {
        _db = db;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");

        if (int.TryParse(userIdStr, out var userId))
        {
            var user = _db.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new { u.Username })
                .FirstOrDefault();

            if (user != null)
            {
                ViewBag.ManagerName = user.Username;
                // User chưa có cột AvatarUrl riêng — layout sẽ tự dùng ảnh mặc định.
            }
        }

        base.OnActionExecuting(context);
    }
}