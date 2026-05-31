namespace QuanLySan.Models
{
    public class ThongBao
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string TieuDe { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string LienKet { get; set; } = string.Empty;
        public bool DaDoc { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}
