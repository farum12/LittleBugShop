using LittleBugShop.Models;

namespace LittleBugShop.Services;

public interface IPaymentProcessor
{
    /// <summary>
    /// Process a payment for an order
    /// </summary>
    Task<PaymentResult> ProcessPayment(PaymentRequest request, PaymentMethod paymentMethod, decimal amount);
    
    /// <summary>
    /// Process a refund for a completed transaction
    /// </summary>
    Task<RefundResult> ProcessRefund(string transactionId, decimal amount, string reason);
    
    /// <summary>
    /// Get the current status of a payment transaction
    /// </summary>
    Task<PaymentStatus> GetPaymentStatus(string transactionId);
}
