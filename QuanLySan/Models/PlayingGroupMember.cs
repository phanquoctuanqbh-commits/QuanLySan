namespace QuanLySan.Models
{
    public class PlayingGroupMember
    {
        public int Id { get; set; }
        public int PlayingGroupId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = "Member";
        public bool RatingGiven { get; set; }
        public double RatingReceived { get; set; }
        public string ReviewComment { get; set; } = string.Empty;

        public PlayingGroup? PlayingGroup { get; set; }
    }
}
