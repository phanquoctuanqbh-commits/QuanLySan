using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLySan.Data;
using QuanLySan.Models;

[Authorize]
public class DatSanController : Controller
{
    private readonly ApplicationDbContext _context;
    private const string TrangThaiDaHuy = "Cancelled";
    private const string TrangThaiDaThanhToan = "Paid";

    public DatSanController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Create(int sanId, DateTime? ngay, TimeSpan? gioBatDau, TimeSpan? gioKetThuc)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var taiKhoan = await GetOrCreateTaiKhoanAsync(userId);
        if (taiKhoan.CanhCaoDen.HasValue && taiKhoan.CanhCaoDen.Value > DateTime.Now)
        {
            TempData["Error"] = $"Tài khoản của bạn đang bị cảnh cáo đến {taiKhoan.CanhCaoDen.Value:dd/MM/yyyy HH:mm} vì lý do: {taiKhoan.LyDoCanhCao}. Bạn không thể thực hiện đặt sân.";
            return RedirectToAction("Details", "San", new { id = sanId });
        }

        var san = await _context.Sans.Include(x => x.LoaiSan).FirstOrDefaultAsync(x => x.Id == sanId && x.TrangThai);
        if (san == null)
        {
            return NotFound();
        }

        var bookingDate = ngay ?? DateTime.Today;
        ViewBag.San = san;
        ViewBag.SoDu = taiKhoan.SoDu;
        await LoadBookingViewDataAsync(sanId, bookingDate);
        return View(new DatSan
        {
            SanId = sanId,
            Ngay = bookingDate,
            GioBatDau = gioBatDau ?? new TimeSpan(7, 0, 0),
            GioKetThuc = gioKetThuc ?? new TimeSpan(8, 0, 0)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DatSan model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var taiKhoan = await GetOrCreateTaiKhoanAsync(userId);
        if (taiKhoan.CanhCaoDen.HasValue && taiKhoan.CanhCaoDen.Value > DateTime.Now)
        {
            TempData["Error"] = $"Tài khoản của bạn đang bị cảnh cáo đến {taiKhoan.CanhCaoDen.Value:dd/MM/yyyy HH:mm} vì lý do: {taiKhoan.LyDoCanhCao}. Bạn không thể thực hiện đặt sân.";
            return RedirectToAction("Details", "San", new { id = model.SanId });
        }

        var san = await _context.Sans.FirstOrDefaultAsync(x => x.Id == model.SanId && x.TrangThai);
        if (san == null)
        {
            return NotFound();
        }
        var soGio = (decimal)(model.GioKetThuc - model.GioBatDau).TotalHours;
        var tongTienGoc = soGio > 0 ? soGio * san.Gia : 0;
        var (voucher, giamGia, voucherError) = await TinhGiamGiaAsync(model.MaVoucher, tongTienGoc);
        var tongTien = Math.Max(0, tongTienGoc - giamGia);

        ViewBag.San = san;
        ViewBag.SoDu = taiKhoan.SoDu;
        await LoadBookingViewDataAsync(model.SanId, model.Ngay);

        if (model.Ngay.Date < DateTime.Today)
        {
            ModelState.AddModelError(nameof(model.Ngay), "Không thể đặt ngày trong quá khứ.");
        }

        if (model.GioKetThuc <= model.GioBatDau)
        {
            ModelState.AddModelError(nameof(model.GioKetThuc), "Giờ kết thúc phải lớn hơn giờ bắt đầu.");
        }

        var bookingsInDay = await _context.DatSans.Where(x =>
            x.SanId == model.SanId &&
            x.Ngay.Date == model.Ngay.Date &&
            x.TrangThai != TrangThaiDaHuy)
            .ToListAsync();

        var maintenanceInDay = await _context.BaoTriSans
            .Where(x => x.SanId == model.SanId && x.TrangThai && x.Ngay.Date == model.Ngay.Date)
            .ToListAsync();

        var trung = bookingsInDay.Any(x =>
            model.GioBatDau < x.GioKetThuc &&
            model.GioKetThuc > x.GioBatDau);

        var trungBaoTri = maintenanceInDay.Any(x =>
            model.GioBatDau < x.GioKetThuc &&
            model.GioKetThuc > x.GioBatDau);

        if (trung)
        {
            ModelState.AddModelError(string.Empty, "Khung giờ này đã có người đặt. Vui lòng chọn giờ khác.");
        }

        if (trungBaoTri)
        {
            ModelState.AddModelError(string.Empty, "Khung giờ này đang được đánh dấu bảo trì. Vui lòng chọn giờ khác.");
        }

        if (trung || trungBaoTri)
        {
            ViewBag.GoiYKhungGio = GoiYKhungGioTrong(model.GioKetThuc - model.GioBatDau, bookingsInDay, maintenanceInDay);
        }

        if (!string.IsNullOrWhiteSpace(voucherError))
        {
            ModelState.AddModelError(nameof(model.MaVoucher), voucherError);
        }

        if (tongTien > taiKhoan.SoDu)
        {
            ModelState.AddModelError(string.Empty, $"Số dư không đủ để thanh toán. Bạn cần {tongTien:N0} VND nhưng hiện có {taiKhoan.SoDu:N0} VND.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.UserId = userId;
        model.Ngay = model.Ngay.Date;
        model.TongTien = tongTien;
        model.GiamGia = giamGia;
        model.VoucherId = voucher?.Id;
        model.MaVoucher = voucher?.Ma ?? string.Empty;
        model.TrangThai = TrangThaiDaThanhToan;
        model.MaXacNhan = TaoMaXacNhan();

        _context.DatSans.Add(model);
        if (voucher != null)
        {
            voucher.SoLuotDaDung += 1;
        }

        taiKhoan.SoDu -= model.TongTien;
        _context.GiaoDichs.Add(new GiaoDich
        {
            UserId = userId,
            TaiKhoanNguoiDungId = taiKhoan.Id,
            DatSan = model,
            LoaiGiaoDich = "Payment",
            SoTien = -model.TongTien,
            SoDuSauGiaoDich = taiKhoan.SoDu,
            NoiDung = $"Thanh toán đặt sân {san.TenSan}" + (giamGia > 0 ? $" (giảm {giamGia:N0} VND)" : string.Empty)
        });
        TaoThongBao(userId, "Đặt sân thành công", $"Đơn {model.MaXacNhan} đã được thanh toán. Bạn có thể mở phiếu QR để xác nhận khi đến sân.", string.Empty);

        await _context.SaveChangesAsync();
        TempData["Success"] = "Đặt sân và thanh toán thành công. Hệ thống đã tạo phiếu QR xác nhận cho đơn của bạn.";

        return RedirectToAction(nameof(Ticket), new { id = model.Id });
    }

    public async Task<IActionResult> MyBookings()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var dataRaw = await _context.DatSans
            .Include(x => x.San)
            .ThenInclude(x => x!.LoaiSan)
            .Include(x => x.DanhGiaSan)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.Ngay)
            .ToListAsync();

        var data = dataRaw
            .OrderByDescending(x => x.Ngay)
            .ThenByDescending(x => x.GioBatDau)
            .ToList();

        return View(data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var booking = await _context.DatSans.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (booking == null)
        {
            return NotFound();
        }

        if (booking.TrangThai != TrangThaiDaHuy)
        {
            await HoanTienNeuCanAsync(booking, userId);
            booking.TrangThai = TrangThaiDaHuy;
            TaoThongBao(userId, "Đơn đặt sân đã hủy", $"Đơn #{booking.Id} đã được hủy và hoàn tiền vào ví.", string.Empty);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã hủy đơn đặt sân và hoàn tiền vào tài khoản.";
        }

        return RedirectToAction(nameof(MyBookings));
    }

    [AllowAnonymous]
    public async Task<IActionResult> Calendar()
    {
        ViewBag.Sans = await _context.Sans.OrderBy(x => x.TenSan).ToListAsync();
        return View();
    }

    [AllowAnonymous]
    public async Task<IActionResult> GetEvents(int? sanId, string? status)
    {
        var bookingQuery = _context.DatSans
            .Include(x => x.San)
            .Where(x => x.TrangThai != TrangThaiDaHuy)
            .AsQueryable();

        if (sanId.HasValue)
        {
            bookingQuery = bookingQuery.Where(x => x.SanId == sanId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            bookingQuery = bookingQuery.Where(x => x.TrangThai == status);
        }

        var bookings = await bookingQuery.ToListAsync();

        var data = bookings.Select(x => new
        {
            title = (x.San != null ? x.San.TenSan : "Sân") + " - " + x.TrangThai,
            start = x.Ngay.Date.Add(x.GioBatDau).ToString("yyyy-MM-ddTHH:mm:ss"),
            end = x.Ngay.Date.Add(x.GioKetThuc).ToString("yyyy-MM-ddTHH:mm:ss"),
            status = x.TrangThai,
            sanId = x.SanId,
            code = x.MaXacNhan,
            color = x.TrangThai == "Confirmed" ? "#1f7a5a" : "#2f6fed"
        });

        var maintenanceQuery = _context.BaoTriSans
            .Include(x => x.San)
            .Where(x => x.TrangThai)
            .AsQueryable();

        if (sanId.HasValue)
        {
            maintenanceQuery = maintenanceQuery.Where(x => x.SanId == sanId.Value);
        }

        var maintenance = await maintenanceQuery
            .Select(x => new
            {
                title = (x.San != null ? x.San.TenSan : "Sân") + " - Bảo trì",
                start = x.Ngay.Date.Add(x.GioBatDau).ToString("yyyy-MM-ddTHH:mm:ss"),
                end = x.Ngay.Date.Add(x.GioKetThuc).ToString("yyyy-MM-ddTHH:mm:ss"),
                status = "Bảo trì",
                sanId = x.SanId,
                code = string.Empty,
                color = "#c24141"
            })
            .ToListAsync();

        return Json(data.Concat(maintenance).OrderBy(x => x.start));
    }

    public async Task<IActionResult> Ticket(int id)
    {
        var booking = await _context.DatSans
            .Include(x => x.San)
            .ThenInclude(x => x!.LoaiSan)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (booking == null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && booking.UserId != userId)
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(booking.MaXacNhan))
        {
            booking.MaXacNhan = TaoMaXacNhan();
            await _context.SaveChangesAsync();
        }

        return View(booking);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rate(int id, int soSao, string? binhLuan)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var booking = await _context.DatSans
            .Include(x => x.DanhGiaSan)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (booking == null)
        {
            return NotFound();
        }

        if (booking.TrangThai == TrangThaiDaHuy)
        {
            TempData["Error"] = "Không thể đánh giá đơn đã hủy.";
            return RedirectToAction(nameof(MyBookings));
        }

        soSao = Math.Clamp(soSao, 1, 5);
        if (booking.DanhGiaSan == null)
        {
            _context.DanhGiaSans.Add(new DanhGiaSan
            {
                DatSanId = booking.Id,
                SanId = booking.SanId,
                UserId = userId,
                SoSao = soSao,
                BinhLuan = binhLuan?.Trim() ?? string.Empty
            });
        }
        else
        {
            booking.DanhGiaSan.SoSao = soSao;
            booking.DanhGiaSan.BinhLuan = binhLuan?.Trim() ?? string.Empty;
            booking.DanhGiaSan.NgayDanhGia = DateTime.Now;
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Cảm ơn bạn đã đánh giá sân.";
        return RedirectToAction(nameof(MyBookings));
    }

    private async Task<decimal> GetSoDuAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var taiKhoan = await GetOrCreateTaiKhoanAsync(userId);
        return taiKhoan.SoDu;
    }

    private async Task<TaiKhoanNguoiDung> GetOrCreateTaiKhoanAsync(string userId)
    {
        var taiKhoan = await _context.TaiKhoanNguoiDungs.FirstOrDefaultAsync(x => x.UserId == userId);
        if (taiKhoan != null)
        {
            return taiKhoan;
        }

        taiKhoan = new TaiKhoanNguoiDung
        {
            UserId = userId,
            SoDu = 0
        };

        _context.TaiKhoanNguoiDungs.Add(taiKhoan);
        await _context.SaveChangesAsync();
        return taiKhoan;
    }

    private async Task HoanTienNeuCanAsync(DatSan booking, string userId)
    {
        var daHoanTien = await _context.GiaoDichs.AnyAsync(x =>
            x.DatSanId == booking.Id &&
            x.UserId == userId &&
            x.LoaiGiaoDich == "Refund");

        if (daHoanTien || booking.TongTien <= 0)
        {
            return;
        }

        var taiKhoan = await GetOrCreateTaiKhoanAsync(userId);
        taiKhoan.SoDu += booking.TongTien;
        _context.GiaoDichs.Add(new GiaoDich
        {
            UserId = userId,
            TaiKhoanNguoiDungId = taiKhoan.Id,
            DatSanId = booking.Id,
            LoaiGiaoDich = "Refund",
            SoTien = booking.TongTien,
            SoDuSauGiaoDich = taiKhoan.SoDu,
            NoiDung = $"Hoàn tiền hủy đơn đặt sân #{booking.Id}"
        });
    }

    private async Task LoadBookingViewDataAsync(int sanId, DateTime ngay)
    {
        ViewBag.Vouchers = await _context.Vouchers
            .Where(x => x.TrangThai && x.NgayBatDau.Date <= DateTime.Today && x.NgayKetThuc.Date >= DateTime.Today && x.SoLuotDaDung < x.SoLuotToiDa)
            .OrderBy(x => x.Ma)
            .ToListAsync();

        ViewBag.KhungGioDaDat = await _context.DatSans
            .Where(x => x.SanId == sanId && x.Ngay.Date == ngay.Date && x.TrangThai != TrangThaiDaHuy)
            .OrderBy(x => x.GioBatDau)
            .ToListAsync();

        ViewBag.KhungGioBaoTri = await _context.BaoTriSans
            .Where(x => x.SanId == sanId && x.Ngay.Date == ngay.Date && x.TrangThai)
            .OrderBy(x => x.GioBatDau)
            .ToListAsync();
    }

    private async Task<(Voucher? Voucher, decimal GiamGia, string? Error)> TinhGiamGiaAsync(string? maVoucher, decimal tongTienGoc)
    {
        if (string.IsNullOrWhiteSpace(maVoucher))
        {
            return (null, 0, null);
        }

        var code = maVoucher.Trim().ToUpperInvariant();
        var voucher = await _context.Vouchers.FirstOrDefaultAsync(x => x.Ma == code);
        if (voucher == null || !voucher.TrangThai)
        {
            return (null, 0, "Mã voucher không tồn tại hoặc đã tạm ngưng.");
        }

        if (voucher.NgayBatDau.Date > DateTime.Today || voucher.NgayKetThuc.Date < DateTime.Today)
        {
            return (null, 0, "Mã voucher không nằm trong thời gian áp dụng.");
        }

        if (voucher.SoLuotDaDung >= voucher.SoLuotToiDa)
        {
            return (null, 0, "Mã voucher đã hết lượt sử dụng.");
        }

        if (tongTienGoc < voucher.DonToiThieu)
        {
            return (null, 0, $"Đơn cần tối thiểu {voucher.DonToiThieu:N0} VND để dùng mã này.");
        }

        var giamGia = voucher.LoaiGiamGia == "Percent"
            ? Math.Round(tongTienGoc * voucher.GiaTri / 100, 0)
            : voucher.GiaTri;

        return (voucher, Math.Min(giamGia, tongTienGoc), null);
    }

    private static List<string> GoiYKhungGioTrong(TimeSpan thoiLuong, IEnumerable<DatSan> bookings, IEnumerable<BaoTriSan> maintenances)
    {
        if (thoiLuong <= TimeSpan.Zero)
        {
            thoiLuong = TimeSpan.FromHours(1);
        }

        var result = new List<string>();
        for (var start = TimeSpan.FromHours(6); start + thoiLuong <= TimeSpan.FromHours(23); start = start.Add(TimeSpan.FromMinutes(30)))
        {
            var end = start + thoiLuong;
            var busy = bookings.Any(x => start < x.GioKetThuc && end > x.GioBatDau) ||
                       maintenances.Any(x => start < x.GioKetThuc && end > x.GioBatDau);
            if (!busy)
            {
                result.Add($"{start:hh\\:mm} - {end:hh\\:mm}");
            }

            if (result.Count == 4)
            {
                break;
            }
        }

        return result;
    }

    private static string TaoMaXacNhan()
    {
        return "QR" + DateTime.Now.ToString("yyMMdd") + Random.Shared.Next(1000, 9999);
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
}
