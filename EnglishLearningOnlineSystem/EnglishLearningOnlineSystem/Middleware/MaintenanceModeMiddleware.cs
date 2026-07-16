using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using EnglishLearningOnlineSystem.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace EnglishLearningOnlineSystem.Middleware;

public class MaintenanceModeMiddleware
{
    private readonly RequestDelegate _next;

    public MaintenanceModeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var userRole = context.Session.GetString("UserRole");

        // Ghi log debug ra file
        try
        {
            var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Path: {path} | Active: ?? | UserRole: {userRole ?? "null"} | SessionId: {context.Session.Id}\n";
            System.IO.File.AppendAllText("maintenance_debug.log", logMessage);
        }
        catch {}

        // 1. Cho phép truy cập tài nguyên tĩnh (static files)
        if (path.StartsWith("/css/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/js/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/lib/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/images/", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/favicon.ico", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // 2. Cho phép các route login/logout để Admin có thể đăng nhập tắt bảo trì
        var isAuthPath = path.StartsWith("/login", StringComparison.OrdinalIgnoreCase) ||
                         path.StartsWith("/logout", StringComparison.OrdinalIgnoreCase) ||
                         path.StartsWith("/Auth/", StringComparison.OrdinalIgnoreCase) ||
                         path.StartsWith("/signin-", StringComparison.OrdinalIgnoreCase);

        var isMaintenancePage = path.Equals("/maintenance", StringComparison.OrdinalIgnoreCase) ||
                                 path.Equals("/Maintenance/UnderMaintenance", StringComparison.OrdinalIgnoreCase);

        // 3. Lấy cấu hình bảo trì từ DB
        var isMaintenanceActive = false;
        DateTime? maintenanceStartAt = null;

        try
        {
            var enabledSetting = await dbContext.SystemSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Key == "MaintenanceModeEnabled");
            
            var startAtSetting = await dbContext.SystemSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Key == "MaintenanceStartAt");

            if (enabledSetting != null && string.Equals(enabledSetting.Value, "true", StringComparison.OrdinalIgnoreCase))
            {
                isMaintenanceActive = true;
            }
            else if (startAtSetting != null && !string.IsNullOrWhiteSpace(startAtSetting.Value))
            {
                if (DateTime.TryParse(startAtSetting.Value, out var startAt))
                {
                    maintenanceStartAt = startAt;
                    if (DateTime.Now >= startAt)
                    {
                        isMaintenanceActive = true;
                    }
                }
            }
        }
        catch (Exception)
        {
            // Tránh crash ứng dụng nếu DB chưa sẵn sàng (chưa migration)
        }

        // 4. Nếu chế độ bảo trì đang hoạt động
        if (isMaintenanceActive)
        {
            // Kiểm tra xem người dùng hiện tại có phải là Admin hay không (Role 2)
            var isAdmin = string.Equals(userRole, "2");

            try
            {
                var decisionLog = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Active: True | Path: {path} | isAdmin: {isAdmin} | isAuthPath: {isAuthPath} | isMaintenancePage: {isMaintenancePage}\n";
                System.IO.File.AppendAllText("maintenance_debug.log", decisionLog);
            }
            catch {}

            if (!isAdmin)
            {
                // Nếu không phải Admin và không phải trang đăng nhập hoặc trang thông báo bảo trì, redirect sang /maintenance
                if (!isAuthPath && !isMaintenancePage)
                {
                    try
                    {
                        System.IO.File.AppendAllText("maintenance_debug.log", $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - REDIRECT TO /maintenance for path: {path}\n");
                    }
                    catch {}
                    context.Response.Redirect("/maintenance");
                    return;
                }
            }
        }
        else
        {
            // Nếu không trong chế độ bảo trì và người dùng cố truy cập /maintenance trực tiếp, redirect về trang chủ
            if (isMaintenancePage)
            {
                context.Response.Redirect("/");
                return;
            }
        }

        await _next(context);
    }
}
