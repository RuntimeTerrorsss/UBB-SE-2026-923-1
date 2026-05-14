using BookingBoardGames.Sharing.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingBoardGames.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly InterfaceSearchAndFilterService _searchService;

        public DashboardController(InterfaceSearchAndFilterService searchService)
        {
            _searchService = searchService;
        }

        // Action methods here...
    }
}
