using DIP.Data;
using DIP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

namespace DIP.Controllers
{
    public class KnowledgeController : Controller
    {
        private readonly DipDbContext _context;

        public KnowledgeController(DipDbContext context)
        {
            _context = context;
        }

        // ========== Index ==========
        public async Task<IActionResult> Index()
        {
            var model = await _context.KmsKnowledges
                .Select(k => new KnowledgeIndexViewModel
                {
                    KfFileId = k.KfFileId,
                    KfFileName = k.KfFileName,
                    KfBrief = k.KfBrief,
                    KfFileTypeId = k.KfFileTypeId,
                    TypeName = k.KfFileType != null ? k.KfFileType.KtNameTw : "", // 假設有導入關聯
                    KfLabel = k.KfLabel,
                    KfReadNum = k.KfReadNum,
                    KfPraiseNum = k.KfPraiseNum,
                    KfTreadNum = k.KfTreadNum,
                    KfStatus = k.KfStatus,
                    ActiveFlag = k.ActiveFlag
                })
                .ToListAsync();  // **這裡 await 一定要有**

            return View(model);  // 傳已經取完的 List
        }

        public IActionResult Home()
        {
            // 取最新更新的公開知識，依 TxDate 排序，取前 10 筆
            var recentKnowledges = _context.KmsKnowledges
                .Where(k => k.KfStatus == 3)
                .OrderByDescending(k => k.TxDate)
                .Take(10)
                .ToList();

            // 取熱門公開知識，依 KfReadNum 排序，取前 10 筆
            var popularKnowledges = _context.KmsKnowledges
                .Where(k => k.KfStatus == 3)
                .OrderByDescending(k => k.KfReadNum)
                .Take(10)
                .ToList();

            var model = new KnowledgeViewModel
            {
                RecentKnowledges = recentKnowledges,
                PopularKnowledges = popularKnowledges
            };

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new KnowledgeCreateViewModel();
            await PrepareDropdowns(vm);
            return View(vm);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KnowledgeCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await PrepareDropdowns(vm);
                return View(vm);
            }

            var maxId = await _context.KmsKnowledges.MaxAsync(k => (long?)k.KfFileId) ?? 0;

            var entity = new KmsKnowledge
            {
                KfFileId = maxId + 1,
                KfFileName = vm.KfFileName,
                KfFileContent = vm.KfFileContent,
                KfBrief = vm.KfBrief,
                KfFileTypeId = vm.KfFileTypeId,
                KfLabel = vm.KfLabel,
                KfStatus = vm.KfStatus,
                KfReadNum = 0,
                KfPraiseNum = 0,
                KfTreadNum = 0,
                CreateId = GetCurrentUserId(),
                CreateDate = DateTime.Now,
                TxId = GetCurrentUserId(),
                TxDate = DateTime.Now,
                ActiveFlag = "Y",
                StatusUpdateFlag = "N"
            };

            _context.KmsKnowledges.Add(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction("Home");
        }


        [HttpGet]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
                return BadRequest();

            var entity = await _context.KmsKnowledges
                .FirstOrDefaultAsync(k => k.KfFileId == id);

            if (entity == null)
                return NotFound();

            var vm = new KnowledgeCreateViewModel
            {
                KfFileId = entity.KfFileId,
                KfFileName = entity.KfFileName,
                KfFileContent = entity.KfFileContent,
                KfBrief = entity.KfBrief,
                KfFileTypeId = entity.KfFileTypeId,
                KfLabel = entity.KfLabel,
                KfStatus = entity.KfStatus
            };

            await PrepareDropdowns(vm);

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(KnowledgeCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await PrepareDropdowns(vm);
                return View(vm);
            }

            if (vm.KfFileId == null)
                return BadRequest();

            var entity = await _context.KmsKnowledges
                .FirstOrDefaultAsync(k => k.KfFileId == vm.KfFileId);

            if (entity == null)
                return NotFound();

            bool statusChanged = entity.KfStatus != vm.KfStatus;

            entity.KfFileName = vm.KfFileName;
            entity.KfFileContent = vm.KfFileContent;
            entity.KfBrief = vm.KfBrief;
            entity.KfFileTypeId = vm.KfFileTypeId;
            entity.KfLabel = vm.KfLabel;
            entity.KfStatus = vm.KfStatus;
            entity.TxId = GetCurrentUserId();
            entity.TxDate = DateTime.Now;

            if (statusChanged)
                entity.StatusUpdateFlag = "Y";

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = vm.KfFileId });
        }

        // ========== 下拉選單準備 ==========
        private async Task PrepareDropdowns(KnowledgeCreateViewModel vm)
        {
            // 狀態選單維持原本
            vm.StatusOptions = await _context.KmsBasicCodeData
                .Where(c => c.CodeNo == "STATUS" && c.ActiveFlag == "Y")
                .Select(c => new DataOption
                {
                    DataValue = c.DataValue,
                    DataNameTw = c.DataNameTw
                })
                .ToListAsync();

            // 先抓所有啟用的分類（KmsTypes）
            var types = await _context.KmsTypes
                .Where(t => t.ActiveFlag == 'Y')
                .ToListAsync();

            // 建立父節點列表 (ParentId==null或0)，依 KtId 排序
            var parents = types.Where(t => t.ParentId == null || t.ParentId == 0)
                               .OrderBy(t => t.KtId)
                               .ToList();

            var list = new List<SelectListItem>();

            foreach (var parent in parents)
            {
                // 加父節點
                list.Add(new SelectListItem
                {
                    Value = parent.KtId.ToString(),
                    Text = parent.KtNameTw
                });

                // 找子節點，依 SeqNo 排序
                var children = types.Where(t => t.ParentId == parent.KtId)
                                    .OrderBy(t => t.SeqNo ?? int.MaxValue)
                                    .ToList();

                foreach (var child in children)
                {
                    list.Add(new SelectListItem
                    {
                        Value = child.KtId.ToString(),
                        Text = "　└─ " + child.KtNameTw // 全形空白縮排
                    });
                }
            }

            // 插入預設項目
            list.Insert(0, new SelectListItem { Value = "", Text = "-- 請選擇 --" });

            // 設定給 ViewModel
            vm.KfFileTypeSelectList = list;

            // 你還可以保留原本的 KmsTypes，如果其他地方會用
            vm.KmsTypes = types;
        }


        public async Task<IActionResult> Details(long id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            long? userId = string.IsNullOrEmpty(userIdString) ? null : long.Parse(userIdString);

            var knowledge = await _context.KmsKnowledges
                .Include(k => k.CreateUser)
                .Include(k => k.KfFileType)
                .FirstOrDefaultAsync(k => k.KfFileId == id && k.ActiveFlag == "Y");

            if (knowledge == null)
                return NotFound();

            // 使用者對此檔案的回應（Like/Dislike）
            string userReaction = null;
            if (userId.HasValue)
            {
                userReaction = _context.KmsFilePraises
                    .Where(r => r.KfFileId == knowledge.KfFileId && r.UserId == userId)
                    .Select(r => r.KpType)
                    .FirstOrDefault(); // "1"=Like, "2"=Dislike, null=未反應
            }

            // 閱讀數 +1
            knowledge.KfReadNum = (knowledge.KfReadNum ?? 0) + 1;
            await _context.SaveChangesAsync();

            var currentUserId = userId ?? 0;
            bool canEditOrDelete = knowledge.CreateUser?.UserId == currentUserId;
            bool kaTypeK_AttentionExists = false;
            if (userId.HasValue)
            {
                kaTypeK_AttentionExists = await _context.KmsAttentions.AnyAsync(a =>
                    a.KaUserId == userId.Value &&
                    a.KaType == "K" &&
                    a.KaAttentionId == id);
            }

            // 撈出所有留言
            var comments = _context.KmsFileComments
                .Where(c => c.KfFileId == id && c.ActiveFlag == "Y")
                .Include(c => c.User)
                .OrderByDescending(c => c.KcTime)
                .ToList();

            // 將留言拆為父留言與子留言
            var parentComments = comments.Where(c => c.KcParentId == null).ToList();
            var childCommentsLookup = comments
                .Where(c => c.KcParentId != null)
                .GroupBy(c => c.KcParentId.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 建立 ViewModel（最多兩層）
            var commentVMs = parentComments.Select(c => new CommentViewModel
            {
                KcId = c.KcId,
                KfFileId = c.KfFileId,
                KcContent = c.KcContent,
                UserName = c.User?.UserName ?? "匿名",
                UserId = c.UserId ?? 0,
                KcTime = c.KcTime,
                CanEdit = c.UserId == currentUserId,
                KcParentId = c.KcParentId,
                Replies = childCommentsLookup.ContainsKey(c.KcId)
                    ? childCommentsLookup[c.KcId].Select(r => new CommentViewModel
                    {
                        KcId = r.KcId,
                        KfFileId = r.KfFileId,
                        KcContent = r.KcContent,
                        UserName = r.User?.UserName ?? "匿名",
                        UserId = r.UserId ?? 0,
                        KcTime = r.KcTime,
                        CanEdit = r.UserId == currentUserId,
                        KcParentId = r.KcParentId,
                        Replies = null
                    }).ToList()
                    : new List<CommentViewModel>()
            }).ToList();

            // Like / Dislike 數量（依資料庫格式查詢：KP_TYPE = "1" 或 "2"）
            int likeCount = _context.KmsFilePraises.Count(r => r.KfFileId == knowledge.KfFileId && r.KpType == "P");
            int dislikeCount = _context.KmsFilePraises.Count(r => r.KfFileId == knowledge.KfFileId && r.KpType == "T");

            var vm = new KnowledgeDetailViewModel
            {
                Knowledge = knowledge,
                FileTypeName = knowledge.KfFileType?.KtNameTw ?? "未知分類",
                Comments = commentVMs,
                CanEditOrDelete = canEditOrDelete,
                LikeCount = likeCount,
                DislikeCount = dislikeCount,
                UserReaction = userReaction, // "1", "2" 或 null
                KaTypeK_AttentionExists = kaTypeK_AttentionExists
            };

            return View(vm);
        }

        public async Task<IActionResult> Draft()
        {
            var currentUserId = GetCurrentUserId();

            var drafts = await _context.KmsKnowledges
                .Where(k => k.KfStatus == 1 && k.CreateId == currentUserId)
                .OrderByDescending(k => k.CreateDate)
                .ToListAsync();

            return View(drafts);
        }


        public async Task<IActionResult> Browsing(string status)
        {
            var query = _context.KmsKnowledges.AsQueryable();

            var lowerStatus = status?.ToLower();

            // 取得目前登入者 ID
            var currentUserId = GetCurrentUserId();

            // 只顯示公開，或屬於自己的草稿/私人
            query = query.Where(k =>
                k.KfStatus == 3 ||             // 公開
                (k.CreateId == currentUserId && k.KfStatus != 3) // 自己的草稿/私人
            );

            // 過濾狀態（草稿/私人/公開）
            if (!string.IsNullOrEmpty(lowerStatus) && lowerStatus != "all")
            {
                switch (lowerStatus)
                {
                    case "unpublished":
                        query = query.Where(k => k.KfStatus == 1);
                        break;
                    case "private":
                        query = query.Where(k => k.KfStatus == 2);
                        break;
                    case "published":
                        query = query.Where(k => k.KfStatus == 3);
                        break;
                }
            }

            var viewModel = await query
                .Select(k => new KnowledgeBrowsingViewModel
                {
                    KfFileId = k.KfFileId,
                    Title = k.KfFileName,
                    Status = k.KfStatus,
                    CreateDate = k.CreateDate,
                    VisitCount = k.KfReadNum ?? 0,
                    CommentCount = _context.KmsFileComments
                        .Count(c => c.KfFileId == k.KfFileId && c.ActiveFlag == "Y"),
                    CreateUserId = k.CreateId
                })
                .ToListAsync();

            ViewBag.CurrentFilter = lowerStatus;

            return View(viewModel);
        }

        // GET: Delete/5
        [HttpGet]
        public IActionResult ConfirmDelete(long id)
        {
            var entity = _context.KmsKnowledges.FirstOrDefault(k => k.KfFileId == id);
            if (entity == null) return NotFound();

            return View(entity); // 可直接用 entity 或包成 ViewModel 顯示
        }

        // POST: Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)

        {
            var entity = await _context.KmsKnowledges.FirstOrDefaultAsync(k => k.KfFileId == id);
            if (entity == null) return NotFound();

            var relatedComments = await _context.KmsFileComments.Where(c => c.KfFileId == id).ToListAsync();
            _context.KmsFileComments.RemoveRange(relatedComments);

            var relatedAttentions = await _context.KmsAttentions.Where(a => a.KaAttentionId == id).ToListAsync();
            _context.KmsAttentions.RemoveRange(relatedAttentions);

            var relatedPraises = await _context.KmsFilePraises.Where(p => p.KfFileId == id).ToListAsync();
            _context.KmsFilePraises.RemoveRange(relatedPraises);

            _context.KmsKnowledges.Remove(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction("Home");
        }

        [HttpGet]
        public async Task<IActionResult> LoadComments(int fileId)
        {
            var comments = await _context.KmsFileComments
                .Include(c => c.User)
                .Where(c => c.KfFileId == fileId)
                .OrderBy(c => c.KcTime)
                .ToListAsync();

            return PartialView("_CommentList", comments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> React(long fileId, string reactionType)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

            var userId = long.Parse(userIdString);

            var existing = await _context.KmsFilePraises
                .FirstOrDefaultAsync(p => p.KfFileId == fileId && p.UserId == userId);

            var file = await _context.KmsKnowledges.FirstOrDefaultAsync(k => k.KfFileId == fileId);
            if (file == null) return NotFound();

            if (existing != null)
            {
                if (existing.KpType == reactionType)
                {
                    // 使用者點了同一反應，代表要取消
                    _context.KmsFilePraises.Remove(existing);

                    if (reactionType == "P") file.KfPraiseNum = Math.Max((file.KfPraiseNum ?? 0) - 1, 0);
                    else if (reactionType == "T") file.KfTreadNum = Math.Max((file.KfTreadNum ?? 0) - 1, 0);
                }
                else
                {
                    // 改變反應種類
                    if (existing.KpType == "P") file.KfPraiseNum = Math.Max((file.KfPraiseNum ?? 0) - 1, 0);
                    else if (existing.KpType == "T") file.KfTreadNum = Math.Max((file.KfTreadNum ?? 0) - 1, 0);

                    existing.KpType = reactionType;
                    existing.KpTime = DateTime.Now;

                    if (reactionType == "P") file.KfPraiseNum = (file.KfPraiseNum ?? 0) + 1;
                    else if (reactionType == "T") file.KfTreadNum = (file.KfTreadNum ?? 0) + 1;
                }
            }
            else
            {
                // 新增讚或踩
                var praise = new KmsFilePraise
                {
                    KfFileId = fileId,
                    UserId = userId,
                    KpType = reactionType,
                    KpTime = DateTime.Now
                };
                _context.KmsFilePraises.Add(praise);

                if (reactionType == "P") file.KfPraiseNum = (file.KfPraiseNum ?? 0) + 1;
                else if (reactionType == "T") file.KfTreadNum = (file.KfTreadNum ?? 0) + 1;
            }

            await _context.SaveChangesAsync();

            // 回傳結果或重定向視需求調整
            return RedirectToAction("Details", new { id = fileId });
        }


        // ========== 假設取得目前登入者 ID ==========
        private long GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(userIdString, out var userId) ? userId : 0;
        }

        public async Task<IActionResult> Attention(string type = "K")
        {
            var currentUserId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var query = _context.KmsAttentions
                .Where(a => a.KaUserId == currentUserId);

            if (type == "K")
            {
                query = query.Where(a => a.KaType == "K");
            }
            else if (type == "S")
            {
                query = query.Where(a => a.KaType == "S");
            }

            var data = await query
                .Join(_context.KmsKnowledges,
                      att => att.KaAttentionId,
                      kn => kn.KfFileId,
                      (att, kn) => new AttentionViewModel
                      {
                          KfFileId = kn.KfFileId,
                          Title = kn.KfFileName,
                          CreateTime = kn.CreateDate,
                          AttentionTime = att.KaTime,
                          Type = att.KaType
                      })
                .ToListAsync();

            ViewBag.CurrentType = type;
            return View(data);
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAttention(long kfFileId, string type, string returnUrl = null)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            try
            {
                var existingAttention = await _context.KmsAttentions
                    .FirstOrDefaultAsync(a => a.KaUserId == userId && a.KaAttentionId == kfFileId && a.KaType == type);

                if (existingAttention != null)
                {
                    _context.KmsAttentions.Remove(existingAttention);
                }
                else
                {
                    var attention = new KmsAttention
                    {
                        KaUserId = userId,
                        KaAttentionId = kfFileId,
                        KaType = type,
                        KaTime = DateTime.Now
                    };
                    await _context.KmsAttentions.AddAsync(attention);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest("操作失敗：" + ex.Message);
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // 預設 fallback
            return RedirectToAction("Details", new { id = kfFileId });
        }

        private async Task<List<RankingViewModel>> GetRankingData(string sortBy)
        {
            var query = _context.KmsKnowledges
                .Where(k => k.ActiveFlag == "Y" && k.CreateUser != null)
                .GroupBy(k => k.CreateUser)
                .Select(g => new RankingViewModel
                {
                    Nickname = g.Key.UserName,
                    KnowledgeQuantity = g.Count(),
                    FavourableArticles = g.Count(x => (x.KfPraiseNum ?? 0) > (x.KfTreadNum ?? 0)),
                    TotalFavourableReviews = g.Sum(x => x.KfPraiseNum ?? 0),
                    BadArticles = g.Count(x => (x.KfTreadNum ?? 0) > (x.KfPraiseNum ?? 0)),
                    TotalBadReviews = g.Sum(x => x.KfTreadNum ?? 0),
                    TotalVisits = g.Sum(x => x.KfReadNum ?? 0)
                });

            return sortBy switch
            {
                "visit" => await query.OrderByDescending(x => x.TotalVisits).Take(50).ToListAsync(),
                _ => await query.OrderByDescending(x => x.TotalFavourableReviews).Take(50).ToListAsync()
            };
        }

        // 預設排行榜畫面
        public async Task<IActionResult> Ranking()
        {
            var rankings = await GetRankingData("default");
            return View(rankings);
        }

        // 熱門排行畫面 (依據 TotalVisits 排序)
        public async Task<IActionResult> PrintHot()
        {
            var rankings = await GetRankingData("visit");
            return View("RankingPrint", rankings); // 共用列印 View
        }

        // 目前排序列印 (依據 TotalFavourableReviews 排序)
        public async Task<IActionResult> PrintRanking()
        {
            var rankings = await GetRankingData("default");
            return View("RankingPrint", rankings); // 共用列印 View
        }

        // 匯出為 CSV
        public async Task<IActionResult> ExportCsv(string sortBy = "default")
        {
            var rankings = await GetRankingData(sortBy);

            var csv = new StringBuilder();
            csv.AppendLine("Nickname,KnowledgeQuantity,FavourableArticles,TotalFavourableReviews,BadArticles,TotalBadReviews,TotalVisits");

            foreach (var r in rankings)
            {
                csv.AppendLine($"{r.Nickname},{r.KnowledgeQuantity},{r.FavourableArticles},{r.TotalFavourableReviews},{r.BadArticles},{r.TotalBadReviews},{r.TotalVisits}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", "ranking.csv");
        }

        public IActionResult Search(string keyword)
        {
            var model = new KnowledgeViewModel
            {
                Keyword = keyword,
                SearchResults = string.IsNullOrWhiteSpace(keyword)
                    ? new List<KmsKnowledge>()
                    : _context.KmsKnowledges
                        .Where(k => k.KfFileName.Contains(keyword))
                        .OrderByDescending(k => k.TxDate)
                        .ToList()
            };

            if (!model.SearchResults.Any())
            {
                ViewBag.Message = "查無相關知識資料。";
            }

            return View(model); // 傳給 Search.cshtml
        }

        public IActionResult Comment()
        {
            var comments = _context.KmsFileComments
                .Include(c => c.User)
                .Where(c => c.ActiveFlag == "Y")
                .OrderByDescending(c => c.KcTime)
                .ToList();

            return View(comments);
        }

        public async Task<IActionResult> AdvancedSearch()
        {
            var model = new KnowledgeViewModel
            {
                Categories = await GetCategorySelectList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AdvancedSearch(KnowledgeViewModel model)
        {
            var query = _context.KmsKnowledges.AsQueryable();

            if (!string.IsNullOrWhiteSpace(model.Keyword))
            {
                query = query.Where(k => k.KfFileName.Contains(model.Keyword));
            }

            if (model.SelectedCategoryId.HasValue && model.SelectedCategoryId.Value != 0)
            {
                query = query.Where(k => k.KfFileTypeId == model.SelectedCategoryId.Value);
            }

            model.SearchResults = await query
                .OrderByDescending(k => k.TxDate)
                .ToListAsync();

            model.Categories = await GetCategorySelectList();

            return View(model);
        }

        private async Task<List<SelectListItem>> GetCategorySelectList()
        {
            var types = await _context.KmsTypes.ToListAsync();

            // 取所有父節點（ParentId == null），並依 KtId 排序
            var parents = types.Where(t => t.ParentId == null || t.ParentId == 0)
                   .OrderBy(t => t.KtId)
                   .ToList();

            var list = new List<SelectListItem>();

            foreach (var parent in parents)
            {
                // 加入父節點
                list.Add(new SelectListItem
                {
                    Value = parent.KtId.ToString(),
                    Text = parent.KtNameTw
                });

                // 找父節點的子節點，依 SeqNo 排序
                var children = types.Where(t => t.ParentId == parent.KtId)
                                    .OrderBy(t => t.SeqNo ?? int.MaxValue)
                                    .ToList();

                foreach (var child in children)
                {
                    list.Add(new SelectListItem
                    {
                        Value = child.KtId.ToString(),
                        Text = "　└─ " + child.KtNameTw  // 全形空白縮排
                    });
                }
            }

            // 預設第一項
            list.Insert(0, new SelectListItem { Value = "0", Text = "所有類別" });

            return list;
        }

        public IActionResult More()
        {
            var data = _context.KmsKnowledges
                .Where(k => k.KfStatus == 3 && k.ActiveFlag == "Y")
                .Select(k => new KnowledgeSummaryViewModel
                {
                    KfFileId = k.KfFileId,
                    KfFileName = k.KfFileName,
                    KfBrief = k.KfBrief,
                    KfReadNum = k.KfReadNum ?? 0,
                    KfPraiseNum = k.KfPraiseNum ?? 0,
                    KfTreadNum = k.KfTreadNum ?? 0,
                    KfStatus = k.KfStatus,
                    TxDate = k.TxDate,
                    KfTypeName = k.KfFileType.KtNameTw, // 導覽屬性
                    CommentCount = k.KmsFileComments.Count()
                })
                .OrderByDescending(k => k.TxDate)
                .ToList();

            return View(data);
        }

    }

}
