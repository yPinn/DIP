namespace DIP.Models
{
    public class ModuleCheckItem
    {
        public long ModuleId { get; set; }
        public string ModulenameTw { get; set; }  // 例如用 MODULENAME_TW 或 MODULENAME_US
        public bool IsChecked { get; set; }
    }

}
