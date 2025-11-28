using LittleBugShop.Models;

namespace LittleBugShop.Services;

public class PaymentSimulator : IPaymentProcessor
{
    private readonly Dictionary<string, PaymentTransaction> _transactions = new();
    
    public async Task<PaymentResult> ProcessPayment(PaymentRequest request, PaymentMethod paymentMethod, decimal amount)
    {
        // Simulate async payment processing
        await Task.Delay(100);
        
        var transactionId = GenerateTransactionId();
        var result = new PaymentResult
        {
            TransactionId = transactionId,
            ProcessedAt = DateTime.UtcNow
        };
        
        // Test-friendly card patterns for predictable outcomes
        if (paymentMethod.Type == PaymentMethodType.CreditCard || paymentMethod.Type == PaymentMethodType.DebitCard)
        {
            var last4 = paymentMethod.CardNumberLast4 ?? "0000";
            
            switch (last4)
            {
                case "0000":
                    // Always succeeds
                    result.Success = true;
                    result.Status = PaymentStatus.Completed;
                    result.Message = "Payment successful";
                    break;
                    
                case "1111":
                    // Insufficient funds
                    result.Success = false;
                    result.Status = PaymentStatus.Failed;
                    result.Message = "Payment failed: Insufficient funds";
                    result.FailureReason = "INSUFFICIENT_FUNDS";
                    break;
                    
                case "2222":
                    // Network timeout
                    result.Success = false;
                    result.Status = PaymentStatus.Failed;
                    result.Message = "Payment failed: Network timeout";
                    result.FailureReason = "NETWORK_TIMEOUT";
                    break;
                    
                case "3333":
                    // Fraud detection
                    result.Success = false;
                    result.Status = PaymentStatus.Failed;
                    result.Message = "Payment failed: Fraud detection triggered";
                    result.FailureReason = "FRAUD_DETECTED";
                    break;
                    
                case "4444":
                    // Card expired
                    result.Success = false;
                    result.Status = PaymentStatus.Failed;
                    result.Message = "Payment failed: Card expired";
                    result.FailureReason = "CARD_EXPIRED";
                    break;
                    
                case "5555":
                    // Invalid CVV
                    result.Success = false;
                    result.Status = PaymentStatus.Failed;
                    result.Message = "Payment failed: Invalid CVV";
                    result.FailureReason = "INVALID_CVV";
                    break;
                    
                case "6666":
                    // Card declined
                    result.Success = false;
                    result.Status = PaymentStatus.Failed;
                    result.Message = "Payment failed: Card declined by issuer";
                    result.FailureReason = "CARD_DECLINED";
                    break;
                    
                default:
                    // Default: success for other cards
                    result.Success = true;
                    result.Status = PaymentStatus.Completed;
                    result.Message = "Payment successful";
                    break;
            }
        }
        else if (paymentMethod.Type == PaymentMethodType.PayPal)
        {
            // PayPal email-based testing
            if (paymentMethod.PayPalEmail?.Contains("fail") == true)
            {
                result.Success = false;
                result.Status = PaymentStatus.Failed;
                result.Message = "Payment failed: PayPal account issue";
                result.FailureReason = "PAYPAL_ACCOUNT_ISSUE";
            }
            else
            {
                result.Success = true;
                result.Status = PaymentStatus.Completed;
                result.Message = "PayPal payment successful";
            }
        }
        
        // Amount-based testing scenarios
        if (amount == 666.00m)
        {
            result.Success = false;
            result.Status = PaymentStatus.Failed;
            result.Message = "Payment failed: Amount validation failed";
            result.FailureReason = "INVALID_AMOUNT";
        }
        else if (amount == 777.00m)
        {
            result.Success = true;
            result.Status = PaymentStatus.Completed;
            result.Message = "Payment successful (lucky amount)";
        }
        else if (amount >= 10000.00m)
        {
            result.Success = false;
            result.Status = PaymentStatus.Failed;
            result.Message = "Payment failed: Amount exceeds limit";
            result.FailureReason = "AMOUNT_LIMIT_EXCEEDED";
        }
        
        return result;
    }
    
    public async Task<RefundResult> ProcessRefund(string transactionId, decimal amount, string reason)
    {
        // Simulate async refund processing
        await Task.Delay(100);
        
        var result = new RefundResult
        {
            TransactionId = transactionId,
            ProcessedAt = DateTime.UtcNow
        };
        
        // Test-friendly refund scenarios
        if (transactionId.Contains("NOREFUND"))
        {
            result.Success = false;
            result.Message = "Refund failed: Transaction not refundable";
            result.RefundedAmount = 0;
        }
        else if (amount == 13.00m)
        {
            result.Success = false;
            result.Message = "Refund failed: Unlucky amount";
            result.RefundedAmount = 0;
        }
        else
        {
            result.Success = true;
            result.Message = $"Refund successful: {amount:C}";
            result.RefundedAmount = amount;
        }
        
        return result;
    }
    
    public async Task<PaymentStatus> GetPaymentStatus(string transactionId)
    {
        await Task.Delay(10);
        
        if (_transactions.TryGetValue(transactionId, out var transaction))
        {
            return transaction.Status;
        }
        
        return PaymentStatus.Pending;
    }
    
    private string GenerateTransactionId()
    {
        return $"TXN_{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }
}
