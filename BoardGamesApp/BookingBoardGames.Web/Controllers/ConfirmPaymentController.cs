using BookingBoardGames.Sharing.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingBoardGames.Web.Controllers
{
    [Authorize]
    public class ConfirmPaymentController : BaseController
    {
        private readonly IReceiptService _receiptService;

        public ConfirmPaymentController(IReceiptService receiptService)
        {
            _receiptService = receiptService;
        }

        // Action methods here...
    }
}
