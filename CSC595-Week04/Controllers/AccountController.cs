using CSC595_Week04.Data;
using CSC595_Week04.Views;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CSC595_Week04.Controllers
{
    /// <summary>
    /// Manages user authentication: registration, login, and logout.
    /// Uses ASP.NET Core Identity for password hashing and sign-in management.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly SignInManager<ProductUser> _signInManager;
        private readonly UserManager<ProductUser> _userManager;

        public AccountController(SignInManager<ProductUser> signInManager,
                                 UserManager<ProductUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET /Account/Register — Display the registration form
        public IActionResult Register()
        {
            return View();
        }

        // POST /Account/Register — Create a new user account from form data
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerModel)
        {
            if (!ModelState.IsValid)
            {
                return View(registerModel);
            }

            // Build the Identity user from the submitted registration form
            ProductUser newUser = new()
            {
                FirstName = registerModel.FirstName,
                LastName  = registerModel.LastName,
                UserName  = registerModel.UserName,
                Email     = registerModel.Email
            };

            var result = await _userManager.CreateAsync(newUser, registerModel.Password);

            if (result.Succeeded)
            {
                // Registration successful — send user straight to the game library
                return RedirectToAction("Index", "Product");
            }

            // Surface Identity errors (e.g. duplicate username, weak password)
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(registerModel);
        }

        // GET /Account/Login — Display the login form
        public IActionResult Login()
        {
            // Redirect already-authenticated users away from the login page
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Product");
            }
            return View();
        }

        // POST /Account/Login — Validate credentials and sign the user in
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Failed to log in");
                return View(loginModel);
            }

            // lockoutOnFailure: false — account lockout is not enabled for this app
            var result = await _signInManager.PasswordSignInAsync(
                loginModel.UserName, loginModel.Password, loginModel.Remember, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Product");
            }

            ModelState.AddModelError("", "Invalid username or password.");
            return View(loginModel);
        }

        // GET /Account/Logout — Sign the user out and return to the game list
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Product");
        }
    }
}
