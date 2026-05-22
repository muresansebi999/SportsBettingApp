using System;
namespace BettingApp.API.Models
{
    public class Match
    {
        public int Id { get; set; }
        public string ApiId { get; set; } = string.Empty;
        public string HomeTeam { get; set; } = string.Empty;
        public string AwayTeam { get; set; } = string.Empty;
        public string League { get; set; } = string.Empty;
        public double HomeOdds { get; set; }
        public double AwayOdds { get; set; }
        public double DrawOdds { get; set; }
        public DateTime StartTime { get; set; }
        public bool IsFinished { get; set; }
        public int? HomeScore { get; set; }
        public int? AwayScore { get; set; }
    }
}