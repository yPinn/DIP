using DIP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DIP.Controllers
{
    public class CommentController : Controller
    {
        private readonly DipDbContext _context;

        public CommentController(DipDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var comments = await _context.KmsFileComments
                .Include(c => c.KfFile)
                .Select(c => new CommentIndexViewModel
                {
                    CommentId = c.KcId,
                    CommentContent = c.KcContent,
                    KnowledgeTitle = c.KfFile.KfFileName,
                    KnowledgeId = c.KfFileId,
                    CommentTime = c.KcTime,
                    ReplyCount = _context.KmsFileComments.Count(r => r.KcParentId == c.KcId)
                })
                .ToListAsync();

            return View(comments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CommentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Details", "Knowledge", new { id = model.KfFileId });
            }

            // 判斷是否已有 KaType='S' 的紀錄
            long currentUserId = GetCurrentUserId().Value;
            
            var comment = new KmsFileComment
            {
                KfFileId = model.KfFileId,
                KcParentId = model.KcParentId,
                KcContent = model.KcContent,
                UserId = GetCurrentUserId(),
                KcTime = DateTime.Now,
                ActiveFlag = "Y",
                AdminCheck = "N"
            };

            _context.KmsFileComments.Add(comment);

            bool alreadyAttentionS = await _context.KmsAttentions.AnyAsync(a =>
                a.KaType == "S" &&
                a.KaAttentionId == model.KfFileId &&
                a.KaUserId == currentUserId);

            if (!alreadyAttentionS)
            {
                var attention = new KmsAttention
                {
                    KaAttentionId = model.KfFileId,
                    KaUserId = currentUserId,
                    KaType = "S",
                    KaTime = DateTime.Now
                };
                _context.KmsAttentions.Add(attention);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Knowledge", new { id = model.KfFileId });
        }


        private long? GetCurrentUserId()
        {
            if (!User.Identity.IsAuthenticated)
                return null;

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdStr) && long.TryParse(userIdStr, out long userId))
            {
                return userId;
            }
            return null;
        }

        // GET
        public async Task<IActionResult> Edit(long id)
        {
            var comment = await _context.KmsFileComments.FindAsync(id);
            if (comment == null) return NotFound();

            if (comment.UserId != GetCurrentUserId() && !User.IsInRole("Admin"))
                return Forbid();

            var model = new CommentViewModel
            {
                KcId = comment.KcId,
                KfFileId = comment.KfFileId,
                KcContent = comment.KcContent
            };

            return View(model);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CommentViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var comment = await _context.KmsFileComments.FindAsync(model.KcId);
            if (comment == null) return NotFound();

            if (comment.UserId != GetCurrentUserId() && !User.IsInRole("Admin"))
                return Forbid();

            comment.KcContent = model.KcContent;
            comment.KcTime = DateTime.Now;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Knowledge", new { id = model.KfFileId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id, string? returnUrl)
        {
            var comment = await _context.KmsFileComments
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.KcId == id);

            if (comment == null)
                return NotFound();

            long? currentUserId = GetCurrentUserId();
            long fileId = comment.KfFileId;

            if (comment.Replies != null && comment.Replies.Any())
            {
                _context.KmsFileComments.RemoveRange(comment.Replies);
            }

            _context.KmsFileComments.Remove(comment);

            var attentions = await _context.KmsAttentions
                .Where(a => a.KaUserId == currentUserId && a.KaAttentionId == fileId && a.KaType == "S")
                .ToListAsync();

            _context.KmsAttentions.RemoveRange(attentions);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Details", "Knowledge", new { id = fileId });
        }


    }
}
