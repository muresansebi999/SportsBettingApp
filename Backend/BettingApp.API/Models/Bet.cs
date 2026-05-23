using System;
using System.Collections.Generic;

namespace BettingApp.API.Models
{
    public enum BetStatus
    {
        Pending = 0,
        Won = 1,
        Lost = 2
    }

    public class Bet
    {
        public int Id { get; set; }

        // Keyed by user Id, not username. Usernames can change (your HotbarController
        // even has an endpoint that renames users); ids don't. This is also the seam
        // for the A->B auth switch: in option A this gets filled from the request DTO,
        // in option B from the JWT NameIdentifier claim. Nothing else in the bet logic
        // touches identity.
        public int UserId { get; set; }

        public decimal Stake { get; set; }

        // Computed server-side from Match odds at placement time. NOT taken from client.
        public decimal TotalOdds { get; set; }
        public decimal PotentialPayout { get; set; }

        public BetStatus Status { get; set; } = BetStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? SettledAt { get; set; }

        public List<BetSelection> Selections { get; set; } = new List<BetSelection>();
    }

    public class BetSelection
    {
        public int Id { get; set; }

        // FK back to the parent bet.
        public int BetId { get; set; }
        public Bet Bet { get; set; } = null!;

        // References Match.Id. Matches are persisted (MatchesController writes API
        // results into the Matches table), so this is a stable reference.
        public int MatchId { get; set; }

        // '1' = home win, 'X' = draw, '2' = away win.
        public string Outcome { get; set; } = string.Empty;

        // Odd snapshotted at placement time, looked up server-side from the Match.
        // Stored so a later odds update on the Match doesn't retroactively change
        // an already-placed bet.
        public decimal Odd { get; set; }
    }
}