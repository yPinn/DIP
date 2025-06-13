public class KnowledgeBrowsingViewModel
{
    public long KfFileId { get; set; }
    public string Title { get; set; }
    public int Status { get; set; } // 1=Published, 0=Unpublished
    public DateTime CreateDate { get; set; }
    public int VisitCount { get; set; }
    public int CommentCount { get; set; }
    public long CreateUserId { get; set; }

}

