using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLySan.Data;
using QuanLySan.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QuanLySan.Controllers
{
    [Authorize(Roles = "User")]
    public class GiaoLuuController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public GiaoLuuController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? loaiSan)
        {
            var userId = GetUserId();
            var query = _context.GiaoLuus.AsQueryable();

            if (!string.IsNullOrEmpty(loaiSan))
            {
                query = query.Where(x => x.LoaiSan == loaiSan);
            }

            var list = await query.OrderByDescending(x => x.NgayDang).ToListAsync();

            // Load contact details mapping for accepted matches
            var userIds = list.SelectMany(x => new[] { x.NguoiDangId, x.NguoiGiaoLuuId })
                              .Where(id => !string.IsNullOrEmpty(id))
                              .Distinct()
                              .ToList();

            var users = await _userManager.Users
                                          .Where(x => userIds.Contains(x.Id))
                                          .ToDictionaryAsync(x => x.Id, x => new { x.Email, x.PhoneNumber });

            ViewBag.Users = users;
            ViewBag.CurrentUserId = userId;
            ViewBag.SelectedLoaiSan = loaiSan;

            return View(list);
        }

        public IActionResult Create()
        {
            return View(new GiaoLuu { NgayChoi = DateTime.Today.AddDays(1) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GiaoLuu model)
        {
            if (string.IsNullOrWhiteSpace(model.TieuDe) || string.IsNullOrWhiteSpace(model.NoiDung))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ tiêu đề và nội dung.");
                return View(model);
            }

            var userId = GetUserId();
            var wallet = await _context.TaiKhoanNguoiDungs.FirstOrDefaultAsync(x => x.UserId == userId);
            
            model.NguoiDangId = userId;
            model.NguoiDangName = wallet != null && !string.IsNullOrEmpty(wallet.HoTen) ? wallet.HoTen : (User.Identity?.Name ?? "Thành viên");
            model.NgayDang = DateTime.Now;
            model.TrangThai = "Pending";
            model.NguoiGiaoLuuId = null;
            model.NguoiGiaoLuuName = null;

            _context.GiaoLuus.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã đăng tin tìm đối thủ giao lưu thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DongYGiaoLuu(int id)
        {
            var post = await _context.GiaoLuus.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            var userId = GetUserId();
            if (post.NguoiDangId == userId)
            {
                TempData["Error"] = "Bạn không thể tự giao lưu với tin đăng của chính mình.";
                return RedirectToAction(nameof(Index));
            }

            if (post.TrangThai != "Pending")
            {
                TempData["Error"] = "Tin đăng này đã có người đồng ý giao lưu trước đó.";
                return RedirectToAction(nameof(Index));
            }

            var wallet = await _context.TaiKhoanNguoiDungs.FirstOrDefaultAsync(x => x.UserId == userId);
            post.NguoiGiaoLuuId = userId;
            post.NguoiGiaoLuuName = wallet != null && !string.IsNullOrEmpty(wallet.HoTen) ? wallet.HoTen : (User.Identity?.Name ?? "Thành viên");
            post.TrangThai = "Accepted";

            // Add notifications for both users
            var identityUserDang = await _userManager.FindByIdAsync(post.NguoiDangId);
            var phoneDang = identityUserDang?.PhoneNumber ?? "Chưa cập nhật";
            var emailDang = identityUserDang?.Email ?? "Chưa cập nhật";

            var identityUserGiaoLuu = await _userManager.FindByIdAsync(userId);
            var phoneGiaoLuu = identityUserGiaoLuu?.PhoneNumber ?? "Chưa cập nhật";
            var emailGiaoLuu = identityUserGiaoLuu?.Email ?? "Chưa cập nhật";

            _context.ThongBaos.Add(new ThongBao
            {
                UserId = post.NguoiDangId,
                TieuDe = "Đối thủ đồng ý giao lưu!",
                NoiDung = $"Thành viên {post.NguoiGiaoLuuName} đã đồng ý giao lưu với bạn ở tin: {post.TieuDe}. Liên hệ: SĐT {phoneGiaoLuu}, Email {emailGiaoLuu}.",
                LienKet = Url.Action(nameof(Index)) ?? string.Empty
            });

            _context.ThongBaos.Add(new ThongBao
            {
                UserId = userId,
                TieuDe = "Giao lưu thành công!",
                NoiDung = $"Bạn đã kết nối giao lưu thành công với {post.NguoiDangName} ở tin: {post.TieuDe}. Liên hệ: SĐT {phoneDang}, Email {emailDang}.",
                LienKet = Url.Action(nameof(Index)) ?? string.Empty
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = "Kết nối thành công! Hai tài khoản đã đồng ý giao lưu. Hãy xem thông tin liên hệ và chuông thông báo.";
            return RedirectToAction(nameof(Index));
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }
    }
}
