public class AttentionViewModel
{
    public long KfFileId { get; set; }
    public string Title { get; set; }
    public DateTime CreateTime { get; set; }      // 知識建立時間
    public DateTime AttentionTime { get; set; }   // 關注時間或首次留言時間
    public string Type { get; set; }              // "K" 或 "S"
}
