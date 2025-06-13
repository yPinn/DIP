using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

public class KnowledgeViewModel
{
    public List<KmsKnowledge> RecentKnowledges { get; set; }
    public List<KmsKnowledge> PopularKnowledges { get; set; }

    // 🔽 為搜尋結果頁加上的欄位
    public List<KmsKnowledge> SearchResults { get; set; }
    public string Keyword { get; set; }
    public List<SelectListItem> Categories { get; set; }  // 類別下拉選單
    public int? SelectedCategoryId { get; set; }          // 選擇的類別 ID
}
