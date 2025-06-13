using DIP.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

public class KnowledgeCreateViewModel
{
    public long? KfFileId { get; set; }  // 新增時是 null，編輯時有值

    [Required(ErrorMessage = "請輸入標題")]
    [StringLength(100)]
    public string KfFileName { get; set; } = null!;

    [Required(ErrorMessage = "請輸入內容")]
    public string KfFileContent { get; set; } = null!;

    [StringLength(200)]
    public string? KfHyperlink { get; set; }

    [Required(ErrorMessage = "請輸入摘要")]
    [StringLength(500)]
    public string KfBrief { get; set; } = null!;

    public string? KfLabel { get; set; }

    [Required(ErrorMessage = "請選擇分類")]
    public int? KfFileTypeId { get; set; }  // 用 int? 方便前端下拉選單空值判斷

    [Required(ErrorMessage = "請選擇狀態")]
    public int KfStatus { get; set; }  // 前端用 string 接收，後端轉 int

    public List<KmsType> KmsTypes { get; set; } = new List<KmsType>();

    // 新增一個用來存 SelectListItem 的屬性，用於下拉選單綁定
    public List<SelectListItem> KfFileTypeSelectList { get; set; } = new List<SelectListItem>();

    public List<DataOption> StatusOptions { get; set; } = new List<DataOption>();
}
