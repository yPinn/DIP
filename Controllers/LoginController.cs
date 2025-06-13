using DIP.Data;
using DIP.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DIP.Controllers
{
    public class LoginController : Controller
    {
        private readonly DipDbContext _context;

        public LoginController(DipDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Index(LoginModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 1. 找出使用者
            var user = await _context.SysUsers
                .FirstOrDefaultAsync(u => u.UserLogid == model.Username && u.Activeflag == "Y");

            if (user == null || user.UserPwd != model.Password)
            {
                ModelState.AddModelError("", "帳號或密碼錯誤");
                return View(model);
            }

            // 2. 從 SysUserRole 表中找該使用者的 RoleId
            var roleId = await _context.SysUserRoles
                .Where(r => r.UserId == user.UserId)
                .Select(r => r.RoleId)
                .FirstOrDefaultAsync(); // 預設只取一個角色

            // 3. 建立 Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserLogid),
                new Claim("DisplayName", user.UserName ?? ""),
                new Claim("RoleId", roleId.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Home", "Knowledge");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Home", "Knowledge");
        }
    }
}
