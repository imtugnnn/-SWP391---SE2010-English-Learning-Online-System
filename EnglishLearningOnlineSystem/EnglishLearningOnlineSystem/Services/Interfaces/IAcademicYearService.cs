//Created by TungDPL
//Create at 7/28/2026
using EnglishLearningOnlineSystem.ViewModels.Admin.AcademicYears;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface IAcademicYearService
{
    Task<List<AcademicYearListItemViewModel>> GetAcademicYearsAsync();
    Task<AcademicYearCreateViewModel> GetCreateViewModelAsync();
    Task<SelectList> GetTeacherSelectListAsync(int? selectedTeacherId);

    Task<AcademicYearCreateResult> CreateAsync(AcademicYearCreateViewModel vm, int? adminId);
    Task<AcademicYearEditResult> GetEditViewModelAsync(int id, int? selectedClassId = null, AddClassViewModel? newClass = null);
    Task<AcademicYearEditResult> AddClassAsync(int id, AcademicYearEditViewModel vm, int? adminId);
    Task<AcademicYearEditResult> LoadStudentsFromExcelAsync(int id, AcademicYearEditViewModel vm);

    Task<AcademicYearActionResult> SetActiveAsync(int id, int? adminId);
    Task<AcademicYearActionResult> RemoveClassAsync(int id, int classId, int? adminId);
    Task<AcademicYearActionResult> RestoreClassAsync(int id, int classId, int? adminId);
    Task<AcademicYearActionResult> AddStudentsToClassAsync(int id, int classId, string? studentEmails, int? adminId);
    Task<AcademicYearActionResult> RemoveStudentFromClassAsync(int id, int classId, string studentEmail, int? adminId);
}
