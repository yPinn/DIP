using Microsoft.AspNetCore.Mvc.Rendering;

namespace DIP.Models
{
    public class RoleModuleIndexViewModel
    {
        public long SelectedRoleId { get; set; }
        public List<SelectListItem> Roles { get; set; } = new();
        public List<RoleModuleViewModel> Modules { get; set; } = new();
    }
}
