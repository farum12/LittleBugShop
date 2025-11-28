namespace LittleBugShop.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } = "User"; // "Admin" or "User"
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property - addresses will be stored separately and joined via UserId
        public List<int> AddressIds { get; set; } = new List<int>();
    }
}
