using DIP.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Text.Encodings.Web;

namespace DIP.Helpers
{
    public static class HtmlHelpers
    {
        public static IHtmlContent RenderModules(this IHtmlHelper htmlHelper, List<RoleModuleViewModel> modules, string prefix, string parentKey = "")
        {
            var writer = new System.IO.StringWriter();

            void Render(List<RoleModuleViewModel> mods, string pre, string pKey)
            {
                writer.Write("<ul class=\"list-unstyled ps-3\">");
                for (int i = 0; i < mods.Count; i++)
                {
                    var module = mods[i];
                    var currentKey = $"{pKey}_{i}";
                    var collapseId = $"collapse_{currentKey}";

                    writer.Write("<li>");

                    // 隱藏欄位
                    writer.Write($"<input type=\"hidden\" name=\"{pre}[{i}].ModuleId\" value=\"{module.ModuleId}\" />");
                    writer.Write($"<input type=\"hidden\" name=\"{pre}[{i}].ModuleName\" value=\"{module.ModuleName}\" />");
                    writer.Write($"<input type=\"hidden\" name=\"{pre}[{i}].SystemId\" value=\"{module.SystemId}\" />");
                    writer.Write($"<input type=\"hidden\" name=\"{pre}[{i}].ParentId\" value=\"{module.ParentId ?? 0}\" />");

                    // checkbox 與 label
                    var checkboxId = $"chk_{currentKey}";
                    var isChecked = module.IsSelected ? "checked" : "";
                    writer.Write($"<input type=\"checkbox\" id=\"{checkboxId}\" name=\"SelectedModuleIds\" " +
                                 $"value=\"{module.ModuleId}\" {isChecked} " +
                                 $"data-id=\"{module.ModuleId}\" data-parent-id=\"{module.ParentId ?? 0}\" class=\"module-checkbox\" />");

                    writer.Write($"<label for=\"{checkboxId}\">{HtmlEncoder.Default.Encode(module.ModuleName)}</label>");

                    // 有子模組，加入折疊區塊
                    if (module.Children != null && module.Children.Count > 0)
                    {
                        writer.Write($"<button class=\"btn btn-sm btn-link\" type=\"button\" data-bs-toggle=\"collapse\" data-bs-target=\"#{collapseId}\">[+]</button>");
                        writer.Write($"<div class=\"collapse show\" id=\"{collapseId}\">");
                        Render(module.Children, $"{pre}[{i}].Children", currentKey);
                        writer.Write("</div>");
                    }
                    writer.Write("</li>");
                }
                writer.Write("</ul>");
            }

            Render(modules, prefix, parentKey);
            return new HtmlString(writer.ToString());
        }
    }
}
