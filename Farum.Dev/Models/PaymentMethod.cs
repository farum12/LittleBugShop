namespace LittleBugShop.Models;

public enum PaymentMethodType
{
    CreditCard = 0,
    DebitCard = 1,
    PayPal = 2
}

public class PaymentMethod
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public PaymentMethodType Type { get; set; }
    
    // For card payments
    public string? CardHolderName { get; set; }
    public string? CardNumberMasked { get; set; }  // e.g., "**** **** **** 0000"
    public string? CardNumberLast4 { get; set; }    // e.g., "0000"
    public string? ExpiryMonth { get; set; }        // e.g., "12"
    public string? ExpiryYear { get; set; }         // e.g., "2026"
    
    // For PayPal
    public string? PayPalEmail { get; set; }
    
    public bool IsDefault { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum PaymentStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Refunded = 3,
    PartiallyRefunded = 4
}

public class PaymentTransaction
{
    public int Id { get; set; }
    public string TransactionId { get; set; } = string.Empty;  // e.g., "TXN_abc123"
    public int? OrderId { get; set; }              // Null if payment failed before order completion
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public int PaymentMethodId { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public string ResponseMessage { get; set; } = string.Empty;
    public string? FailureReason { get; set; }     // e.g., "INSUFFICIENT_FUNDS", "FRAUD_DETECTED"
    public decimal RefundedAmount { get; set; } = 0;
}

// DTOs for payment operations
public class AddPaymentMethodRequest
{
    public PaymentMethodType Type { get; set; }
    
    // For card payments
    public string? CardHolderName { get; set; }
    public string? CardNumber { get; set; }         // Full number for processing
    public string? ExpiryMonth { get; set; }
    public string? ExpiryYear { get; set; }
    public string? Cvv { get; set; }                // Not stored
    
    // For PayPal
    public string? PayPalEmail { get; set; }
}

public class PaymentRequest
{
    public int OrderId { get; set; }
    public int PaymentMethodId { get; set; }
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}

public class RefundRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class RefundResult
{
    public bool Success { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public decimal RefundedAmount { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
