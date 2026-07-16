using System.Diagnostics;
using EnglishLearningOnlineSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace EnglishLearningOnlineSystem.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("/homepage")]
    public IActionResult Homepage()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [Route("/Home/Error/{statusCode?}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? statusCode)
    {
        var model = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = statusCode ?? 500
        };

        // Lấy đường dẫn gốc trước khi bị chuyển hướng xử lý lỗi
        var statusCodeReExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
        if (statusCodeReExecuteFeature != null)
        {
            model.OriginalPath = statusCodeReExecuteFeature.OriginalPath + statusCodeReExecuteFeature.OriginalQueryString;
        }

        // Lấy chi tiết ngoại lệ nếu có (Lỗi 500)
        var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionHandlerPathFeature != null)
        {
            var exception = exceptionHandlerPathFeature.Error;
            model.OriginalPath = exceptionHandlerPathFeature.Path;
            model.ErrorMessage = exception.Message;
            model.StackTrace = exception.StackTrace;
            model.StatusCode = 500;

            // Ghi log chi tiết lỗi cho quản trị viên
            _logger.LogError(exception, "Internal Server Error xảy ra tại {Path}. Request ID: {RequestId}", exceptionHandlerPathFeature.Path, model.RequestId);
        }

        // Trả về View tương ứng theo mã trạng thái
        if (model.StatusCode == 403)
        {
            return View("AccessDenied", model);
        }
        else if (model.StatusCode == 404)
        {
            return View("NotFound", model);
        }
        else if (model.StatusCode == 500)
        {
            return View("ServerError", model);
        }

        return View(model);
    }
}
