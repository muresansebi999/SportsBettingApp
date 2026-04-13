using System.ComponentModel.DataAnnotations;

namespace BettingApp.API.Dtos {
    public class RegisterDto {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty; 

        [Required]
        public string LastName { get; set; } = string.Empty; 

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(20, MinimumLength = 4)]
        public string Password { get; set; } = string.Empty;
    }
}