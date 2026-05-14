using BookingBoardGames.Sharing.DTO;
using BookingBoardGames.Sharing.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BookingBoardGames.Web.Controllers
{
    //[Authorize]
    public class GamesController : Controller
    {
        private readonly InterfaceBookingService _bookingService;

        public GamesController(InterfaceBookingService bookingService)
        {
            _bookingService = bookingService;
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