using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DIP.Models
{
    [Table("KMS_FRIENDLINK")]
    public class KmsFriendlink
    {
        [Key]
        [Column("LINK_SEQ")]
        public int LinkSeq { get; set; }

        [Column("NAME_CN")]
        [MaxLength(50)]
        public string? NameCn { get; set; }

        [Column("NAME_TW")]
        [MaxLength(50)]
        public string? NameTw { get; set; }

        [Column("NAME_US")]
        [MaxLength(50)]
        public string? NameUs { get; set; }

        [Column("NAME_JP")]
        [MaxLength(50)]
        public string? NameJp { get; set; }

        [Required]
        [Column("LINK")]
        [MaxLength(200)]
        public string Link { get; set; } = null!;
    }
}
