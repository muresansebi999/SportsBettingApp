namespace BettingApp.API.Dtos
{
    public class DepositDto
    {
        public string Username { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}