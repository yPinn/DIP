using DIP.Data;
using DIP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class RoleModuleController : Controller
{
    private readonly DipDbContext _context;

    public RoleModuleController(DipDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(long? selectedRoleId)
    {
        if (!selectedRoleId.HasValue)
            selectedRoleId = await _context.SysRoles.Select(r => r.RoleId).FirstOrDefaultAsync();

        var allModules = await _context.SysModules
            .OrderBy(m => m.ModuleId)
            .Select(m => new RoleModuleViewModel
            {
                ModuleId = m.ModuleId,
                ModuleName = m.ModulenameTw,
                SystemId = m.SystemId,
                ParentId = m.Parentid,
                IsSelected = _context.SysRoleModules
                    .Any(rm => rm.RoleId == selectedRoleId && rm.ModuleId == m.ModuleId),
                Children = new List<RoleModuleViewModel>()
            }).ToListAsync();

        // 建立字典以便後續組成樹狀結構
        var moduleDict = allModules.ToDictionary(m => m.ModuleId);
        List<RoleModuleViewModel> rootModules = new List<RoleModuleViewModel>();

        foreach (var module in allModules)
        {
            if (module.ParentId == null || module.ParentId == 0)
            {
                rootModules.Add(module);
            }
            else if (moduleDict.ContainsKey(module.ParentId.Value))
            {
                moduleDict[module.ParentId.Value].Children.Add(module);
            }
            else
            {
                // 若找不到對應的父節點，將它當作 root
                rootModules.Add(module);
            }
        }

        var roles = await _context.SysRoles
        .Select(r => new SelectListItem
        {
            Value = r.RoleId.ToString(),
            Text = r.RolenameTw,
            Selected = (r.RoleId == selectedRoleId)
        }).ToListAsync();

        var viewModel = new RoleModuleIndexViewModel
        {
            SelectedRoleId = selectedRoleId.Value,
            Roles = roles,
            Modules = rootModules
        };

        return View(viewModel);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(long SelectedRoleId, List<long> SelectedModuleIds)
    {
        SelectedModuleIds ??= new List<long>();

        try
        {
            var existing = _context.SysRoleModules.Where(rm => rm.RoleId == SelectedRoleId);
            _context.SysRoleModules.RemoveRange(existing);

            var selectedModules = SelectedModuleIds.Select(mid => new SysRoleModule
            {
                RoleId = SelectedRoleId,
                ModuleId = mid,
                ActiveFlag = "Y",
                CreateDate = DateTime.Now
            });

            await _context.SysRoleModules.AddRangeAsync(selectedModules);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { selectedRoleId = SelectedRoleId });
        }
        catch (Exception ex)
        {
            // TODO: Log exception
            ModelState.AddModelError("", "儲存失敗：" + ex.Message);
            return RedirectToAction("Index", new { selectedRoleId = SelectedRoleId });
        }
    }




}
