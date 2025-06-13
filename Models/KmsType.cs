public class KmsType
{
    public int KtId { get; set; }
    public string KtNameCn { get; set; }
    public string KtNameTw { get; set; }
    public string KtNameJp { get; set; }
    public string KtNameUs { get; set; }
    public int? ParentId { get; set; }
    public int? SeqNo { get; set; }
    public long? CreateId { get; set; }
    public DateTime? CreateDate { get; set; }
    public long? TxId { get; set; }
    public DateTime? TxDate { get; set; }
    public char? ActiveFlag { get; set; }

    // 導航屬性
    public virtual KmsType? Parent { get; set; }
    public virtual ICollection<KmsType> Children { get; set; }
}
