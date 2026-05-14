using BookingBoardGames.Sharing.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingBoardGames.Web.Controllers
{
    [Authorize]
    public class GamesController : Controller
    {
        private readonly InterfaceBookingService _bookingService;

        public GamesController(InterfaceBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // Action methods here...
    }
}
