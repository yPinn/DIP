public class KnowledgeDetailViewModel
{
    public KmsKnowledge Knowledge { get; set; }
    public string FileTypeName { get; set; }
    public List<CommentViewModel> Comments { get; set; }
    public bool CanEditOrDelete { get; set; }  // 新增
    public string UserReaction { get; set; } // "like" / "dislike" / null
    public int LikeCount { get; set; }
    public int DislikeCount { get; set; }
    public string KmxFilePraiser { get; set; } // "1"=Like, "2"=Dislike, null=尚未點
    public bool KaTypeK_AttentionExists { get; set; } // 判斷是否已主動關注

}
