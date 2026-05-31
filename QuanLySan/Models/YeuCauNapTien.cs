namespace QuanLySan.Models
{
    public class YeuCauNapTien
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public decimal SoTien { get; set; }
        public string PhuongThuc { get; set; } = string.Empty;
        public string MaThamChieu { get; set; } = string.Empty;
        public string GhiChu { get; set; } = string.Empty;
        public string TrangThai { get; set; } = "Pending";
        public DateTime NgayGui { get; set; } = DateTime.Now;
        public DateTime? NgayXuLy { get; set; }
    }
}
