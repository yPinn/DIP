using System;
using System.Collections.Generic;

namespace DIP.Models
{
    public partial class SysModule
    {
        public long ModuleId { get; set; }

        public string? ModulenameCn { get; set; }
        public string? ModulenameTw { get; set; }
        public string? ModulenameJp { get; set; }
        public string? ModulenameUs { get; set; }

        public string? ModuleImg { get; set; }
        public string? ModuleSrc { get; set; }

        public int? Parentid { get; set; }
        public int? SeqNo { get; set; }

        public string? SystemId { get; set; }
        public string? SystemType { get; set; }

        public long? CreateId { get; set; }
        public DateTime? CreateDate { get; set; }
        public long? TxId { get; set; }
        public DateTime? TxDate { get; set; }

        public string? ActiveFlag { get; set; }


        public virtual ICollection<SysRoleModule> RoleModules { get; set; } = new HashSet<SysRoleModule>();

    }
}
