# LittleBugShop Application Flow Diagrams

## 1. Authentication Flow

```mermaid
graph TD
    A[User] -->|POST /api/users/register| B{Registration}
    B -->|Success| C[User Created with 'User' Role]
    B -->|Fail| D[Error: Username exists]
    
    C --> E[User]
    E -->|POST /api/users/login| F{Credentials Valid?}
    F -->|Yes| G[Generate JWT Token]
    G --> H[Set HTTP-Only Cookie]
    G --> I[Return Token in Response]
    H --> J[User Authenticated]
    I --> J
    F -->|No| K[Error: Invalid credentials]
    
    J -->|All subsequent requests| L[JWT in Cookie OR Authorization Header]
```

## 2. Product Browsing & Search Flow

```mermaid
graph TD
    A[User] -->|GET /api/products| B{Apply Filters?}
    B -->|No filters| C[Return All Products]
    B -->|With filters| D[Filter by searchTerm]
    D --> E[Filter by genre]
    E --> F[Filter by author]
    F --> G[Filter by price range]
    G --> H[Sort results]
    H --> I[Return Filtered Products]
    
    A -->|GET /api/products/:id| J[Get Single Product]
    J --> K[Return Product Details + Stock Status]
    
    A -->|GET /api/products/:id/availability| L[Check Stock Availability]
    L --> M{In Stock?}
    M -->|Yes| N[Return: InStock/LowStock + Quantity]
    M -->|No| O[Return: OutOfStock]
```

## 3. Shopping Cart Flow

```mermaid
graph TD
    A[Authenticated User] -->|GET /api/cart| B{Cart Exists?}
    B -->|No| C[Create Empty Cart]
    B -->|Yes| D[Return Existing Cart]
    C --> D
    
    A -->|POST /api/cart/items| E{Product Exists?}
    E -->|No| F[Error: Product not found]
    E -->|Yes| G{Stock Available?}
    G -->|No| H[Error: Insufficient stock]
    G -->|Yes| I{Product in Cart?}
    I -->|Yes| J[Increase Quantity]
    I -->|No| K[Add New Cart Item]
    J --> L[Update Cart]
    K --> L
    L --> M[Return Updated Cart with TotalPrice]
    
    A -->|PUT /api/cart/items/:id| N{Valid Quantity?}
    N -->|No| O[Error: Quantity <= 0]
    N -->|Yes| P{Stock Available?}
    P -->|No| Q[Error: Insufficient stock]
    P -->|Yes| R[Update Item Quantity]
    R --> L
    
    A -->|DELETE /api/cart/items/:id| S[Remove Item from Cart]
    S --> L
    
    A -->|DELETE /api/cart| T[Clear All Items]
    T --> U[Return Empty Cart]
```

## 4. Two-Step Checkout Flow (Order Creation + Payment)

```mermaid
graph TD
    A[Authenticated User] -->|POST /api/orders/create| B{Cart Empty?}
    B -->|Yes| C[Error: Cart is empty]
    B -->|No| D[Validate All Items]
    
    D --> E{Products Exist?}
    E -->|No| F[Error: Product no longer exists]
    E -->|Yes| G{Stock Available for All?}
    G -->|No| H[Error: Insufficient stock]
    G -->|Yes| I[Reserve Stock]
    
    I --> J[Create Order with Status: Pending]
    J --> K[Set Expiration: 15 minutes]
    K --> L[Keep Cart Items]
    L --> M[Return Order ID + Expiration Time]
    
    M --> N[User: GET /api/payment-methods]
    N --> O{Has Payment Methods?}
    O -->|No| P[POST /api/payment-methods - Add Card/PayPal]
    O -->|Yes| Q[Select Payment Method]
    P --> Q
    
    Q -->|POST /api/payments/process| R{Payment Valid?}
    R -->|Success| S[Payment Status: Completed]
    R -->|Failed| T[Payment Status: Failed]
    
    S --> U[Order Status: Processing]
    U --> V[Clear Cart]
    V --> W[Return Success + Transaction ID]
    
    T --> X[Order Remains Pending]
    X --> Y[Cart Preserved for Retry]
    Y --> Z[Return Failure Reason]
    
    AA[Timeout: 15 min] --> AB[DELETE /api/orders/:id/cancel]
    AB --> AC[Restore Stock]
    AC --> AD[Delete Pending Order]
```

## 5. Payment Processing Flow

```mermaid
graph TD
    A[User] -->|POST /api/payments/process| B{Order Exists?}
    B -->|No| C[Error: Order not found]
    B -->|Yes| D{Order Status = Pending?}
    D -->|No| E[Error: Order already processed/cancelled]
    D -->|Yes| F{Payment Method Valid?}
    F -->|No| G[Error: Invalid payment method]
    F -->|Yes| H[PaymentSimulator: Process Payment]
    
    H --> I{Test Card Pattern?}
    I -->|xxxx0000| J[Success]
    I -->|xxxx1111| K[Failed: Insufficient Funds]
    I -->|xxxx2222| L[Failed: Timeout]
    I -->|xxxx3333| M[Failed: Fraud Detection]
    I -->|xxxx4444| N[Failed: Card Expired]
    I -->|xxxx5555| O[Failed: Invalid CVV]
    I -->|xxxx6666| P[Failed: Declined]
    
    J --> Q[Create Transaction: Status=Completed]
    K --> R[Create Transaction: Status=Failed]
    L --> R
    M --> R
    N --> R
    O --> R
    P --> R
    
    Q --> S[Update Order: Status=Processing]
    S --> T[Link Transaction to Order]
    T --> U[Apply Coupon if present]
    U --> V[Clear User Cart]
    V --> W[Return PaymentResult: Success=true]
    
    R --> X[Update Order: Keep Pending]
    X --> Y[Link Failed Transaction]
    Y --> Z[Keep Cart for Retry]
    Z --> AA[Return PaymentResult: Success=false + FailureReason]
```

## 6. Payment Method Management Flow

```mermaid
graph TD
    A[User] -->|GET /api/payment-methods| B[Return User's Payment Methods]
    
    A -->|POST /api/payment-methods| C{Type?}
    C -->|CreditCard/DebitCard| D[Validate Card Details]
    C -->|PayPal| E[Validate PayPal Email]
    
    D --> F[Mask Card Number - Keep Last 4]
    F --> G[Store: CardNumberLast4, ExpiryMonth/Year]
    G --> H{Is First Method?}
    
    E --> H
    
    H -->|Yes| I[Set as Default]
    H -->|No| J[Store as Additional Method]
    I --> K[Return Payment Method]
    J --> K
    
    A -->|PUT /api/payment-methods/:id/set-default| L{Method Exists?}
    L -->|No| M[Error: Not found]
    L -->|Yes| N[Unset Previous Default]
    N --> O[Set New Default]
    O --> K
    
    A -->|DELETE /api/payment-methods/:id| P{Has Pending Orders?}
    P -->|Yes| Q[Error: Cannot delete - pending orders exist]
    P -->|No| R[Delete Payment Method]
    R --> S[Return Success]
```

## 7. Order Management Flow (Updated)

```mermaid
graph TD
    A[User] -->|GET /api/orders/my-orders| B[Return User's Orders with Payment Status]
    A -->|GET /api/orders/pending| C[Return Pending Orders + Time Remaining]
    
    D[Admin] -->|GET /api/orders| E[Return All Orders]
    
    A -->|GET /api/orders/:id| F[Return Order + Transaction Details]
    
    A -->|DELETE /api/orders/:id/cancel| G{Order Status?}
    G -->|Pending| H[Restore Reserved Stock]
    G -->|Other| I[Error: Cannot cancel]
    H --> J[Delete Order]
    J --> K[Return Success]
    
    D -->|PUT /api/orders/:id/status| L{New Status?}
    L -->|Cancelled| M{Previously Cancelled?}
    M -->|No| N[Restore Stock for All Items]
    M -->|Yes| O[Skip Stock Restoration]
    N --> P[Update Order Status]
    O --> P
    L -->|Processing/Shipped/Delivered| P
    P --> Q[Return Updated Order]
    
    R[User tries Admin Action] -->|PUT /api/orders/:id/status| S[Error: Forbidden - Admin only]
```

## 6. Stock Management Flow (Admin)

```mermaid
graph TD
    A[Admin] -->|PUT /api/products/:id/stock| B{Quantity Valid?}
    B -->|Negative| C[Error: Cannot be negative]
    B -->|Valid| D[Set Stock to Exact Quantity]
    D --> E[Return Updated Stock Status]
    
    A -->|POST /api/products/:id/stock/increase| F{Amount > 0?}
    F -->|No| G[Error: Amount must be > 0]
    F -->|Yes| H[Add Amount to Stock]
    H --> E
    
    A -->|POST /api/products/:id/stock/decrease| I{Amount > 0?}
    I -->|No| G
    I -->|Yes| J{Enough Stock?}
    J -->|No| K[Error: Cannot decrease by X]
    J -->|Yes| L[Subtract Amount from Stock]
    L --> E
    
    M[User tries Stock Management] --> N[Error: Forbidden - Admin only]
```

## 8. Refund Processing Flow (Admin)

```mermaid
graph TD
    A[Admin] -->|POST /api/payments/refund| B{Transaction Exists?}
    B -->|No| C[Error: Transaction not found]
    B -->|Yes| D{Transaction Status = Completed?}
    D -->|No| E[Error: Can only refund completed payments]
    D -->|Yes| F{Refund Amount Valid?}
    F -->|> Transaction Amount| G[Error: Cannot refund more than paid]
    F -->|Valid| H{Full Refund?}
    
    H -->|Yes| I[PaymentSimulator: Process Full Refund]
    H -->|No| J[PaymentSimulator: Process Partial Refund]
    
    I --> K[Update Transaction: Status=Refunded]
    J --> L[Update Transaction: Status=PartiallyRefunded]
    
    K --> M[Update RefundedAmount]
    L --> M
    
    M --> N[Restore Stock to Products]
    N --> O[Update Order Status to Cancelled]
    O --> P[Return RefundResult: Success=true]
```

## 9. Complete User Journey Example (Updated with Payments)

```mermaid
graph TD
    A[New User] -->|Register| B[Account Created]
    B -->|Login| C[Receive JWT Token]
    C -->|Add Payment Method| D[CreditCard ending in 0000]
    D -->|Browse Products| E[Search: Fiction Genre]
    E -->|Find Products| F[Product 1: $23.99]
    F -->|Add to Cart: 2 copies| G[Cart: 2 items, $47.98]
    G -->|Create Order| H[POST /api/orders/create]
    H --> I[Order #1: Pending, Stock Reserved, Expires in 15min]
    
    I -->|Process Payment| J[POST /api/payments/process]
    J -->|Card 0000: Success| K[Transaction: TXN_abc123, Status=Completed]
    K --> L[Order #1: Status=Processing]
    L --> M[Cart Cleared]
    
    N[Alternative: Payment Fails] -->|Card 1111| O[Transaction: Failed - Insufficient Funds]
    O --> P[Order #1: Still Pending]
    P --> Q[Cart Preserved]
    Q -->|Retry with Different Card| R[Update Payment Method]
    R --> J
    
    S[Admin Reviews Orders] -->|GET /api/payments/admin/transactions| T[View All Transactions]
    T -->|See Successful Payment| U[Ship Order #1]
    U -->|Update Status| V[Order #1: Shipped]
    V --> W[Order #1: Delivered]
    
    X[User Requests Refund] -->|Admin Issues Refund| Y[POST /api/payments/refund]
    Y --> Z[Stock Restored, Order Cancelled]
```

## 10. Data Model Relationships (Updated)

```mermaid
graph TD
    A[New User] -->|Register| B[Account Created]
    B -->|Login| C[Receive JWT Token]
    C -->|Browse Products| D[Search: Fantasy Genre]
    D -->|Filter by Author| E[Find Harry Potter Books]
    E -->|Check Stock| F[Product 17: 50 in stock]
    F -->|Add to Cart: 2 copies| G[Cart: 2 items, $45.98]
    G -->|Browse More| H[Add Product 18: 1 copy]
    H -->|View Cart| I[Cart: 3 items, $69.97]
    I -->|Checkout| J[Stock Decreased: P17=48, P18=44]
    J -->|Order Created| K[Order #1, Status: Pending]
    
    L[Admin Reviews Orders] -->|View All Orders| M[See Order #1]
    M -->|Update Status| N[Order #1: Processing]
    N -->|Ship Order| O[Order #1: Shipped]
    O -->|Deliver| P[Order #1: Delivered]
    
    Q[User Cancels Different Order] -->|Admin Cancels| R[Order #2: Cancelled]
    R --> S[Stock Restored Automatically]
```

## 8. Data Model Relationships

```mermaid
erDiagram
    USER ||--o{ ORDER : places
    USER ||--o| CART : has
    USER ||--o{ PAYMENT_METHOD : owns
    USER ||--o{ REVIEW : writes
    USER ||--o{ WISHLIST : has
    USER ||--o{ ADDRESS : has
    USER ||--o{ COUPON_USAGE : uses
    
    CART ||--o{ CART_ITEM : contains
    ORDER ||--o{ ORDER_ITEM : contains
    ORDER ||--o| PAYMENT_TRANSACTION : has
    ORDER ||--o| PAYMENT_METHOD : uses
    ORDER ||--o| ADDRESS : ships_to
    ORDER ||--o| COUPON : applied
    
    PRODUCT ||--o{ ORDER_ITEM : referenced_by
    PRODUCT ||--o{ CART_ITEM : referenced_by
    PRODUCT ||--o{ REVIEW : has
    PRODUCT ||--o{ WISHLIST : in
    
    COUPON ||--o{ COUPON_USAGE : tracked_by
    
    REVIEW ||--o{ REVIEW_HELPFUL : has_votes
    
    USER {
        int Id PK
        string Username
        string Password
        string Role
        string Email
        string FirstName
        string LastName
        string PhoneNumber
    }
    
    PRODUCT {
        int Id PK
        string Name
        string Author
        string Genre
        string ISBN
        decimal Price
        int StockQuantity
        int LowStockThreshold
        string StockStatus
        string Description
        string CoverImageUrl
    }
    
    CART {
        int Id PK
        int UserId FK
        DateTime LastUpdated
        decimal TotalPrice
        int TotalItems
    }
    
    CART_ITEM {
        int Id PK
        int CartId FK
        int ProductId FK
        string ProductName
        string Author
        decimal UnitPrice
        int Quantity
        decimal TotalPrice
    }
    
    ORDER {
        int Id PK
        int UserId FK
        DateTime OrderDate
        decimal TotalPrice
        OrderStatus Status
        PaymentStatus PaymentStatus
        string TransactionId
        int PaymentMethodId FK
        int ShippingAddressId FK
        DateTime ExpiresAt
        int CouponId FK
    }
    
    ORDER_ITEM {
        int Id PK
        int OrderId FK
        int ProductId FK
        string ProductName
        int Quantity
        decimal UnitPrice
        decimal TotalPrice
    }
    
    PAYMENT_METHOD {
        int Id PK
        int UserId FK
        PaymentMethodType Type
        string CardHolderName
        string CardNumberMasked
        string CardNumberLast4
        int ExpiryMonth
        int ExpiryYear
        string PayPalEmail
        bool IsDefault
    }
    
    PAYMENT_TRANSACTION {
        string TransactionId PK
        int OrderId FK
        decimal Amount
        PaymentStatus Status
        string FailureReason
        decimal RefundedAmount
        DateTime CreatedAt
    }
    
    ADDRESS {
        int Id PK
        int UserId FK
        string StreetAddress
        string City
        string State
        string ZipCode
        string Country
        bool IsDefaultShipping
        bool IsDefaultBilling
    }
    
    REVIEW {
        int Id PK
        int ProductId FK
        int UserId FK
        int Rating
        string Title
        string Comment
        DateTime CreatedAt
        bool IsVerifiedPurchase
        bool IsHidden
    }
    
    REVIEW_HELPFUL {
        int Id PK
        int ReviewId FK
        int UserId FK
        bool IsHelpful
    }
    
    WISHLIST {
        int Id PK
        int UserId FK
        int ProductId FK
        DateTime AddedAt
    }
    
    COUPON {
        int Id PK
        string Code
        CouponType Type
        decimal DiscountValue
        decimal MinimumPurchase
        DateTime ValidFrom
        DateTime ValidUntil
        int MaxUses
        int CurrentUses
        bool IsActive
    }
    
    COUPON_USAGE {
        int Id PK
        int CouponId FK
        int UserId FK
        int OrderId FK
        DateTime UsedAt
    }
```

## 11. Authorization Matrix (Updated)

| Endpoint | Anonymous | User | Admin |
|----------|-----------|------|-------|
| **Authentication** |
| POST /api/users/register | ✅ | ✅ | ✅ |
| POST /api/users/login | ✅ | ✅ | ✅ |
| POST /api/users/logout | ✅ | ✅ | ✅ |
| GET /api/session/user | ❌ | ✅ | ✅ |
| **Products** |
| GET /api/products | ✅ | ✅ | ✅ |
| GET /api/products/:id | ✅ | ✅ | ✅ |
| GET /api/products/:id/availability | ✅ | ✅ | ✅ |
| PUT /api/products/:id/stock | ❌ | ❌ | ✅ |
| POST /api/products/:id/stock/increase | ❌ | ❌ | ✅ |
| POST /api/products/:id/stock/decrease | ❌ | ❌ | ✅ |
| **Cart** |
| GET /api/cart | ❌ | ✅ | ✅ |
| POST /api/cart/items | ❌ | ✅ | ✅ |
| PUT /api/cart/items/:id | ❌ | ✅ | ✅ |
| DELETE /api/cart/items/:id | ❌ | ✅ | ✅ |
| DELETE /api/cart | ❌ | ✅ | ✅ |
| **Orders** |
| POST /api/orders/create | ❌ | ✅ | ✅ |
| GET /api/orders/pending | ❌ | ✅ | ✅ |
| DELETE /api/orders/:id/cancel | ❌ | ✅ | ✅ |
| POST /api/cart/checkout | ❌ | ✅ | ✅ |
| POST /api/orders/place | ❌ | ✅ | ✅ |
| GET /api/orders | ❌ | ❌ | ✅ |
| GET /api/orders/my-orders | ❌ | ✅ | ✅ |
| GET /api/orders/:id | ❌ | ✅ | ✅ |
| PUT /api/orders/:id/status | ❌ | ❌ | ✅ |
| DELETE /api/orders/:id | ❌ | ✅ | ✅ |
| **Payment Methods** |
| GET /api/payment-methods | ❌ | ✅ | ✅ |
| GET /api/payment-methods/:id | ❌ | ✅ | ✅ |
| POST /api/payment-methods | ❌ | ✅ | ✅ |
| PUT /api/payment-methods/:id/set-default | ❌ | ✅ | ✅ |
| DELETE /api/payment-methods/:id | ❌ | ✅ | ✅ |
| **Payments** |
| POST /api/payments/process | ❌ | ✅ | ✅ |
| GET /api/payments/transactions | ❌ | ✅ | ✅ |
| GET /api/payments/transactions/:id | ❌ | ✅ | ✅ |
| POST /api/payments/refund | ❌ | ❌ | ✅ |
| GET /api/payments/admin/transactions | ❌ | ❌ | ✅ |
| GET /api/payments/admin/statistics | ❌ | ❌ | ✅ |
| **Profile & Addresses** |
| GET /api/profile | ❌ | ✅ | ✅ |
| PUT /api/profile | ❌ | ✅ | ✅ |
| POST /api/profile/addresses | ❌ | ✅ | ✅ |
| PUT /api/profile/addresses/:id | ❌ | ✅ | ✅ |
| DELETE /api/profile/addresses/:id | ❌ | ✅ | ✅ |
| PUT /api/profile/addresses/:id/set-default-shipping | ❌ | ✅ | ✅ |
| PUT /api/profile/addresses/:id/set-default-billing | ❌ | ✅ | ✅ |
| **Reviews** |
| GET /api/products/:productId/reviews | ✅ | ✅ | ✅ |
| GET /api/products/:productId/reviews/summary | ✅ | ✅ | ✅ |
| POST /api/products/:productId/reviews | ❌ | ✅ | ✅ |
| PUT /api/products/:productId/reviews/:id | ❌ | ✅ (own) | ✅ |
| DELETE /api/products/:productId/reviews/:id | ❌ | ✅ (own) | ✅ |
| POST /api/products/:productId/reviews/:id/helpful | ❌ | ✅ | ✅ |
| PUT /api/products/:productId/reviews/:id/hide | ❌ | ❌ | ✅ |
| **Wishlist** |
| GET /api/wishlist | ❌ | ✅ | ✅ |
| POST /api/wishlist | ❌ | ✅ | ✅ |
| DELETE /api/wishlist/:id | ❌ | ✅ | ✅ |
| POST /api/wishlist/:id/move-to-cart | ❌ | ✅ | ✅ |
| **Coupons** |
| POST /api/coupons/validate | ❌ | ✅ | ✅ |
| GET /api/coupons | ❌ | ❌ | ✅ |
| POST /api/coupons | ❌ | ❌ | ✅ |
| PUT /api/coupons/:id | ❌ | ❌ | ✅ |
| DELETE /api/coupons/:id | ❌ | ❌ | ✅ |
| GET /api/coupons/usage-report | ❌ | ❌ | ✅ |

## 12. Stock Status State Machine

```mermaid
stateDiagram-v2
    [*] --> InStock: StockQuantity > LowStockThreshold
    [*] --> LowStock: 0 < StockQuantity <= LowStockThreshold
    [*] --> OutOfStock: StockQuantity = 0
    
    InStock --> LowStock: Order placed, stock decreases
    LowStock --> OutOfStock: Order placed, stock reaches 0
    OutOfStock --> LowStock: Admin increases stock (small amount)
    LowStock --> InStock: Admin increases stock (large amount)
    OutOfStock --> InStock: Admin restocks
    
    InStock --> InStock: Admin increases stock
    LowStock --> LowStock: Small stock changes
    
    note right of OutOfStock
        Cannot add to cart
        Cannot place order
    end note
    
    note right of LowStock
        Can order (if quantity available)
        Warning indicator
    end note
    
    note right of InStock
        Normal operations
        Full stock available
    end note
```

## 13. Order Status Lifecycle (Updated with Payments)

```mermaid
stateDiagram-v2
    [*] --> Pending: Order Created (Stock Reserved)
    
    Pending --> Processing: Payment Successful
    Pending --> Cancelled: Payment Failed (15 min timeout)
    Pending --> Cancelled: User Cancels
    
    Processing --> Shipped: Admin ships
    Processing --> Cancelled: Admin cancels (with refund)
    
    Shipped --> Delivered: Admin confirms delivery
    Shipped --> Cancelled: Admin cancels (with refund)
    
    Delivered --> [*]: Final state
    
    Cancelled --> [*]: Stock restored
    
    note right of Cancelled
        Stock quantities
        automatically restored
        Refund processed if paid
    end note
    
    note right of Pending
        Stock reserved
        Cart preserved
        15-minute expiration
        Payment not yet processed
    end note
    
    note right of Processing
        Payment completed
        Cart cleared
        Ready to ship
    end note
```

## 14. Payment Status Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Pending: Order Created
    
    Pending --> Completed: Payment Successful
    Pending --> Failed: Payment Failed
    
    Completed --> Refunded: Full Refund Issued
    Completed --> PartiallyRefunded: Partial Refund Issued
    
    PartiallyRefunded --> Refunded: Additional Refund (if total)
    
    Failed --> Pending: User Retries Payment
    
    Refunded --> [*]: Final state
    
    note right of Failed
        Cart preserved
        Order remains pending
        User can retry
    end note
    
    note right of Completed
        Payment confirmed
        Order processing
        Stock reserved
    end note
    
    note right of Refunded
        Money returned
        Stock restored
        Order cancelled
    end note
```

## Key Features Summary

### User Features:
- ✅ Register/Login with JWT authentication
- ✅ Browse and search products (by name, author, genre, price)
- ✅ Check product availability and stock status
- ✅ Manage shopping cart (add, update, remove items)
- ✅ **Two-step checkout** (create order → process payment)
- ✅ **Manage payment methods** (Credit/Debit cards, PayPal)
- ✅ **Process payments** with test card patterns
- ✅ **Retry failed payments** (cart preserved)
- ✅ **Cancel pending orders** (stock restored)
- ✅ View order history with payment status
- ✅ View transaction history
- ✅ **User profile management** (personal info, addresses)
- ✅ **Multiple addresses** (shipping/billing, default settings)
- ✅ **Write and manage reviews** (ratings, comments, verified purchase badge)
- ✅ **Vote on reviews** (helpful/not helpful)
- ✅ **Wishlist management** (add, remove, move to cart)
- ✅ **Apply discount coupons** (percentage/fixed, validation)
- ✅ Stock validation prevents over-ordering

### Admin Features:
- ✅ All user features
- ✅ View all orders in system
- ✅ Update order status (Pending → Processing → Shipped → Delivered)
- ✅ Cancel orders (with automatic stock restoration)
- ✅ **Issue refunds** (full/partial, stock restoration)
- ✅ **View all transactions** with filtering
- ✅ **Payment statistics** (revenue, success rate, failure reasons)
- ✅ Manage product stock (set, increase, decrease)
- ✅ **Hide/unhide reviews** (moderation)
- ✅ **Manage coupons** (create, edit, delete, usage reports)
- ✅ Full inventory control

### System Features:
- ✅ JWT authentication (cookie + header support)
- ✅ Role-based authorization (Admin vs User)
- ✅ **Two-step checkout flow** (order creation + payment processing)
- ✅ **Payment simulation** with test card patterns (7 failure scenarios)
- ✅ **Payment retry logic** (cart preservation on failure)
- ✅ **15-minute order expiration** for pending orders
- ✅ **Automatic stock restoration** on cancellation/refund
- ✅ **Transaction audit trail** (success and failure tracking)
- ✅ **Multiple payment methods** per user
- ✅ **Secure card storage** (masked numbers, last 4 digits only)
- ✅ Real-time stock availability checking
- ✅ Shopping cart persistence per user
- ✅ Order status tracking with payment integration
- ✅ **Review verification** (verified purchase badge)
- ✅ **Review helpfulness voting** (community feedback)
- ✅ **Coupon validation** (expiry, usage limits, minimum purchase)
- ✅ **Address management** (multiple addresses, default settings)
- ✅ Computed properties (prices, totals, stock status, review statistics)

### Test Automation Features:
- ✅ **Deterministic payment outcomes** (test card patterns)
- ✅ **Predictable failure scenarios** (7 distinct failure types)
- ✅ **Comprehensive HTTP test files** (60+ payment scenarios, 40+ review scenarios, etc.)
- ✅ **Stock simulation** (low stock warnings, out of stock handling)
- ✅ **Time-based scenarios** (order expiration, coupon validity)
- ✅ **Multi-user testing** (3 seeded users with different roles)
- ✅ **Complete API coverage** (80+ endpoints across 11 controllers)
