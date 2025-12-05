using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LittleBugShop.Data;
using LittleBugShop.Models;
using System.Security.Claims;

namespace LittleBugShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        [HttpGet]
        public ActionResult<Cart> GetCart()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized(new ErrorResponse(401, "User not authenticated."));
            }

            var userId = int.Parse(userIdClaim);
            var cart = Database.Carts.FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                // Create empty cart for user
                cart = new Cart
                {
                    Id = Database.Carts.Any() ? Database.Carts.Max(c => c.Id) + 1 : 1,
                    UserId = userId,
                    LastUpdated = DateTime.UtcNow
                };
                Database.Carts.Add(cart);
            }

            return Ok(cart);
        }

        [HttpPost("items")]
        public ActionResult<Cart> AddToCart([FromBody] AddToCartRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized(new ErrorResponse(401, "User not authenticated."));
            }

            var userId = int.Parse(userIdClaim);

            // Validate product exists
            var product = Database.Products.FirstOrDefault(p => p.Id == request.ProductId);
            if (product == null)
            {
                return NotFound(new ErrorResponse(404, $"Product with ID {request.ProductId} not found."));
            }

            // Validate quantity
            if (request.Quantity <= 0)
            {
                return BadRequest(new ErrorResponse(400, "Quantity must be greater than zero."));
            }

            // Check stock availability
            if (!product.IsAvailable(request.Quantity))
            {
                return BadRequest(new ErrorResponse(400, $"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}"));
            }

            // Get or create cart
            var cart = Database.Carts.FirstOrDefault(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart
                {
                    Id = Database.Carts.Any() ? Database.Carts.Max(c => c.Id) + 1 : 1,
                    UserId = userId,
                    LastUpdated = DateTime.UtcNow
                };
                Database.Carts.Add(cart);
            }

            // Check if product already in cart
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
            if (existingItem != null)
            {
                var newQuantity = existingItem.Quantity + request.Quantity;
                
                // Check stock for new total quantity
                if (!product.IsAvailable(newQuantity))
                {
                    return BadRequest(new ErrorResponse(400, $"Cannot add {request.Quantity} more. Cart has {existingItem.Quantity}, available stock: {product.StockQuantity}"));
                }

                existingItem.Quantity = newQuantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    Id = cart.Items.Any() ? cart.Items.Max(i => i.Id) + 1 : 1,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Author = product.Author,
                    UnitPrice = product.Price,
                    Quantity = request.Quantity
                };
                cart.Items.Add(cartItem);
            }

            cart.LastUpdated = DateTime.UtcNow;
            return Ok(cart);
        }

        [HttpPut("items/{itemId}")]
        public ActionResult<Cart> UpdateCartItem(int itemId, [FromBody] UpdateCartItemRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized(new ErrorResponse(401, "User not authenticated."));
            }

            var userId = int.Parse(userIdClaim);
            var cart = Database.Carts.FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                return NotFound(new ErrorResponse(404, "Cart not found."));
            }

            var cartItem = cart.Items.FirstOrDefault(i => i.Id == itemId);
            if (cartItem == null)
            {
                return NotFound(new ErrorResponse(404, "Item not found in cart."));
            }

            if (request.Quantity <= 0)
            {
                return BadRequest(new ErrorResponse(400, "Quantity must be greater than zero."));
            }

            // Check stock availability
            var product = Database.Products.FirstOrDefault(p => p.Id == cartItem.ProductId);
            if (product != null && !product.IsAvailable(request.Quantity))
            {
                return BadRequest(new ErrorResponse(400, $"Insufficient stock. Available: {product.StockQuantity}"));
            }

            cartItem.Quantity = request.Quantity;
            cart.LastUpdated = DateTime.UtcNow;

            return Ok(cart);
        }

        [HttpDelete("items/{itemId}")]
        public ActionResult<Cart> RemoveFromCart(int itemId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized(new ErrorResponse(401, "User not authenticated."));
            }

            var userId = int.Parse(userIdClaim);
            var cart = Database.Carts.FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                return NotFound(new ErrorResponse(404, "Cart not found."));
            }

            var cartItem = cart.Items.FirstOrDefault(i => i.Id == itemId);
            if (cartItem == null)
            {
                return NotFound(new ErrorResponse(404, "Item not found in cart."));
            }

            cart.Items.Remove(cartItem);
            cart.LastUpdated = DateTime.UtcNow;

            return Ok(cart);
        }

        [HttpDelete]
        public ActionResult<Cart> ClearCart()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized(new ErrorResponse(401, "User not authenticated."));
            }

            var userId = int.Parse(userIdClaim);
            var cart = Database.Carts.FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                return NotFound(new ErrorResponse(404, "Cart not found."));
            }

            cart.Items.Clear();
            cart.AppliedCouponCode = null;
            cart.DiscountAmount = 0;
            cart.LastUpdated = DateTime.UtcNow;

            return Ok(cart);
        }

        [HttpPost("checkout")]
        public ActionResult<Order> CheckoutCart()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized(new ErrorResponse(401, "User not authenticated."));
            }

            var userId = int.Parse(userIdClaim);
            var cart = Database.Carts.FirstOrDefault(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
            {
                return BadRequest(new ErrorResponse(400, "Cart is empty."));
            }

            // Validate all items have sufficient stock
            foreach (var cartItem in cart.Items)
            {
                var product = Database.Products.FirstOrDefault(p => p.Id == cartItem.ProductId);
                if (product == null)
                {
                    return BadRequest(new ErrorResponse(400, $"Product '{cartItem.ProductName}' no longer exists."));
                }

                if (!product.IsAvailable(cartItem.Quantity))
                {
                    return BadRequest(new ErrorResponse(400, $"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}, In cart: {cartItem.Quantity}"));
                }
            }

            // Create order items from cart
            var orderItems = new List<OrderItem>();
            int orderItemId = 1;

            foreach (var cartItem in cart.Items)
            {
                var orderItem = new OrderItem
                {
                    Id = orderItemId++,
                    ProductId = cartItem.ProductId,
                    ProductName = cartItem.ProductName,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
                    TotalPrice = cartItem.TotalPrice
                };
                orderItems.Add(orderItem);
            }

            // Decrease stock
            foreach (var cartItem in cart.Items)
            {
                var product = Database.Products.FirstOrDefault(p => p.Id == cartItem.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= cartItem.Quantity;
                }
            }

            // Create order
            var order = new Order
            {
                Id = Database.Orders.Any() ? Database.Orders.Max(o => o.Id) + 1 : 1,
                UserId = userId,
                Items = orderItems,
                TotalPrice = cart.TotalPrice,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending
            };

            Database.Orders.Add(order);

            // Track coupon usage if applied
            if (!string.IsNullOrEmpty(cart.AppliedCouponCode))
            {
                var coupon = Database.Coupons.FirstOrDefault(c => c.Code == cart.AppliedCouponCode);
                if (coupon != null)
                {
                    coupon.CurrentUses++;

                    var usage = new CouponUsage
                    {
                        Id = Database.CouponUsages.Any() ? Database.CouponUsages.Max(u => u.Id) + 1 : 1,
                        CouponId = coupon.Id,
                        UserId = userId,
                        OrderId = order.Id,
                        UsedAt = DateTime.UtcNow
                    };
                    Database.CouponUsages.Add(usage);
                }
            }

            // Clear cart after successful checkout
            cart.Items.Clear();
            cart.AppliedCouponCode = null;
            cart.DiscountAmount = 0;
            cart.LastUpdated = DateTime.UtcNow;

            return CreatedAtAction("GetOrder", "Orders", new { id = order.Id }, order);
        }

        /// <summary>
        /// Apply coupon code to cart
        /// </summary>
        [HttpPost("apply-coupon")]
        public ActionResult ApplyCoupon([FromBody] ApplyCouponRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var cart = Database.Carts.FirstOrDefault(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
                return BadRequest(new ErrorResponse(400, "Cart is empty."));

            // Find coupon
            var coupon = Database.Coupons.FirstOrDefault(c => c.Code.ToUpper() == request.Code.ToUpper());
            if (coupon == null)
                return NotFound(new ErrorResponse(404, "Invalid coupon code."));

            // Validate coupon
            var validationResult = ValidateCoupon(coupon);
            if (!validationResult.IsValid)
                return BadRequest(new ErrorResponse(400, validationResult.ErrorMessage!));

            // Calculate discount
            var subtotal = cart.Subtotal;
            decimal discountAmount = 0;

            if (coupon.Type == DiscountType.Percentage)
            {
                discountAmount = subtotal * (coupon.Value / 100);
            }
            else // FixedAmount
            {
                discountAmount = Math.Min(coupon.Value, subtotal); // Can't discount more than subtotal
            }

            // Apply to cart
            cart.AppliedCouponCode = coupon.Code;
            cart.DiscountAmount = Math.Round(discountAmount, 2);
            cart.LastUpdated = DateTime.UtcNow;

            return Ok(new
            {
                message = "Coupon applied successfully",
                coupon = new
                {
                    code = coupon.Code,
                    type = coupon.Type,
                    value = coupon.Value
                },
                cart = new
                {
                    subtotal = cart.Subtotal,
                    discountAmount = cart.DiscountAmount,
                    totalPrice = cart.TotalPrice,
                    appliedCouponCode = cart.AppliedCouponCode
                }
            });
        }

        /// <summary>
        /// Remove coupon from cart
        /// </summary>
        [HttpDelete("remove-coupon")]
        public ActionResult RemoveCoupon()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var cart = Database.Carts.FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
                return NotFound(new ErrorResponse(404, "Cart not found."));

            if (string.IsNullOrEmpty(cart.AppliedCouponCode))
                return BadRequest(new ErrorResponse(400, "No coupon applied to cart."));

            cart.AppliedCouponCode = null;
            cart.DiscountAmount = 0;
            cart.LastUpdated = DateTime.UtcNow;

            return Ok(new
            {
                message = "Coupon removed successfully",
                cart = new
                {
                    subtotal = cart.Subtotal,
                    discountAmount = cart.DiscountAmount,
                    totalPrice = cart.TotalPrice
                }
            });
        }

        private (bool IsValid, string? ErrorMessage) ValidateCoupon(Coupon coupon)
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

    public class ApplyCouponRequest
    {
        public string Code { get; set; } = string.Empty;
    }

    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateCartItemRequest
    {
        public int Quantity { get; set; }
    }
}
