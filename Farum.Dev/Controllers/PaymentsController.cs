using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LittleBugShop.Data;
using LittleBugShop.Models;
using LittleBugShop.Services;

namespace LittleBugShop.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentProcessor _paymentProcessor;

    public PaymentsController()
    {
        _paymentProcessor = new PaymentSimulator();
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        // Get the order
        var order = Database.Orders.FirstOrDefault(o => o.Id == request.OrderId && o.UserId == userId);
        if (order == null)
        {
            return NotFound(new { error = "Order not found" });
        }
        
        // Verify order is pending payment
        if (order.PaymentStatus != PaymentStatus.Pending)
        {
            return BadRequest(new { error = $"Order payment status is {order.PaymentStatus}, cannot process payment" });
        }
        
        // Check if order has expired
        if (order.ExpiresAt.HasValue && order.ExpiresAt.Value < DateTime.UtcNow)
        {
            order.Status = OrderStatus.Cancelled;
            order.PaymentStatus = PaymentStatus.Failed;
            
            // Restore stock
            foreach (var item in order.Items)
            {
                var product = Database.Products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                }
            }
            
            return BadRequest(new { error = "Order has expired. Stock has been restored. Please create a new order." });
        }
        
        // Get payment method
        var paymentMethod = Database.PaymentMethods.FirstOrDefault(pm => 
            pm.Id == request.PaymentMethodId && pm.UserId == userId);
            
        if (paymentMethod == null)
        {
            return BadRequest(new { error = "Invalid payment method" });
        }
        
        // Process payment
        var paymentResult = await _paymentProcessor.ProcessPayment(request, paymentMethod, order.TotalPrice);
        
        // Create transaction record
        var transaction = new PaymentTransaction
        {
            Id = Database.PaymentTransactions.Any() ? Database.PaymentTransactions.Max(t => t.Id) + 1 : 1,
            TransactionId = paymentResult.TransactionId,
            OrderId = paymentResult.Success ? order.Id : null,
            UserId = userId,
            Amount = order.TotalPrice,
            Status = paymentResult.Status,
            PaymentMethodId = paymentMethod.Id,
            ProcessedAt = paymentResult.ProcessedAt,
            ResponseMessage = paymentResult.Message,
            FailureReason = paymentResult.FailureReason
        };
        
        Database.PaymentTransactions.Add(transaction);
        
        if (paymentResult.Success)
        {
            // Update order
            order.PaymentStatus = PaymentStatus.Completed;
            order.TransactionId = paymentResult.TransactionId;
            order.PaymentMethodId = paymentMethod.Id;
            
            // Get cart and track coupon usage if applicable
            var cart = Database.Carts.FirstOrDefault(c => c.UserId == userId);
            if (cart != null && !string.IsNullOrEmpty(cart.AppliedCouponCode))
            {
                var coupon = Database.Coupons.FirstOrDefault(c => c.Code == cart.AppliedCouponCode);
                if (coupon != null)
                {
                    coupon.CurrentUses++;
                    var couponUsage = new CouponUsage
                    {
                        Id = Database.CouponUsages.Any() ? Database.CouponUsages.Max(cu => cu.Id) + 1 : 1,
                        CouponId = coupon.Id,
                        UserId = userId,
                        OrderId = order.Id,
                        UsedAt = DateTime.UtcNow
                    };
                    Database.CouponUsages.Add(couponUsage);
                }
            }
            
            // Clear cart after successful payment
            if (cart != null)
            {
                cart.Items.Clear();
                cart.AppliedCouponCode = null;
                cart.DiscountAmount = 0;
            }
            
            return Ok(new
            {
                message = "Payment successful",
                order = new
                {
                    order.Id,
                    order.UserId,
                    order.TotalPrice,
                    order.Status,
                    order.PaymentStatus,
                    order.TransactionId,
                    order.PaymentMethodId,
                    order.OrderDate,
                    Items = order.Items
                },
                transaction = new
                {
                    transaction.Id,
                    transaction.TransactionId,
                    transaction.Amount,
                    transaction.Status,
                    transaction.ProcessedAt,
                    transaction.ResponseMessage
                }
            });
        }
        else
        {
            // Payment failed - order stays pending, cart unchanged
            return BadRequest(new
            {
                error = paymentResult.Message,
                canRetry = true,
                transaction = new
                {
                    transaction.Id,
                    transaction.TransactionId,
                    transaction.Amount,
                    transaction.Status,
                    transaction.ProcessedAt,
                    transaction.ResponseMessage,
                    transaction.FailureReason
                }
            });
        }
    }

    [HttpGet("transactions")]
    public IActionResult GetMyTransactions()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var transactions = Database.PaymentTransactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.ProcessedAt)
            .ToList();
        
        return Ok(transactions);
    }

    [HttpGet("transactions/{id}")]
    public IActionResult GetTransaction(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var transaction = Database.PaymentTransactions.FirstOrDefault(t => 
            t.Id == id && t.UserId == userId);
        
        if (transaction == null)
        {
            return NotFound(new { error = "Transaction not found" });
        }
        
        return Ok(transaction);
    }

    [HttpPost("refund")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ProcessRefund([FromBody] RefundRequest request)
    {
        var transaction = Database.PaymentTransactions.FirstOrDefault(t => 
            t.TransactionId == request.TransactionId);
        
        if (transaction == null)
        {
            return NotFound(new { error = "Transaction not found" });
        }
        
        if (transaction.Status != PaymentStatus.Completed)
        {
            return BadRequest(new { error = "Can only refund completed transactions" });
        }
        
        if (request.Amount <= 0 || request.Amount > (transaction.Amount - transaction.RefundedAmount))
        {
            return BadRequest(new { error = "Invalid refund amount" });
        }
        
        var refundResult = await _paymentProcessor.ProcessRefund(
            transaction.TransactionId, 
            request.Amount, 
            request.Reason);
        
        if (refundResult.Success)
        {
            transaction.RefundedAmount += request.Amount;
            
            if (transaction.RefundedAmount >= transaction.Amount)
            {
                transaction.Status = PaymentStatus.Refunded;
            }
            else
            {
                transaction.Status = PaymentStatus.PartiallyRefunded;
            }
            
            // Update order status if fully refunded
            if (transaction.OrderId.HasValue && transaction.Status == PaymentStatus.Refunded)
            {
                var order = Database.Orders.FirstOrDefault(o => o.Id == transaction.OrderId.Value);
                if (order != null)
                {
                    order.PaymentStatus = PaymentStatus.Refunded;
                    order.Status = OrderStatus.Cancelled;
                    
                    // Restore stock
                    foreach (var item in order.Items)
                    {
                        var product = Database.Products.FirstOrDefault(p => p.Id == item.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity += item.Quantity;
                        }
                    }
                }
            }
            
            return Ok(new
            {
                message = refundResult.Message,
                transaction = new
                {
                    transaction.TransactionId,
                    transaction.Status,
                    RefundedAmount = transaction.RefundedAmount,
                    RemainingAmount = transaction.Amount - transaction.RefundedAmount
                }
            });
        }
        else
        {
            return BadRequest(new { error = refundResult.Message });
        }
    }

    [HttpGet("admin/transactions")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetAllTransactions([FromQuery] PaymentStatus? status = null)
    {
        var query = Database.PaymentTransactions.AsEnumerable();
        
        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }
        
        var transactions = query
            .OrderByDescending(t => t.ProcessedAt)
            .ToList();
        
        return Ok(transactions);
    }

    [HttpGet("admin/statistics")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetPaymentStatistics()
    {
        var allTransactions = Database.PaymentTransactions;
        
        var stats = new
        {
            TotalTransactions = allTransactions.Count,
            SuccessfulTransactions = allTransactions.Count(t => t.Status == PaymentStatus.Completed || t.Status == PaymentStatus.PartiallyRefunded || t.Status == PaymentStatus.Refunded),
            FailedTransactions = allTransactions.Count(t => t.Status == PaymentStatus.Failed),
            TotalRevenue = allTransactions
                .Where(t => t.Status == PaymentStatus.Completed || t.Status == PaymentStatus.PartiallyRefunded || t.Status == PaymentStatus.Refunded)
                .Sum(t => t.Amount - t.RefundedAmount),
            TotalRefunded = allTransactions.Sum(t => t.RefundedAmount),
            SuccessRate = allTransactions.Any() 
                ? Math.Round((double)allTransactions.Count(t => t.Status == PaymentStatus.Completed) / allTransactions.Count * 100, 2)
                : 0,
            FailureReasons = allTransactions
                .Where(t => !string.IsNullOrEmpty(t.FailureReason))
                .GroupBy(t => t.FailureReason)
                .Select(g => new { Reason = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList()
        };
        
        return Ok(stats);
    }
}
