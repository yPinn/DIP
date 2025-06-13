using DIP.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("KMS_FILECOMMENT")]
public class KmsFileComment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  // 這行告訴 EF 這是 DB 自動產生的欄位
    [Column("KC_ID")]
    public long KcId { get; set; }

    [Column("KC_TIME")]
    public DateTime? KcTime { get; set; }

    [Column("USER_ID")]
    public long? UserId { get; set; }

    [Column("KF_FILEID")]
    public long KfFileId { get; set; }

    [Column("KC_CONTENT")]
    [MaxLength(500)]
    public string? KcContent { get; set; }

    [Column("KC_PARENT_ID")]
    public long? KcParentId { get; set; }

    [Column("ACTIVEFLAG")]
    [MaxLength(1)]
    public string? ActiveFlag { get; set; }

    [Column("ADMIN_CHECK")]
    [MaxLength(1)]
    public string? AdminCheck { get; set; }

    // 導覽屬性：關聯到知識主檔
    [ForeignKey(nameof(KfFileId))]
    public virtual KmsKnowledge KfFile { get; set; } = null!;

    // 導覽屬性：父留言（可為 null）
    [ForeignKey(nameof(KcParentId))]
    public virtual KmsFileComment? ParentComment { get; set; }

    // 導覽屬性：子留言集合
    public virtual ICollection<KmsFileComment> Replies { get; set; } = new List<KmsFileComment>();

    // 新增導覽屬性：留言者 User
    [ForeignKey(nameof(UserId))]
    public virtual SysUser? User { get; set; }
}