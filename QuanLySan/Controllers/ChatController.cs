using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLySan.Data;
using QuanLySan.Models;

[Authorize]
public class ChatController : Controller
{
    private readonly ApplicationDbContext _context;

    public ChatController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        if (User.IsInRole("Admin") || User.IsInRole("Owner") || User.IsInRole("ChuSan"))
        {
            return RedirectToAction(nameof(Admin));
        }

        var userId = GetUserId();
        var messages = await _context.ChatMessages
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.NgayGui)
            .ToListAsync();

        var unreadAdminMessages = messages.Where(x => x.GuiBoiAdmin && !x.DaDoc).ToList();
        foreach (var message in unreadAdminMessages)
        {
            message.DaDoc = true;
        }

        if (unreadAdminMessages.Any())
        {
            await _context.SaveChangesAsync();
        }

        return View(messages);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(string noiDung)
    {
        if (User.IsInRole("Admin") || User.IsInRole("Owner") || User.IsInRole("ChuSan"))
        {
            return RedirectToAction(nameof(Admin));
        }

        if (string.IsNullOrWhiteSpace(noiDung))
        {
            TempData["Error"] = "Vui lòng nhập nội dung tin nhắn.";
            return RedirectToAction(nameof(Index));
        }

        _context.ChatMessages.Add(new ChatMessage
        {
            UserId = GetUserId(),
            UserName = User.Identity?.Name ?? "Khách hàng",
            NoiDung = noiDung.Trim(),
            GuiBoiAdmin = false
        });

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Owner,ChuSan")]
    public async Task<IActionResult> Admin(string? userId)
    {
        var messages = await _context.ChatMessages
            .OrderByDescending(x => x.NgayGui)
            .ToListAsync();

        var threads = messages
            .GroupBy(x => x.UserId)
            .Select(x =>
            {
                var last = x.OrderByDescending(y => y.NgayGui).First();
                return new ChatThread(
                    x.Key,
                    last.UserName,
                    last.NoiDung,
                    last.NgayGui,
                    x.Count(y => !y.GuiBoiAdmin && !y.DaDoc));
            })
            .OrderByDescending(x => x.LastTime)
            .ToList();

        userId ??= threads.FirstOrDefault()?.UserId;
        var selectedMessages = string.IsNullOrWhiteSpace(userId)
            ? new List<ChatMessage>()
            : await _context.ChatMessages
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.NgayGui)
                .ToListAsync();

        foreach (var message in selectedMessages.Where(x => !x.GuiBoiAdmin && !x.DaDoc))
        {
            message.DaDoc = true;
        }

        if (selectedMessages.Any(x => !x.GuiBoiAdmin && x.DaDoc))
        {
            await _context.SaveChangesAsync();
        }

        ViewBag.Threads = threads;
        ViewBag.SelectedUserId = userId;
        return View(selectedMessages);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Owner,ChuSan")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendAdmin(string userId, string noiDung)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(noiDung))
        {
            TempData["Error"] = "Vui lòng chọn khách hàng và nhập nội dung phản hồi.";
            return RedirectToAction(nameof(Admin), new { userId });
        }

        var userName = await _context.ChatMessages
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.NgayGui)
            .Select(x => x.UserName)
            .FirstOrDefaultAsync() ?? userId;

        _context.ChatMessages.Add(new ChatMessage
        {
            UserId = userId,
            UserName = userName,
            NoiDung = noiDung.Trim(),
            GuiBoiAdmin = true
        });

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Admin), new { userId });
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }
}
