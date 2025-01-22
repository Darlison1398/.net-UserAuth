using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using AuthUser.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class UserService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public UserService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<bool> RegisterUser(string username, string email, string password)
    {
        if (_context.Users.Any(u => u.Email == email))
        {
            throw new Exception("Email already exists");
        }

        var user = new User(username, email, BCrypt.Net.BCrypt.HashPassword(password));

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string> LoginUser(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            throw new Exception("Invalid username");
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            throw new Exception("Invalid password");
        }

        // Gera o token JWT
        return GenerateJwtToken(user);
    }

    // Método para gerar o token JWT
    private string GenerateJwtToken(User user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user), "User cannot be null");
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            //new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            //new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            new Claim("userId", user.Id.ToString())

        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GetUsernameByEmail(string email)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user != null)
        {
            return user.Username; 
        }
        return null; 
    }

    public async Task<int?> GetUserIdByEmail(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        
        // Retorna o ID do usuário ou null caso o usuário não seja encontrado
        return user?.Id;
    }

    public async Task<User> GetUserByIdAsync(int userId)
    {
        // Supondo que você tenha um DbContext configurado para acessar o banco
        return await _context.Users.FindAsync(userId);
    }

    public async Task<User> GetUserByEmailAsync(string email)
    {
        //return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> GetUserById(int id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<bool> UpdateUser(User user)
    {
        _context.Users.Update(user);
        return await _context.SaveChangesAsync() > 0;
    }

}