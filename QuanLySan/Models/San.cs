namespace QuanLySan.Models
{
    public class San
    {
        public int Id { get; set; }
        public string TenSan { get; set; } = string.Empty;
        public int LoaiSanId { get; set; }
        public decimal Gia { get; set; }
        public string MoTa { get; set; } = string.Empty;
        public string HinhAnh { get; set; } = string.Empty;
        public bool TrangThai { get; set; } = true;
        public string DiaChi { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string OwnerId { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;

        public LoaiSan? LoaiSan { get; set; }
        public ICollection<DatSan> DatSans { get; set; } = new List<DatSan>();
        public ICollection<CourtReport> Reports { get; set; } = new List<CourtReport>();
    }
}
