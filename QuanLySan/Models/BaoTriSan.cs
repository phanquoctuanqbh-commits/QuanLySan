namespace QuanLySan.Models
{
    public class BaoTriSan
    {
        public int Id { get; set; }
        public int SanId { get; set; }
        public DateTime Ngay { get; set; } = DateTime.Today;
        public TimeSpan GioBatDau { get; set; } = new TimeSpan(7, 0, 0);
        public TimeSpan GioKetThuc { get; set; } = new TimeSpan(8, 0, 0);
        public string LyDo { get; set; } = string.Empty;
        public bool TrangThai { get; set; } = true;

        public San? San { get; set; }
    }
}
