namespace QuanLySan.Models
{
    public class GiaoDich
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int TaiKhoanNguoiDungId { get; set; }
        public int? DatSanId { get; set; }
        public string LoaiGiaoDich { get; set; } = string.Empty;
        public decimal SoTien { get; set; }
        public decimal SoDuSauGiaoDich { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public DateTime NgayGiaoDich { get; set; } = DateTime.Now;

        public TaiKhoanNguoiDung? TaiKhoanNguoiDung { get; set; }
        public DatSan? DatSan { get; set; }
    }
}
