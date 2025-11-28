using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LittleBugShop.Data;
using LittleBugShop.Models;
using System.Security.Claims;

namespace LittleBugShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require authentication for all order endpoints
    public class OrdersController : ControllerBase
    {
        [HttpPost("create")]
        public ActionResult CreateOrder([FromBody] CreateOrderRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Get user's cart
            var cart = Database.Carts.FirstOrDefault(c => c.UserId == userId);
            if (cart == null || !cart.Items.Any())
            {
                return BadRequest(new { error = "Cart is empty" });
            }
            
            // Validate shipping address if provided
            if (request.ShippingAddressId.HasValue)
            {
                var address = Database.Addresses.FirstOrDefault(a => 
                    a.Id == request.ShippingAddressId.Value && a.UserId == userId);
                    
                if (address == null)
                {
                    return BadRequest(new { error = "Invalid shipping address" });
                }
            }
            
            // Check stock availability for all items
            foreach (var cartItem in cart.Items)
            {
                var product = Database.Products.FirstOrDefault(p => p.Id == cartItem.ProductId);
                if (product == null)
                {
                    return BadRequest(new { error = $"Product {cartItem.ProductId} not found" });
                }
                
                if (!product.IsAvailable(cartItem.Quantity))
                {
                    return BadRequest(new { error = $"Insufficient stock for {product.Name}. Available: {product.StockQuantity}, Requested: {cartItem.Quantity}" });
                }
            }
            
            // Reserve stock
            foreach (var cartItem in cart.Items)
            {
                var product = Database.Products.FirstOrDefault(p => p.Id == cartItem.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= cartItem.Quantity;
                }
            }
            
            // Create order items from cart
            var orderItems = cart.Items.Select((item, index) => new OrderItem
            {
                Id = index + 1,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.UnitPrice * item.Quantity
            }).ToList();
            
            // Create pending order
            var order = new Order
            {
                Id = Database.Orders.Any() ? Database.Orders.Max(o => o.Id) + 1 : 1,
                UserId = userId,
                Items = orderItems,
                TotalPrice = cart.TotalPrice,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                ShippingAddressId = request.ShippingAddressId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15) // 15 minutes to complete payment
            };
            
            Database.Orders.Add(order);
            
            // Cart is NOT cleared yet - only after successful payment
            
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, new
            {
                order.Id,
                order.UserId,
                order.TotalPrice,
                order.Status,
                order.PaymentStatus,
                order.ShippingAddressId,
                order.ExpiresAt,
                order.OrderDate,
                Items = order.Items,
                Message = "Order created. Please complete payment within 15 minutes."
            });
        }

        [HttpPost("place")]
        public ActionResult<Order> PlaceOrder(PlaceOrderRequest request)
        {
            // Get authenticated user's ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized("User not authenticated.");
            }

            var authenticatedUserId = int.Parse(userIdClaim);

            // Validate that the user in the request matches the authenticated user
            if (request.UserId != authenticatedUserId)
            {
                return Forbid("You can only place orders for yourself.");
            }

            // Validate user exists
            var user = Database.Users.FirstOrDefault(u => u.Id == request.UserId);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            // Validate items list is not empty
            if (request.Items == null || !request.Items.Any())
            {
                return BadRequest("Order must contain at least one item.");
            }

            var orderItems = new List<OrderItem>();
            decimal totalOrderPrice = 0;
            int orderItemId = 1;

            // Process each item in the order
            foreach (var item in request.Items)
            {
                // Validate product exists
                var product = Database.Products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product == null)
                {
                    return BadRequest($"Product with ID {item.ProductId} not found.");
                }

                // Validate quantity
                if (item.Quantity <= 0)
                {
                    return BadRequest($"Quantity for product '{product.Name}' must be greater than zero.");
                }

                // Check stock availability
                if (!product.IsAvailable(item.Quantity))
                {
                    return BadRequest($"Insufficient stock for product '{product.Name}'. Available: {product.StockQuantity}, Requested: {item.Quantity}");
                }

                // Calculate item total price
                var itemTotalPrice = product.Price * item.Quantity;

                // Create order item
                var orderItem = new OrderItem
                {
                    Id = orderItemId++,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    TotalPrice = itemTotalPrice
                };

                orderItems.Add(orderItem);
                totalOrderPrice += itemTotalPrice;
            }

            // Decrease stock quantities for all items
            foreach (var item in request.Items)
            {
                var product = Database.Products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                }
            }

            // Create order
            var order = new Order
            {
                Id = Database.Orders.Any() ? Database.Orders.Max(o => o.Id) + 1 : 1,
                UserId = request.UserId,
                Items = orderItems,
                TotalPrice = totalOrderPrice,
                OrderDate = DateTime.UtcNow
            };

            Database.Orders.Add(order);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")] // Only admins can see all orders
        public ActionResult<IEnumerable<Order>> GetOrders()
        {
            return Ok(Database.Orders);
        }

        [HttpGet("my-orders")]
        public ActionResult<IEnumerable<Order>> GetMyOrders()
        {
            // Get authenticated user's ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized("User not authenticated.");
            }

            var authenticatedUserId = int.Parse(userIdClaim);

            // Return only orders belonging to the authenticated user
            var myOrders = Database.Orders.Where(o => o.UserId == authenticatedUserId).ToList();
            return Ok(myOrders);
        }

        [HttpGet("{id}")]
        public ActionResult<Order> GetOrder(int id)
        {
            var order = Database.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteOrder(int id)
        {
            var order = Database.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            Database.Orders.Remove(order);
            return NoContent();
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public ActionResult<Order> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            var order = Database.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            var oldStatus = order.Status;

            // If changing to Cancelled status, restore stock
            if (request.Status == OrderStatus.Cancelled && oldStatus != OrderStatus.Cancelled)
            {
                foreach (var item in order.Items)
                {
                    var product = Database.Products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                    }
                }
            }

            order.Status = request.Status;
            return Ok(order);
        }

        [HttpGet("pending")]
        public ActionResult GetPendingOrders()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var pendingOrders = Database.Orders
                .Where(o => o.UserId == userId && o.PaymentStatus == PaymentStatus.Pending)
                .Select(o => new
                {
                    o.Id,
                    o.TotalPrice,
                    o.OrderDate,
                    o.ExpiresAt,
                    o.ShippingAddressId,
                    MinutesRemaining = o.ExpiresAt.HasValue 
                        ? Math.Max(0, (o.ExpiresAt.Value - DateTime.UtcNow).TotalMinutes) 
                        : 0,
                    IsExpired = o.ExpiresAt.HasValue && o.ExpiresAt.Value < DateTime.UtcNow,
                    Items = o.Items
                })
                .ToList();
            
            return Ok(pendingOrders);
        }

        [HttpDelete("{id}/cancel")]
        public ActionResult CancelOrder(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var order = Database.Orders.FirstOrDefault(o => o.Id == id && o.UserId == userId);
            
            if (order == null)
            {
                return NotFound(new { error = "Order not found" });
            }
            
            // Can only cancel pending payment orders
            if (order.PaymentStatus != PaymentStatus.Pending)
            {
                return BadRequest(new { error = $"Cannot cancel order with payment status: {order.PaymentStatus}" });
            }
            
            // Restore stock
            foreach (var item in order.Items)
            {
                var product = Database.Products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                }
            }
            
            // Mark as cancelled
            order.Status = OrderStatus.Cancelled;
            order.PaymentStatus = PaymentStatus.Failed;
            
            return Ok(new
            {
                message = "Order cancelled successfully",
                stockRestored = true,
                order
            });
        }
    }

    public class CreateOrderRequest
    {
        public int? ShippingAddressId { get; set; }
    }

    public class UpdateOrderStatusRequest
    {
        public OrderStatus Status { get; set; }
    }
}
