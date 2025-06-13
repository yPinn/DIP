using DIP.Data;
using DIP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

namespace DIP.Controllers
{
    public class UserController : Controller
    {
        private readonly DipDbContext _context;

        public UserController(DipDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.SysUsers.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.UserLogid.Contains(search) || u.UserName.Contains(search));
            }

            var users = await query.ToListAsync();
            return View(users);
        }


        // GET: SysUser/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SysUser/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SysUser user)
        {
            if (ModelState.IsValid)
            {
                if (_context.SysUsers.Any(u => u.UserId == user.UserId))
                {
                    ModelState.AddModelError("UserId", "此 UserId 已存在，請輸入其他值。");
                    return View(user);
                }

                user.CreateDate = DateTime.Now;
                user.TxDate = DateTime.Now;
                user.Activeflag ??= "Y"; // 預設啟用

                _context.SysUsers.Add(user);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string userLogid)
        {
            if (string.IsNullOrEmpty(userLogid))
            {
                ViewBag.Error = "請選擇使用者";
                var users = await _context.SysUsers.ToListAsync();
                return View(users);
            }

            var user = await _context.SysUsers.FirstOrDefaultAsync(u => u.UserLogid == userLogid);
            if (user == null)
            {
                ViewBag.Error = "找不到使用者";
                var users = await _context.SysUsers.ToListAsync();
                return View(users);
            }

            string defaultPassword = "888888";

            var passwordHasher = new PasswordHasher<SysUser>();
            user.UserPwd = passwordHasher.HashPassword(user, defaultPassword);

            await _context.SaveChangesAsync();

            ViewBag.Success = $"{user.UserLogid} 密碼已重設為預設值。";
            var userList = await _context.SysUsers.ToListAsync();
            return View(userList);
        }


        [HttpGet]
        public IActionResult ResetPassword()
        {
            var users = _context.SysUsers.ToList();
            return View(users);
        }

        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.SysUsers.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("UserId,UserLogid,UserName,UserMail,Activeflag")] SysUser user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.SysUsers.FindAsync(id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    // 更新可編輯欄位
                    existingUser.UserName = user.UserName;
                    existingUser.UserMail = user.UserMail;
                    existingUser.Activeflag = user.Activeflag;
                    existingUser.TxDate = DateTime.Now; // 更新時間戳

                    _context.Update(existingUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SysUserExists(user.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var sysUser = await _context.SysUsers.FindAsync(id);
            if (sysUser != null)
            {
                _context.SysUsers.Remove(sysUser);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool SysUserExists(long id)
        {
            return _context.SysUsers.Any(e => e.UserId == id);
        }

        [Authorize]
        public IActionResult Profile()
        {
            var userLogid = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userLogid))
                return RedirectToAction("Index", "Login");

            var user = _context.SysUsers.FirstOrDefault(u => u.UserLogid == userLogid);

            if (user == null) return NotFound();

            return View(user);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdatePersonalInfo(SysUser model)
        {
            var user = _context.SysUsers.FirstOrDefault(u => u.UserId == model.UserId);
            if (user == null) return NotFound();

            user.UserName = model.UserName;
            user.UserMail = model.UserMail;
            user.UserDesc = model.UserDesc;
            user.TxDate = DateTime.Now;

            _context.SaveChanges();

            return RedirectToAction("Profile");
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View(); // 乾淨表單
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userLogid = User.FindFirstValue(ClaimTypes.Name);
            var user = _context.SysUsers.FirstOrDefault(u => u.UserLogid == userLogid);
            if (user == null) return NotFound();

            // 檢查舊密碼是否正確（直接比對明文）
            if (user.UserPwd != model.OldPassword)
            {
                ModelState.AddModelError("OldPassword", "舊密碼錯誤");
                return View(model);
            }

            // 儲存新密碼（明文）
            user.UserPwd = model.NewPassword;
            user.TxDate = DateTime.Now;

            _context.SaveChanges();

            return RedirectToAction("Home", "Knowledge");
        }





    }
}