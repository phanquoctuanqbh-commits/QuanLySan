namespace QuanLySan.Models
{
    public class MatchApplication
    {
        public int Id { get; set; }
        public int MatchPostId { get; set; }
        public string ApplicantUserId { get; set; } = string.Empty;
        public string ApplicantName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string SkillLevel { get; set; } = "Basic";
        public double PreviousRating { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public MatchPost? MatchPost { get; set; }
    }
}
