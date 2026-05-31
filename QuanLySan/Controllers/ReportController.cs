using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLySan.Data;
using QuanLySan.Models;
using System.Security.Claims;

[Authorize]
public class ReportController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public ReportController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Create(int sanId)
    {
        var court = await _context.Sans.Include(x => x.LoaiSan).FirstOrDefaultAsync(x => x.Id == sanId);
        if (court == null)
        {
            return NotFound();
        }

        ViewBag.Court = court;
        return View(new CourtReport { SanId = sanId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourtReport model)
    {
        var court = await _context.Sans.FirstOrDefaultAsync(x => x.Id == model.SanId);
        if (court == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(model.Reason) || string.IsNullOrWhiteSpace(model.Description))
        {
            ViewBag.Court = court;
            ModelState.AddModelError(string.Empty, "Vui lòng nhập lý do và nội dung tố cáo.");
            return View(model);
        }

        model.UserId = GetUserId();
        model.UserName = User.Identity?.Name ?? string.Empty;
        model.OwnerId = court.OwnerId;
        model.Status = "Pending";
        model.CreatedAt = DateTime.Now;

        _context.CourtReports.Add(model);

        if (!string.IsNullOrWhiteSpace(court.OwnerId))
        {
            AddNotification(court.OwnerId, "Sân bị tố cáo", $"Sân {court.TenSan} vừa nhận một phiếu tố cáo: {model.Reason}.", Url.Action(nameof(Owner), "Report") ?? string.Empty);
        }

        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        foreach (var admin in admins)
        {
            AddNotification(admin.Id, "Phiếu tố cáo mới", $"Người dùng vừa tố cáo sân {court.TenSan}.", Url.Action(nameof(Admin), "Report") ?? string.Empty);
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã gửi tố cáo. Admin sẽ nhận phiếu và chủ sân sẽ nhận thông báo.";
        return RedirectToAction("Details", "San", new { id = court.Id });
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Admin()
    {
        var reports = await _context.CourtReports
            .Include(x => x.San)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return View(reports);
    }

    [Authorize(Roles = "Owner,ChuSan")]
    public async Task<IActionResult> Owner()
    {
        var userId = GetUserId();
        var reports = await _context.CourtReports
            .Include(x => x.San)
            .Where(x => x.OwnerId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return View(reports);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var allowed = new[] { "Pending", "Reviewing", "Resolved", "Rejected" };
        if (!allowed.Contains(status))
        {
            return BadRequest();
        }

        var report = await _context.CourtReports.Include(x => x.San).FirstOrDefaultAsync(x => x.Id == id);
        if (report == null)
        {
            return NotFound();
        }

        report.Status = status;
        report.ResolvedAt = status is "Resolved" or "Rejected" ? DateTime.Now : null;

        if (!string.IsNullOrWhiteSpace(report.UserId))
        {
            AddNotification(report.UserId, "Tố cáo đã cập nhật", $"Phiếu tố cáo sân {report.San?.TenSan} hiện là: {status}.", Url.Action("Details", "San", new { id = report.SanId }) ?? string.Empty);
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật trạng thái tố cáo.";
        return RedirectToAction(nameof(Admin));
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
}
