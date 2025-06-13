using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("KMS_FILEPRAISE")]
public class KmsFilePraise
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("KP_ID")]
    public long KpId { get; set; }

    [Column("KP_TIME")]
    public DateTime? KpTime { get; set; }

    [Column("USER_ID")]
    public long? UserId { get; set; }

    [Column("KF_FILEID")]
    public long KfFileId { get; set; }

    [Column("KP_TYPE")]
    [StringLength(1)]
    public string KpType { get; set; } // "P"=Like, "T"=Dislike

    // Navigation property
    [ForeignKey(nameof(KfFileId))]
    public virtual KmsKnowledge Knowledge { get; set; }
}
