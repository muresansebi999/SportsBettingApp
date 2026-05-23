using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BettingApp.API.Data;
using BettingApp.API.Models;
using BettingApp.API.Dtos;

namespace BettingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // OPTION B: add [Authorize] here and the whole controller requires a valid JWT.
    public class BetsController : ControllerBase
    {
        private readonly DataContext _context;

        public BetsController(DataContext context)
        {
            _context = context;
        }

        // =========================================================================
        // IDENTITY SEAM. This is the only place that decides "who is this bet for".
        // Option A: read username from the DTO and look up the user (current).
        // Option B: ignore the DTO, read User.FindFirst(ClaimTypes.NameIdentifier)
        //           from the JWT and parse it to an int. Change ONLY this method.
        // Returns the user, or null if not found.
        // =========================================================================
        private async Task<User?> ResolveUserAsync(string usernameFromDto)
        {
            if (string.IsNullOrWhiteSpace(usernameFromDto)) return null;
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == usernameFromDto.ToLower());

            // --- OPTION B version, for later ---
            // var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            // if (!int.TryParse(idClaim, out var userId)) return null;
            // return await _context.Users.FindAsync(userId);
        }

        // =========================================================================
        // POST /api/bets  -- place an accumulator bet.
        // =========================================================================
        [HttpPost]
        public async Task<IActionResult> PlaceBet([FromBody] PlaceBetDto dto)
        {
            var user = await ResolveUserAsync(dto.Username);
            if (user == null) return NotFound("Userul nu a fost găsit.");

            if (dto.Stake <= 0) return BadRequest("Miza trebuie să fie pozitivă.");
            if (dto.Selections == null || dto.Selections.Count == 0)
                return BadRequest("Biletul este gol.");

            // One selection per match. Reject duplicates outright rather than silently
            // collapsing them, so the client can't sneak two picks on one match.
            var distinctMatchIds = dto.Selections.Select(s => s.MatchId).Distinct().Count();
            if (distinctMatchIds != dto.Selections.Count)
                return BadRequest("Doar o selecție per meci este permisă.");

            // Validate each outcome string up front.
            foreach (var sel in dto.Selections)
            {
                if (sel.Outcome != "1" && sel.Outcome != "X" && sel.Outcome != "2")
                    return BadRequest($"Pronostic invalid: '{sel.Outcome}'.");
            }

            // Load every referenced match in one query.
            var matchIds = dto.Selections.Select(s => s.MatchId).ToList();
            var matches = await _context.Matches
                .Where(m => matchIds.Contains(m.Id))
                .ToListAsync();

            if (matches.Count != dto.Selections.Count)
                return BadRequest("Unul sau mai multe meciuri nu există.");

            // Build the bet, computing odds SERVER-SIDE. Money math stays in decimal;
            // odds are stored as double on Match so we cast each one as we go.
            decimal totalOdds = 1m;
            var betSelections = new List<BetSelection>();

            foreach (var sel in dto.Selections)
            {
                var match = matches.First(m => m.Id == sel.MatchId);

                // Can't bet on a match that's already started or finished.
                if (match.IsFinished)
                    return BadRequest($"Meciul {match.HomeTeam} - {match.AwayTeam} este deja terminat.");
                if (match.StartTime != default && match.StartTime <= DateTime.Now)
                    return BadRequest($"Meciul {match.HomeTeam} - {match.AwayTeam} a început deja.");

                double rawOdd = sel.Outcome switch
                {
                    "1" => match.HomeOdds,
                    "X" => match.DrawOdds,
                    "2" => match.AwayOdds,
                    _ => 0
                };

                if (rawOdd <= 0)
                    return BadRequest($"Cotă indisponibilă pentru {match.HomeTeam} - {match.AwayTeam}.");

                decimal odd = (decimal)rawOdd;
                totalOdds *= odd;

                betSelections.Add(new BetSelection
                {
                    MatchId = match.Id,
                    Outcome = sel.Outcome,
                    Odd = odd
                });
            }

            decimal potentialPayout = Math.Round(dto.Stake * totalOdds, 2);
            totalOdds = Math.Round(totalOdds, 2);

            // Deduct-and-save atomically. Re-check balance INSIDE the transaction so a
            // concurrent bet/withdraw can't let two requests both pass the check.
            // SQLite serializes writers, so this is belt-and-suspenders, but correct.
            using var tx = await _context.Database.BeginTransactionAsync();

            // Re-read the user inside the tx to get the current balance.
            var freshUser = await _context.Users.FirstAsync(u => u.Id == user.Id);
            if (freshUser.Balance < dto.Stake)
            {
                await tx.RollbackAsync();
                return BadRequest("Fonduri insuficiente.");
            }

            freshUser.Balance -= dto.Stake;

            var bet = new Bet
            {
                UserId = freshUser.Id,
                Stake = dto.Stake,
                TotalOdds = totalOdds,
                PotentialPayout = potentialPayout,
                Status = BetStatus.Pending,
                CreatedAt = DateTime.Now,
                Selections = betSelections
            };

            _context.Bets.Add(bet);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return Ok(new
            {
                message = "Pariu plasat cu succes.",
                betId = bet.Id,
                newBalance = freshUser.Balance,
                totalOdds = bet.TotalOdds,
                potentialPayout = bet.PotentialPayout
            });
        }

        // =========================================================================
        // POST /api/bets/settle  -- settle all pending bets against finished matches.
        // WARNING: this pays out money. It MUST be admin-gated in production. There is
        // no role system on User yet, so for the demo it is left open. Do not ship
        // this open. Add a role check (or [Authorize(Roles="Admin")]) before then.
        // =========================================================================
        [HttpPost("settle")]
        public async Task<IActionResult> SettleBets()
        {
            var pending = await _context.Bets
                .Include(b => b.Selections)
                .Where(b => b.Status == BetStatus.Pending)
                .ToListAsync();

            if (pending.Count == 0)
                return Ok(new { message = "Niciun pariu de decontat.", settled = 0 });

            // Gather all match ids referenced by pending bets, load them once.
            var matchIds = pending
                .SelectMany(b => b.Selections.Select(s => s.MatchId))
                .Distinct()
                .ToList();

            var matches = await _context.Matches
                .Where(m => matchIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id);

            int wonCount = 0, lostCount = 0, skipped = 0;

            using var tx = await _context.Database.BeginTransactionAsync();

            foreach (var bet in pending)
            {
                // A bet can only settle once EVERY selection's match is finished.
                bool allFinished = bet.Selections.All(s =>
                    matches.TryGetValue(s.MatchId, out var m) && m.IsFinished);

                if (!allFinished) { skipped++; continue; }

                // Accumulator: bet wins only if ALL selections are correct.
                bool allCorrect = true;
                foreach (var sel in bet.Selections)
                {
                    var m = matches[sel.MatchId];
                    string actual = ResultOf(m); // "1" | "X" | "2" | "" if scores missing
                    if (actual == "" || actual != sel.Outcome)
                    {
                        allCorrect = false;
                        break;
                    }
                }

                if (allCorrect)
                {
                    var winner = await _context.Users.FirstAsync(u => u.Id == bet.UserId);
                    winner.Balance += bet.PotentialPayout;
                    bet.Status = BetStatus.Won;
                    wonCount++;
                }
                else
                {
                    bet.Status = BetStatus.Lost;
                    lostCount++;
                }

                bet.SettledAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return Ok(new
            {
                message = "Decontare completă.",
                won = wonCount,
                lost = lostCount,
                skipped = skipped // still pending because not all matches finished
            });
        }

        // Derive 1/X/2 from stored scores. Returns "" if scores aren't both present,
        // which makes the settle loop treat the bet as not-yet-resolvable rather than
        // guessing.
        private static string ResultOf(Match m)
        {
            if (m.HomeScore is null || m.AwayScore is null) return "";
            if (m.HomeScore > m.AwayScore) return "1";
            if (m.HomeScore == m.AwayScore) return "X";
            return "2";
        }

        // =========================================================================
        // GET /api/bets/my?username=...  -- bet history for the user.
        // Same identity seam note applies: in option B drop the query param and read
        // the JWT instead.
        // =========================================================================
        [HttpGet("my")]
        public async Task<IActionResult> MyBets([FromQuery] string username)
        {
            var user = await ResolveUserAsync(username);
            if (user == null) return NotFound("Userul nu a fost găsit.");

            var bets = await _context.Bets
                .Include(b => b.Selections)
                .Where(b => b.UserId == user.Id)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            // Pull the match teams for display in one query.
            var matchIds = bets.SelectMany(b => b.Selections.Select(s => s.MatchId)).Distinct().ToList();
            var matches = await _context.Matches
                .Where(m => matchIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id);

            var result = bets.Select(b => new BetResponseDto
            {
                Id = b.Id,
                Stake = b.Stake,
                TotalOdds = b.TotalOdds,
                PotentialPayout = b.PotentialPayout,
                Status = b.Status.ToString(),
                CreatedAt = b.CreatedAt,
                SettledAt = b.SettledAt,
                Selections = b.Selections.Select(s => new BetSelectionResponseDto
                {
                    MatchId = s.MatchId,
                    HomeTeam = matches.TryGetValue(s.MatchId, out var m) ? m.HomeTeam : "?",
                    AwayTeam = matches.TryGetValue(s.MatchId, out var m2) ? m2.AwayTeam : "?",
                    Outcome = s.Outcome,
                    Odd = s.Odd
                }).ToList()
            }).ToList();

            return Ok(result);
        }
    }
}