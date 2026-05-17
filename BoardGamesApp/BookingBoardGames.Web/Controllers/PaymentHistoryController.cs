using BookingBoardGames.Data.Enum;
using BookingBoardGames.Sharing.Services;
using BookingBoardGames.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace BookingBoardGames.Web.Controllers
{
    public class PaymentHistoryController : BaseController
    {
        private readonly IServicePayment _servicePayment;

        public PaymentHistoryController(IServicePayment servicePayment)
        {
            _servicePayment = servicePayment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var redirect = RequireLogin();
            if (redirect != null) return redirect;

            var userId = CurrentUserId ?? -1;
            BookingBoardGames.Data.Enum.SessionContext.GetInstance().UserId = userId;

            var result = await _servicePayment.GetFilteredPayments(
                filter: FilterType.Newest,
                paymentMethod: PaymentMethod.ALL,
                searchQuery: string.Empty,
                pageNumber: 1,
                pageSize: 10
            );

            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Filter(
            FilterType filter,
            PaymentMethod paymentMethod,
            string searchQuery = "",
            int pageNumber = 1,
            int pageSize = 10)
        {
            var redirect = RequireLogin();
            if (redirect != null) return Json(new { error = "Not logged in" });

            var userId = CurrentUserId ?? -1;
            BookingBoardGames.Data.Enum.SessionContext.GetInstance().UserId = userId;


            var result = await _servicePayment.GetFilteredPayments(
                filter,
                paymentMethod,
                searchQuery,
                pageNumber,
                pageSize
            );

            var totalAmount = _servicePayment.CalculateTotalAmount(result.Items);

            return Json(new
            {
                items = result.Items,
                totalCount = result.TotalCount,
                totalPages = result.TotalPages,
                pageNumber = result.PageNumber,
                totalAmount = totalAmount.ToString("C")
            });
        }
    }
}