using System.ComponentModel.DataAnnotations;

namespace AuthUser.Models
{
    public class User 
    {
        public int Id { get; set; }

        public string Username { get; set; }
        
        [MaxLength(255)]
        public string Email { get; set; }

        public string PasswordHash  { get; set; }

        public ICollection<Product> Products { get; set; }
        public User()
        {
            Products = new List<Product>();
        }

        public User(string username, string email, string passwordHash)
        {
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            Products = new List<Product>();
        }
    }

}