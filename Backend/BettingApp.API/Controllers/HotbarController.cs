using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BettingApp.API.Data;
using BettingApp.API.Dtos;

namespace BettingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // AM SCOS [Authorize] DE AICI!
    public class HotbarController : ControllerBase
    {
        private readonly DataContext _context;

        public HotbarController(DataContext context)
        {
            _context = context;
        }

        // Ruta devine api/hotbar/info/numele-tau
        [HttpGet("info/{username}")]
        public async Task<IActionResult> GetInfo(string username)
        {
            // Căutăm direct în baza de date după nume
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username.ToLower());
            
            if (user == null) return NotFound("Userul nu există în baza de date.");

            // Trimitem cei 100$ și numele către Angular
            return Ok(new { 
                username = user.Username, 
                balance = user.Balance 
            });
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositDto request)
        {
            if (request.Amount <= 0) return BadRequest("Suma invalidă.");

            // Căutăm userul în baza de date după numele primit de la Angular
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username.ToLower());
            
            if (user == null) return NotFound("Userul nu a fost găsit.");

            // Adăugăm banii și salvăm direct
            user.Balance += request.Amount;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Depunere cu succes", newBalance = user.Balance });
        }
    }
}