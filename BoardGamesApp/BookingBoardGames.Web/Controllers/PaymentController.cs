using System;
using System.Threading.Tasks;
using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Sharing.Services;
using BookingBoardGames.Web.Models.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingBoardGames.Web.Controllers
{
    [Authorize]
    public class PaymentController : BaseController
    {
        private readonly ICardPaymentService _cardPaymentService;

        public PaymentController(ICardPaymentService cardPaymentService)
        {
            _cardPaymentService = cardPaymentService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var redirect = RequireLogin();
            if (redirect != null) return redirect;

            return RedirectToAction(nameof(CardPayment));
        }

        [HttpGet]
        public IActionResult CardPayment(int requestIdentifier, int clientIdentifier, int ownerIdentifier)
        {
            var redirect = RequireLogin();
            if (redirect != null) return redirect;

            return View(new PaymentViewModel
            {
                RequestIdentifier = requestIdentifier,
                ClientIdentifier = clientIdentifier,
                OwnerIdentifier = ownerIdentifier,
                PaymentMethod = "Card",
                DateOfTransaction = DateTime.Now,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CardPayment(PaymentViewModel model)
        {
            var redirect = RequireLogin();
            if (redirect != null) return redirect;

            model.PaymentMethod = "Card";

            if (model.RequestIdentifier <= 0)
            {
                ModelState.AddModelError(nameof(model.RequestIdentifier), "Invalid request identifier.");
            }

            if (model.ClientIdentifier <= 0)
            {
                ModelState.AddModelError(nameof(model.ClientIdentifier), "Invalid client identifier.");
            }

            if (model.OwnerIdentifier <= 0)
            {
                ModelState.AddModelError(nameof(model.OwnerIdentifier), "Invalid owner identifier.");
            }

            if (!ModelState.IsValid)
            {
                model.CardNumber = string.Empty;
                model.Cvv = string.Empty;
                model.CardholderName = string.Empty;
                model.Expiry = string.Empty;
                return View(model);
            }

            try
            {
                var result = await _cardPaymentService.AddCardPayment(
                    model.RequestIdentifier,
                    model.ClientIdentifier,
                    model.OwnerIdentifier,
                    model.Amount
                );

                ViewBag.SuccessMessage = $"Payment successful! Transaction ID: {result.TransactionIdentifier}";
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }
    }
}