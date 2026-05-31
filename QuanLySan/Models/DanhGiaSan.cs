namespace QuanLySan.Models
{
    public class DanhGiaSan
    {
        public int Id { get; set; }
        public int DatSanId { get; set; }
        public int SanId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int SoSao { get; set; }
        public string BinhLuan { get; set; } = string.Empty;
        public DateTime NgayDanhGia { get; set; } = DateTime.Now;

        public DatSan? DatSan { get; set; }
        public San? San { get; set; }
    }
}
