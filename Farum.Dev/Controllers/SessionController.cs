using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LittleBugShop.Data;

namespace LittleBugShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SessionController : ControllerBase
    {
        /// <summary>
        /// Get current user's session details
        /// </summary>
        [HttpGet]
        public IActionResult GetSession()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var user = Database.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            // Get JWT token from Authorization header or cookie
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
            {
                token = Request.Cookies["AuthToken"];
            }

            // Decode JWT to get expiration time
            DateTime? tokenExpiration = null;
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);
                    tokenExpiration = jwtToken.ValidTo;
                }
                catch
                {
                    // If token parsing fails, continue without expiration
                }
            }

            var addressCount = Database.Addresses.Count(a => a.UserId == userId);
            var orderCount = Database.Orders.Count(o => o.UserId == userId);

            return Ok(new
            {
                session = new
                {
                    isAuthenticated = true,
                    tokenExpiration = tokenExpiration,
                    tokenExpiresIn = tokenExpiration.HasValue 
                        ? (tokenExpiration.Value - DateTime.UtcNow).TotalMinutes 
                        : (double?)null,
                    jwt = token
                },
                user = new
                {
                    id = user.Id,
                    username = user.Username,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    fullName = $"{user.FirstName} {user.LastName}".Trim(),
                    phoneNumber = user.PhoneNumber,
                    role = user.Role,
                    createdAt = user.CreatedAt,
                    updatedAt = user.UpdatedAt
                },
                stats = new
                {
                    addressCount,
                    orderCount
                }
            });
        }
    }
}
