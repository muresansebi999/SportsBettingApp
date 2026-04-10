using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BettingApp.API.Data; // Asigură-te că acesta este namespace-ul tău pentru DataContext
using BettingApp.API.Models; // Asigură-te că acesta este namespace-ul tău pentru User
using BettingApp.API.Dtos; // Litera mică aici!

namespace BettingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _context;

        public AuthController(DataContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(RegisterDto registerDto)
        {
            // Verificăm dacă user-ul există deja
            if (await _context.Users.AnyAsync(x => x.Username == registerDto.Username.ToLower()))
                return BadRequest("Username is taken");

            using var hmac = new System.Security.Cryptography.HMACSHA512();

            var user = new User
            {
                Username = registerDto.Username.ToLower(),
                Email = registerDto.Email,
                PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key,
                Balance = 100.00m,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        
        [HttpPost("login")]
        public async Task<ActionResult<User>> Login(LoginDto loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.Username == loginDto.Username.ToLower());

            if (user == null) return Unauthorized("Invalid username");

            using var hmac = new System.Security.Cryptography.HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
            }

            return user;
        }
    }
}