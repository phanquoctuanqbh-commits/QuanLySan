namespace QuanLySan.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public bool GuiBoiAdmin { get; set; }
        public bool DaDoc { get; set; }
        public DateTime NgayGui { get; set; } = DateTime.Now;
    }
}
