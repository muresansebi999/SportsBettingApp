namespace BettingApp.API.Dtos
{
    public class WithdrawDto
    {
        public string Username { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
