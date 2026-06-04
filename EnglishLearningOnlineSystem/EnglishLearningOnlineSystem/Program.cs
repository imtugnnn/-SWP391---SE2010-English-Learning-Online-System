using Microsoft.EntityFrameworkCore;
using EnglishLearningOnlineSystem.Repositories.Implementations;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Implementations;
using EnglishLearningOnlineSystem.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký các dịch vụ cơ bản của hệ thống
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<EnglishLearningOnlineSystem.Data.AppDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
// ===================================================================================

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 2. Đoạn code tự động khởi tạo và cập nhật Database khi chạy ứng dụng [cite: 121]
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Nhờ đã đăng ký ở trên nên hàm này sẽ không còn bị crash lỗi "No service" nữa
        var context = services.GetRequiredService<EnglishLearningOnlineSystem.Data.AppDbContext>();
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
