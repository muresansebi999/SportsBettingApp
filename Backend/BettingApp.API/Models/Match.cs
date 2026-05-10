using System;
namespace BettingApp.API.Models
{
    public class Match
    {
        public int Id { get; set; }
        public string HomeTeam { get; set; } = string.Empty;
        public string AwayTeam { get; set; } = string.Empty;
        public string League { get; set; } = string.Empty;
        public double HomeOdds { get; set; }
        public double AwayOdds { get; set; }
        public double DrawOdds { get; set; }
    }
}