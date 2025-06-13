using System;
using System.Collections.Generic;

namespace DIP.Models;

public partial class SysUser
{
    public long UserId { get; set; }

    public string? UserLogid { get; set; }

    public string? UserPwd { get; set; }

    public string? UserName { get; set; }

    public string? UserImg { get; set; }

    public string? ImgContentType { get; set; }

    public string? UserMail { get; set; }

    public string? UserDesc { get; set; }

    public long? CreateId { get; set; }

    public DateTime? CreateDate { get; set; }

    public long? TxId { get; set; }

    public DateTime? TxDate { get; set; }

    public string? Activeflag { get; set; }

    public virtual SysUserRole? UserRole { get; set; }


}
