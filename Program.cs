using DIP.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 服務註冊
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

// 資料庫連線配置 - 支援 Docker 環境變數
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? "Server=localhost;Database=DipDb;User=root;Password=password;";

builder.Services.AddDbContext<DipDbContext>(options =>
    options.UseMySql(connectionString,
                     ServerVersion.AutoDetect(connectionString)));

// 修復 DataProtection 警告 - 需要加入正確的 using
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/tmp/keys"));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
        options.AccessDeniedPath = "/Login/Denied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
        
        // Docker 環境中的 Cookie 設定
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
                     .RequireAuthenticatedUser()
                     .Build();
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(policy));
});

builder.Services.AddAuthorization();

// 建立應用程式
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // Docker 環境中暫時移除 HSTS
    // app.UseHsts();
}

// Docker 環境中移除 HTTPS 重定向以避免問題
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// 確保 PermissionMiddleware 存在才使用
try 
{
    app.UseMiddleware<PermissionMiddleware>();
}
catch (Exception ex)
{
    Console.WriteLine($"PermissionMiddleware 載入失敗: {ex.Message}");
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 添加健康檢查端點供 Docker 使用（允許匿名訪問）
app.MapGet("/health", () => Results.Json(new { 
    status = "healthy", 
    timestamp = DateTime.UtcNow,
    application = "DIP",
    version = Environment.GetEnvironmentVariable("BUILD_NUMBER") ?? "1.0",
    environment = app.Environment.EnvironmentName,
    database = "connected"
})).AllowAnonymous();

// 不需要驗證的測試端點
app.MapGet("/ping", () => "pong").AllowAnonymous();

// 啟動應用程式前的日誌
Console.WriteLine("🎉 ================================");
Console.WriteLine("🚀 DIP Application Starting...");
Console.WriteLine($"📅 Build Time: {Environment.GetEnvironmentVariable("BUILD_TIME") ?? "Unknown"}");
Console.WriteLine($"🔢 Build Number: {Environment.GetEnvironmentVariable("BUILD_NUMBER") ?? "1"}");
Console.WriteLine($"🌍 Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"🗄️ Database: {connectionString.Substring(0, Math.Min(50, connectionString.Length))}...");
Console.WriteLine("🎉 ================================");

app.Run();