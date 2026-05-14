using System.Collections.Generic;
using BookingBoardGames.Web.Models.Games;
using BookingBoardGames.Web.Models.Rentals;

namespace BookingBoardGames.Web.Models.Dashboard
{
    public class DashboardViewModel
    {
        public List<GameViewModel> MyGames { get; set; } = new List<GameViewModel>();
        public List<RentalViewModel> ActiveRentals { get; set; } = new List<RentalViewModel>();
        
        public int TotalGamesOwned => MyGames.Count;
        public int TotalActiveRentals => ActiveRentals.Count;
    }
}
