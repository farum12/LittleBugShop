# Payment Simulation System Summary

## Overview
The payment simulation system enables realistic two-step checkout with payment processing, failure handling, retry capabilities, and refund operations. Designed specifically for test automation, it provides predictable payment outcomes based on test card patterns and amounts.

**Key Feature**: Two-step checkout process separates order creation from payment, allowing orders to remain pending while users retry failed payments - mimicking real-world e-commerce scenarios.

---

## Architecture

### Payment Processing Flow
```
Cart with Items
    ↓
Create Order (POST /api/orders/create)
    ├─ Validates cart not empty
    ├─ Checks stock availability
    ├─ Reserves stock (decrements)
    ├─ Creates order (PaymentStatus = Pending)
    ├─ Sets 15-minute expiration
    └─ Cart remains intact
    ↓
Process Payment (POST /api/payments/process)
    ├─ Validates order exists & pending
    ├─ Checks not expired
    ├─ Calls IPaymentProcessor
    ├─ Creates PaymentTransaction record
    └─ Result: Success or Failure
        ├─ SUCCESS:
        │   ├─ Update order (PaymentStatus = Completed)
        │   ├─ Track coupon usage
        │   ├─ Clear cart
        │   └─ Return transaction details
        └─ FAILURE:
            ├─ Order stays pending
            ├─ Cart unchanged
            ├─ Log failed transaction
            └─ User can retry
```

### Core Components

**1. IPaymentProcessor Interface**
```csharp
Task<PaymentResult> ProcessPayment(PaymentRequest, PaymentMethod, decimal amount);
Task<RefundResult> ProcessRefund(string transactionId, decimal amount, string reason);
Task<PaymentStatus> GetPaymentStatus(string transactionId);
```

**2. PaymentSimulator Class**
- Implements `IPaymentProcessor`
- Provides test-friendly predictable outcomes
- Simulates async processing (100ms delay)
- Generates unique transaction IDs

**3. Controllers**
- `PaymentMethodsController` - Manage credit cards, PayPal accounts
- `PaymentsController` - Process payments, refunds, transaction history
- `OrdersController` - Create orders, cancel, view pending

---

## Data Models

### PaymentMethod
```csharp
public class PaymentMethod
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public PaymentMethodType Type { get; set; }  // CreditCard, DebitCard, PayPal
    
    // Card fields
    public string? CardHolderName { get; set; }
    public string? CardNumberMasked { get; set; }   // "**** **** **** 0000"
    public string? CardNumberLast4 { get; set; }     // "0000"
    public string? ExpiryMonth { get; set; }         // "12"
    public string? ExpiryYear { get; set; }          // "2026"
    
    // PayPal field
    public string? PayPalEmail { get; set; }
    
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**Security Notes**:
- Full card number NEVER stored
- CVV not stored (only used during addition)
- Only last 4 digits and masked version kept

### PaymentTransaction
```csharp
public class PaymentTransaction
{
    public int Id { get; set; }
    public string TransactionId { get; set; }       // "TXN_ABC12345"
    public int? OrderId { get; set; }               // Null if payment failed
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public int PaymentMethodId { get; set; }
    public DateTime ProcessedAt { get; set; }
    public string ResponseMessage { get; set; }
    public string? FailureReason { get; set; }      // e.g., "INSUFFICIENT_FUNDS"
    public decimal RefundedAmount { get; set; }
}
```

**Audit Trail**: Every payment attempt (success or failure) creates a transaction record.

### PaymentStatus Enum
```csharp
public enum PaymentStatus
{
    Pending = 0,             // Order created, awaiting payment
    Completed = 1,           // Payment successful
    Failed = 2,              // Payment failed
    Refunded = 3,            // Fully refunded
    PartiallyRefunded = 4    // Some amount refunded
}
```

### Updated Order Model
```csharp
public class Order
{
    // ... existing fields
    
    // Payment information
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public string? TransactionId { get; set; }
    public int? PaymentMethodId { get; set; }
    
    // Shipping information
    public int? ShippingAddressId { get; set; }
    
    // Order expiration
    public DateTime? ExpiresAt { get; set; }        // 15 min from creation
}
```

---

## Test Card Patterns

### Card Number Last 4 Digits

| Last 4 | Outcome | Failure Reason | Use Case |
|--------|---------|----------------|----------|
| **0000** | ✅ Success | - | Happy path testing |
| **1111** | ❌ Fail | INSUFFICIENT_FUNDS | Test retry logic |
| **2222** | ❌ Fail | NETWORK_TIMEOUT | Test timeout handling |
| **3333** | ❌ Fail | FRAUD_DETECTED | Test fraud scenarios |
| **4444** | ❌ Fail | CARD_EXPIRED | Test expiry validation |
| **5555** | ❌ Fail | INVALID_CVV | Test CVV validation |
| **6666** | ❌ Fail | CARD_DECLINED | Test generic decline |
| **Other** | ✅ Success | - | Default success |

### Amount-Based Patterns

| Amount | Outcome | Failure Reason | Use Case |
|--------|---------|----------------|----------|
| **$666.00** | ❌ Fail | INVALID_AMOUNT | Test amount validation |
| **$777.00** | ✅ Success | - | Lucky amount |
| **≥ $10,000** | ❌ Fail | AMOUNT_LIMIT_EXCEEDED | Test limits |
| **Other** | Based on card | - | Standard flow |

### PayPal Email Patterns

| Email | Outcome | Failure Reason |
|-------|---------|----------------|
| Contains "fail" | ❌ Fail | PAYPAL_ACCOUNT_ISSUE |
| Other | ✅ Success | - |

**Example**: `fail@example.com` → fails, `john@example.com` → succeeds

---

## API Endpoints

### Payment Methods Management

#### **GET /api/payment-methods**
List all payment methods for authenticated user.

**Response** (200 OK):
```json
[
  {
    "id": 3,
    "userId": 2,
    "type": 0,
    "cardHolderName": "John Doe",
    "cardNumberMasked": "**** **** **** 0000",
    "cardNumberLast4": "0000",
    "expiryMonth": "12",
    "expiryYear": "2028",
    "isDefault": true,
    "createdAt": "2025-01-15T10:00:00Z"
  }
]
```

---

#### **POST /api/payment-methods**
Add a new payment method.

**Request**:
```json
{
  "type": 0,
  "cardHolderName": "John Doe",
  "cardNumber": "4532123456780000",
  "expiryMonth": "12",
  "expiryYear": "2028",
  "cvv": "123"
}
```

**Validation**:
- Card: Requires cardHolderName, cardNumber (13+ digits), expiryMonth/Year, cvv (3+ digits)
- PayPal: Requires payPalEmail
- First method automatically set as default

**Response** (201 Created):
```json
{
  "id": 10,
  "cardNumberMasked": "**** **** **** 0000",
  "cardNumberLast4": "0000",
  "isDefault": false
}
```

---

#### **PUT /api/payment-methods/{id}/set-default**
Set a payment method as default.

**Response** (200 OK): Updated payment method

---

#### **DELETE /api/payment-methods/{id}**
Delete a payment method.

**Validation**: Cannot delete if pending orders use it

**Response** (204 No Content)

---

### Two-Step Checkout

#### **POST /api/orders/create**
Create pending order, reserve stock, keep cart.

**Request**:
```json
{
  "shippingAddressId": 2
}
```

**Process**:
1. Validates cart not empty
2. Checks stock availability
3. Reserves stock (decrements StockQuantity)
4. Creates order with PaymentStatus = Pending
5. Sets ExpiresAt = 15 minutes from now
6. **Cart NOT cleared**

**Response** (201 Created):
```json
{
  "id": 42,
  "userId": 2,
  "totalPrice": 45.90,
  "status": 0,
  "paymentStatus": 0,
  "shippingAddressId": 2,
  "expiresAt": "2025-11-28T10:45:00Z",
  "orderDate": "2025-11-28T10:30:00Z",
  "items": [...],
  "message": "Order created. Please complete payment within 15 minutes."
}
```

**Errors**:
- 400: Cart empty, invalid address, insufficient stock

---

#### **POST /api/payments/process**
Process payment for pending order.

**Request**:
```json
{
  "orderId": 42,
  "paymentMethodId": 3
}
```

**Validation**:
1. Order exists and belongs to user
2. PaymentStatus = Pending
3. Not expired (ExpiresAt > now)
4. PaymentMethod exists and belongs to user

**Success Response** (200 OK):
```json
{
  "message": "Payment successful",
  "order": {
    "id": 42,
    "paymentStatus": 1,
    "transactionId": "TXN_ABC12345",
    "totalPrice": 45.90
  },
  "transaction": {
    "id": 15,
    "transactionId": "TXN_ABC12345",
    "amount": 45.90,
    "status": 1,
    "processedAt": "2025-11-28T10:31:00Z",
    "responseMessage": "Payment successful"
  }
}
```

**Failure Response** (400 Bad Request):
```json
{
  "error": "Payment failed: Insufficient funds",
  "canRetry": true,
  "transaction": {
    "id": 16,
    "transactionId": "TXN_DEF45678",
    "amount": 45.90,
    "status": 2,
    "processedAt": "2025-11-28T10:31:30Z",
    "responseMessage": "Payment failed: Insufficient funds",
    "failureReason": "INSUFFICIENT_FUNDS"
  }
}
```

**On Success**:
- Order.PaymentStatus = Completed
- Cart cleared
- Coupon usage tracked (if applied)
- Stock remains reserved

**On Failure**:
- Order stays PaymentStatus = Pending
- Cart unchanged
- Stock still reserved
- User can retry

---

### Order Management

#### **GET /api/orders/pending**
View pending payment orders with time remaining.

**Response** (200 OK):
```json
[
  {
    "id": 42,
    "totalPrice": 45.90,
    "orderDate": "2025-11-28T10:30:00Z",
    "expiresAt": "2025-11-28T10:45:00Z",
    "minutesRemaining": 12.5,
    "isExpired": false,
    "items": [...]
  }
]
```

---

#### **DELETE /api/orders/{id}/cancel**
Cancel pending order, restore stock.

**Requirements**:
- Order must have PaymentStatus = Pending
- User must own order

**Process**:
1. Validates order pending
2. Restores stock to products
3. Sets Status = Cancelled, PaymentStatus = Failed

**Response** (200 OK):
```json
{
  "message": "Order cancelled successfully",
  "stockRestored": true,
  "order": {...}
}
```

---

### Transaction History

#### **GET /api/payments/transactions**
Get authenticated user's payment transaction history.

**Response** (200 OK):
```json
[
  {
    "id": 15,
    "transactionId": "TXN_ABC12345",
    "orderId": 42,
    "amount": 45.90,
    "status": 1,
    "paymentMethodId": 3,
    "processedAt": "2025-11-28T10:31:00Z",
    "responseMessage": "Payment successful"
  },
  {
    "id": 16,
    "transactionId": "TXN_DEF45678",
    "orderId": null,
    "amount": 45.90,
    "status": 2,
    "paymentMethodId": 4,
    "processedAt": "2025-11-28T10:29:00Z",
    "responseMessage": "Payment failed: Insufficient funds",
    "failureReason": "INSUFFICIENT_FUNDS"
  }
]
```

---

### Admin Endpoints

#### **POST /api/payments/refund**
Process refund for completed transaction (Admin only).

**Request**:
```json
{
  "transactionId": "TXN_ABC12345",
  "amount": 45.90,
  "reason": "Customer requested refund"
}
```

**Validation**:
- Transaction Status = Completed
- Amount ≤ (Transaction.Amount - RefundedAmount)

**Response** (200 OK):
```json
{
  "message": "Refund successful: $45.90",
  "transaction": {
    "transactionId": "TXN_ABC12345",
    "status": 3,
    "refundedAmount": 45.90,
    "remainingAmount": 0
  }
}
```

**Behavior**:
- Full refund: Status = Refunded, Order.Status = Cancelled, stock restored
- Partial refund: Status = PartiallyRefunded

---

#### **GET /api/payments/admin/transactions**
View all payment transactions (Admin only).

**Query Parameters**:
- `status` (optional): Filter by PaymentStatus (0-4)

**Response**: Array of all transactions

---

#### **GET /api/payments/admin/statistics**
Get payment statistics (Admin only).

**Response** (200 OK):
```json
{
  "totalTransactions": 150,
  "successfulTransactions": 120,
  "failedTransactions": 30,
  "totalRevenue": 5432.50,
  "totalRefunded": 234.00,
  "successRate": 80.00,
  "failureReasons": [
    { "reason": "INSUFFICIENT_FUNDS", "count": 15 },
    { "reason": "NETWORK_TIMEOUT", "count": 8 },
    { "reason": "FRAUD_DETECTED", "count": 5 },
    { "reason": "CARD_DECLINED", "count": 2 }
  ]
}
```

---

## Seed Data

### Payment Methods (9 seeded)

**Admin User (userId: 1)**:
- ID 1: Credit Card **** 0000 (default, success card)
- ID 2: Debit Card **** 1111 (fail card)

**User (userId: 2)**:
- ID 3: Credit Card **** 0000 (default, success)
- ID 4: Credit Card **** 1111 (insufficient funds)
- ID 5: PayPal john.doe@example.com (success)
- ID 8: Debit Card **** 4444 (expired)
- ID 9: Credit Card **** 6666 (declined)

**User2 (userId: 3)**:
- ID 6: Credit Card **** 2222 (default, timeout)
- ID 7: Credit Card **** 3333 (fraud)

**Purpose**: Provides variety of test scenarios out of the box.

---

## Complete Two-Step Checkout Flow

### Scenario: Successful Purchase with Coupon

```http
# 1. Add items to cart
POST /api/cart/items
{ "productId": 17, "quantity": 2 }

# 2. Apply coupon
POST /api/cart/apply-coupon
{ "code": "SAVE10" }

# 3. View cart (verify discount)
GET /api/cart
→ { "subtotal": 50.00, "discountAmount": 5.00, "totalPrice": 45.00 }

# 4. Create order (reserves stock, keeps cart)
POST /api/orders/create
{ "shippingAddressId": 2 }
→ { "id": 42, "paymentStatus": 0, "expiresAt": "..." }

# 5. Process payment
POST /api/payments/process
{ "orderId": 42, "paymentMethodId": 3 }
→ { "message": "Payment successful" }

# 6. Verify cart cleared
GET /api/cart
→ { "items": [] }

# 7. Verify coupon usage tracked
GET /api/admin/coupons/1/usage
→ Shows usage record for order 42
```

---

### Scenario: Failed Payment with Retry

```http
# 1-4. Same as above (create order)

# 5. First attempt - insufficient funds card (1111)
POST /api/payments/process
{ "orderId": 42, "paymentMethodId": 4 }
→ 400 Bad Request: "Payment failed: Insufficient funds"

# 6. Verify cart still has items
GET /api/cart
→ { "items": [...] }  # NOT cleared

# 7. Verify order still pending
GET /api/orders/pending
→ { "id": 42, "minutesRemaining": 13.2 }

# 8. Retry with success card (0000)
POST /api/payments/process
{ "orderId": 42, "paymentMethodId": 3 }
→ 200 OK: "Payment successful"

# 9. Now cart is cleared
GET /api/cart
→ { "items": [] }

# 10. View transaction history (2 attempts logged)
GET /api/payments/transactions
→ [
  { "transactionId": "TXN_...", "status": 1, "orderId": 42 },  # Success
  { "transactionId": "TXN_...", "status": 2, "orderId": null } # Failed
]
```

---

### Scenario: Order Cancellation

```http
# 1-4. Create order as usual

# 5. Decide not to pay, cancel order
DELETE /api/orders/42/cancel
→ { "message": "Order cancelled", "stockRestored": true }

# 6. Verify stock restored
GET /api/products/17
→ { "stockQuantity": 52 }  # Back to original

# 7. Cart still intact, can modify and try again
GET /api/cart
→ { "items": [...] }  # Still has items
```

---

### Scenario: Order Expiration

```http
# 1-4. Create order

# 5. Wait 15+ minutes (or manually set ExpiresAt to past)

# 6. Try to pay expired order
POST /api/payments/process
{ "orderId": 42, "paymentMethodId": 3 }
→ 400 Bad Request: "Order has expired. Stock has been restored."

# Order automatically cancelled, stock restored
```

---

## Test Scenarios for Automation

### Payment Method Management
✅ Add credit card (success)  
✅ Add debit card (success)  
✅ Add PayPal account (success)  
✅ Add card with invalid CVV (validation error)  
✅ Add card missing required fields (validation error)  
✅ Set default payment method  
✅ Delete payment method  
✅ Cannot delete method with pending order  
✅ Update card expiry date  

### Two-Step Checkout
✅ Create order from cart (success)  
✅ Create order with empty cart (error)  
✅ Create order with out-of-stock item (error)  
✅ Create order reserves stock  
✅ Cart preserved after order creation  

### Payment Processing
✅ Pay with success card (0000)  
✅ Pay with insufficient funds card (1111)  
✅ Pay with timeout card (2222)  
✅ Pay with fraud card (3333)  
✅ Pay with expired card (4444)  
✅ Pay with declined card (6666)  
✅ Pay with amount $666.00 (fails)  
✅ Pay with amount $777.00 (succeeds)  
✅ Pay with PayPal success email  
✅ Pay with PayPal fail email  

### Retry Logic
✅ Failed payment keeps cart  
✅ Failed payment keeps order pending  
✅ Retry same order with different card  
✅ Multiple retry attempts logged  
✅ Final success clears cart  

### Order Management
✅ View pending orders  
✅ Cancel pending order  
✅ Cancelled order restores stock  
✅ Cannot cancel paid order  
✅ Cannot pay already-paid order  
✅ Cannot pay expired order  
✅ Expired order auto-cancelled  

### Refunds (Admin)
✅ Full refund restores stock  
✅ Partial refund doesn't restore stock  
✅ Cannot refund failed transaction  
✅ Cannot refund more than transaction amount  
✅ Refund updates order status  

### Integration
✅ Coupon usage tracked on successful payment  
✅ Coupon usage NOT tracked on failed payment  
✅ Cart discount preserved during payment retries  
✅ Shipping address linked to order  

### Security & Authorization
✅ User can only see own payment methods  
✅ User can only pay own orders  
✅ User cannot view all transactions (admin only)  
✅ User cannot process refunds (admin only)  
✅ Cannot pay with someone else's payment method  

---

## Integration with Existing Features

### Cart System
- Cart preserved during order creation
- Cleared only after successful payment
- Discount/coupon preserved during retries

### Coupon System
- Usage tracked on successful payment
- Usage NOT tracked on failed payment
- `CouponUsage` record created with OrderId

### Order System
- Added PaymentStatus field
- Added TransactionId, PaymentMethodId
- Added ShippingAddressId
- Added ExpiresAt for timeout

### Stock Management
- Reserved on order creation
- Restored on cancellation
- Restored on full refund
- Restored on expiration

---

## Summary Statistics

### Models
- 6 new classes: PaymentMethod, PaymentTransaction, PaymentRequest, PaymentResult, RefundRequest, RefundResult
- 2 new enums: PaymentMethodType, PaymentStatus
- 1 updated model: Order (5 new fields)

### Endpoints (18 total)
- **Payment Methods (5)**:
  - GET /api/payment-methods
  - GET /api/payment-methods/{id}
  - POST /api/payment-methods
  - PUT /api/payment-methods/{id}/set-default
  - DELETE /api/payment-methods/{id}
  
- **Payments (6)**:
  - POST /api/payments/process
  - GET /api/payments/transactions
  - GET /api/payments/transactions/{id}
  - POST /api/payments/refund (admin)
  - GET /api/payments/admin/transactions (admin)
  - GET /api/payments/admin/statistics (admin)
  
- **Orders (3 new)**:
  - POST /api/orders/create
  - GET /api/orders/pending
  - DELETE /api/orders/{id}/cancel
  
- **Plus 4 existing order endpoints**

### Database
- 2 new collections: PaymentMethods, PaymentTransactions
- 9 seed payment methods
- 0 seed transactions (created at runtime)

### Test Patterns
- 7 card failure patterns
- 2 amount-based patterns
- 1 PayPal failure pattern
- 15+ complete test scenarios in Payments.http

### Files Created/Modified
- `Models/PaymentMethod.cs` (created)
- `Services/IPaymentProcessor.cs` (created)
- `Services/PaymentSimulator.cs` (created)
- `Controllers/PaymentMethodsController.cs` (created)
- `Controllers/PaymentsController.cs` (created)
- `Models/Order.cs` (modified)
- `Controllers/OrdersController.cs` (modified)
- `Database.cs` (modified)
- `Tests/Payments.http` (created)
- `PAYMENT_SUMMARY.md` (this file)

---

## Quick Start Example

```http
### 1. Login
POST http://localhost:5052/api/users/login
{ "username": "User", "password": "qazwsxedcrfv12345" }

### 2. View payment methods
GET http://localhost:5052/api/payment-methods
Authorization: Bearer {token}

### 3. Add items to cart
POST http://localhost:5052/api/cart/items
{ "productId": 17, "quantity": 2 }

### 4. Create order (reserves stock)
POST http://localhost:5052/api/orders/create
{ "shippingAddressId": 2 }
→ Returns orderId: 42

### 5. Process payment
POST http://localhost:5052/api/payments/process
{ "orderId": 42, "paymentMethodId": 3 }
→ Success: cart cleared, order completed

### 6. View transaction
GET http://localhost:5052/api/payments/transactions
→ Shows payment history
```

---

*Last Updated: 2025-11-28*  
*Feature Status: ✅ Complete - Two-step checkout, payment simulation, retry logic, refunds, transaction tracking*
