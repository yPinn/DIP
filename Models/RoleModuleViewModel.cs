using Microsoft.AspNetCore.Mvc.Rendering;

namespace DIP.Models
{
    public class RoleModuleViewModel
    {
        public long ModuleId { get; set; }
        public string? ModuleName { get; set; }
        public string? SystemId { get; set; }
        public bool IsSelected { get; set; }
        public long? ParentId { get; set; }  // 父節點Id（方便用）
        public List<RoleModuleViewModel> Children { get; set; } = new List<RoleModuleViewModel>();
    }

}
