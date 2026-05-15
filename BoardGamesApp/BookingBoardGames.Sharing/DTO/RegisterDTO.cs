using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardGames.Sharing.DTO
{
    public class RegisterDTO
    {
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }
}
