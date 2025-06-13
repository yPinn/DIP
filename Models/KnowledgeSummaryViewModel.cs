public class KnowledgeSummaryViewModel
{
    public long KfFileId { get; set; }
    public string KfFileName { get; set; }
    public string KfBrief { get; set; }
    public int KfReadNum { get; set; }
    public int KfPraiseNum { get; set; }
    public int KfTreadNum { get; set; }
    public int KfStatus { get; set; }
    public DateTime TxDate { get; set; }
    public string? KfTypeName { get; set; }
    public int CommentCount { get; set; }
}
