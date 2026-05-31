namespace QuanLySan.Models
{
    public class SiteSetting
    {
        public int Id { get; set; }
        public string BusinessName { get; set; } = "SportHub";
        public string Address { get; set; } = "12 Nguyen Van Bao, Ward 4, Go Vap, Ho Chi Minh City";
        public string PhoneNumber { get; set; } = "0909 123 456";
        public string Email { get; set; } = "support@sporthub.vn";
        public string OpeningHours { get; set; } = "06:00 - 23:00 daily";
        public string MapEmbedUrl { get; set; } = "https://www.google.com/maps?q=12%20Nguyen%20Van%20Bao%20Go%20Vap%20Ho%20Chi%20Minh&output=embed";
        public string Description { get; set; } = "Online sport court booking and management system.";
    }
}
