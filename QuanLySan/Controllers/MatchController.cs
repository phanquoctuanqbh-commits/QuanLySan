using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLySan.Data;
using QuanLySan.Models;
using System.Security.Claims;

public class MatchController : Controller
{
    private readonly ApplicationDbContext _context;

    public MatchController(ApplicationDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(int? sportTypeId, DateTime? date, string? skillLevel)
    {
        var today = DateTime.Today;
        var query = _context.MatchPosts
            .Include(x => x.SportType)
            .Include(x => x.Court)
            .Include(x => x.Applications)
            .Where(x => x.Status == "Open" && x.Date.Date >= today)
            .AsQueryable();

        if (sportTypeId.HasValue)
        {
            query = query.Where(x => x.SportTypeId == sportTypeId.Value);
        }

        if (date.HasValue)
        {
            query = query.Where(x => x.Date.Date == date.Value.Date);
        }

        if (!string.IsNullOrWhiteSpace(skillLevel))
        {
            query = query.Where(x => x.SkillLevel == skillLevel);
        }

        ViewBag.SportTypes = await _context.LoaiSans.OrderBy(x => x.TenLoai).ToListAsync();
        ViewBag.SportTypeId = sportTypeId;
        ViewBag.Date = date;
        ViewBag.SkillLevel = skillLevel;

        var posts = await query
            .OrderBy(x => x.Date)
            .ThenBy(x => x.StartTime)
            .ToListAsync();

        ViewBag.Suggestions = await GetSuggestionsAsync(posts);
        return View(posts);
    }

    [Authorize]
    public async Task<IActionResult> Create()
    {
        await LoadFormDataAsync();
        return View(new MatchPost
        {
            Date = DateTime.Today,
            StartTime = new TimeSpan(18, 0, 0),
            EndTime = new TimeSpan(19, 0, 0),
            NeededPlayers = 1
        });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MatchPost model, IFormFile? image)
    {
        if (model.EndTime <= model.StartTime)
        {
            ModelState.AddModelError(nameof(model.EndTime), "Giờ kết thúc phải lớn hơn giờ bắt đầu.");
        }

        if (model.Date.Date < DateTime.Today)
        {
            ModelState.AddModelError(nameof(model.Date), "Ngày chơi không được ở quá khứ.");
        }

        if (model.NeededPlayers < 1)
        {
            ModelState.AddModelError(nameof(model.NeededPlayers), "Số người cần tìm phải từ 1 trở lên.");
        }

        if (!ModelState.IsValid)
        {
            await LoadFormDataAsync();
            return View(model);
        }

        model.UserId = GetUserId();
        model.UserName = User.Identity?.Name ?? string.Empty;
        model.CourtType = await _context.LoaiSans
            .Where(x => x.Id == model.SportTypeId)
            .Select(x => x.TenLoai)
            .FirstOrDefaultAsync() ?? string.Empty;
        model.Status = "Open";
        model.ImageUrl = await SaveImageAsync(image) ?? string.Empty;

        _context.MatchPosts.Add(model);
        await _context.SaveChangesAsync();
        AddNotification(model.UserId, "Đã đăng bài tìm người chơi", $"Bài tìm người chơi {model.CourtType} đã được mở.", Url.Action(nameof(Details), "Match", new { id = model.Id }) ?? string.Empty);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Đã đăng bài tìm đối thủ/chơi cùng.";
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var post = await _context.MatchPosts
            .Include(x => x.SportType)
            .Include(x => x.Court)
            .Include(x => x.Applications)
            .Include(x => x.PlayingGroup)
            .ThenInclude(x => x!.Members)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (post == null)
        {
            return NotFound();
        }

        ViewBag.CurrentUserId = GetUserId();
        ViewBag.UserRatings = await GetUserRatingsAsync(post.Applications.Select(x => x.ApplicantUserId).Append(post.UserId).Distinct());
        return View(post);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(int id, string? message, string? skillLevel)
    {
        var userId = GetUserId();
        var post = await _context.MatchPosts.FirstOrDefaultAsync(x => x.Id == id && x.Status == "Open");
        if (post == null)
        {
            return NotFound();
        }

        if (post.UserId == userId)
        {
            TempData["Error"] = "Bạn không thể ứng tuyển vào bài của chính mình.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var existed = await _context.MatchApplications.AnyAsync(x => x.MatchPostId == id && x.ApplicantUserId == userId);
        if (existed)
        {
            TempData["Error"] = "Bạn đã ứng tuyển bài này.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var rating = await GetUserRatingAsync(userId);
        _context.MatchApplications.Add(new MatchApplication
        {
            MatchPostId = id,
            ApplicantUserId = userId,
            ApplicantName = User.Identity?.Name ?? string.Empty,
            Message = message?.Trim() ?? string.Empty,
            SkillLevel = string.IsNullOrWhiteSpace(skillLevel) ? "Basic" : skillLevel,
            PreviousRating = rating
        });

        AddNotification(post.UserId, "Có người ứng tuyển", $"{User.Identity?.Name} đã ứng tuyển bài tìm người chơi của bạn.", Url.Action(nameof(Details), "Match", new { id }) ?? string.Empty);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Đã gửi ứng tuyển.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize]
    public async Task<IActionResult> MyPosts()
    {
        var userId = GetUserId();
        var posts = await _context.MatchPosts
            .Include(x => x.SportType)
            .Include(x => x.Applications)
            .Include(x => x.PlayingGroup)
            .Where(x => x.UserId == userId || x.Applications.Any(a => a.ApplicantUserId == userId))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return View(posts);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Decide(int applicationId, string status)
    {
        if (status is not ("Accepted" or "Rejected"))
        {
            return BadRequest();
        }

        var application = await _context.MatchApplications
            .Include(x => x.MatchPost)
            .ThenInclude(x => x!.PlayingGroup)
            .ThenInclude(x => x!.Members)
            .FirstOrDefaultAsync(x => x.Id == applicationId);

        if (application?.MatchPost == null)
        {
            return NotFound();
        }

        if (application.MatchPost.UserId != GetUserId())
        {
            return Forbid();
        }

        application.Status = status;
        if (status == "Accepted")
        {
            var group = application.MatchPost.PlayingGroup ?? new PlayingGroup { MatchPostId = application.MatchPost.Id };
            if (application.MatchPost.PlayingGroup == null)
            {
                _context.PlayingGroups.Add(group);
            }

            AddGroupMember(group, application.MatchPost.UserId, application.MatchPost.UserName, "Creator");
            AddGroupMember(group, application.ApplicantUserId, application.ApplicantName, "Member");

            var acceptedCount = await _context.MatchApplications.CountAsync(x => x.MatchPostId == application.MatchPostId && x.Status == "Accepted");
            if (acceptedCount + 1 >= application.MatchPost.NeededPlayers)
            {
                application.MatchPost.Status = "Closed";
            }
        }

        AddNotification(application.ApplicantUserId, status == "Accepted" ? "Ứng tuyển được chấp nhận" : "Ứng tuyển bị từ chối", $"Bài {application.MatchPost.CourtType} đã cập nhật trạng thái: {status}.", Url.Action(nameof(Details), "Match", new { id = application.MatchPostId }) ?? string.Empty);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Đã cập nhật ứng tuyển.";
        return RedirectToAction(nameof(Details), new { id = application.MatchPostId });
    }

    [Authorize]
    public async Task<IActionResult> BookGroup(int id)
    {
        var group = await _context.PlayingGroups
            .Include(x => x.MatchPost)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (group?.MatchPost == null)
        {
            return NotFound();
        }

        if (!await IsGroupMemberAsync(id, GetUserId()))
        {
            return Forbid();
        }

        var courtId = group.MatchPost.CourtId ?? await _context.Sans
            .Where(x => x.LoaiSanId == group.MatchPost.SportTypeId && x.TrangThai)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        if (courtId == 0)
        {
            TempData["Error"] = "Chưa có sân phù hợp để đặt.";
            return RedirectToAction(nameof(Details), new { id = group.MatchPostId });
        }

        return RedirectToAction("Create", "DatSan", new
        {
            sanId = courtId,
            ngay = group.MatchPost.Date.ToString("yyyy-MM-dd"),
            gioBatDau = group.MatchPost.StartTime,
            gioKetThuc = group.MatchPost.EndTime
        });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RateMember(int groupId, string targetUserId, double stars, string? comment)
    {
        if (!await IsGroupMemberAsync(groupId, GetUserId()))
        {
            return Forbid();
        }

        stars = Math.Clamp(stars, 1, 5);
        var member = await _context.PlayingGroupMembers.FirstOrDefaultAsync(x => x.PlayingGroupId == groupId && x.UserId == targetUserId);
        if (member == null)
        {
            return NotFound();
        }

        member.RatingReceived = stars;
        member.ReviewComment = comment?.Trim() ?? string.Empty;

        var reviewer = await _context.PlayingGroupMembers.FirstOrDefaultAsync(x => x.PlayingGroupId == groupId && x.UserId == GetUserId());
        if (reviewer != null)
        {
            reviewer.RatingGiven = true;
        }

        AddNotification(targetUserId, "Bạn có đánh giá mới", $"Bạn nhận {stars:0.#} sao từ nhóm chơi.", Url.Action(nameof(MyPosts), "Match") ?? string.Empty);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Đã gửi đánh giá thành viên.";
        return RedirectToAction(nameof(MyPosts));
    }

    private async Task LoadFormDataAsync()
    {
        ViewBag.SportTypes = await _context.LoaiSans.OrderBy(x => x.TenLoai).ToListAsync();
        ViewBag.Courts = await _context.Sans.Include(x => x.LoaiSan).OrderBy(x => x.TenSan).ToListAsync();
    }

    private async Task<List<MatchPost>> GetSuggestionsAsync(List<MatchPost> currentPosts)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return new List<MatchPost>();
        }

        var userId = GetUserId();
        var userPosts = await _context.MatchPosts
            .Where(x => x.UserId == userId || x.Applications.Any(a => a.ApplicantUserId == userId))
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .ToListAsync();

        if (!userPosts.Any())
        {
            return currentPosts.Take(3).ToList();
        }

        var sportIds = userPosts.Select(x => x.SportTypeId).Distinct().ToList();
        var skillLevels = userPosts.Select(x => x.SkillLevel).Distinct().ToList();

        return currentPosts
            .Where(x => x.UserId != userId && sportIds.Contains(x.SportTypeId) && skillLevels.Contains(x.SkillLevel))
            .Take(3)
            .ToList();
    }

    private async Task<Dictionary<string, double>> GetUserRatingsAsync(IEnumerable<string> userIds)
    {
        var ids = userIds.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
        return await _context.PlayingGroupMembers
            .Where(x => ids.Contains(x.UserId) && x.RatingReceived > 0)
            .GroupBy(x => x.UserId)
            .Select(x => new { UserId = x.Key, Rating = x.Average(y => y.RatingReceived) })
            .ToDictionaryAsync(x => x.UserId, x => x.Rating);
    }

    private async Task<double> GetUserRatingAsync(string userId)
    {
        return await _context.PlayingGroupMembers
            .Where(x => x.UserId == userId && x.RatingReceived > 0)
            .Select(x => x.RatingReceived)
            .DefaultIfEmpty(0)
            .AverageAsync();
    }

    private void AddGroupMember(PlayingGroup group, string userId, string userName, string role)
    {
        if (group.Members.Any(x => x.UserId == userId))
        {
            return;
        }

        group.Members.Add(new PlayingGroupMember
        {
            UserId = userId,
            UserName = userName,
            Role = role
        });
    }

    private async Task<bool> IsGroupMemberAsync(int groupId, string userId)
    {
        return await _context.PlayingGroupMembers.AnyAsync(x => x.PlayingGroupId == groupId && x.UserId == userId);
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }

    private void AddNotification(string userId, string title, string content, string url)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        _context.ThongBaos.Add(new ThongBao
        {
            UserId = userId,
            TieuDe = title,
            NoiDung = content,
            LienKet = url
        });
    }

    private static async Task<string?> SaveImageAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return null;
        }

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(extension))
        {
            return null;
        }

        var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "match");
        Directory.CreateDirectory(folder);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(folder, fileName);

        await using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);
        return "/images/match/" + fileName;
    }
}
