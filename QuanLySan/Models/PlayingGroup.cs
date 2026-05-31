namespace QuanLySan.Models
{
    public class PlayingGroup
    {
        public int Id { get; set; }
        public int MatchPostId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int? BookingId { get; set; }

        public MatchPost? MatchPost { get; set; }
        public DatSan? Booking { get; set; }
        public ICollection<PlayingGroupMember> Members { get; set; } = new List<PlayingGroupMember>();
    }
}
