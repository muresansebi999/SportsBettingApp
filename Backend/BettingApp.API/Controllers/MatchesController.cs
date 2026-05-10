using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BettingApp.API.Data;

namespace BettingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatchesController : ControllerBase
    {
        private readonly DataContext _context;

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
    }
}