public class CommentIndexViewModel
{
    public long CommentId { get; set; }
    public string CommentContent { get; set; }
    public string KnowledgeTitle { get; set; }
    public long KnowledgeId { get; set; }
    public DateTime? CommentTime { get; set; }
    public int ReplyCount { get; set; }
}
