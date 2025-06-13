using DIP.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("KMS_KNOWLEDGE", Schema = "dbo")]
public class KmsKnowledge
{
    [Key]
    [Column("KF_FILEID")]
    public long KfFileId { get; set; }

    [Required]
    [Column("KF_FILENAME")]
    [MaxLength(100)]
    public string KfFileName { get; set; }

    [Required]
    [Column("KF_FILECONTENT", TypeName = "text")]
    public string KfFileContent { get; set; }

    [Column("KFHYPERLINK")]
    [MaxLength(200)]
    public string? KfHyperlink { get; set; }

    [Required]
    [Column("KF_BRIEF")]
    [MaxLength(500)]
    public string KfBrief { get; set; }

    [Column("KF_READNUM")]
    public int? KfReadNum { get; set; } = 0;

    [Required]
    [Column("KF_FILETYPEID")]
    [MaxLength(50)]
    public int? KfFileTypeId { get; set; }

    [Column("KF_LABEL")]
    [MaxLength(100)]
    public string? KfLabel { get; set; }

    [Required]
    [Column("KF_STATUS")]
    public int KfStatus { get; set; }

    [Required]
    [Column("CREATE_ID")]
    public long CreateId { get; set; }

    [Required]
    [Column("CREATE_DATE")]
    public DateTime CreateDate { get; set; }

    [Required]
    [Column("TX_ID")]
    public long TxId { get; set; }

    [Required]
    [Column("TX_DATE")]
    public DateTime TxDate { get; set; }

    [Required]
    [Column("ACTIVEFLAG")]
    [MaxLength(1)]
    public string ActiveFlag { get; set; }

    [Required]
    [Column("STATUS_UPDATE_FLAG")]
    [MaxLength(1)]
    public string StatusUpdateFlag { get; set; }

    [Column("KF_PRAISENUM")]
    public int? KfPraiseNum { get; set; } = 0;

    [Column("KF_TREADNUM")]
    public int? KfTreadNum { get; set; } = 0;

    public virtual KmsType KfFileType { get; set; }

    public virtual ICollection<KmsFileComment> KmsFileComments { get; set; }

    // 新增導覽屬性：留言者 User
    [ForeignKey(nameof(CreateId))]
    public virtual SysUser? CreateUser { get; set; }
}
