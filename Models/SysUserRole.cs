using System;

namespace DIP.Models
{
    public partial class SysUserRole
    {
        public long UserId { get; set; }
        public long RoleId { get; set; }
        public long? CreateId { get; set; }
        public DateTime? CreateDate { get; set; }
        public long? TxId { get; set; }
        public DateTime? TxDate { get; set; }

        // 導覽屬性
        public virtual SysUser? User { get; set; }
        public virtual SysRole? Role { get; set; }
    }
}
