using BookingBoardGames.Data.Enum;
using BookingBoardGames.Sharing.DTO;
using BookingBoardGames.Sharing.Mapper;
using BookingBoardGames.Sharing.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BookingBoardGames.Web.Controllers
{
    [Authorize]
    public class GamesController : BaseController
    {
        private readonly InterfaceBookingService _bookingService;
        private readonly InterfaceSearchAndFilterService _searchService;

        public GamesController(InterfaceBookingService bookingService, InterfaceSearchAndFilterService searchService)
        {
            _bookingService = bookingService;
            _searchService = searchService;
        }
        public async Task<IActionResult> Index()
        {
            var games = await _searchService.SearchGamesByFilter(new FilterCriteria());
            return View(games);
        }

        public async Task<IActionResult> Details(int id)
        {
            var booking = await _bookingService.GetBookingInformationForSpecificGame(id);
            if (booking == null)
            {
                return NotFound();
            }

            var unavailableRanges = await _bookingService.GetUnavailableTimeRanges(id);
            ViewBag.UnavailableRanges = unavailableRanges;
            booking = booking with { ImageUrl = GameImageMapper.GetImageUrl(booking.Name) };
            return View(booking);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmBooking(int id, DateTime startDate, DateTime endDate)
        {
            var booking = await _bookingService.GetBookingInformationForSpecificGame(id);
            if (booking == null)
            {
                return NotFound();
            }

            var timeRange = new TimeRange(startDate, endDate);
            bool isAvailable = await _bookingService.CheckGameAvailability(id, timeRange);

            if (!isAvailable)
            {
                TempData["Error"] = "The game is not available for the selected period.";
                return RedirectToAction("Details", new { id });
            }

            decimal totalPrice = _bookingService.CalculateTotalPriceForRentingASpecificGame(booking.Price, timeRange);
            int totalDays = _bookingService.CalculateNumberOfDaysInAGivenTimeRange(timeRange);

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.TotalPrice = totalPrice;
            ViewBag.TotalDays = totalDays;

            return View(booking);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmBooking(int id, DateTime startDate, DateTime endDate, string confirm)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int clientId))
            {
                return Unauthorized();
            }

            var timeRange = new TimeRange(startDate, endDate);
            await _bookingService.AddBooking(id, clientId, timeRange);

            TempData["Success"] = "Booking confirmed successfully!";
            return RedirectToAction("Index", "Dashboard");
        }
    }
}