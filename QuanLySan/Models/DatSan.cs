namespace QuanLySan.Models
{
    public class DatSan
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int SanId { get; set; }
        public DateTime Ngay { get; set; }
        public TimeSpan GioBatDau { get; set; }
        public TimeSpan GioKetThuc { get; set; }
        public string TrangThai { get; set; } = "Pending";
        public decimal TongTien { get; set; }
        public decimal GiamGia { get; set; }
        public string MaVoucher { get; set; } = string.Empty;
        public int? VoucherId { get; set; }
        public string MaXacNhan { get; set; } = string.Empty;

        public San? San { get; set; }
        public Voucher? Voucher { get; set; }
        public DanhGiaSan? DanhGiaSan { get; set; }
        public ICollection<GiaoDich> GiaoDichs { get; set; } = new List<GiaoDich>();
    }
}
