using System;

namespace BettingApp.API.Models
{
    public class MatchUpdateDto
    {
        public double? HomeOdds { get; set; }
        public double? AwayOdds { get; set; }
        public double? DrawOdds { get; set; }
        public DateTime? StartTime { get; set; }
        public bool? IsFinished { get; set; }
        public int? HomeScore { get; set; }
        public int? AwayScore { get; set; }
    }
}
