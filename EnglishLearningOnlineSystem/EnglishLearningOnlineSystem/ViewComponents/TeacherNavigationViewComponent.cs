using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearningOnlineSystem.ViewComponents;

public class TeacherNavigationViewComponent : ViewComponent
{
    private readonly IClassRepository _classRepository;
    private readonly IUserRepository _userRepository;

    public TeacherNavigationViewComponent(
        IClassRepository classRepository,
        IUserRepository userRepository)
    {
        _classRepository = classRepository;
        _userRepository = userRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var model = new TeacherNavigationViewModel
        {
            CurrentAction = ViewContext.RouteData.Values["action"]?.ToString() ?? string.Empty
        };

        var rawTeacherId = HttpContext.Session.GetString("UserId");
        var rawRoleId = HttpContext.Session.GetString("UserRole");
        if (!int.TryParse(rawTeacherId, out var teacherId) || rawRoleId != "3")
        {
            return View(model);
        }

        var teacher = await _userRepository.GetByIdAsync(teacherId);
        model.TeacherName = teacher?.Username ?? "Giáo viên";

        var classes = await _classRepository.GetClassesByTeacherIdAsync(teacherId);
        model.Classes = classes
            .OrderBy(c => c.ClassName)
            .Select(c => new TeacherNavigationClassViewModel
            {
                ClassId = c.ClassId,
                ClassName = c.ClassName,
                GradeLevel = c.GradeLevel ?? string.Empty
            })
            .ToList();

        var selectedClassId = ResolveSelectedClassId();
        model.SelectedClassId = model.Classes.Any(c => c.ClassId == selectedClassId)
            ? selectedClassId
            : null;

        return View(model);
    }

    private int? ResolveSelectedClassId()
    {
        if (TryParseClassId(ViewContext.RouteData.Values["classId"], out var routeClassId))
        {
            return routeClassId;
        }

        if (TryParseClassId(HttpContext.Request.Query["classId"].FirstOrDefault(), out var queryClassId))
        {
            return queryClassId;
        }

        var pageModel = ViewContext.ViewData.Model;
        var classIdProperty = pageModel?.GetType().GetProperty("ClassId");
        return TryParseClassId(classIdProperty?.GetValue(pageModel), out var modelClassId)
            ? modelClassId
            : null;
    }

    private static bool TryParseClassId(object? value, out int classId)
    {
        return int.TryParse(value?.ToString(), out classId) && classId > 0;
    }
}
