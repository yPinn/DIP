using DIP.Data;
using DIP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DIP.Controllers
{
    public class RoleController : Controller
    {
        private readonly DipDbContext _context;

        public RoleController(DipDbContext context)
        {
            _context = context;
        }

        // 角色列表，支援搜尋
        [HttpGet]
        public async Task<IActionResult> Index(string search)
        {
            var query = _context.SysRoles.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(r =>
                    (r.RolenameCn ?? "").ToLower().Contains(lowerSearch) ||
                    (r.RolenameTw ?? "").ToLower().Contains(lowerSearch) ||
                    (r.RolenameJp ?? "").ToLower().Contains(lowerSearch) ||
                    (r.RolenameUs ?? "").ToLower().Contains(lowerSearch));
            }

            var roles = await query.ToListAsync();
            ViewData["search"] = search ?? "";
            return View(roles);
        }

        // 顯示新增表單
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 新增角色 - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SysRole role)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // 假設使用者有正確輸入唯一的 RoleId
                    role.CreateDate ??= DateTime.Now;
                    role.TxDate ??= DateTime.Now;
                    role.SystemId ??= "KMWeb";
                    role.SystemType ??= "2";
                    role.ActiveFlag ??= "Y";

                    _context.SysRoles.Add(role);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "寫入失敗：" + ex.Message);
                }
            }

            return View(role);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var role = await _context.SysRoles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, SysRole updatedRole)
        {
            if (id != updatedRole.RoleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingRole = await _context.SysRoles.FindAsync(id);
                if (existingRole == null)
                    return NotFound();

                if (await TryUpdateModelAsync<SysRole>(existingRole, "",
                    r => r.RolenameTw, r => r.RoleDesc, r => r.ActiveFlag))
                {
                    try
                    {
                        existingRole.TxDate = DateTime.Now;
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        ModelState.AddModelError("", "資料更新時發生衝突，請稍後再試。");
                    }
                }
            }

            return View(updatedRole);
        }


        [HttpGet]
        public IActionResult Delete(long id)
        {
            var role = _context.SysRoles.Find(id);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        // DeleteConfirmed 改成非同步
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var role = await _context.SysRoles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            _context.SysRoles.Remove(role);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
