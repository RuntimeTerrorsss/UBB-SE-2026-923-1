using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Sharing.Services;
using BookingBoardGames.Web.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingBoardGames.Web.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IUserService userService;

        public AccountController(IUserService userService)
        {
            userService = userService;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registeringUserViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(registeringUserViewModel);
            }

            var user = new User
            {
                Username = registeringUserViewModel.Username,
                DisplayName = registeringUserViewModel.DisplayName,
                Email = registeringUserViewModel.Email,
                PasswordHash = registeringUserViewModel.Password,
                City = registeringUserViewModel.City,
                Country = registeringUserViewModel.Country,
                PhoneNumber = registeringUserViewModel.PhoneNumber,
                Street = registeringUserViewModel.Street,
                StreetNumber = registeringUserViewModel.StreetNumber
            };
            var success = await userService.RegisterUserAsync(user);

            if (!success)
            {
                ModelState.AddModelError("", "Registration failed. Try again.");
                return View(registeringUserViewModel);
            }

            return RedirectToAction("Login");
        }
        // Action methods here...
    }
}
