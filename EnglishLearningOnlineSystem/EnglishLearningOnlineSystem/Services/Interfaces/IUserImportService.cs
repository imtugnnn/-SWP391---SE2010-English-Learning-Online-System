//Create by TungDPL
//Create at 7/28/2026
using EnglishLearningOnlineSystem.Services.Models;
using Microsoft.AspNetCore.Http;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface IUserImportService
{
    Task<UserImportServiceResult> ImportUsersFromExcelAsync(IFormFile importFile);
}
