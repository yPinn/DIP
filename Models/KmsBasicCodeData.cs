using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("KMS_BASICCODE_DATA", Schema = "dbo")]
public class KmsBasicCodeData
{
    [Key]
    [Column("DATAID")]
    public long DataId { get; set; }

    [Column("CODENO")]
    [MaxLength(30)]
    public string? CodeNo { get; set; }

    [Column("DATAVALUE")]
    [MaxLength(30)]
    public string? DataValue { get; set; }

    [Column("DATANAME_CN")]
    [MaxLength(30)]
    public string? DataNameCn { get; set; }

    [Column("DATANAME_TW")]
    [MaxLength(30)]
    public string? DataNameTw { get; set; }

    [Column("DATANAME_JP")]
    [MaxLength(30)]
    public string? DataNameJp { get; set; }

    [Column("DATANAME_US")]
    [MaxLength(30)]
    public string? DataNameUs { get; set; }

    [Column("ACTIVEFLAG")]
    [MaxLength(1)]
    public string? ActiveFlag { get; set; }
}
