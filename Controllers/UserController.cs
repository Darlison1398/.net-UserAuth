using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthUser.Controllers
{
    [Route("user")]
    [ApiController]
    public class UserController : Controller
    {

        private readonly AppDbContext _context;

        // Construtor para injetar o DbContext
        public UserController(AppDbContext context)
        {
            _context = context;
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
                return Unauthorized("Usuário não autenticado ou token inválido.");
            }

            // Passa o username para a view
            ViewBag.Username = username;
            
            var parsedUserId = int.Parse(userId);
            var products = _context.Products.Where(p => p.UserId == parsedUserId).ToList();

            // Retorna a página Main
            return View("~/Views/User/Main.cshtml", products);
        }
    }
}
