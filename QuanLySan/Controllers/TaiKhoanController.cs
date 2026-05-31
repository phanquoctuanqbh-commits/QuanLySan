using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLySan.Data;
using QuanLySan.Models;

[Authorize]
public class TaiKhoanController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public TaiKhoanController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        var taiKhoan = await GetOrCreateTaiKhoanAsync(userId);
        var giaoDichs = await _context.GiaoDichs
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.NgayGiaoDich)
            .Take(10)
            .ToListAsync();
        var yeuCauNapTiens = await _context.YeuCauNapTiens
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.NgayGui)
            .Take(5)
            .ToListAsync();
        var thongBaos = await _context.ThongBaos
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.NgayTao)
            .Take(6)
            .ToListAsync();

        ViewBag.GiaoDichs = giaoDichs;
        ViewBag.YeuCauNapTiens = yeuCauNapTiens;
        ViewBag.ThongBaos = thongBaos;
        return View(taiKhoan);
    }

    public async Task<IActionResult> ThanhToan(decimal soTien = 500000)
    {
        if (soTien <= 0)
        {
            soTien = 500000;
        }

        await GetOrCreateTaiKhoanAsync(GetUserId());
        ViewBag.SoTien = soTien;
        ViewBag.NoiDungChuyenKhoan = $"NAP {User.Identity?.Name}";
        ViewBag.VnPayConfigured = VnPayConfigured();
        return View();
    }

    public async Task<IActionResult> LichSu()
    {
        var userId = GetUserId();
        await GetOrCreateTaiKhoanAsync(userId);

        var giaoDichs = await _context.GiaoDichs
            .Include(x => x.DatSan)
            .ThenInclude(x => x!.San)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.NgayGiaoDich)
            .ToListAsync();

        return View(giaoDichs);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult NapTien(decimal soTien)
    {
        if (soTien <= 0)
        {
            TempData["Error"] = "Số tiền nạp phải lớn hơn 0.";
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(ThanhToan), new { soTien });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuiYeuCauNapTien(decimal soTien, string phuongThuc, string maThamChieu, string ghiChu)
    {
        if (soTien <= 0)
        {
            TempData["Error"] = "Số tiền nạp phải lớn hơn 0.";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(maThamChieu))
        {
            TempData["Error"] = "Vui lòng nhập mã giao dịch hoặc mã thẻ cào để gửi yêu cầu kiểm tra.";
            return RedirectToAction(nameof(ThanhToan), new { soTien });
        }

        var userId = GetUserId();
        await GetOrCreateTaiKhoanAsync(userId);

        _context.YeuCauNapTiens.Add(new YeuCauNapTien
        {
            UserId = userId,
            SoTien = soTien,
            PhuongThuc = phuongThuc,
            MaThamChieu = maThamChieu.Trim(),
            GhiChu = ghiChu?.Trim() ?? string.Empty
        });

        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã gửi yêu cầu nạp tiền. Số dư sẽ được cộng sau khi quản trị kiểm tra chính xác.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ThanhToanVnPay(decimal soTien)
    {
        if (soTien <= 0)
        {
            TempData["Error"] = "Số tiền nạp phải lớn hơn 0.";
            return RedirectToAction(nameof(Index));
        }

        var maThamChieu = "VNPAY" + DateTime.Now.ToString("yyyyMMddHHmmss");
        if (!VnPayConfigured())
        {
            return RedirectToAction(nameof(VnPayDemo), new { soTien, maThamChieu });
        }

        return Redirect(BuildVnPayUrl(soTien, maThamChieu));
    }

    public IActionResult VnPayDemo(decimal soTien, string maThamChieu)
    {
        if (soTien <= 0 || string.IsNullOrWhiteSpace(maThamChieu))
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.SoTien = soTien;
        ViewBag.MaThamChieu = maThamChieu;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VnPayDemoSuccess(decimal soTien, string maThamChieu)
    {
        if (soTien <= 0 || string.IsNullOrWhiteSpace(maThamChieu))
        {
            TempData["Error"] = "Thông tin thanh toán VNPAY không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        await CongTienVaoViAsync(GetUserId(), soTien, "VNPAY Demo", maThamChieu);
        TempData["Success"] = $"Thanh toán VNPAY demo thành công. Ví đã được cộng {soTien:N0} VND.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> VnPayReturn()
    {
        var responseCode = Request.Query["vnp_ResponseCode"].ToString();
        var amountRaw = Request.Query["vnp_Amount"].ToString();
        var transactionNo = Request.Query["vnp_TransactionNo"].ToString();
        var orderInfo = Request.Query["vnp_OrderInfo"].ToString();

        if (VnPayConfigured() && !ValidateVnPaySignature())
        {
            TempData["Error"] = "Chữ ký phản hồi VNPAY không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        if (responseCode != "00" || !decimal.TryParse(amountRaw, out var amount))
        {
            TempData["Error"] = "Thanh toán VNPAY chưa thành công.";
            return RedirectToAction(nameof(Index));
        }

        var soTien = amount / 100;
        var maThamChieu = string.IsNullOrWhiteSpace(transactionNo) ? orderInfo : transactionNo;
        await CongTienVaoViAsync(GetUserId(), soTien, "VNPAY", maThamChieu);
        TempData["Success"] = $"Thanh toán VNPAY thành công. Ví đã được cộng {soTien:N0} VND.";
        return RedirectToAction(nameof(Index));
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
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

    private async Task CongTienVaoViAsync(string userId, decimal soTien, string phuongThuc, string maThamChieu)
    {
        var existed = await _context.GiaoDichs.AnyAsync(x =>
            x.UserId == userId &&
            x.LoaiGiaoDich == "TopUp" &&
            x.NoiDung.Contains(maThamChieu));

        if (existed)
        {
            return;
        }

        var taiKhoan = await GetOrCreateTaiKhoanAsync(userId);
        taiKhoan.SoDu += soTien;
        _context.GiaoDichs.Add(new GiaoDich
        {
            UserId = userId,
            TaiKhoanNguoiDungId = taiKhoan.Id,
            LoaiGiaoDich = "TopUp",
            SoTien = soTien,
            SoDuSauGiaoDich = taiKhoan.SoDu,
            NoiDung = $"Nạp tiền qua {phuongThuc} - {maThamChieu}"
        });

        await _context.SaveChangesAsync();
    }

    private bool VnPayConfigured()
    {
        return !string.IsNullOrWhiteSpace(_configuration["VnPay:TmnCode"]) &&
               !string.IsNullOrWhiteSpace(_configuration["VnPay:HashSecret"]);
    }

    private string BuildVnPayUrl(decimal soTien, string maThamChieu)
    {
        var returnUrl = _configuration["VnPay:ReturnUrl"];
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            returnUrl = Url.Action(nameof(VnPayReturn), "TaiKhoan", null, Request.Scheme) ?? string.Empty;
        }

        var parameters = new SortedDictionary<string, string>
        {
            ["vnp_Version"] = "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = _configuration["VnPay:TmnCode"] ?? string.Empty,
            ["vnp_Amount"] = ((long)(soTien * 100)).ToString(),
            ["vnp_CreateDate"] = DateTime.Now.ToString("yyyyMMddHHmmss"),
            ["vnp_CurrCode"] = "VND",
            ["vnp_IpAddr"] = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
            ["vnp_Locale"] = "vn",
            ["vnp_OrderInfo"] = maThamChieu,
            ["vnp_OrderType"] = "billpayment",
            ["vnp_ReturnUrl"] = returnUrl,
            ["vnp_TxnRef"] = maThamChieu
        };

        var signData = BuildQuery(parameters);
        parameters["vnp_SecureHash"] = HmacSha512(_configuration["VnPay:HashSecret"] ?? string.Empty, signData);
        return (_configuration["VnPay:BaseUrl"] ?? string.Empty) + "?" + BuildQuery(parameters);
    }

    private bool ValidateVnPaySignature()
    {
        var receivedHash = Request.Query["vnp_SecureHash"].ToString();
        if (string.IsNullOrWhiteSpace(receivedHash))
        {
            return false;
        }

        var parameters = new SortedDictionary<string, string>();
        foreach (var item in Request.Query)
        {
            if (item.Key != "vnp_SecureHash" && item.Key != "vnp_SecureHashType")
            {
                parameters[item.Key] = item.Value.ToString();
            }
        }

        var expectedHash = HmacSha512(_configuration["VnPay:HashSecret"] ?? string.Empty, BuildQuery(parameters));
        return string.Equals(receivedHash, expectedHash, StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildQuery(SortedDictionary<string, string> parameters)
    {
        return string.Join("&", parameters.Select(x => $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(x.Value)}"));
    }

    private static string HmacSha512(string key, string input)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(input);
        using var hmac = new HMACSHA512(keyBytes);
        return Convert.ToHexString(hmac.ComputeHash(inputBytes)).ToLowerInvariant();
    }
}
