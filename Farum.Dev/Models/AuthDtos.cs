namespace LittleBugShop.Models
{
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public UserInfo User { get; set; } = new UserInfo();
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class RegisterResponse
    {
        public string Message { get; set; } = string.Empty;
        public UserInfo User { get; set; } = new UserInfo();
    }

    public class UserInfo
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
