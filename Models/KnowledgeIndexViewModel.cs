public class KnowledgeIndexViewModel
{
    public long KfFileId { get; set; }
    public string KfFileName { get; set; }
    public string KfBrief { get; set; }
    public int? KfFileTypeId { get; set; }  
    public string TypeName { get; set; }    // 文章分類名稱
    public string KfLabel { get; set; }
    public int? KfReadNum { get; set; }
    public int? KfPraiseNum { get; set; }
    public int? KfTreadNum { get; set; }
    public int KfStatus { get; set; }
    public string ActiveFlag { get; set; }
}
