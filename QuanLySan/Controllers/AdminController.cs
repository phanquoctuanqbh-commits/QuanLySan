using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLySan.Data;
using QuanLySan.Models;
using System.Security.Claims;

[Authorize]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private const string TrangThaiDaThanhToan = "Paid";
    private const string TrangThaiDaXacNhan = "Confirmed";
    private const string TrangThaiDaHuy = "Cancelled";

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        ViewBag.TongSan = await _context.Sans.CountAsync();
        ViewBag.SanDangMo = await _context.Sans.CountAsync(x => x.TrangThai);
        ViewBag.TongDon = await _context.DatSans.CountAsync();
        ViewBag.DonChoDuyet = await _context.DatSans.CountAsync(x => x.TrangThai == TrangThaiDaThanhToan);
        ViewBag.DoanhThu = await _context.DatSans
            .Where(x => x.TrangThai == TrangThaiDaXacNhan || x.TrangThai == TrangThaiDaThanhToan)
            .SumAsync(x => x.TongTien);

        var fromDate = DateTime.Today.AddDays(-6);
        var revenueRaw = await _context.DatSans
            .Where(x => x.Ngay.Date >= fromDate && (x.TrangThai == TrangThaiDaXacNhan || x.TrangThai == TrangThaiDaThanhToan))
            .GroupBy(x => x.Ngay.Date)
            .Select(x => new { Ngay = x.Key, DoanhThu = x.Sum(y => y.TongTien), SoDon = x.Count() })
            .ToListAsync();

        ViewBag.RevenueChart = Enumerable.Range(0, 7)
            .Select(i =>
            {
                var date = fromDate.AddDays(i);
                var row = revenueRaw.FirstOrDefault(x => x.Ngay == date);
                return new DashboardBar(date.ToString("dd/MM"), row?.DoanhThu ?? 0, row?.SoDon ?? 0);
            })
            .ToList();

        var topRaw = await _context.DatSans
            .Include(x => x.San)
            .Where(x => x.TrangThai != TrangThaiDaHuy)
            .ToListAsync();

        ViewBag.TopSans = topRaw
            .GroupBy(x => x.San?.TenSan ?? "Sân")
            .Select(x => new DashboardBar(x.Key, x.Sum(y => y.TongTien), x.Count()))
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToList();

        var hourRaw = await _context.DatSans
            .Where(x => x.TrangThai != TrangThaiDaHuy)
            .Select(x => x.GioBatDau)
            .ToListAsync();

        var hourRows = hourRaw
            .GroupBy(x => x.Hours)
            .Select(x => new DashboardBar(x.Key + ":00", 0, x.Count()))
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToList();
        ViewBag.PeakHours = hourRows;

        var bookingsRaw = await _context.DatSans
            .Include(x => x.San)
            .OrderByDescending(x => x.Ngay)
            .Take(30)
            .ToListAsync();

        var bookings = bookingsRaw
            .OrderByDescending(x => x.Ngay)
            .ThenByDescending(x => x.GioBatDau)
            .ToList();

        return View(bookings);
    }

    [Authorize(Roles = "Admin,Owner,ChuSan")]
    public async Task<IActionResult> LichSuDatSan()
    {
        var dataQuery = _context.DatSans
            .Include(x => x.San)
            .ThenInclude(x => x!.LoaiSan)
            .AsQueryable();

        if (IsOwnerOnly())
        {
            var userId = GetUserId();
            dataQuery = dataQuery.Where(x => x.San != null && x.San.OwnerId == userId);
        }

        var dataRaw = await dataQuery.OrderByDescending(x => x.Ngay).ToListAsync();

        var data = dataRaw
            .OrderByDescending(x => x.Ngay)
            .ThenByDescending(x => x.GioBatDau)
            .ToList();

        return View(data);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> LichSuGiaoDich()
    {
        var data = await _context.GiaoDichs
            .Include(x => x.DatSan)
            .ThenInclude(x => x!.San)
            .OrderByDescending(x => x.NgayGiaoDich)
            .ToListAsync();

        return View(data);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> YeuCauNapTien()
    {
        var data = await _context.YeuCauNapTiens
            .OrderByDescending(x => x.NgayGui)
            .ToListAsync();

        return View(data);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Vouchers()
    {
        var vouchers = await _context.Vouchers
            .OrderByDescending(x => x.TrangThai)
            .ThenBy(x => x.Ma)
            .ToListAsync();

        return View(vouchers);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateVoucher(Voucher voucher)
    {
        voucher.Ma = voucher.Ma.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(voucher.Ma) || voucher.GiaTri <= 0)
        {
            TempData["Error"] = "Vui lòng nhập mã voucher và giá trị giảm hợp lệ.";
            return RedirectToAction(nameof(Vouchers));
        }

        if (await _context.Vouchers.AnyAsync(x => x.Ma == voucher.Ma))
        {
            TempData["Error"] = "Mã voucher này đã tồn tại.";
            return RedirectToAction(nameof(Vouchers));
        }

        _context.Vouchers.Add(voucher);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã tạo voucher mới.";
        return RedirectToAction(nameof(Vouchers));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleVoucher(int id)
    {
        var voucher = await _context.Vouchers.FindAsync(id);
        if (voucher == null)
        {
            return NotFound();
        }

        voucher.TrangThai = !voucher.TrangThai;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật trạng thái voucher.";
        return RedirectToAction(nameof(Vouchers));
    }

    [Authorize(Roles = "Admin,Owner,ChuSan")]
    public async Task<IActionResult> BaoTri()
    {
        var userId = GetUserId();
        var courtQuery = _context.Sans.AsQueryable();
        if (IsOwnerOnly())
        {
            courtQuery = courtQuery.Where(x => x.OwnerId == userId);
        }

        ViewBag.Sans = await courtQuery.OrderBy(x => x.TenSan).ToListAsync();

        var dataQuery = _context.BaoTriSans
            .Include(x => x.San)
            .AsQueryable();
        if (IsOwnerOnly())
        {
            dataQuery = dataQuery.Where(x => x.San != null && x.San.OwnerId == userId);
        }

        var data = await dataQuery.OrderByDescending(x => x.Ngay).ThenBy(x => x.GioBatDau).ToListAsync();

        return View(data);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Owner,ChuSan")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBaoTri(BaoTriSan model)
    {
        if (model.GioKetThuc <= model.GioBatDau)
        {
            TempData["Error"] = "Giờ kết thúc bảo trì phải lớn hơn giờ bắt đầu.";
            return RedirectToAction(nameof(BaoTri));
        }

        if (IsOwnerOnly() && !await _context.Sans.AnyAsync(x => x.Id == model.SanId && x.OwnerId == GetUserId()))
        {
            return Forbid();
        }

        var hasBooking = await _context.DatSans.AnyAsync(x =>
            x.SanId == model.SanId &&
            x.Ngay.Date == model.Ngay.Date &&
            x.TrangThai != TrangThaiDaHuy &&
            model.GioBatDau < x.GioKetThuc &&
            model.GioKetThuc > x.GioBatDau);

        if (hasBooking)
        {
            TempData["Error"] = "Khung giờ này đang có đơn đặt. Hãy hủy/xử lý đơn trước khi tạo bảo trì.";
            return RedirectToAction(nameof(BaoTri));
        }

        model.Ngay = model.Ngay.Date;
        _context.BaoTriSans.Add(model);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã thêm lịch bảo trì sân.";
        return RedirectToAction(nameof(BaoTri));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Owner,ChuSan")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBaoTri(int id)
    {
        var item = await _context.BaoTriSans.FindAsync(id);
        if (item == null)
        {
            return NotFound();
        }

        if (IsOwnerOnly() && !await _context.Sans.AnyAsync(x => x.Id == item.SanId && x.OwnerId == GetUserId()))
        {
            return Forbid();
        }

        item.TrangThai = !item.TrangThai;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật trạng thái bảo trì.";
        return RedirectToAction(nameof(BaoTri));
    }

    [Authorize(Roles = "Admin,Owner,ChuSan")]
    public IActionResult XacNhanQR()
    {
        return View();
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SiteSettings()
    {
        var settings = await GetOrCreateSiteSettingsAsync();
        return View(settings);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SiteSettings(SiteSetting model)
    {
        var settings = await GetOrCreateSiteSettingsAsync();
        settings.BusinessName = model.BusinessName?.Trim() ?? string.Empty;
        settings.Address = model.Address?.Trim() ?? string.Empty;
        settings.PhoneNumber = model.PhoneNumber?.Trim() ?? string.Empty;
        settings.Email = model.Email?.Trim() ?? string.Empty;
        settings.OpeningHours = model.OpeningHours?.Trim() ?? string.Empty;
        settings.MapEmbedUrl = model.MapEmbedUrl?.Trim() ?? string.Empty;
        settings.Description = model.Description?.Trim() ?? string.Empty;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật thông tin website trong database.";
        return RedirectToAction(nameof(SiteSettings));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Owner,ChuSan")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XacNhanQR(string maXacNhan)
    {
        maXacNhan = maXacNhan?.Trim().ToUpperInvariant() ?? string.Empty;
        var booking = await _context.DatSans
            .Include(x => x.San)
            .FirstOrDefaultAsync(x => x.MaXacNhan == maXacNhan);

        if (booking == null)
        {
            TempData["Error"] = "Không tìm thấy đơn với mã xác nhận này.";
            return View();
        }

        if (IsOwnerOnly() && booking.San?.OwnerId != GetUserId())
        {
            return Forbid();
        }

        if (booking.TrangThai == TrangThaiDaHuy)
        {
            TempData["Error"] = "Đơn này đã hủy, không thể xác nhận.";
            return View(booking);
        }

        booking.TrangThai = TrangThaiDaXacNhan;
        TaoThongBao(booking.UserId, "Đơn đã được xác nhận tại sân", $"Mã {booking.MaXacNhan} đã được quản trị xác nhận.", string.Empty);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã xác nhận đơn đặt sân bằng mã QR.";
        return View(booking);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var allowed = new[] { TrangThaiDaThanhToan, TrangThaiDaXacNhan, TrangThaiDaHuy };
        if (!allowed.Contains(status))
        {
            return BadRequest();
        }

        var booking = await _context.DatSans.FindAsync(id);
        if (booking == null)
        {
            return NotFound();
        }

        if (status == TrangThaiDaHuy && booking.TrangThai != TrangThaiDaHuy)
        {
            await HoanTienNeuCanAsync(booking);
        }

        booking.TrangThai = status;
        TaoThongBao(booking.UserId, "Trạng thái đơn đã cập nhật", $"Đơn {booking.MaXacNhan} hiện là: {status}.", string.Empty);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật trạng thái đơn.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XuLyNapTien(int id, string ketQua)
    {
        var yeuCau = await _context.YeuCauNapTiens.FindAsync(id);
        if (yeuCau == null)
        {
            return NotFound();
        }

        if (yeuCau.TrangThai != "Pending")
        {
            TempData["Error"] = "Yêu cầu này đã được xử lý.";
            return RedirectToAction(nameof(YeuCauNapTien));
        }

        if (ketQua == "Approved")
        {
            var taiKhoan = await _context.TaiKhoanNguoiDungs.FirstOrDefaultAsync(x => x.UserId == yeuCau.UserId);
            if (taiKhoan == null)
            {
                taiKhoan = new TaiKhoanNguoiDung
                {
                    UserId = yeuCau.UserId,
                    SoDu = 0
                };
                _context.TaiKhoanNguoiDungs.Add(taiKhoan);
                await _context.SaveChangesAsync();
            }

            taiKhoan.SoDu += yeuCau.SoTien;
            yeuCau.TrangThai = "Approved";
            yeuCau.NgayXuLy = DateTime.Now;

            _context.GiaoDichs.Add(new GiaoDich
            {
                UserId = yeuCau.UserId,
                TaiKhoanNguoiDungId = taiKhoan.Id,
                LoaiGiaoDich = "TopUp",
                SoTien = yeuCau.SoTien,
                SoDuSauGiaoDich = taiKhoan.SoDu,
                NoiDung = $"Nạp tiền qua {yeuCau.PhuongThuc} - {yeuCau.MaThamChieu}"
            });
            TaoThongBao(yeuCau.UserId, "Nạp tiền thành công", $"Ví của bạn đã được cộng {yeuCau.SoTien:N0} VND.", string.Empty);

            TempData["Success"] = "Đã duyệt yêu cầu và cộng tiền vào tài khoản.";
        }
        else
        {
            yeuCau.TrangThai = "Rejected";
            yeuCau.NgayXuLy = DateTime.Now;
            TaoThongBao(yeuCau.UserId, "Yêu cầu nạp tiền bị từ chối", "Quản trị viên đã từ chối yêu cầu nạp tiền của bạn.", string.Empty);
            TempData["Success"] = "Đã từ chối yêu cầu nạp tiền.";
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(YeuCauNapTien));
    }

    private async Task HoanTienNeuCanAsync(DatSan booking)
    {
        var daHoanTien = await _context.GiaoDichs.AnyAsync(x =>
            x.DatSanId == booking.Id &&
            x.UserId == booking.UserId &&
            x.LoaiGiaoDich == "Refund");

        if (daHoanTien || booking.TongTien <= 0)
        {
            return;
        }

        var taiKhoan = await _context.TaiKhoanNguoiDungs.FirstOrDefaultAsync(x => x.UserId == booking.UserId);
        if (taiKhoan == null)
        {
            taiKhoan = new TaiKhoanNguoiDung
            {
                UserId = booking.UserId,
                SoDu = 0
            };
            _context.TaiKhoanNguoiDungs.Add(taiKhoan);
            await _context.SaveChangesAsync();
        }

        taiKhoan.SoDu += booking.TongTien;
        _context.GiaoDichs.Add(new GiaoDich
        {
            UserId = booking.UserId,
            TaiKhoanNguoiDungId = taiKhoan.Id,
            DatSanId = booking.Id,
            LoaiGiaoDich = "Refund",
            SoTien = booking.TongTien,
            SoDuSauGiaoDich = taiKhoan.SoDu,
            NoiDung = $"Hoàn tiền do quản trị hủy đơn #{booking.Id}"
        });
    }

    private void TaoThongBao(string userId, string tieuDe, string noiDung, string lienKet)
    {
        _context.ThongBaos.Add(new ThongBao
        {
            UserId = userId,
            TieuDe = tieuDe,
            NoiDung = noiDung,
            LienKet = lienKet
        });
    }

    private bool IsOwnerOnly()
    {
        return !User.IsInRole("Admin") && (User.IsInRole("Owner") || User.IsInRole("ChuSan"));
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }

    private async Task<SiteSetting> GetOrCreateSiteSettingsAsync()
    {
        var settings = await _context.SiteSettings.FirstOrDefaultAsync();
        if (settings != null)
        {
            return settings;
        }

        settings = new SiteSetting();
        _context.SiteSettings.Add(settings);
        await _context.SaveChangesAsync();
        return settings;
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> NguoiDung()
    {
        var users = await _context.Users.ToListAsync();
        var userRoles = await _context.UserRoles.ToListAsync();
        var roles = await _context.Roles.ToListAsync();
        var wallets = await _context.TaiKhoanNguoiDungs.ToListAsync();

        var userList = (from u in users
                        join ur in userRoles on u.Id equals ur.UserId into urGroup
                        from ur in urGroup.DefaultIfEmpty()
                        join r in roles on (ur != null ? ur.RoleId : null) equals r.Id into rGroup
                        from r in rGroup.DefaultIfEmpty()
                        join w in wallets on u.Id equals w.UserId into wGroup
                        from w in wGroup.DefaultIfEmpty()
                        select new UserManagementViewModel
                        {
                            UserId = u.Id,
                            Email = u.Email ?? "Chưa cập nhật",
                            PhoneNumber = u.PhoneNumber ?? "Chưa cập nhật",
                            RoleName = r != null ? (r.Name == "ChuSan" ? "Chủ sân" : (r.Name == "Admin" ? "Quản trị viên" : "Khách hàng")) : "Khách hàng",
                            HoTen = w != null && !string.IsNullOrEmpty(w.HoTen) ? w.HoTen : "Chưa cập nhật",
                            SoDu = w != null ? w.SoDu : 0,
                            CanhCaoDen = w != null ? w.CanhCaoDen : null,
                            LyDoCanhCao = w != null ? w.LyDoCanhCao : null
                        }).ToList();

        return View(userList);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> XoaNguoiDung(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            var wallet = await _context.TaiKhoanNguoiDungs.FirstOrDefaultAsync(x => x.UserId == id);
            if (wallet != null)
            {
                _context.TaiKhoanNguoiDungs.Remove(wallet);
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa người dùng thành công khỏi hệ thống.";
        }
        else
        {
            TempData["Error"] = "Không tìm thấy người dùng.";
        }

        return RedirectToAction(nameof(NguoiDung));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CanhCaoNguoiDung(string userId, int soNgay, string lyDo)
    {
        var wallet = await _context.TaiKhoanNguoiDungs.FirstOrDefaultAsync(x => x.UserId == userId);
        if (wallet != null)
        {
            if (soNgay <= 0)
            {
                wallet.CanhCaoDen = null;
                wallet.LyDoCanhCao = null;
                TaoThongBao(userId, "Gỡ cảnh cáo tài khoản", "Tài khoản của bạn đã được gỡ cảnh cáo. Bạn có thể sử dụng tất cả tính năng bình thường.", string.Empty);
                TempData["Success"] = "Đã gỡ cảnh cáo cho thành viên.";
            }
            else
            {
                wallet.CanhCaoDen = DateTime.Now.AddDays(soNgay);
                wallet.LyDoCanhCao = lyDo?.Trim() ?? "Cảnh cáo từ quản trị viên";
                TaoThongBao(userId, "Tài khoản bị cảnh cáo", $"Tài khoản bị cảnh cáo đến {wallet.CanhCaoDen.Value:dd/MM/yyyy HH:mm} vì lý do: {wallet.LyDoCanhCao}.", string.Empty);
                TempData["Success"] = $"Đã cảnh cáo thành viên {soNgay} ngày.";
            }
            await _context.SaveChangesAsync();
        }
        else
        {
            TempData["Error"] = "Không tìm thấy thông tin ví thành viên.";
        }
        return RedirectToAction(nameof(NguoiDung));
    }
}
