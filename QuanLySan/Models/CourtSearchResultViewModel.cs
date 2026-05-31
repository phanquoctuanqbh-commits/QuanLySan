using System;
using System.Collections.Generic;

namespace QuanLySan.Models
{
    public class CourtSearchResultViewModel
    {
        public San Court { get; set; } = null!;
        public double? Distance { get; set; }
        public bool IsFreeInSelectedSlot { get; set; }
        public List<TimeSpanRange> BookedSlots { get; set; } = new();
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
    }

    public class TimeSpanRange
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
        public string Label { get; set; } = string.Empty;
    }
}
