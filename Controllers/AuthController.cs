using Microsoft.AspNetCore.Mvc;
using AuthUser.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AuthUser.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly UserService _userService;

        public AuthController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] string username, [FromForm]string email, [FromForm] string password)
        {
            try
            {
                await _userService.RegisterUser(username, email, password);
                return RedirectToAction("Login");
            }
            catch
            {
                ModelState.AddModelError("", "An error occurred while registering.");
                return View();
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] string email, [FromForm] string password)
        {
            try
            {
                var token = await _userService.LoginUser(email, password);
                if (token == null)
                {
                    ModelState.AddModelError("", "Invalid email or password.");
                    return View();
                }
                var username = await _userService.GetUsernameByEmail(email);
                var userId = await _userService.GetUserIdByEmail(email); 
                HttpContext.Session.SetString("Token", token);
                HttpContext.Session.SetString("UserEmail", email);
                HttpContext.Session.SetString("Username", username);
                HttpContext.Session.SetString("UserId", userId.ToString());

                return RedirectToAction("Main", "User");
            }
            catch
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }
        }
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Ok(new { userId });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Limpa os dados da sessão
            HttpContext.Session.Remove("Token");
            HttpContext.Session.Remove("UserEmail");
            return RedirectToAction("Login"); // Redireciona para a página de login
        }
    }
}


