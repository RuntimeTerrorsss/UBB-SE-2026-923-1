using System.ComponentModel.DataAnnotations;

namespace BookingBoardGames.Web.Models.Account
{
    public class RegisterViewModel
    {
        [Required] public string Username { get; set; }
        [Required, EmailAddress] public string Email { get; set; }
        [Required, MinLength(6)] public string Password { get; set; }
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
