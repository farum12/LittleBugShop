using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LittleBugShop.Data;
using LittleBugShop.Models;

namespace LittleBugShop.Controllers;

[ApiController]
[Route("api/payment-methods")]
[Authorize]
public class PaymentMethodsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetPaymentMethods()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var paymentMethods = Database.PaymentMethods.Where(pm => pm.UserId == userId).ToList();
        return Ok(paymentMethods);
    }

    [HttpGet("{id}")]
    public IActionResult GetPaymentMethod(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var paymentMethod = Database.PaymentMethods.FirstOrDefault(pm => pm.Id == id && pm.UserId == userId);
        
        if (paymentMethod == null)
        {
            return NotFound(new { error = "Payment method not found" });
        }
        
        return Ok(paymentMethod);
    }

    [HttpPost]
    public IActionResult AddPaymentMethod([FromBody] AddPaymentMethodRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        // Validate request
        if (request.Type == PaymentMethodType.CreditCard || request.Type == PaymentMethodType.DebitCard)
        {
            if (string.IsNullOrEmpty(request.CardHolderName))
            {
                return BadRequest(new { error = "Card holder name is required" });
            }
            if (string.IsNullOrEmpty(request.CardNumber) || request.CardNumber.Length < 13)
            {
                return BadRequest(new { error = "Invalid card number" });
            }
            if (string.IsNullOrEmpty(request.ExpiryMonth) || string.IsNullOrEmpty(request.ExpiryYear))
            {
                return BadRequest(new { error = "Expiry date is required" });
            }
            if (string.IsNullOrEmpty(request.Cvv) || request.Cvv.Length < 3)
            {
                return BadRequest(new { error = "Invalid CVV" });
            }
        }
        else if (request.Type == PaymentMethodType.PayPal)
        {
            if (string.IsNullOrEmpty(request.PayPalEmail))
            {
                return BadRequest(new { error = "PayPal email is required" });
            }
        }
        
        // Get the last 4 digits of card number
        var last4 = request.CardNumber?.Length >= 4 
            ? request.CardNumber.Substring(request.CardNumber.Length - 4) 
            : "0000";
        
        // Mask the card number
        var masked = request.CardNumber != null 
            ? $"**** **** **** {last4}" 
            : null;
        
        // If this is the first payment method, make it default
        var isFirstMethod = !Database.PaymentMethods.Any(pm => pm.UserId == userId);
        
        var paymentMethod = new PaymentMethod
        {
            Id = Database.PaymentMethods.Count + 1,
            UserId = userId,
            Type = request.Type,
            CardHolderName = request.CardHolderName,
            CardNumberMasked = masked,
            CardNumberLast4 = last4,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            PayPalEmail = request.PayPalEmail,
            IsDefault = isFirstMethod,
            CreatedAt = DateTime.UtcNow
        };
        
        Database.PaymentMethods.Add(paymentMethod);
        
        return CreatedAtAction(nameof(GetPaymentMethod), new { id = paymentMethod.Id }, paymentMethod);
    }

    [HttpPut("{id}")]
    public IActionResult UpdatePaymentMethod(int id, [FromBody] AddPaymentMethodRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var paymentMethod = Database.PaymentMethods.FirstOrDefault(pm => pm.Id == id && pm.UserId == userId);
        
        if (paymentMethod == null)
        {
            return NotFound(new { error = "Payment method not found" });
        }
        
        // Validate request
        if (request.Type == PaymentMethodType.CreditCard || request.Type == PaymentMethodType.DebitCard)
        {
            if (string.IsNullOrEmpty(request.CardHolderName))
            {
                return BadRequest(new { error = "Card holder name is required" });
            }
            if (string.IsNullOrEmpty(request.ExpiryMonth) || string.IsNullOrEmpty(request.ExpiryYear))
            {
                return BadRequest(new { error = "Expiry date is required" });
            }
        }
        
        // Update fields (card number cannot be changed for security)
        paymentMethod.CardHolderName = request.CardHolderName;
        paymentMethod.ExpiryMonth = request.ExpiryMonth;
        paymentMethod.ExpiryYear = request.ExpiryYear;
        
        if (request.Type == PaymentMethodType.PayPal)
        {
            paymentMethod.PayPalEmail = request.PayPalEmail;
        }
        
        return Ok(paymentMethod);
    }

    [HttpDelete("{id}")]
    public IActionResult DeletePaymentMethod(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var paymentMethod = Database.PaymentMethods.FirstOrDefault(pm => pm.Id == id && pm.UserId == userId);
        
        if (paymentMethod == null)
        {
            return NotFound(new { error = "Payment method not found" });
        }
        
        // Check if there are pending orders using this payment method
        var hasPendingOrders = Database.Orders.Any(o => 
            o.UserId == userId && 
            o.PaymentMethodId == id && 
            o.PaymentStatus == PaymentStatus.Pending);
        
        if (hasPendingOrders)
        {
            return BadRequest(new { error = "Cannot delete payment method with pending orders. Please complete or cancel those orders first." });
        }
        
        // If this was the default, make another one default
        if (paymentMethod.IsDefault)
        {
            var nextMethod = Database.PaymentMethods
                .FirstOrDefault(pm => pm.UserId == userId && pm.Id != id);
            
            if (nextMethod != null)
            {
                nextMethod.IsDefault = true;
            }
        }
        
        Database.PaymentMethods.Remove(paymentMethod);
        
        return NoContent();
    }

    [HttpPut("{id}/set-default")]
    public IActionResult SetDefaultPaymentMethod(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var paymentMethod = Database.PaymentMethods.FirstOrDefault(pm => pm.Id == id && pm.UserId == userId);
        
        if (paymentMethod == null)
        {
            return NotFound(new { error = "Payment method not found" });
        }
        
        // Unset all other defaults for this user
        foreach (var pm in Database.PaymentMethods.Where(pm => pm.UserId == userId))
        {
            pm.IsDefault = false;
        }
        
        // Set this one as default
        paymentMethod.IsDefault = true;
        
        return Ok(paymentMethod);
    }
}
