using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("KMS_ATTENTION")]
public class KmsAttention
{
    [Key]
    [Column("KA_ID")]
    public long KaId { get; set; } // 關注表主鍵，不需顯示

    [Column("KA_TIME")]
    public DateTime KaTime { get; set; } // 關注時間

    [Column("KA_USERID")]
    public long KaUserId { get; set; } // 關注者使用者ID

    [Column("KA_TYPE")]
    [MaxLength(1)]
    public string KaType { get; set; } = "K"; // 關注類型：K = 主動收藏, S = 自我關注

    [Column("KA_ATTENTIONID")]
    public long KaAttentionId { get; set; } // 關注的知識ID，對應 KMS_KNOWLEDGE.KF_FILEID

    [Column("KA_ATTENTIONNUM")]
    public long? KaAttentionNum { get; set; } // 預留欄位，不使用

    // 導覽屬性（可選，若有設定）
    [ForeignKey("KaAttentionId")]
    public virtual KmsKnowledge? Knowledge { get; set; }
}
