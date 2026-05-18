using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BettingApp.API.Data;
using BettingApp.API.Dtos;

namespace BettingApp.API.Controllers
{
    public class UpdateProfileRequest
    {
        public string OldUsername { get; set; } = string.Empty;
        public string NewUsername { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/[controller]")]
    public class HotbarController : ControllerBase
    {
        private readonly DataContext _context;

        public HotbarController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("info/{username}")]
        public async Task<IActionResult> GetInfo(string username)
        {
            if (string.IsNullOrEmpty(username)) return BadRequest("Username invalid.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
            if (user == null) return NotFound("Userul nu există în baza de date.");

            string formattedDate = (user.DateOfBirth.Year > 1900) 
                ? user.DateOfBirth.ToString("dd/MM/yyyy") 
                : "Nespecificată";

            return Ok(new { 
                username = user.Username, balance = user.Balance, email = user.Email,
                firstName = user.FirstName, lastName = user.LastName, dateOfBirth = formattedDate 
            });
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositDto request)
        {
            if (request.Amount <= 0) return BadRequest("Suma invalidă.");
            if (string.IsNullOrEmpty(request.Username)) return BadRequest("Username lipsă.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower());
            if (user == null) return NotFound("Userul nu a fost găsit.");

            user.Balance += request.Amount;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Depunere cu succes", newBalance = user.Balance });
        }

        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawDto request)
        {
            if (request.Amount <= 0) return BadRequest("Suma invalidă.");
            if (string.IsNullOrEmpty(request.Username)) return BadRequest("Username lipsă.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower());
            if (user == null) return NotFound("Userul nu a fost găsit.");

            if (user.Balance < request.Amount) return BadRequest("Fonduri insuficiente.");

            user.Balance -= request.Amount;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Retragere cu succes", newBalance = user.Balance });
        }

        // NOU: Am schimbat în HttpPost ca să nu mai fie blocat de browser
        [HttpPost("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try 
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == request.OldUsername.ToLower());
                if (user == null) return NotFound("Userul nu a fost găsit.");

                if (!string.IsNullOrWhiteSpace(request.NewUsername) && request.OldUsername.ToLower() != request.NewUsername.ToLower())
                {
                    if (await _context.Users.AnyAsync(x => x.Username.ToLower() == request.NewUsername.ToLower()))
                        return Conflict("Acest username este deja folosit de altcineva.");
                    user.Username = request.NewUsername.ToLower();
                }

                if (!string.IsNullOrWhiteSpace(request.Email)) 
                {
                    user.Email = request.Email;
                }

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Profil actualizat cu succes!", newUsername = user.Username, newEmail = user.Email });
            }
            catch (Exception ex)
            {
                // Prindem eroarea exacta de la baza de date
                return StatusCode(500, new { message = $"Eroare Server: {ex.Message}" });
            }
        }
    }
}