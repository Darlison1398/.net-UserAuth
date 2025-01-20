using System.Security.Claims;
using AuthUser.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthUser.Controllers
{
    [Authorize]
    [Route("products")]
    [ApiController]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpGet("index")] 
        public IActionResult Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var products = _context.Products.Where(p => p.UserId == userId).ToList();
            return View(products);
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create([FromForm] string description, [FromForm] int quantity)
        {
            if (ModelState.IsValid)
            {
                var userIdClaim = User.FindFirstValue("userId");

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    TempData["ErrorMessage"] = "Error: Invalid user ID.";
                    return Unauthorized("The user ID in the token is invalid."); // Retorna erro caso o ID seja inválido
                }

                var product = new Product
                {
                    Description = description,
                    Quantity = quantity,
                    UserId = userId
                };

                _context.Products.Add(product);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Product created successfully!";
                return RedirectToAction("Main", "User"); // Redireciona para a página principal
            }

            TempData["ErrorMessage"] = "Error: Could not create the product.";
            return View();
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int? id){
            if (id == null)
            {
                return NotFound();
            }
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            var userIdClaim = User.FindFirstValue("userId");
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("You can't edit this product."); // Retorna erro caso o ID seja inválido
            }
            return View(product);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] string description, [FromForm] int quantity)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Error: Product not found.";
                return NotFound();
            }

            // Verifica se o produto pertence ao usuário logado
            var userIdClaim = User.FindFirstValue("userId");
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId) || product.UserId != userId)
            {
                TempData["ErrorMessage"] = "Error: You cannot edit this product.";
                return Unauthorized("You don't have permission to edit that product.");
            }

            // Atualiza os valores do produto
            product.Description = description;
            product.Quantity = quantity;

            // Salva as alterações no banco de dados
            _context.Update(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product updated successfully!";
            return RedirectToAction("Main", "User");
        }


        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Error: Product not found.";
                return NotFound();
            }

            // Verifica se o produto pertence ao usuário logado
            var userIdClaim = User.FindFirstValue("userId");
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId) || product.UserId != userId)
            {
                TempData["ErrorMessage"] = "Error: You cannot delete this product.";
                return Unauthorized("You are not allowed to delete this product.");
            }

            // Remove o produto do banco de dados
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Product deleted successfully!";
            return RedirectToAction("Main", "User");
        }


    }
}