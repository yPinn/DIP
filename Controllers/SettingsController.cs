using DIP.Data;
using DIP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DIP.Controllers
{
    public class SettingsController : Controller
    {
        private readonly DipDbContext _context;

        public SettingsController(DipDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var rawList = _context.KmsTypes
                .Select(x => new KmsTypeViewModel
                {
                    KtId = x.KtId,
                    KtNameTw = x.KtNameTw,
                    ParentId = x.ParentId,
                    Children = new List<KmsTypeViewModel>() // 預設避免 null
                })
                .ToList();

            var tree = BuildTree(rawList, null);
            return View(tree);
        }


        private List<KmsTypeViewModel> BuildTree(List<KmsTypeViewModel> list, int? parentId)
        {
            return list
                .Where(x => x.ParentId == parentId)
                .Select(x => {
                    x.Children = BuildTree(list, x.KtId);
                    return x;
                })
                .ToList();
        }


        [HttpPost]
        public IActionResult Save(KmsType model)
        {
            if (model.KtId == 0) // ➜ 表示新增
            {
       
                _context.KmsTypes.Add(model);
            }
            else
            {
                var existing = _context.KmsTypes.Find(model.KtId);
                if (existing != null)
                {
                    existing.KtNameTw = model.KtNameTw;
                    existing.ParentId = model.ParentId; 
                    existing.TxId = GetCurrentUserId();
                    existing.TxDate = DateTime.Now;

                }
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteF([FromBody] DeleteRequest request)
        {
            try
            {
                if (request == null || request.Id <= 0)
                    return Json(new { success = false, message = "無效的分類ID。" });

                var target = _context.KmsTypes.Find(request.Id);
                if (target == null)
                    return Json(new { success = false, message = "找不到指定分類。" });

                DeleteRecursive(target.KtId);
                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "刪除時發生錯誤：" + ex.Message });
            }
        }

        private void DeleteRecursive(int id)
        {
            var children = _context.KmsTypes.Where(x => x.ParentId == id).ToList();

            foreach (var child in children)
            {
                DeleteRecursive(child.KtId);
            }

            var target = _context.KmsTypes.Find(id);
            if (target != null)
            {
                _context.KmsTypes.Remove(target);
            }
        }

        public class DeleteRequest
        {
            public int Id { get; set; }
        }


        [HttpGet]
        public IActionResult CreateF(int? parentId)
        {
            var model = new KmsTypeViewModel();
            if (parentId.HasValue)
                model.ParentId = parentId.Value;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateF(KmsTypeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var entity = new KmsType
                {
                    KtNameCn = model.KtNameCn,
                    KtNameTw = model.KtNameTw,
                    KtNameJp = model.KtNameJp,
                    KtNameUs = model.KtNameUs,
                    ParentId = model.ParentId,
                    CreateId = GetCurrentUserId(),
                    CreateDate = DateTime.Now,
                    TxId = GetCurrentUserId(),
                    TxDate = DateTime.Now,
                    ActiveFlag = 'Y'
                };

                _context.KmsTypes.Add(entity);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(model);
        }


        // GET: /Settings/EditF/{id}
        public IActionResult EditF(int id)
        {
            var entity = _context.KmsTypes.Find(id);
            if (entity == null)
                return NotFound();

            var model = new KmsTypeViewModel
            {
                KtId = entity.KtId,
                KtNameCn = entity.KtNameCn,
                KtNameTw = entity.KtNameTw,
                KtNameJp = entity.KtNameJp,
                KtNameUs = entity.KtNameUs,
                ParentId = entity.ParentId
            };

            return View(model);
        }

        // POST: /Settings/EditF
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditF(KmsTypeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existing = _context.KmsTypes.Find(model.KtId);
                if (existing == null)
                    return NotFound();

                existing.KtNameCn = model.KtNameCn;
                existing.KtNameTw = model.KtNameTw;
                existing.KtNameJp = model.KtNameJp;
                existing.KtNameUs = model.KtNameUs;
                existing.ParentId = model.ParentId;
                existing.TxId = GetCurrentUserId();
                existing.TxDate = DateTime.Now;

                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(model);
        }



        private long GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(userIdString, out var userId) ? userId : 0;
        }



        // 友鏈列表頁
        public async Task<IActionResult> Links()
        {
            var links = await _context.KmsFriendlinks
                .OrderBy(f => f.LinkSeq)
                .ToListAsync();

            return View(links);
        }

        // 新增友鏈頁（GET）
        public IActionResult CreateLink()
        {
            return View();
        }

        // 新增友鏈頁（POST）
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLink(KmsFriendlink model)
        {
            if (ModelState.IsValid)
            {
                _context.KmsFriendlinks.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Links));
            }
            return View(model);
        }

        // 編輯友鏈頁（GET）
        public async Task<IActionResult> EditLink(int id)
        {
            var link = await _context.KmsFriendlinks.FindAsync(id);
            if (link == null) return NotFound();

            return View(link);
        }

        // 編輯友鏈頁（POST）
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLink(KmsFriendlink model)
        {
            if (ModelState.IsValid)
            {
                _context.KmsFriendlinks.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Links));
            }
            return View(model);
        }

        // 刪除友鏈確認頁（GET）
        public async Task<IActionResult> DeleteLink(int id)
        {
            var link = await _context.KmsFriendlinks.FindAsync(id);
            if (link == null) return NotFound();

            return View(link);
        }

        // 刪除友鏈實際執行（POST）
        [HttpPost, ActionName("DeleteLink")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLinkConfirmed(int id)
        {
            var link = await _context.KmsFriendlinks.FindAsync(id);
            if (link != null)
            {
                _context.KmsFriendlinks.Remove(link);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Links));
        }
    }
}
