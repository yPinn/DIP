public class CommentViewModel
{
    public long KcId { get; set; }
    public long KfFileId { get; set; }
    public long? KcParentId { get; set; }
    public string KcContent { get; set; }
    public DateTime? KcTime { get; set; }
    public string? UserName { get; set; }
    public long? UserId { get; set; }  
    public bool CanEdit { get; set; }

    public List<CommentViewModel> Replies { get; set; } = new List<CommentViewModel>();
}
