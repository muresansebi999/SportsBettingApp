namespace BettingApp.API.Dtos
{
    public class UserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public decimal Balance { get; set; } // <--- AM ADĂUGAT ASTA
    }
}