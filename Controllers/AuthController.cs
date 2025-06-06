using Microsoft.AspNetCore.Mvc;
using AuthUser.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

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
                TempData["ErrorMessage"] = "An error occurred while registering.";
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
                    TempData["ErrorMessage"] = "Erro: Invalid email or password.";
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
                TempData["ErrorMessage"] = "Erro: Invalid email or password.";
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }
        }


        [HttpGet("perfil")]
        public async Task<IActionResult> Perfil()
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
            return RedirectToAction("Index", "Home"); // Redireciona para a página de login
        }

    }
}


