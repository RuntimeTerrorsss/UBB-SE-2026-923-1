using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Sharing.Services;
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
            this.userService = userService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Login(string identifier, string password)
        {
            if (string.IsNullOrEmpty(identifier) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please complete all fields";
                return View();
            }

            var user = await userService.LoginAsync(identifier, password);

            if (user != null)
            {
               
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Username/Email or password incorrect.";
            return View();
        }
    }
}
