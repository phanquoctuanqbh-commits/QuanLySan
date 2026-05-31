using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLySan.Data;
using System.Security.Claims;

[Authorize]
public class NotificationController : Controller
{
    private readonly ApplicationDbContext _context;

    public NotificationController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var notifications = await _context.ThongBaos
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.NgayTao)
            .ToListAsync();

        return View(notifications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var notification = await _context.ThongBaos.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (notification == null)
        {
            return NotFound();
        }

        notification.DaDoc = true;
        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(notification.LienKet) && !notification.LienKet.StartsWith("booking-reminder:", StringComparison.OrdinalIgnoreCase))
        {
            return Redirect(notification.LienKet);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var notifications = await _context.ThongBaos.Where(x => x.UserId == userId && !x.DaDoc).ToListAsync();
        foreach (var item in notifications)
        {
            item.DaDoc = true;
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
