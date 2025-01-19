namespace AuthUser.Models
{
    public class Product 
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}

