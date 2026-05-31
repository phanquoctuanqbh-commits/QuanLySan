namespace QuanLySan.Models
{
    public class LoaiSan
    {
        public int Id { get; set; }
        public string TenLoai { get; set; } = string.Empty;

        public ICollection<San> Sans { get; set; } = new List<San>();
    }
}
