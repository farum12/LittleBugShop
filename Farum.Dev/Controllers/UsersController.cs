using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LittleBugShop.Data;
using LittleBugShop.Models;
using LittleBugShop.Services;
using System.Linq;

namespace LittleBugShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly JwtService _jwtService;

        public UsersController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }
        [HttpPost("register")]
        public ActionResult<RegisterResponse> Register(RegisterRequest request)
        {
            if (Database.Users.Any(u => u.Username == request.Username))
            {
                return BadRequest(new { message = "Username already exists." });
            }

            if (string.IsNullOrWhiteSpace(request.Username) || request.Username.Length < 3)
            {
                return BadRequest(new { message = "Username must be at least 3 characters long." });
            }

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            {
                return BadRequest(new { message = "Password must be at least 6 characters long." });
            }

            var user = new User
            {
                Id = Database.Users.Any() ? Database.Users.ToList().Max(u => u.Id) + 1 : 1,
                Username = request.Username,
                Password = request.Password,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                AddressIds = new List<int>()
            };

            Database.Users.Add(user);

            var response = new RegisterResponse
            {
                Message = "Registration successful",
                User = new UserInfo
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                }
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, response);
        }

        [HttpPost("login")]
        public ActionResult<LoginResponse> Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username and password are required." });
            }

            var user = Database.Users.FirstOrDefault(u => u.Username == request.Username && u.Password == request.Password);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);

            // Set token in HTTP-only cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Use HTTPS in production
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1)
            };
            Response.Cookies.Append("AuthToken", token, cookieOptions);

            var response = new LoginResponse
            {
                Message = "Login successful",
                Token = token,
                User = new UserInfo
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                }
            };

            return Ok(response);
        }

        [HttpPost("logout")]
        public ActionResult Logout()
        {
            // Remove the authentication cookie
            Response.Cookies.Delete("AuthToken");
            return Ok(new { message = "Logout successful" });
        }

        [HttpGet("{id}")]
        public ActionResult<User> GetUser(int id)
        {
            var user = Database.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        /// <summary>
        /// Admin: Get all users
        /// </summary>
        [HttpGet("admin/users")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAllUsers()
        {
            var users = Database.Users.Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.FirstName,
                u.LastName,
                u.PhoneNumber,
                u.Role,
                u.CreatedAt,
                u.UpdatedAt,
                AddressCount = u.AddressIds.Count
            }).ToList();

            return Ok(users);
        }

        /// <summary>
        /// Admin: Get user by ID with full details
        /// </summary>
        [HttpGet("admin/users/{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetUserById(int id)
        {
            var user = Database.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var addresses = Database.Addresses.Where(a => a.UserId == id).ToList();
            var orders = Database.Orders.Where(o => o.UserId == id).ToList();

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email,
                user.FirstName,
                user.LastName,
                user.PhoneNumber,
                user.Role,
                user.CreatedAt,
                user.UpdatedAt,
                Addresses = addresses,
                OrderCount = orders.Count,
                TotalSpent = orders.Sum(o => o.TotalPrice)
            });
        }

        /// <summary>
        /// Admin: Update user details
        /// </summary>
        [HttpPut("admin/users/{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateUser(int id, [FromBody] AdminUpdateUserRequest request)
        {
            var user = Database.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(request.Email))
                user.Email = request.Email;
            if (!string.IsNullOrWhiteSpace(request.FirstName))
                user.FirstName = request.FirstName;
            if (!string.IsNullOrWhiteSpace(request.LastName))
                user.LastName = request.LastName;
            if (request.PhoneNumber != null)
                user.PhoneNumber = request.PhoneNumber;
            if (!string.IsNullOrWhiteSpace(request.Role))
                user.Role = request.Role;

            user.UpdatedAt = DateTime.UtcNow;

            return Ok(new { message = "User updated successfully", user = new { user.Id, user.Username, user.Email, user.FirstName, user.LastName, user.PhoneNumber, user.Role, user.UpdatedAt } });
        }

        /// <summary>
        /// Admin: Reset user password
        /// </summary>
        [HttpPost("admin/users/{id}/reset-password")]
        [Authorize(Roles = "Admin")]
        public IActionResult ResetPassword(int id, [FromBody] ResetPasswordRequest request)
        {
            var user = Database.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
                return BadRequest(new { message = "New password must be at least 6 characters long" });

            user.Password = request.NewPassword;
            user.UpdatedAt = DateTime.UtcNow;

            return Ok(new { message = "Password reset successfully" });
        }
    }

    public class AdminUpdateUserRequest
    {
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string NewPassword { get; set; } = string.Empty;
    }
}
