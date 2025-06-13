using DIP.Models;

namespace DIP.ViewModels
{
    public class UserRoleViewModel
    {
        public long UserId { get; set; }
        public long RoleId { get; set; }

        // 下拉選單資料來源
        public List<SysUser>? Users { get; set; }
        public List<SysRole>? Roles { get; set; }
    }
}
