using System;
using System.Collections.Generic;

namespace DIP.Models;

public partial class SysRole
{
    public long RoleId { get; set; }

    public string? RolenameCn { get; set; }

    public string? RolenameTw { get; set; }

    public string? RolenameJp { get; set; }

    public string? RolenameUs { get; set; }

    public string? SystemId { get; set; }

    public string? SystemType { get; set; }

    public string? RoleDesc { get; set; }

    public string? ActiveFlag { get; set; }

    public long? CreateId { get; set; }

    public DateTime? CreateDate { get; set; }

    public long? TxId { get; set; }

    public DateTime? TxDate { get; set; }

    public virtual ICollection<SysUserRole> UserRoles { get; set; } = new HashSet<SysUserRole>();

    public virtual ICollection<SysRoleModule> RoleModules { get; set; } = new HashSet<SysRoleModule>();

}
