using DIP.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection; // 新增這個 using

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

// 修復 DataProtection 問題 - 加入 try-catch 以防目錄權限問題
try 
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo("/tmp/keys"));
}
catch (Exception ex)
{
    Console.WriteLine($"DataProtection 設定失敗: {ex.Message}，使用預設設定");
    builder.Services.AddDataProtection();
}

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
}

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
    var middlewareType = Type.GetType("PermissionMiddleware");
    if (middlewareType != null)
    {
        app.UseMiddleware(middlewareType);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"PermissionMiddleware 載入失敗: {ex.Message}");
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 添加健康檢查端點（允許匿名訪問）
app.MapGet("/health", () => Results.Json(new { 
    status = "healthy", 
    timestamp = DateTime.UtcNow,
    application = "DIP",
    version = Environment.GetEnvironmentVariable("BUILD_NUMBER") ?? "1.0",
    environment = app.Environment.EnvironmentName
})).AllowAnonymous();

app.MapGet("/ping", () => "pong").AllowAnonymous();

Console.WriteLine("🚀 DIP Application 啟動中...");
Console.WriteLine($"環境: {app.Environment.EnvironmentName}");
Console.WriteLine($"建置版本: {Environment.GetEnvironmentVariable("BUILD_NUMBER") ?? "1.0"}");

app.Run();