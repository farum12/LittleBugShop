using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LittleBugShop.Data;
using LittleBugShop.Models;

namespace LittleBugShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CouponsController : ControllerBase
    {
        /// <summary>
        /// Validate a coupon code (preview without applying)
        /// </summary>
        [HttpGet("validate/{code}")]
        [Authorize]
        public IActionResult ValidateCoupon(string code)
        {
            var coupon = Database.Coupons.FirstOrDefault(c => c.Code.ToUpper() == code.ToUpper());

            if (coupon == null)
                return NotFound(new { message = "Coupon code not found", isValid = false });

            var validationResult = ValidateCouponRules(coupon);

            if (!validationResult.IsValid)
            {
                return Ok(new
                {
                    message = validationResult.ErrorMessage,
                    isValid = false,
                    coupon = new { coupon.Code, coupon.Type, coupon.Value }
                });
            }

            return Ok(new
            {
                message = "Coupon is valid",
                isValid = true,
                coupon = new
                {
                    coupon.Code,
                    coupon.Type,
                    coupon.Value,
                    coupon.ExpirationDate,
                    usesRemaining = coupon.MaxUsesTotal.HasValue ? coupon.MaxUsesTotal.Value - coupon.CurrentUses : (int?)null
                }
            });
        }

        /// <summary>
        /// Admin: Get all coupons
        /// </summary>
        [HttpGet("admin/coupons")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAllCoupons()
        {
            var coupons = Database.Coupons.Select(c => new
            {
                c.Id,
                c.Code,
                c.Type,
                c.Value,
                c.ExpirationDate,
                c.MaxUsesTotal,
                c.IsActive,
                c.CurrentUses,
                c.CreatedAt,
                IsExpired = c.ExpirationDate.HasValue && c.ExpirationDate.Value < DateTime.UtcNow,
                UsesRemaining = c.MaxUsesTotal.HasValue ? c.MaxUsesTotal.Value - c.CurrentUses : (int?)null
            }).ToList();

            return Ok(coupons);
        }

        /// <summary>
        /// Admin: Create new coupon
        /// </summary>
        [HttpPost("admin/coupons")]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateCoupon([FromBody] CreateCouponRequest request)
        {
            // Validate code uniqueness
            if (Database.Coupons.Any(c => c.Code.ToUpper() == request.Code.ToUpper()))
                return BadRequest(new { message = "Coupon code already exists" });

            if (string.IsNullOrWhiteSpace(request.Code) || request.Code.Length < 3)
                return BadRequest(new { message = "Coupon code must be at least 3 characters" });

            if (request.Value <= 0)
                return BadRequest(new { message = "Discount value must be greater than 0" });

            if (request.Type == DiscountType.Percentage && request.Value > 100)
                return BadRequest(new { message = "Percentage discount cannot exceed 100%" });

            var coupon = new Coupon
            {
                Id = Database.Coupons.Any() ? Database.Coupons.Max(c => c.Id) + 1 : 1,
                Code = request.Code.ToUpper(),
                Type = request.Type,
                Value = request.Value,
                ExpirationDate = request.ExpirationDate,
                MaxUsesTotal = request.MaxUsesTotal,
                IsActive = true,
                CurrentUses = 0,
                CreatedAt = DateTime.UtcNow
            };

            Database.Coupons.Add(coupon);

            return CreatedAtAction(nameof(GetAllCoupons), new { id = coupon.Id }, new
            {
                message = "Coupon created successfully",
                coupon
            });
        }

        /// <summary>
        /// Admin: Update coupon
        /// </summary>
        [HttpPut("admin/coupons/{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateCoupon(int id, [FromBody] UpdateCouponRequest request)
        {
            var coupon = Database.Coupons.FirstOrDefault(c => c.Id == id);
            if (coupon == null)
                return NotFound(new { message = "Coupon not found" });

            // Validate code uniqueness if changing
            if (!string.IsNullOrWhiteSpace(request.Code) && request.Code.ToUpper() != coupon.Code)
            {
                if (Database.Coupons.Any(c => c.Code.ToUpper() == request.Code.ToUpper() && c.Id != id))
                    return BadRequest(new { message = "Coupon code already exists" });

                coupon.Code = request.Code.ToUpper();
            }

            if (request.Value.HasValue)
            {
                if (request.Value.Value <= 0)
                    return BadRequest(new { message = "Discount value must be greater than 0" });

                if (coupon.Type == DiscountType.Percentage && request.Value.Value > 100)
                    return BadRequest(new { message = "Percentage discount cannot exceed 100%" });

                coupon.Value = request.Value.Value;
            }

            if (request.ExpirationDate.HasValue)
                coupon.ExpirationDate = request.ExpirationDate;

            if (request.MaxUsesTotal.HasValue)
                coupon.MaxUsesTotal = request.MaxUsesTotal;

            if (request.IsActive.HasValue)
                coupon.IsActive = request.IsActive.Value;

            return Ok(new { message = "Coupon updated successfully", coupon });
        }

        /// <summary>
        /// Admin: Delete coupon
        /// </summary>
        [HttpDelete("admin/coupons/{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteCoupon(int id)
        {
            var coupon = Database.Coupons.FirstOrDefault(c => c.Id == id);
            if (coupon == null)
                return NotFound(new { message = "Coupon not found" });

            Database.Coupons.Remove(coupon);

            // Remove associated usage records
            var usages = Database.CouponUsages.Where(u => u.CouponId == id).ToList();
            foreach (var usage in usages)
            {
                Database.CouponUsages.Remove(usage);
            }

            return Ok(new { message = "Coupon deleted successfully" });
        }

        /// <summary>
        /// Admin: Get coupon usage statistics
        /// </summary>
        [HttpGet("admin/coupons/{id}/usage")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetCouponUsage(int id)
        {
            var coupon = Database.Coupons.FirstOrDefault(c => c.Id == id);
            if (coupon == null)
                return NotFound(new { message = "Coupon not found" });

            var usages = Database.CouponUsages
                .Where(u => u.CouponId == id)
                .Select(u => new
                {
                    u.Id,
                    u.UserId,
                    Username = Database.Users.FirstOrDefault(user => user.Id == u.UserId)?.Username,
                    u.OrderId,
                    u.UsedAt
                })
                .OrderByDescending(u => u.UsedAt)
                .ToList();

            return Ok(new
            {
                coupon = new
                {
                    coupon.Id,
                    coupon.Code,
                    coupon.Type,
                    coupon.Value,
                    coupon.CurrentUses,
                    coupon.MaxUsesTotal,
                    coupon.IsActive
                },
                totalUses = usages.Count,
                usages
            });
        }

        private (bool IsValid, string? ErrorMessage) ValidateCouponRules(Coupon coupon)
        {
            if (!coupon.IsActive)
                return (false, "Coupon is inactive");

            if (coupon.ExpirationDate.HasValue && coupon.ExpirationDate.Value < DateTime.UtcNow)
                return (false, "Coupon has expired");

            if (coupon.MaxUsesTotal.HasValue && coupon.CurrentUses >= coupon.MaxUsesTotal.Value)
                return (false, "Coupon has reached maximum usage limit");

            return (true, null);
        }
    }

    public class CreateCouponRequest
    {
        public string Code { get; set; } = string.Empty;
        public DiscountType Type { get; set; }
        public decimal Value { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int? MaxUsesTotal { get; set; }
    }

    public class UpdateCouponRequest
    {
        public string? Code { get; set; }
        public decimal? Value { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int? MaxUsesTotal { get; set; }
        public bool? IsActive { get; set; }
    }
}
