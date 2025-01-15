using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthUser.Controllers
{
    [Route("user")]
    [ApiController]
    public class UserController : Controller
    {
        [Authorize]
        [HttpGet("main")]
        public IActionResult Main()
        {
            // Obtém o username diretamente do token JWT
            var username = User.FindFirst(ClaimTypes.Name)?.Value;

            // Verifica se o username foi encontrado no token
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("Usuário não autenticado ou token inválido.");
            }

            // Passa o username para a view
            ViewBag.Username = username;

            // Retorna a página Main
            return View("~/Views/User/Main.cshtml");
        }
    }
}
