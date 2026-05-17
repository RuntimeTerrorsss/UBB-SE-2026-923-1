using BookingBoardGames.Data.Enum;
using BookingBoardGames.Data.Interfaces;
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
        private readonly IConversationService _conversationService;
        private readonly IConversationRepository _conversationRepository;

        public GamesController(InterfaceBookingService bookingService, InterfaceSearchAndFilterService searchService, IConversationService conversationService, IConversationRepository conversationRepository)
        {
            _bookingService = bookingService;
            _searchService = searchService;
            _conversationService = conversationService;
            _conversationRepository = conversationRepository;
        }
        public async Task<IActionResult> Index()
        {
            var games = await _searchService.SearchGamesByFilter(new FilterCriteria());
            return View(games);
        }

        [AllowAnonymous]
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
            var redirect = RequireLogin();
            if (redirect != null) return redirect;

            int clientId = CurrentUserId ?? -1;
            if (clientId == -1) return Unauthorized();

            var timeRange = new TimeRange(startDate, endDate);
            var booking = await _bookingService.GetBookingInformationForSpecificGame(id);
            if (booking == null) return NotFound();

            try
            {
                await _bookingService.AddBooking(id, clientId, timeRange);
            }
            catch (Exception)
            {
                TempData["Error"] = "This game is already booked for the selected period.";
                return RedirectToAction("Details", new { id });
            }

            try
            {
                _conversationService.Initialize(clientId);
                int conversationId = await _conversationRepository.FindOrCreateConversationBetweenUsers(clientId, booking.UserId);

                int totalDays = _bookingService.CalculateNumberOfDaysInAGivenTimeRange(timeRange);
                decimal totalPrice = _bookingService.CalculateTotalPriceForRentingASpecificGame(booking.Price, timeRange);

                var rentalMessage = new MessageDataTransferObject(
                    Id: 0,
                    ConversationId: conversationId,
                    SenderId: clientId,
                    ReceiverId: booking.UserId,
                    SentAt: DateTime.Now,
                    Content: $"{booking.Name}: {startDate:dd MMM yyyy} – {endDate:dd MMM yyyy} ({totalDays} day(s), total {totalPrice})",
                    Type: MessageType.MessageRentalRequest,
                    ImageUrl: string.Empty,
                    IsResolved: false,
                    IsAccepted: false,
                    IsAcceptedByBuyer: false,
                    IsAcceptedBySeller: false,
                    PaymentId: -1,
                    RequestId: id
                );

                await _conversationService.SendMessage(rentalMessage);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Booking saved but message failed: {ex.Message}";
                return RedirectToAction("Index", "Games");
            }

            TempData["Success"] = "Booking request sent!";
            return RedirectToAction("Index", "Home");
        }

    }
}