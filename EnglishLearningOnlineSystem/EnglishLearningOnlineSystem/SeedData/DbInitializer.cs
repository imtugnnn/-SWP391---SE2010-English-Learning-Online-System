using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace EnglishLearningOnlineSystem.SeedData;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Seed SystemSettings (needed for maintenance mode config)
        if (!await context.SystemSettings.AnyAsync(x => x.Key == "MaintenanceModeEnabled"))
        {
            context.SystemSettings.Add(new SystemSetting
            {
                Key = "MaintenanceModeEnabled",
                Value = "false",
                Description = "Bật/Tắt chế độ bảo trì hệ thống (true/false)",
                UpdatedAt = DateTime.UtcNow
            });
        }
        if (!await context.SystemSettings.AnyAsync(x => x.Key == "MaintenanceStartAt"))
        {
            context.SystemSettings.Add(new SystemSetting
            {
                Key = "MaintenanceStartAt",
                Value = "",
                Description = "Thời gian bắt đầu bảo trì hệ thống (định dạng yyyy-MM-dd HH:mm:ss)",
                UpdatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
    }
}
