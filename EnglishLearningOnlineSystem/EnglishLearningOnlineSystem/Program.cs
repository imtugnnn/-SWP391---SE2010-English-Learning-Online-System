using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using EnglishLearningOnlineSystem.Repositories.Implementations;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Implementations;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.Services.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký các dịch vụ và middleware của ứng dụng
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<EnglishLearningOnlineSystem.Data.AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Đăng ký Repository và Service cho Dependency Injection
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ISystemNotificationRepository, SystemNotificationRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserImportService, UserImportService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ISystemNotificationService, SystemNotificationService>();

builder.Services.AddScoped<IStudentDashboardRepository, StudentDashboardRepository>();
builder.Services.AddScoped<IStudentDashboardService, StudentDashboardService>();
builder.Services.AddScoped<IStudentProfileRepository, StudentProfileRepository>();
builder.Services.AddScoped<IStudentProfileService, StudentProfileService>();
builder.Services.AddScoped<IStudentCourseRepository, StudentCourseRepository>();
builder.Services.AddScoped<IStudentCourseService, StudentCourseService>();
builder.Services.AddScoped<IStudentLessonRepository, StudentLessonRepository>();
builder.Services.AddScoped<IStudentLessonService, StudentLessonService>();
builder.Services.AddScoped<IStudentLessonDetailRepository, StudentLessonDetailRepository>();
builder.Services.AddScoped<IStudentLessonDetailService, StudentLessonDetailService>();
builder.Services.AddScoped<IVocabularyRepository, VocabularyRepository>();
builder.Services.AddScoped<IVocabularyService, VocabularyService>();

builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<ILessonAnalyticsRepository, LessonAnalyticsRepository>();
builder.Services.AddScoped<ILessonAnalyticsService, LessonAnalyticsService>();
builder.Services.AddScoped<IMiniGameRepository, MiniGameRepository>();
builder.Services.AddScoped<IMiniGameService, MiniGameService>();
builder.Services.AddScoped<IWordScrambleService, WordScrambleService>();
builder.Services.AddScoped<IStudentGameProgressRepository, StudentGameProgressRepository>();
builder.Services.AddScoped<IMatchingGameService, MatchingGameService>();

builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IQuizService, QuizService>();

builder.Services.AddScoped<IQuizAttemptRepository, QuizAttemptRepository>();
builder.Services.AddScoped<IQuizAttemptService, QuizAttemptService>();
builder.Services.AddScoped<IFlashcardRepository, FlashcardRepository>();
builder.Services.AddScoped<IFlashcardService, FlashcardService>();
builder.Services.AddScoped<IAdaptiveLearningService, AdaptiveLearningService>();

builder.Services.AddScoped<IClassRepository, ClassRepository>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IAcademicYearRepository, AcademicYearRepository>();
builder.Services.AddScoped<IAcademicYearService, AcademicYearService>();
builder.Services.AddScoped<IStudentManagementService, StudentManagementService>();
builder.Services.AddScoped<IAssignmentRepository, AssignmentRepository>();
builder.Services.AddScoped<IAssignmentProgressRepository, AssignmentProgressRepository>();
builder.Services.AddScoped<IAssignmentProgressService, AssignmentProgressService>();
// Luồng Teacher dùng Repository/Unit of Work, không truy cập AppDbContext từ Service.
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<ITeacherDashboardRepository, TeacherDashboardRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITeacherAssignmentService, TeacherAssignmentService>();
builder.Services.AddScoped<ITeacherDashboardService, TeacherDashboardService>();

// Cấu hình Session
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.AccessDeniedPath = "/Home/Error/403";
        options.LoginPath = "/login";
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    });
// ===================================================================================

var app = builder.Build();

// Cấu hình xử lý lỗi và bảo mật cho môi trường Production
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error/500");
    app.UseHsts();
}
else
{
    app.UseExceptionHandler("/Home/Error/500");
}

// Cấu hình pipeline xử lý request
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

app.UseRouting();

app.UseSession();

app.UseMiddleware<EnglishLearningOnlineSystem.Middleware.MaintenanceModeMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Cấu hình route mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Tự động cập nhật Database và seed dữ liệu khi khởi động ứng dụng
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<EnglishLearningOnlineSystem.Data.AppDbContext>();

        context.Database.Migrate();

        EnglishLearningOnlineSystem.SeedData.DbInitializer
            .SeedAsync(context, app.Environment.IsDevelopment())
            .GetAwaiter()
            .GetResult();

        Console.WriteLine("=== ĐÃ CẬP NHẬT DATABASE VÀ BẢNG THÀNH CÔNG ===");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi khi tự động cập nhật Database.");
    }
}

app.Run();
