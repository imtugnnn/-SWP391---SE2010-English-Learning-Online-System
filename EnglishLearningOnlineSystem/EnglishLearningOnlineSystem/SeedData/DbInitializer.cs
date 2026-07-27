using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.SeedData;

/// <summary>
/// Khởi tạo cấu hình bắt buộc trong hệ thống.
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAsync(
        AppDbContext context,
        bool includeDemoData = false)
    {
        await SeedSystemSettingsAsync(context);
    }

    private static async Task SeedSystemSettingsAsync(AppDbContext context)
    {
        if (!await context.SystemSettings.AnyAsync(x => x.Key == "MaintenanceModeEnabled"))
        {
            context.SystemSettings.Add(new SystemSetting
            {
                Key = "MaintenanceModeEnabled",
                Value = "false",
                Description = "Bật/tắt chế độ bảo trì hệ thống (true/false)",
                UpdatedAt = DateTime.UtcNow
            });
        }

        if (!await context.SystemSettings.AnyAsync(x => x.Key == "MaintenanceStartAt"))
        {
            context.SystemSettings.Add(new SystemSetting
            {
                Key = "MaintenanceStartAt",
                Value = string.Empty,
                Description = "Thời gian bắt đầu bảo trì hệ thống (định dạng yyyy-MM-dd HH:mm:ss)",
                UpdatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
    }
}
