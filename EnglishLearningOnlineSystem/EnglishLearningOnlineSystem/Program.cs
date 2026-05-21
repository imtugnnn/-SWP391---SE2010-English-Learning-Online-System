using EnglishLearningOnlineSystem.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký các dịch vụ cơ bản của hệ thống
builder.Services.AddRazorPages();

// ================= BỔ SUNG ĐOẠN ĐĂNG KÝ NÀY VÀO DƯỚI AddRazorPages =================
// Đọc chuỗi kết nối từ biến môi trường, nếu không có thì lấy mặc định từ appsettings
// 1. Lấy thông tin tài khoản từ biến môi trường hệ thống của bạn
var dbUser = Environment.GetEnvironmentVariable("DB_USERNAME") ?? "sa";

var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

var connectionString =
    $"Server=localhost;" +
    $"Database=EnglishLearningDB;" +
    $"User Id={dbUser};" +
    $"Password={dbPassword};" +
    $"TrustServerCertificate=True;";

builder.Services.AddDbContext<DBContext>(options =>
    options.UseSqlServer(connectionString));
// ===================================================================================

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

// 2. Đoạn code tự động khởi tạo và cập nhật Database khi chạy ứng dụng [cite: 121]
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Nhờ đã đăng ký ở trên nên hàm này sẽ không còn bị crash lỗi "No service" nữa
        var context = services.GetRequiredService<DBContext>();
        context.Database.Migrate(); 
        Console.WriteLine("=== ĐÃ CẬP NHẬT DATABASE VÀ BẢNG THÀNH CÔNG ===");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi khi tự động cập nhật Database.");
    }
}

app.Run();