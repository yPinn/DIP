namespace DIP.Models
{
    public partial class SysRoleModule
    {
        public long RoleId { get; set; }
        public long ModuleId { get; set; }
        public string? ActiveFlag { get; set; }

        public long? CreateId { get; set; }
        public DateTime? CreateDate { get; set; }
        public long? TxId { get; set; }
        public DateTime? TxDate { get; set; }

        // 導覽屬性
        public virtual SysRole? Role { get; set; }
        public virtual SysModule? Module { get; set; }
    }
}
