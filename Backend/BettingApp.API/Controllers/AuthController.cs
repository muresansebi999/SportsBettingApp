using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BettingApp.API.Data;
using BettingApp.API.Models;
using BettingApp.API.Dtos;
using BettingApp.API.Services;

namespace BettingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly TokenService _tokenService;

        public AuthController(DataContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(x => x.Username == registerDto.Username.ToLower()))
                return Conflict("Username is taken");

            var today = DateTime.Today;
            var age = today.Year - registerDto.DateOfBirth.Year;
            if (registerDto.DateOfBirth.Date > today.AddYears(-age)) age--;
            if (age < 18) return Unauthorized("Trebuie să ai minim 18 ani.");

            using var hmac = new System.Security.Cryptography.HMACSHA512();

            var user = new User
            {
                Username = registerDto.Username.ToLower(),
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                DateOfBirth = registerDto.DateOfBirth,
                PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key,
                Balance = 100.00m,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Returnăm datele, inclusiv Balanța
            return new UserDto
            {
                Username = user.Username,
                Token = _tokenService.CreateToken(user),
                Balance = user.Balance
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.Username == loginDto.Username.ToLower());
            if (user == null) return Unauthorized("Invalid username");

            using var hmac = new System.Security.Cryptography.HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i = 0; i < computedHash.Length; i++)
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");

            // Returnăm datele, inclusiv Balanța
            return new UserDto
            {
                Username = user.Username,
                Token = _tokenService.CreateToken(user),
                Balance = user.Balance
            };
        }
    }
}