using System.ComponentModel.DataAnnotations;

public class KmsTypeViewModel
{
    public int KtId { get; set; }
    [Required]
    public string KtNameCn { get; set; }
    [Required]
    public string KtNameTw { get; set; }
    [Required]
    public string KtNameJp { get; set; }
    [Required]
    public string KtNameUs { get; set; }
    public int? ParentId { get; set; }

    // 加上這個屬性，方便 BuildTree 組成樹狀
    public List<KmsTypeViewModel> Children { get; set; } = new List<KmsTypeViewModel>();
}
