namespace QuanLySan.Models
{
    public class MatchPost
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int SportTypeId { get; set; }
        public string CourtType { get; set; } = string.Empty;
        public int? CourtId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int NeededPlayers { get; set; } = 1;
        public string SkillLevel { get; set; } = "Basic";
        public string PreferredGender { get; set; } = "Any";
        public string PreferredAge { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Status { get; set; } = "Open";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public LoaiSan? SportType { get; set; }
        public San? Court { get; set; }
        public ICollection<MatchApplication> Applications { get; set; } = new List<MatchApplication>();
        public PlayingGroup? PlayingGroup { get; set; }
    }
}
