using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AuthUser.Models;

namespace AuthUser.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        if (User.Identity.IsAuthenticated) // Verifica se o usuário está logado
        {
            var userName = HttpContext.Session.GetString("Username");
            ViewData["UserName"] = userName ?? "Usuário"; // Armazena no ViewData (valor padrão "Usuário")
        }
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
