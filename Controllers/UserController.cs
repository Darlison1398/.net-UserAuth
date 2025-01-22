using AuthUser.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthUser.Controllers
{
    [Route("user")]
    [ApiController]
    public class UserController : Controller
    {

        private readonly UserService _userService;
        private readonly AppDbContext _context;

        // Construtor para injetar o DbContext
        public UserController(AppDbContext context, UserService _userService)
        {
            _context = context;
            _userService = _userService;
        }

        [Authorize]
        [HttpGet("main")]
        public IActionResult Main()
        {
            // Obtém o username diretamente do token JWT
            //var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var username = HttpContext.Session.GetString("Username");
            var userId = HttpContext.Session.GetString("UserId");

            // Verifica se o username foi encontrado no token
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("Unauthenticated user or invalid token.");
            }

            // Passa o username para a view
            ViewBag.Username = username;
            
            var parsedUserId = int.Parse(userId);
            var products = _context.Products.Where(p => p.UserId == parsedUserId).ToList();

            // Retorna a página Main
            return View("~/Views/User/Main.cshtml", products);
        }


        [Authorize]
        [HttpGet("Perfil")]
        public IActionResult Perfil()
        {
            //var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("Username");
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if ( string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(); // Se não encontrar os dados, retorna um erro (não autenticado)
            }

            var userProfile = new UserProfile
            {
                //UserId = userId,
                Username = username,
                UserEmail = userEmail
            };
            return View(userProfile);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Update([FromForm] UserProfile userProfile)
        {
            if (!ModelState.IsValid)
            {
                return View(userProfile);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier); 
            //var userIdClaim = User.FindFirstValue("userId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                TempData["ErrorMessage"] = "Error: You cannot edit this profile.";
                return Unauthorized("You don't have permission to edit this profile.");
            }

            var user = await _userService.GetUserById(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.Username = userProfile.Username;
            user.Email = userProfile.UserEmail;
            user.PasswordHash = userProfile.UserPassword;

            var result = await _userService.UpdateUser(user);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Failed to update user profile.");
                return View(userProfile);
            }

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Main");
        }

    }
}
