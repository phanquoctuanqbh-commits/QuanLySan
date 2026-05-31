using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLySan.Data;
using QuanLySan.Models;
using System.Security.Claims;

public class SanController : Controller
{
    private readonly ApplicationDbContext _context;

    public SanController(ApplicationDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    public IActionResult Index(int? loaiSanId)
    {
        ViewBag.LoaiSans = _context.LoaiSans.OrderBy(x => x.TenLoai).ToList();
        ViewBag.LoaiSanId = loaiSanId;
        ViewBag.DiemDanhGia = _context.DanhGiaSans
            .GroupBy(x => x.SanId)
            .Select(x => new { SanId = x.Key, Diem = x.Average(y => y.SoSao), SoLuot = x.Count() })
            .ToDictionary(x => x.SanId, x => (Diem: x.Diem, SoLuot: x.SoLuot));

        var query = _context.Sans.Include(s => s.LoaiSan).AsQueryable();
        if (loaiSanId.HasValue)
        {
            query = query.Where(x => x.LoaiSanId == loaiSanId.Value);
        }

        return View(query.OrderByDescending(x => x.TrangThai).ThenBy(x => x.TenSan).ToList());
    }

    [AllowAnonymous]
    public async Task<IActionResult> TimKiem(
        string? query,
        int? loaiSanId,
        decimal? giaMin,
        decimal? giaMax,
        DateTime? ngay,
        TimeSpan? gioBatDau,
        TimeSpan? gioKetThuc,
        double? userLat,
        double? userLng,
        double? maxDistance,
        string? sortBy)
    {
        var dateToCheck = ngay ?? DateTime.Today;
        ViewBag.Ngay = dateToCheck;
        ViewBag.Query = query;
        ViewBag.LoaiSanId = loaiSanId;
        ViewBag.GiaMin = giaMin;
        ViewBag.GiaMax = giaMax;
        ViewBag.GioBatDau = gioBatDau;
        ViewBag.GioKetThuc = gioKetThuc;
        ViewBag.UserLat = userLat;
        ViewBag.UserLng = userLng;
        ViewBag.MaxDistance = maxDistance;
        ViewBag.SortBy = sortBy;

        ViewBag.LoaiSans = await _context.LoaiSans.OrderBy(x => x.TenLoai).ToListAsync();

        var courtsQuery = _context.Sans.Include(s => s.LoaiSan).Where(x => x.TrangThai).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var q = query.ToLower();
            courtsQuery = courtsQuery.Where(x => x.TenSan.ToLower().Contains(q) 
                                              || x.MoTa.ToLower().Contains(q) 
                                              || x.DiaChi.ToLower().Contains(q));
        }

        if (loaiSanId.HasValue)
        {
            courtsQuery = courtsQuery.Where(x => x.LoaiSanId == loaiSanId.Value);
        }

        if (giaMin.HasValue)
        {
            courtsQuery = courtsQuery.Where(x => x.Gia >= giaMin.Value);
        }
        if (giaMax.HasValue)
        {
            courtsQuery = courtsQuery.Where(x => x.Gia <= giaMax.Value);
        }

        var courts = await courtsQuery.ToListAsync();

        var ratings = await _context.DanhGiaSans
            .GroupBy(x => x.SanId)
            .Select(x => new { SanId = x.Key, Diem = x.Average(y => y.SoSao), SoLuot = x.Count() })
            .ToDictionaryAsync(x => x.SanId, x => (Diem: x.Diem, SoLuot: x.SoLuot));
        ViewBag.DiemDanhGia = ratings;

        var bookings = await _context.DatSans
            .Where(x => x.Ngay.Date == dateToCheck.Date && x.TrangThai != "Cancelled")
            .ToListAsync();

        var maintenances = await _context.BaoTriSans
            .Where(x => x.Ngay.Date == dateToCheck.Date && x.TrangThai)
            .ToListAsync();

        var courtList = courts.Select(court =>
        {
            double? distance = null;
            if (userLat.HasValue && userLng.HasValue && court.Latitude != 0 && court.Longitude != 0)
            {
                distance = CalculateDistance(userLat.Value, userLng.Value, court.Latitude, court.Longitude);
            }

            bool isFree = true;
            if (gioBatDau.HasValue && gioKetThuc.HasValue)
            {
                var courtBookings = bookings.Where(x => x.SanId == court.Id);
                var courtMaint = maintenances.Where(x => x.SanId == court.Id);

                var conflictBooking = courtBookings.Any(x => gioBatDau.Value < x.GioKetThuc && gioKetThuc.Value > x.GioBatDau);
                var conflictMaint = courtMaint.Any(x => gioBatDau.Value < x.GioKetThuc && gioKetThuc.Value > x.GioBatDau);

                isFree = !conflictBooking && !conflictMaint;
            }

            var bookedHours = bookings.Where(x => x.SanId == court.Id)
                .Select(x => new TimeSpanRange { Start = x.GioBatDau, End = x.GioKetThuc, Label = "Đã đặt" })
                .Concat(maintenances.Where(x => x.SanId == court.Id).Select(x => new TimeSpanRange { Start = x.GioBatDau, End = x.GioKetThuc, Label = "Bảo trì" }))
                .OrderBy(x => x.Start)
                .ToList();

            double avgRating = 0;
            int ratingCount = 0;
            if (ratings.TryGetValue(court.Id, out var ratingInfo))
            {
                avgRating = ratingInfo.Diem;
                ratingCount = ratingInfo.SoLuot;
            }

            return new CourtSearchResultViewModel
            {
                Court = court,
                Distance = distance,
                IsFreeInSelectedSlot = isFree,
                BookedSlots = bookedHours,
                AverageRating = avgRating,
                RatingCount = ratingCount
            };
        }).ToList();

        if (maxDistance.HasValue)
        {
            double limit = maxDistance.Value;
            courtList = courtList.Where(x => x.Distance.HasValue && x.Distance.Value <= limit).ToList();
        }

        if (gioBatDau.HasValue && gioKetThuc.HasValue)
        {
            courtList = courtList.Where(x => x.IsFreeInSelectedSlot).ToList();
        }

        courtList = sortBy switch
        {
            "price_asc" => courtList.OrderBy(x => x.Court.Gia).ToList(),
            "price_desc" => courtList.OrderByDescending(x => x.Court.Gia).ToList(),
            "distance" => courtList.Where(x => x.Distance.HasValue).OrderBy(x => x.Distance.GetValueOrDefault()).Concat(courtList.Where(x => !x.Distance.HasValue)).ToList(),
            "rating" => courtList.OrderByDescending(x => x.AverageRating).ToList(),
            _ => courtList.OrderBy(x => x.Court.TenSan).ToList()
        };

        return View(courtList);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id, DateTime? ngay)
    {
        var court = await _context.Sans
            .Include(x => x.LoaiSan)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (court == null)
        {
            return NotFound();
        }

        var dateToCheck = ngay ?? DateTime.Today;
        ViewBag.Ngay = dateToCheck;

        var ratings = await _context.DanhGiaSans
            .Where(x => x.SanId == id)
            .OrderByDescending(x => x.NgayDanhGia)
            .ToListAsync();

        var userIds = ratings.Select(x => x.UserId).Distinct().ToList();
        var usersDict = await _context.Users
            .Where(x => userIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => MaskEmail(x.Email ?? "user@sporthub.vn"));
        ViewBag.UserEmails = usersDict;

        ViewBag.Reviews = ratings;
        ViewBag.AverageRating = ratings.Any() ? ratings.Average(x => x.SoSao) : 0.0;
        ViewBag.RatingCount = ratings.Count;

        var bookings = await _context.DatSans
            .Where(x => x.SanId == id && x.Ngay.Date == dateToCheck.Date && x.TrangThai != "Cancelled")
            .ToListAsync();

        var maintenances = await _context.BaoTriSans
            .Where(x => x.SanId == id && x.Ngay.Date == dateToCheck.Date && x.TrangThai)
            .ToListAsync();

        ViewBag.BookedSlots = bookings.Select(x => new TimeSpanRange { Start = x.GioBatDau, End = x.GioKetThuc, Label = "Đã đặt" })
            .Concat(maintenances.Select(x => new TimeSpanRange { Start = x.GioBatDau, End = x.GioKetThuc, Label = "Bảo trì" }))
            .OrderBy(x => x.Start)
            .ToList();

        return View(court);
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var R = 6371; // Radius of the earth in km
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
            ;
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var d = R * c; // Distance in km
        return d;
    }

    private static double ToRadians(double val)
    {
        return (Math.PI / 180) * val;
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return "user***";
        var parts = email.Split('@');
        if (parts.Length < 2) return "user***";
        var username = parts[0];
        var domain = parts[1];
        if (username.Length <= 3) return username + "***@" + domain;
        return username.Substring(0, 3) + "***@" + domain;
    }

    [Authorize(Roles = "Admin,Owner,ChuSan")]
    public IActionResult Create()
    {
        LoadLoaiSan();
        return View(new San { TrangThai = true });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Owner,ChuSan")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(San san, List<IFormFile>? files)
    {
        if (!ModelState.IsValid)
        {
            LoadLoaiSan();
            return View(san);
        }

        if (IsOwnerOnly())
        {
            san.OwnerId = GetUserId();
            san.OwnerName = User.Identity?.Name ?? string.Empty;
        }

        san.HinhAnh = await SaveImagesAsync(files) ?? GetDefaultImage(san.LoaiSanId);
        _context.Sans.Add(san);
        await _context.SaveChangesAsync();
        TaoThongBao(GetUserId(), "Thêm sân mới thành công", $"Sân {san.TenSan} đã được lưu vào database.", Url.Action(nameof(Details), "San", new { id = san.Id }) ?? string.Empty);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã thêm sân mới.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Owner,ChuSan")]
    public async Task<IActionResult> Edit(int id)
    {
        var san = await _context.Sans.FindAsync(id);
        if (san == null)
        {
            return NotFound();
        }

        if (!CanManageCourt(san))
        {
            return Forbid();
        }

        LoadLoaiSan();
        return View(san);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Owner,ChuSan")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, San san, List<string>? existingImages, List<IFormFile>? files)
    {
        if (id != san.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            LoadLoaiSan();
            return View(san);
        }

        var current = await _context.Sans.FindAsync(id);
        if (current == null)
        {
            return NotFound();
        }

        if (!CanManageCourt(current))
        {
            return Forbid();
        }

        current.TenSan = san.TenSan;
        current.LoaiSanId = san.LoaiSanId;
        current.Gia = san.Gia;
        current.MoTa = san.MoTa;
        current.TrangThai = san.TrangThai;
        current.DiaChi = san.DiaChi;
        current.Latitude = san.Latitude;
        current.Longitude = san.Longitude;
        
        var finalImages = new List<string>();
        if (existingImages != null && existingImages.Any())
        {
            finalImages.AddRange(existingImages.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        var uploadedImages = await SaveImagesAsync(files);
        if (!string.IsNullOrEmpty(uploadedImages))
        {
            var uploadedPaths = uploadedImages.Split(';', StringSplitOptions.RemoveEmptyEntries);
            finalImages.AddRange(uploadedPaths);
        }
        
        current.HinhAnh = finalImages.Any() ? string.Join(";", finalImages) : GetDefaultImage(current.LoaiSanId);

        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật sân.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Owner,ChuSan")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var san = await _context.Sans.Include(x => x.DatSans).FirstOrDefaultAsync(x => x.Id == id);
        if (san == null)
        {
            return NotFound();
        }

        if (!CanManageCourt(san))
        {
            return Forbid();
        }

        if (san.DatSans.Any())
        {
            san.TrangThai = false;
            TempData["Success"] = "Sân đã có đơn đặt nên hệ thống chuyển sang tạm ngừng.";
        }
        else
        {
            _context.Sans.Remove(san);
            TempData["Success"] = "Đã xóa sân.";
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Owner,ChuSan")]
    public async Task<IActionResult> MyCourts()
    {
        var userId = GetUserId();
        var courts = await _context.Sans
            .Include(x => x.LoaiSan)
            .Where(x => x.OwnerId == userId)
            .OrderBy(x => x.TenSan)
            .ToListAsync();

        var courtIds = courts.Select(x => x.Id).ToList();
        var revenue = await _context.DatSans
            .Where(x => courtIds.Contains(x.SanId) && (x.TrangThai == "Paid" || x.TrangThai == "Confirmed"))
            .GroupBy(x => x.SanId)
            .Select(x => new { SanId = x.Key, Total = x.Sum(y => y.TongTien), Count = x.Count() })
            .ToDictionaryAsync(x => x.SanId, x => (x.Total, x.Count));

        ViewBag.Revenue = revenue;
        return View(courts);
    }

    private void LoadLoaiSan()
    {
        ViewBag.LoaiSan = _context.LoaiSans.OrderBy(x => x.TenLoai).ToList();
    }

    private bool IsOwnerOnly()
    {
        return !User.IsInRole("Admin") && (User.IsInRole("Owner") || User.IsInRole("ChuSan"));
    }

    private bool CanManageCourt(San san)
    {
        return User.IsInRole("Admin") || san.OwnerId == GetUserId();
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }

    private void TaoThongBao(string userId, string tieuDe, string noiDung, string lienKet)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        _context.ThongBaos.Add(new ThongBao
        {
            UserId = userId,
            TieuDe = tieuDe,
            NoiDung = noiDung,
            LienKet = lienKet
        });
    }

    private string GetDefaultImage(int loaiSanId)
    {
        var tenLoai = _context.LoaiSans
            .Where(x => x.Id == loaiSanId)
            .Select(x => x.TenLoai)
            .FirstOrDefault();

        return tenLoai switch
        {
            "Football" => "/images/Etihad.jpg",
            "Badminton" => "/images/scl.jpg",
            "Tennis" => "/images/stn.jpg",
            "Pickleball" => "/images/spick.jpg",
            "Volleyball" => "/images/voll.jfif",
            "Golf" => "/images/goft.jfif",
            "Table Tennis" => "/images/tt.jfif",
            _ => "/images/sbr.jpg"
        };
    }

    private static async Task<string?> SaveImageAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return null;
        }

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".jfif", ".gif", ".bmp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(extension))
        {
            return null;
        }

        var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
        Directory.CreateDirectory(folder);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(folder, fileName);

        await using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);
        return "/images/" + fileName;
    }

    private static async Task<string?> SaveImagesAsync(List<IFormFile>? files)
    {
        if (files == null || files.Count == 0)
        {
            return null;
        }

        var savedPaths = new List<string>();
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".jfif", ".gif", ".bmp" };
        var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
        Directory.CreateDirectory(folder);

        foreach (var file in files)
        {
            if (file == null || file.Length == 0) continue;
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(extension)) continue;

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var path = Path.Combine(folder, fileName);

            await using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
            savedPaths.Add("/images/" + fileName);
        }

        return savedPaths.Any() ? string.Join(";", savedPaths) : null;
    }
}
