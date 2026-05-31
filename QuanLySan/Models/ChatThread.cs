namespace QuanLySan.Models
{
    public record ChatThread(string UserId, string UserName, string LastMessage, DateTime LastTime, int UnreadCount);
}
