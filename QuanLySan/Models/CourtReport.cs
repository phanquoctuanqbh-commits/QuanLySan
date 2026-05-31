namespace QuanLySan.Models
{
    public class CourtReport
    {
        public int Id { get; set; }
        public int SanId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string OwnerId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ResolvedAt { get; set; }

        public San? San { get; set; }
    }
}
