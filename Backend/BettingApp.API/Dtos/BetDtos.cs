using System;
using System.Collections.Generic;

namespace BettingApp.API.Dtos
{
    // What the client sends to place a bet. Note what is NOT here: no odds,
    // no totalOdds, no payout. The server looks all of that up itself from
    // the Matches table. The client cannot influence the money math.
    public class PlaceBetDto
    {
        // OPTION A (current): client sends the logged-in username, same pattern
        // as DepositDto/WithdrawDto in HotbarController. This is the ONE field
        // that becomes insecure-by-trust. To migrate to option B: delete this
        // field, add [Authorize] to the controller, and change ResolveUserIdAsync
        // to read the JWT instead. Nothing else changes.
        public string Username { get; set; } = string.Empty;

        public decimal Stake { get; set; }

        public List<PlaceBetSelectionDto> Selections { get; set; } = new List<PlaceBetSelectionDto>();
    }

    public class PlaceBetSelectionDto
    {
        public int MatchId { get; set; }
        public string Outcome { get; set; } = string.Empty; // "1" | "X" | "2"
    }

    // Shapes returned to the client for bet history (GET /api/bets/my).
    public class BetResponseDto
    {
        public int Id { get; set; }
        public decimal Stake { get; set; }
        public decimal TotalOdds { get; set; }
        public decimal PotentialPayout { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? SettledAt { get; set; }
        public List<BetSelectionResponseDto> Selections { get; set; } = new List<BetSelectionResponseDto>();
    }

    public class BetSelectionResponseDto
    {
        public int MatchId { get; set; }
        public string HomeTeam { get; set; } = string.Empty;
        public string AwayTeam { get; set; } = string.Empty;
        public string Outcome { get; set; } = string.Empty;
        public decimal Odd { get; set; }
    }
}