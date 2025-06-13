using DIP.Data;
using DIP.Models;
using DIP.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace DIP.Controllers
{
    public class UserRoleController : Controller
    {
        private readonly DipDbContext _context;

        public UserRoleController(DipDbContext context)
        {
            _context = context;
        }

        // GET: UserRole/Index
        public async Task<IActionResult> Index()
        {
            var userRoles = await _context.SysUserRoles
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .ToListAsync();

            return View(userRoles);
        }


        // GET: UserRole/Create
        public async Task<IActionResult> Create()
        {
            var vm = new UserRoleViewModel
            {
                Users = await _context.SysUsers.ToListAsync(),
                Roles = await _context.SysRoles.ToListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserRoleViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var currentUserId = GetCurrentUserId() ?? 0;
                var now = DateTime.Now;

                var existingUserRole = await _context.SysUserRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == vm.UserId);

                if (existingUserRole != null)
                {
                    existingUserRole.RoleId = vm.RoleId;
                    existingUserRole.TxId = currentUserId;
                    existingUserRole.TxDate = now;
                    _context.SysUserRoles.Update(existingUserRole);
                }
                else
                {
                    var newUserRole = new SysUserRole
                    {
                        UserId = vm.UserId,
                        RoleId = vm.RoleId,
                        CreateId = currentUserId,
                        CreateDate = now,
                        TxId = currentUserId,
                        TxDate = now
                    };
                    _context.SysUserRoles.Add(newUserRole);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            vm.Users = await _context.SysUsers.ToListAsync();
            vm.Roles = await _context.SysRoles.ToListAsync();
            return View(vm);
        }


        private long? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (long.TryParse(userIdClaim, out var userId))
                return userId;
            return null;
        }

    }
}
