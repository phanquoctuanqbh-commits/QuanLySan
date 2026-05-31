namespace QuanLySan.Models
{
    public class UserManagementViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public decimal SoDu { get; set; }
        public DateTime? CanhCaoDen { get; set; }
        public string? LyDoCanhCao { get; set; }
    }
}
