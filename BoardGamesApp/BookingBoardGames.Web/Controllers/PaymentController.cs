using System;
using BookingBoardGames.Sharing.Services;
using BookingBoardGames.Web.Models.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingBoardGames.Web.Controllers
{
    [Authorize]
    public class PaymentController : BaseController
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var redirect = RequireLogin();
            if (redirect != null)
            {
                return redirect;
            }

            return RedirectToAction(nameof(CardPayment));
        }

        [HttpGet]
        public IActionResult CardPayment()
        {
            var redirect = RequireLogin();
            if (redirect != null)
            {
                return redirect;
            }

            return View(new PaymentViewModel
            {
                PaymentMethod = "Card",
                DateOfTransaction = DateTime.Now,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CardPayment(PaymentViewModel model)
        {
            var redirect = RequireLogin();
            if (redirect != null)
            {
                return redirect;
            }

            model.PaymentMethod = "Card";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ViewBag.SuccessMessage = "Payment submitted successfully.";
            return View(model);
        }
    }
}
