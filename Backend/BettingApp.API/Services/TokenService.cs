using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BettingApp.API.Models;
using Microsoft.IdentityModel.Tokens;


namespace BettingApp.API.Services
{
    public class TokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username)
            };

           var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("acesta-este-un-token-key-foarte-lung-si-sigur-pentru-proiectul-meu-de-betting-2024!"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}