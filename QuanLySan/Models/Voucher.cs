namespace QuanLySan.Models
{
    public class Voucher
    {
        public int Id { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public string LoaiGiamGia { get; set; } = "Fixed";
        public decimal GiaTri { get; set; }
        public decimal DonToiThieu { get; set; }
        public DateTime NgayBatDau { get; set; } = DateTime.Today;
        public DateTime NgayKetThuc { get; set; } = DateTime.Today.AddMonths(1);
        public int SoLuotToiDa { get; set; } = 50;
        public int SoLuotDaDung { get; set; }
        public bool TrangThai { get; set; } = true;
        public string MoTa { get; set; } = string.Empty;

        public ICollection<DatSan> DatSans { get; set; } = new List<DatSan>();
    }
}
