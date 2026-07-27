//Created by TungDPL
//Create at 6/24/2026
//Last update: 7/28/2026
using EnglishLearningOnlineSystem.Helpers.Admin.AcademicYears;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels.Admin.AcademicYears;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearningOnlineSystem.Controllers.Admin;

// BR-AY-08: Only users with the Administrator role are authorized to create, update, activate, or delete Academic Years. (TODO: Implement authorization checks)
public class AcademicYearsController : Controller
{
    private readonly IAcademicYearService _academicYearService;

    public AcademicYearsController(IAcademicYearService academicYearService)
    {
        _academicYearService = academicYearService;
    }

    public async Task<IActionResult> Index()
    {
        var years = await _academicYearService.GetAcademicYearsAsync();
        return View("~/Views/Admin/AcademicYears/Index.cshtml", years);
    }

    public async Task<IActionResult> Create()
    {
        var vm = await _academicYearService.GetCreateViewModelAsync();
        return View("~/Views/Admin/AcademicYears/Create.cshtml", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AcademicYearCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Admin/AcademicYears/Create.cshtml", vm);
        }

        var result = await _academicYearService.CreateAsync(vm, GetCurrentUserId());
        if (!result.Success)
        {
            ApplyValidationErrors(result.Errors);
            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
            }

            return View("~/Views/Admin/AcademicYears/Create.cshtml", vm);
        }

        return RedirectToAction(nameof(Edit), new { id = result.AcademicYearId });
    }

    public async Task<IActionResult> Edit(int id, int? selectedClassId)
    {
        var result = await _academicYearService.GetEditViewModelAsync(id, selectedClassId);
        if (result.NotFound || result.ViewModel == null)
        {
            return NotFound();
        }

        ViewBag.TeacherId = await _academicYearService.GetTeacherSelectListAsync(result.ViewModel.NewClass.TeacherId);
        return View("~/Views/Admin/AcademicYears/Edit.cshtml", result.ViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddClass(int id, AcademicYearEditViewModel vm)
    {
        var result = await _academicYearService.AddClassAsync(id, vm, GetCurrentUserId());
        if (result.NotFound)
        {
            return NotFound();
        }

        if (!result.Success)
        {
            ApplyValidationErrors(result.Errors);
            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }

            var reloadVm = result.ViewModel ?? vm;
            ViewBag.TeacherId = await _academicYearService.GetTeacherSelectListAsync(reloadVm.NewClass.TeacherId);
            return View("~/Views/Admin/AcademicYears/Edit.cshtml", reloadVm);
        }

        TempData["SuccessMessage"] = result.SuccessMessage;
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpGet]
    public IActionResult DownloadClassTemplate(int id)
    {
        var templateBytes = AcademicYearExcelHelper.CreateTemplate();
        var fileName = $"academic-year-{id}-student-template.xlsx";
        return File(
            templateBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoadStudentsFromExcel(int id, AcademicYearEditViewModel vm)
    {
        var result = await _academicYearService.LoadStudentsFromExcelAsync(id, vm);
        if (result.NotFound)
        {
            return NotFound();
        }

        if (!result.Success)
        {
            ApplyValidationErrors(result.Errors);
            var reloadVm = result.ViewModel ?? vm;
            ViewBag.TeacherId = await _academicYearService.GetTeacherSelectListAsync(reloadVm.NewClass.TeacherId);
            return View("~/Views/Admin/AcademicYears/Edit.cshtml", reloadVm);
        }

        ViewBag.TeacherId = await _academicYearService.GetTeacherSelectListAsync(result.ViewModel?.NewClass.TeacherId);
        return View("~/Views/Admin/AcademicYears/Edit.cshtml", result.ViewModel ?? vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(int id)
    {
        var result = await _academicYearService.SetActiveAsync(id, GetCurrentUserId());
        if (result.NotFound)
        {
            return NotFound();
        }

        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Success
            ? result.SuccessMessage
            : result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveClass(int id, int classId)
    {
        var result = await _academicYearService.RemoveClassAsync(id, classId, GetCurrentUserId());
        if (result.NotFound)
        {
            return NotFound();
        }

        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Success
            ? result.SuccessMessage
            : result.ErrorMessage;

        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreClass(int id, int classId)
    {
        var result = await _academicYearService.RestoreClassAsync(id, classId, GetCurrentUserId());
        if (result.NotFound)
        {
            return NotFound();
        }

        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Success
            ? result.SuccessMessage
            : result.ErrorMessage;

        return RedirectToAction(nameof(Edit), new { id, selectedClassId = classId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddStudentsToClass(int id, int classId, string? studentEmails)
    {
        var result = await _academicYearService.AddStudentsToClassAsync(id, classId, studentEmails, GetCurrentUserId());
        if (result.NotFound)
        {
            return NotFound();
        }

        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Success
            ? result.SuccessMessage
            : result.ErrorMessage;

        return RedirectToAction(nameof(Edit), new { id, selectedClassId = classId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveStudentFromClass(int id, int classId, string studentEmail)
    {
        var result = await _academicYearService.RemoveStudentFromClassAsync(id, classId, studentEmail, GetCurrentUserId());
        if (result.NotFound)
        {
            return NotFound();
        }

        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Success
            ? result.SuccessMessage
            : result.ErrorMessage;

        return RedirectToAction(nameof(Edit), new { id, selectedClassId = classId });
    }

    private int? GetCurrentUserId()
    {
        var raw = HttpContext.Session.GetString("UserId");
        return int.TryParse(raw, out var id) ? id : null;
    }

    private void ApplyValidationErrors(IEnumerable<AcademicYearValidationError> errors)
    {
        foreach (var error in errors)
        {
            ModelState.AddModelError(error.Key, error.Message);
        }
    }
}
