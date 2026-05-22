using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BettingApp.API.Data;
using BettingApp.API.Models;
using System.Text.Json;

namespace BettingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatchesController : ControllerBase
    {
        private readonly DataContext _context;
        private static readonly HttpClient _httpClient = new HttpClient();
        //intri pe https://the-odds-api.com si iti pui mail primesti key si pui aici intre ghilimele
        private const string ApiKey = "";

        public MatchesController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetMatches()
        {
            var matches = await _context.Matches.ToListAsync();
            return Ok(matches);
        }

        [HttpGet("league/{league}")]
        public async Task<ActionResult> GetMatchesByLeague(string league)
        {
            var matches = await _context.Matches
                .Where(m => m.League == league)
                .ToListAsync();
            return Ok(matches);
        }

        [HttpPost("update")]
        public async Task<ActionResult> UpdateMatchesFromApi()
        {
            var sportKeys = new[] { "soccer_epl", "soccer_spain_la_liga", "soccer_italy_serie_a", "soccer_germany_bundesliga", "soccer_fifa_world_cup" };

            foreach (var sport in sportKeys)
            {
                await UpdateOddsForSport(sport);
                await UpdateScoresForSport(sport);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Matches updated successfully" });
        }
        
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateMatch(int id, [FromBody] MatchUpdateDto dto)
        {
            var match = await _context.Matches.FindAsync(id);
            if (match == null) return NotFound("Match not found.");

            if (dto.HomeOdds.HasValue) match.HomeOdds = dto.HomeOdds.Value;
            if (dto.AwayOdds.HasValue) match.AwayOdds = dto.AwayOdds.Value;
            if (dto.DrawOdds.HasValue) match.DrawOdds = dto.DrawOdds.Value;
            if (dto.StartTime.HasValue) match.StartTime = dto.StartTime.Value;
            if (dto.IsFinished.HasValue) match.IsFinished = dto.IsFinished.Value;
            
            if (dto.HomeScore.HasValue) match.HomeScore = dto.HomeScore.Value;
            if (dto.AwayScore.HasValue) match.AwayScore = dto.AwayScore.Value;

            await _context.SaveChangesAsync();
            return Ok(match);
        }
        private async Task UpdateOddsForSport(string sport)
        {
            string url = $"https://api.the-odds-api.com/v4/sports/{sport}/odds/?apiKey={ApiKey}&regions=eu&markets=h2h";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return;

            var content = await response.Content.ReadAsStringAsync();
            var oddsEvents = JsonSerializer.Deserialize<List<OddsApiEvent>>(content);

            if (oddsEvents == null) return;

            foreach (var evt in oddsEvents)
            {
                var match = await _context.Matches.FirstOrDefaultAsync(m => m.ApiId == evt.Id);
                if (match == null)
                {
                    match = new Match
                    {
                        ApiId = evt.Id,
                        League = evt.SportTitle,
                        HomeTeam = evt.HomeTeam,
                        AwayTeam = evt.AwayTeam,
                        StartTime = evt.CommenceTime
                    };
                    _context.Matches.Add(match);
                }

                var bookmaker = evt.Bookmakers.FirstOrDefault();
                var market = bookmaker?.Markets.FirstOrDefault(m => m.Key == "h2h");

                if (market != null)
                {
                    var homeOutcome = market.Outcomes.FirstOrDefault(o => o.Name == evt.HomeTeam);
                    var awayOutcome = market.Outcomes.FirstOrDefault(o => o.Name == evt.AwayTeam);
                    var drawOutcome = market.Outcomes.FirstOrDefault(o => o.Name == "Draw");

                    if (homeOutcome != null) match.HomeOdds = homeOutcome.Price;
                    if (awayOutcome != null) match.AwayOdds = awayOutcome.Price;
                    if (drawOutcome != null) match.DrawOdds = drawOutcome.Price;
                }
            }
        }

        private async Task UpdateScoresForSport(string sport)
        {
            string url = $"https://api.the-odds-api.com/v4/sports/{sport}/scores/?apiKey={ApiKey}&daysFrom=3";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return;

            var content = await response.Content.ReadAsStringAsync();
            var scoreEvents = JsonSerializer.Deserialize<List<ScoresApiEvent>>(content);

            if (scoreEvents == null) return;

            foreach (var evt in scoreEvents)
            {
                var match = await _context.Matches.FirstOrDefaultAsync(m => m.ApiId == evt.Id);
                if (match != null)
                {
                    match.IsFinished = evt.Completed;

                    if (evt.Completed && evt.Scores != null)
                    {
                        var homeScoreStr = evt.Scores.FirstOrDefault(s => s.Name == match.HomeTeam)?.Score;
                        var awayScoreStr = evt.Scores.FirstOrDefault(s => s.Name == match.AwayTeam)?.Score;

                        if (int.TryParse(homeScoreStr, out int homeScore)) match.HomeScore = homeScore;
                        if (int.TryParse(awayScoreStr, out int awayScore)) match.AwayScore = awayScore;
                    }
                }
            }
        }
    }
}