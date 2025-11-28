using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LittleBugShop.Data;
using LittleBugShop.Models;
using LittleBugShop.Services;

namespace LittleBugShop.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        /// <summary>
        /// Get current user's profile with addresses
        /// </summary>
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var user = Database.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            var addresses = Database.Addresses.Where(a => a.UserId == userId).ToList();

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
                Addresses = addresses
            });
        }

        /// <summary>
        /// Update current user's profile
        /// </summary>
        [HttpPut("profile")]
        public IActionResult UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var user = Database.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(request.Email))
                user.Email = request.Email;
            if (!string.IsNullOrWhiteSpace(request.FirstName))
                user.FirstName = request.FirstName;
            if (!string.IsNullOrWhiteSpace(request.LastName))
                user.LastName = request.LastName;
            if (request.PhoneNumber != null) // Allow setting to null
                user.PhoneNumber = request.PhoneNumber;

            user.UpdatedAt = DateTime.UtcNow;

            return Ok(new { message = "Profile updated successfully", user = new { user.Id, user.Username, user.Email, user.FirstName, user.LastName, user.PhoneNumber, user.UpdatedAt } });
        }

        /// <summary>
        /// Add a new address
        /// </summary>
        [HttpPost("profile/addresses")]
        public IActionResult AddAddress([FromBody] AddAddressRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var user = Database.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            var newAddress = new Address
            {
                Id = Database.Addresses.Any() ? Database.Addresses.Max(a => a.Id) + 1 : 1,
                UserId = userId,
                AddressType = request.AddressType,
                Street = request.Street,
                City = request.City,
                State = request.State,
                PostalCode = request.PostalCode,
                Country = request.Country,
                IsDefault = request.IsDefault
            };

            // If setting as default, unset other defaults
            if (newAddress.IsDefault)
            {
                foreach (var addr in Database.Addresses.Where(a => a.UserId == userId && a.IsDefault))
                {
                    addr.IsDefault = false;
                }
            }

            Database.Addresses.Add(newAddress);
            user.AddressIds.Add(newAddress.Id);
            user.UpdatedAt = DateTime.UtcNow;

            return CreatedAtAction(nameof(GetProfile), new { id = newAddress.Id }, new { message = "Address added successfully", address = newAddress });
        }

        /// <summary>
        /// Update an existing address
        /// </summary>
        [HttpPut("profile/addresses/{id}")]
        public IActionResult UpdateAddress(int id, [FromBody] AddAddressRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var address = Database.Addresses.FirstOrDefault(a => a.Id == id && a.UserId == userId);

            if (address == null)
                return NotFound(new { message = "Address not found or access denied" });

            // Update fields
            address.AddressType = request.AddressType;
            address.Street = request.Street;
            address.City = request.City;
            address.State = request.State;
            address.PostalCode = request.PostalCode;
            address.Country = request.Country;
            address.IsDefault = request.IsDefault;

            // If setting as default, unset other defaults
            if (address.IsDefault)
            {
                foreach (var addr in Database.Addresses.Where(a => a.UserId == userId && a.Id != id && a.IsDefault))
                {
                    addr.IsDefault = false;
                }
            }

            var user = Database.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
                user.UpdatedAt = DateTime.UtcNow;

            return Ok(new { message = "Address updated successfully", address });
        }

        /// <summary>
        /// Delete an address
        /// </summary>
        [HttpDelete("profile/addresses/{id}")]
        public IActionResult DeleteAddress(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var address = Database.Addresses.FirstOrDefault(a => a.Id == id && a.UserId == userId);

            if (address == null)
                return NotFound(new { message = "Address not found or access denied" });

            Database.Addresses.Remove(address);

            var user = Database.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.AddressIds.Remove(id);
                user.UpdatedAt = DateTime.UtcNow;
            }

            return Ok(new { message = "Address deleted successfully" });
        }

        /// <summary>
        /// Set an address as default
        /// </summary>
        [HttpPut("profile/addresses/{id}/set-default")]
        public IActionResult SetDefaultAddress(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var address = Database.Addresses.FirstOrDefault(a => a.Id == id && a.UserId == userId);

            if (address == null)
                return NotFound(new { message = "Address not found or access denied" });

            // Unset all other defaults for this user
            foreach (var addr in Database.Addresses.Where(a => a.UserId == userId && a.IsDefault))
            {
                addr.IsDefault = false;
            }

            address.IsDefault = true;

            var user = Database.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
                user.UpdatedAt = DateTime.UtcNow;

            return Ok(new { message = "Default address updated successfully", address });
        }

        /// <summary>
        /// Change password
        /// </summary>
        [HttpPut("profile/change-password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var user = Database.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            // Verify old password
            if (user.Password != request.OldPassword)
                return BadRequest(new { message = "Old password is incorrect" });

            // Validate new password
            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
                return BadRequest(new { message = "New password must be at least 6 characters long" });

            user.Password = request.NewPassword;
            user.UpdatedAt = DateTime.UtcNow;

            return Ok(new { message = "Password changed successfully" });
        }
    }

    public class UpdateProfileRequest
    {
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class AddAddressRequest
    {
        public AddressType AddressType { get; set; }
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
