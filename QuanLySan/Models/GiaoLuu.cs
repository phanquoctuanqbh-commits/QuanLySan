using System;

namespace QuanLySan.Models
{
    public class GiaoLuu
    {
        public int Id { get; set; }
        public string NguoiDangId { get; set; } = string.Empty;
        public string NguoiDangName { get; set; } = string.Empty;
        public string TieuDe { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string LoaiSan { get; set; } = "Football"; // Football, Badminton, Tennis
        public DateTime NgayChoi { get; set; } = DateTime.Today;
        public DateTime NgayDang { get; set; } = DateTime.Now;
        public string? NguoiGiaoLuuId { get; set; }
        public string? NguoiGiaoLuuName { get; set; }
        public string TrangThai { get; set; } = "Pending"; // Pending, Accepted
    }
}
