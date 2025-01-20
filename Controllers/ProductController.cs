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
                    TempData["ErrorMessage"] = "Erro: ID do usuário inválido.";
                    return Unauthorized("O ID do usuário no token é inválido."); // Retorna erro caso o ID seja inválido
                }

                var product = new Product
                {
                    Description = description,
                    Quantity = quantity,
                    UserId = userId
                };

                _context.Products.Add(product);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Produto criado com sucesso!";
                return RedirectToAction("Main", "User"); // Redireciona para a página principal
            }

            TempData["ErrorMessage"] = "Erro: Não foi possível criar o produto.";
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
                return Unauthorized("Você não pode editar esse produto."); // Retorna erro caso o ID seja inválido
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
                TempData["ErrorMessage"] = "Erro: Produto não encontrado.";
                return NotFound();
            }

            // Verifica se o produto pertence ao usuário logado
            var userIdClaim = User.FindFirstValue("userId");
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId) || product.UserId != userId)
            {
                TempData["ErrorMessage"] = "Erro: Você não pode editar este produto.";
                return Unauthorized("Você não tem permissão para editar esse produto.");
            }

            // Atualiza os valores do produto
            product.Description = description;
            product.Quantity = quantity;

            // Salva as alterações no banco de dados
            _context.Update(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Produto atualizado com sucesso!";
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
                TempData["ErrorMessage"] = "Erro: Produto não encontrado.";
                return NotFound();
            }

            // Verifica se o produto pertence ao usuário logado
            var userIdClaim = User.FindFirstValue("userId");
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId) || product.UserId != userId)
            {
                TempData["ErrorMessage"] = "Erro: Você não pode excluir este produto.";
                return Unauthorized("Você não tem permissão para deletar esse produto.");
            }

            // Remove o produto do banco de dados
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Produto excluído com sucesso!";
            return RedirectToAction("Main", "User");
        }


    }
}