namespace QuanLySan.Models
{
    public class TaiKhoanNguoiDung
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public decimal SoDu { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public string HoTen { get; set; } = string.Empty;
        public DateTime? CanhCaoDen { get; set; }
        public string? LyDoCanhCao { get; set; }

        public ICollection<GiaoDich> GiaoDichs { get; set; } = new List<GiaoDich>();
    }
}
