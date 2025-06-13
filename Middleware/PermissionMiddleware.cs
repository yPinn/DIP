using DIP.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

public class PermissionMiddleware
{
    private readonly RequestDelegate _next;

    public PermissionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, DipDbContext db)
    {
        // 先取得 request path
        var path = context.Request.Path.ToString().ToLower();

        // 白名單（免登入訪問） - 保持原樣
        var allowAnonymousPaths = new[] { "/css", "/js", "/lib", "/login", "/home/accessdenied" };
        if (allowAnonymousPaths.Any(p => path.StartsWith(p)))
        {
            await _next(context);
            return;
        }

        // 取得登入資訊
        var roleIdStr = context.User.FindFirst("RoleId")?.Value;
        if (string.IsNullOrEmpty(roleIdStr) || !long.TryParse(roleIdStr, out long roleId))
        {
            context.Response.Redirect("/Login");
            return;
        }

        // 登入後免權限檢查的路徑，改用 IsPathMatch
        var allowLoggedInNoPermissionPaths = new[] 
        { 
            "/knowledge/details",
            "/knowledge/toggleattention",
            "/knowledge/react",
            "/knowledge/delete",
            "/comment/create",
            "/knowledge/edit",
            "/user/profile",
            "/user/updatepersonalinfo"
        };
        if (allowLoggedInNoPermissionPaths.Any(p => IsPathMatch(path, p)))
        {
            await _next(context);
            return;
        }

        // 模組權限判斷（同樣使用 IsPathMatch）
        var matchedModules = await db.SysModules
            .AsNoTracking()
            .Where(m => !string.IsNullOrEmpty(m.ModuleSrc))
            .ToListAsync();

        var matchedModulesFiltered = matchedModules
            .Where(m => IsPathMatch(path, m.ModuleSrc))
            .ToList();

        if (!matchedModulesFiltered.Any())
        {
            await _next(context);
            return;
        }

        var matchedModuleIds = matchedModulesFiltered.Select(m => m.ModuleId).ToList();

        bool hasPermission = await db.SysRoleModules
            .AsNoTracking()
            .AnyAsync(rm => rm.RoleId == roleId &&
                            matchedModuleIds.Contains(rm.ModuleId) &&
                            rm.ActiveFlag == "Y");

        if (!hasPermission)
        {
            context.Response.Redirect("/Home/AccessDenied");
            return;
        }

        await _next(context);

    }

    private bool IsPathMatch(string path, string moduleSrc)
    {
        path = path.TrimEnd('/').ToLower();
        moduleSrc = moduleSrc.TrimEnd('/').ToLower();

        return path == moduleSrc || path.StartsWith(moduleSrc + "/");
    }



}
