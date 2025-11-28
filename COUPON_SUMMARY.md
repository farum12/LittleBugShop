# Coupon & Discount System Summary

## Overview
The coupon system enables users to apply discount codes to their shopping carts and provides administrators with full coupon lifecycle management. The system supports both percentage-based and fixed-amount discounts with flexible expiration and usage limits.

**Design Philosophy**: Simplified approach focused on core functionality - no complex conditions, minimum purchases, product restrictions, or user-specific limits. This makes it ideal for test automation scenarios with predictable, easy-to-validate behavior.

---

## Data Models

### Coupon
Represents a discount code that can be applied to a shopping cart.

```csharp
public class Coupon
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;      // e.g., "SAVE10", "WELCOME5"
    public DiscountType Type { get; set; }                 // Percentage or FixedAmount
    public decimal Value { get; set; }                     // 10 (10%) or 5.00 ($5)
    public DateTime? ExpirationDate { get; set; }          // null = never expires
    public int? MaxUsesTotal { get; set; }                 // null = unlimited
    public bool IsActive { get; set; } = true;             // Enable/disable without deletion
    public int CurrentUses { get; set; } = 0;              // Tracks usage count
    public DateTime CreatedAt { get; set; }                // Audit trail
}
```

**Key Fields**:
- `Code`: Unique coupon identifier (3-20 characters, case-insensitive)
- `Type`: Enum - Percentage (0) or FixedAmount (1)
- `Value`: For percentage: 0.01-100, for fixed: > 0
- `ExpirationDate`: UTC datetime, null = no expiration
- `MaxUsesTotal`: null = unlimited, 0 = effectively disabled
- `IsActive`: Soft delete - deactivate without removing usage history
- `CurrentUses`: Incremented on each successful order checkout

### DiscountType
Enum defining discount calculation method.

```csharp
public enum DiscountType
{
    Percentage = 0,    // Value is percentage (e.g., 10 = 10% off)
    FixedAmount = 1    // Value is dollar amount (e.g., 5.00 = $5 off)
}
```

### CouponUsage
Tracks each time a coupon is used in an order for audit and analytics.

```csharp
public class CouponUsage
{
    public int Id { get; set; }
    public int CouponId { get; set; }      // Links to Coupon
    public int UserId { get; set; }         // Who used it
    public int OrderId { get; set; }        // Which order
    public DateTime UsedAt { get; set; }    // When used
}
```

### Cart Extensions
Shopping cart extended to support coupon application:

```csharp
public class Cart
{
    // ... existing properties
    public string? AppliedCouponCode { get; set; }         // Currently applied coupon code
    
    // Computed Properties
    public decimal Subtotal => Items.Sum(i => i.UnitPrice * i.Quantity);
    public decimal DiscountAmount { get; set; }            // Calculated discount
    public decimal TotalPrice => Subtotal - DiscountAmount; // Final price after discount
}
```

**Calculation Flow**:
1. User adds items to cart → `Subtotal` computed from items
2. User applies coupon → `DiscountAmount` calculated based on coupon type
3. `TotalPrice` = `Subtotal` - `DiscountAmount`
4. Checkout → Usage tracked, cart cleared (including coupon)

---

## API Endpoints

### User Endpoints

#### **Validate Coupon**
Preview coupon details and validity without applying.

```http
GET /api/coupons/validate/{code}
Authorization: Bearer {token}
```

**Response** (200 OK):
```json
{
  "id": 1,
  "code": "SAVE10",
  "type": 0,
  "value": 10.0,
  "expirationDate": null,
  "maxUsesTotal": null,
  "isActive": true,
  "currentUses": 0,
  "createdAt": "2025-01-01T00:00:00Z"
}
```

**Errors**:
- 404 Not Found: Coupon code doesn't exist
- 400 Bad Request: Coupon invalid (expired, max uses, inactive)

---

#### **Apply Coupon to Cart**
Apply a discount code to the current cart.

```http
POST /api/cart/apply-coupon
Authorization: Bearer {token}
Content-Type: application/json

{
  "code": "SAVE10"
}
```

**Response** (200 OK):
```json
{
  "userId": 1,
  "items": [...],
  "appliedCouponCode": "SAVE10",
  "subtotal": 50.00,
  "discountAmount": 5.00,
  "totalPrice": 45.00
}
```

**Validation Rules**:
1. Cart must not be empty
2. Coupon code must exist (case-insensitive)
3. Coupon must be active (`IsActive = true`)
4. Not expired (`ExpirationDate` null or future)
5. Under usage limit (`CurrentUses < MaxUsesTotal` or null)

**Discount Calculation**:
- **Percentage**: `discount = subtotal × (value / 100)`
  - Example: $50 subtotal × (10 / 100) = $5 off
- **FixedAmount**: `discount = min(value, subtotal)`
  - Example: $5 off a $50 cart = $5, but $5 off a $3 cart = $3 (can't be negative)

**Errors**:
- 400 Bad Request: Cart empty, coupon invalid/expired/max uses/inactive
- 404 Not Found: Coupon code doesn't exist

---

#### **Remove Coupon from Cart**
Remove the currently applied coupon.

```http
DELETE /api/cart/remove-coupon
Authorization: Bearer {token}
```

**Response** (200 OK):
```json
{
  "userId": 1,
  "items": [...],
  "appliedCouponCode": null,
  "subtotal": 50.00,
  "discountAmount": 0,
  "totalPrice": 50.00
}
```

**Errors**:
- 400 Bad Request: No coupon currently applied

---

### Admin Endpoints

#### **List All Coupons**
Get all coupons with usage statistics.

```http
GET /api/admin/coupons
Authorization: Bearer {adminToken}
```

**Response** (200 OK):
```json
[
  {
    "id": 1,
    "code": "SAVE10",
    "type": 0,
    "value": 10.0,
    "expirationDate": null,
    "maxUsesTotal": null,
    "isActive": true,
    "currentUses": 42,
    "createdAt": "2025-01-01T00:00:00Z"
  },
  {
    "id": 3,
    "code": "WINTER20",
    "type": 0,
    "value": 20.0,
    "expirationDate": "2025-02-15T23:59:59Z",
    "maxUsesTotal": 100,
    "isActive": true,
    "currentUses": 15,
    "createdAt": "2025-01-01T00:00:00Z"
  }
]
```

**Errors**:
- 403 Forbidden: User is not admin

---

#### **Create Coupon**
Create a new discount coupon.

```http
POST /api/admin/coupons
Authorization: Bearer {adminToken}
Content-Type: application/json

{
  "code": "NEWYEAR25",
  "type": 0,
  "value": 25,
  "expirationDate": "2026-01-31T23:59:59Z",
  "maxUsesTotal": 200
}
```

**Request Model** (`CreateCouponRequest`):
```csharp
{
  "code": string,              // Required, 3-20 chars
  "type": DiscountType,        // Required, 0=Percentage, 1=FixedAmount
  "value": decimal,            // Required, > 0
  "expirationDate": DateTime?, // Optional, null = no expiration
  "maxUsesTotal": int?         // Optional, null = unlimited
}
```

**Response** (201 Created):
```json
{
  "id": 7,
  "code": "NEWYEAR25",
  "type": 0,
  "value": 25.0,
  "expirationDate": "2026-01-31T23:59:59Z",
  "maxUsesTotal": 200,
  "isActive": true,
  "currentUses": 0,
  "createdAt": "2025-01-15T10:30:00Z"
}
```

**Validation**:
- Code must be unique (case-insensitive)
- Code length: 3-20 characters
- Value > 0
- If Percentage: value ≤ 100
- If expiration provided: must be future date

**Errors**:
- 400 Bad Request: Validation failed (duplicate code, invalid value, etc.)
- 403 Forbidden: User is not admin

---

#### **Update Coupon**
Modify an existing coupon (all fields optional).

```http
PUT /api/admin/coupons/{id}
Authorization: Bearer {adminToken}
Content-Type: application/json

{
  "value": 15,
  "isActive": false
}
```

**Request Model** (`UpdateCouponRequest`):
```csharp
{
  "code": string?,             // Optional, must be unique if changed
  "type": DiscountType?,       // Optional
  "value": decimal?,           // Optional, > 0
  "expirationDate": DateTime?, // Optional
  "maxUsesTotal": int?,        // Optional
  "isActive": bool?            // Optional
}
```

**Response** (200 OK):
```json
{
  "id": 1,
  "code": "SAVE10",
  "type": 0,
  "value": 15.0,
  "expirationDate": null,
  "maxUsesTotal": null,
  "isActive": false,
  "currentUses": 42,
  "createdAt": "2025-01-01T00:00:00Z"
}
```

**Common Use Cases**:
- Deactivate coupon: `{ "isActive": false }`
- Extend expiration: `{ "expirationDate": "2026-12-31T23:59:59Z" }`
- Change discount: `{ "value": 20 }`
- Rename code: `{ "code": "NEWSAVE10" }`

**Errors**:
- 404 Not Found: Coupon ID doesn't exist
- 400 Bad Request: Validation failed (duplicate new code, invalid value, etc.)
- 403 Forbidden: User is not admin

---

#### **Delete Coupon**
Permanently delete a coupon and all usage records.

```http
DELETE /api/admin/coupons/{id}
Authorization: Bearer {adminToken}
```

**Response** (204 No Content)

**Behavior**:
- Deletes coupon record
- Deletes all associated `CouponUsage` records (cascade delete)
- Cannot be undone - consider using `IsActive = false` for soft delete

**Errors**:
- 404 Not Found: Coupon ID doesn't exist
- 403 Forbidden: User is not admin

---

#### **Get Coupon Usage Statistics**
View detailed usage history for a coupon.

```http
GET /api/admin/coupons/{id}/usage
Authorization: Bearer {adminToken}
```

**Response** (200 OK):
```json
{
  "coupon": {
    "id": 1,
    "code": "SAVE10",
    "type": 0,
    "value": 10.0,
    "currentUses": 3,
    "maxUsesTotal": null
  },
  "usages": [
    {
      "id": 1,
      "couponId": 1,
      "userId": 2,
      "orderId": 15,
      "usedAt": "2025-01-14T14:30:00Z"
    },
    {
      "id": 5,
      "couponId": 1,
      "userId": 3,
      "orderId": 22,
      "usedAt": "2025-01-15T09:15:00Z"
    }
  ]
}
```

**Use Cases**:
- Audit trail: Who used which coupon and when
- Analytics: Track popular coupons
- Investigation: Verify usage limits working correctly
- Reporting: Generate usage statistics

**Errors**:
- 404 Not Found: Coupon ID doesn't exist
- 403 Forbidden: User is not admin

---

## Validation Rules

### Code Validation
- **Length**: 3-20 characters
- **Uniqueness**: Case-insensitive (SAVE10 = save10)
- **Characters**: Alphanumeric recommended (no strict regex)

### Value Validation
- **Percentage** (Type = 0):
  - Range: 0.01 - 100
  - Example: 10 = 10% off
- **FixedAmount** (Type = 1):
  - Range: > 0
  - Example: 5.00 = $5.00 off

### Expiration Validation
- **Create**: Must be future date if provided
- **Update**: Can set past dates (to expire immediately)
- **null**: Never expires

### Active Coupon Criteria
For a coupon to be applied to a cart, ALL must be true:
1. `IsActive = true`
2. `ExpirationDate` is null OR > current UTC time
3. `MaxUsesTotal` is null OR `CurrentUses < MaxUsesTotal`

---

## Discount Calculation Logic

### Percentage Discount
```csharp
decimal discountAmount = cart.Subtotal * (coupon.Value / 100);
```

**Example**:
- Cart subtotal: $75.00
- Coupon: SAVE10 (10% off)
- Calculation: $75.00 × (10 / 100) = $7.50
- Final price: $75.00 - $7.50 = **$67.50**

### Fixed Amount Discount
```csharp
decimal discountAmount = Math.Min(coupon.Value, cart.Subtotal);
```

**Example 1** (normal):
- Cart subtotal: $50.00
- Coupon: WELCOME5 ($5 off)
- Calculation: min($5.00, $50.00) = $5.00
- Final price: $50.00 - $5.00 = **$45.00**

**Example 2** (discount > subtotal):
- Cart subtotal: $3.00
- Coupon: WELCOME5 ($5 off)
- Calculation: min($5.00, $3.00) = $3.00
- Final price: $3.00 - $3.00 = **$0.00**

**Note**: Discount cannot create negative total. If fixed discount exceeds subtotal, discount = subtotal (free order).

---

## Usage Tracking Flow

### Checkout Process
When a user checks out with a coupon applied:

1. **Verify Coupon Applied**:
   ```csharp
   if (!string.IsNullOrEmpty(cart.AppliedCouponCode))
   ```

2. **Find Coupon**:
   ```csharp
   var coupon = Database.Coupons.FirstOrDefault(c => c.Code == cart.AppliedCouponCode);
   ```

3. **Increment Usage Counter**:
   ```csharp
   coupon.CurrentUses++;
   ```

4. **Create Usage Record**:
   ```csharp
   var usage = new CouponUsage
   {
       Id = Database.CouponUsages.Count + 1,
       CouponId = coupon.Id,
       UserId = userId,
       OrderId = order.Id,
       UsedAt = DateTime.UtcNow
   };
   Database.CouponUsages.Add(usage);
   ```

5. **Clear Cart Coupon**:
   ```csharp
   cart.AppliedCouponCode = null;
   cart.DiscountAmount = 0;
   ```

### Audit Trail
Each checkout creates:
- **Order** record with final price (after discount)
- **CouponUsage** record linking coupon → user → order
- **Updated** `CurrentUses` count on coupon

This enables:
- Per-user usage tracking (query `CouponUsages` by `UserId`)
- Per-coupon analytics (query `CouponUsages` by `CouponId`)
- Order audit (which coupon was used for each order)
- Usage limit enforcement (compare `CurrentUses` to `MaxUsesTotal`)

---

## Seed Data

### Default Coupons
Six coupons seeded for testing various scenarios:

| Code | Type | Value | Expires | Max Uses | Current | Active | Purpose |
|------|------|-------|---------|----------|---------|--------|---------|
| **SAVE10** | % | 10 | Never | ∞ | 0 | ✅ | Basic percentage coupon |
| **WELCOME5** | $ | 5.00 | Never | ∞ | 0 | ✅ | Basic fixed amount coupon |
| **WINTER20** | % | 20 | +30 days | 100 | 15 | ✅ | Limited uses (85 left) |
| **EXPIRED** | % | 15 | -5 days | ∞ | 25 | ✅ | Expired (test expiration) |
| **LIMITED50** | $ | 10.00 | Never | 50 | 50 | ✅ | Max uses reached |
| **INACTIVE** | % | 25 | Never | ∞ | 0 | ❌ | Inactive (test soft delete) |

### Test Scenarios Covered
- ✅ Valid unlimited coupons (SAVE10, WELCOME5)
- ✅ Limited usage with remaining uses (WINTER20)
- ✅ Expired coupon (EXPIRED)
- ✅ Max uses reached (LIMITED50)
- ✅ Inactive coupon (INACTIVE)
- ✅ Percentage vs fixed amount (both types)
- ✅ With and without expiration dates

---

## Test Scenarios

### Basic Flow
1. **Login** as user
2. **Add items** to cart
3. **Validate coupon** (GET `/api/coupons/validate/SAVE10`)
4. **Apply coupon** (POST `/api/cart/apply-coupon` with `"SAVE10"`)
5. **Verify discount** (GET `/api/cart` - check `discountAmount`)
6. **Checkout** (POST `/api/cart/checkout`)
7. **Verify usage tracked** (Admin: GET `/api/admin/coupons/1/usage`)

### Validation Tests
- ❌ Apply to **empty cart** → 400 Bad Request
- ❌ Apply **expired** coupon (EXPIRED) → 400 Bad Request
- ❌ Apply **max uses** coupon (LIMITED50) → 400 Bad Request
- ❌ Apply **inactive** coupon (INACTIVE) → 400 Bad Request
- ❌ Apply **non-existent** code → 404 Not Found
- ✅ Apply **valid** coupon (SAVE10) → 200 OK with discount

### Calculation Tests
- **Percentage**: $50 cart + 10% off = $45 total
- **Fixed**: $30 cart + $5 off = $25 total
- **Edge**: $3 cart + $5 off = $0 total (not negative)
- **Rounding**: Verify decimal precision preserved

### Admin Tests
- ✅ Create valid coupon → 201 Created
- ❌ Create with **duplicate code** → 400 Bad Request
- ❌ Create with **code too short** (< 3 chars) → 400 Bad Request
- ❌ Create with **percentage > 100** → 400 Bad Request
- ❌ Create with **value ≤ 0** → 400 Bad Request
- ✅ Update coupon value → 200 OK
- ✅ Deactivate coupon → 200 OK
- ✅ Delete coupon → 204 No Content
- ✅ Get usage statistics → 200 OK with usage array

### Authorization Tests
- ❌ Regular user **GET** `/api/admin/coupons` → 403 Forbidden
- ❌ Regular user **POST** `/api/admin/coupons` → 403 Forbidden
- ❌ Regular user **PUT** `/api/admin/coupons/1` → 403 Forbidden
- ❌ Regular user **DELETE** `/api/admin/coupons/1` → 403 Forbidden
- ❌ No token **GET** `/api/coupons/validate/SAVE10` → 401 Unauthorized
- ❌ No token **POST** `/api/cart/apply-coupon` → 401 Unauthorized

### Edge Cases
- **Case sensitivity**: Apply "save10" (lowercase) → Should work (case-insensitive)
- **Replace coupon**: Apply SAVE10, then apply WELCOME5 → Second replaces first
- **Remove non-existent**: Remove coupon when none applied → 400 Bad Request
- **Checkout without coupon**: Should work normally (no usage tracking)
- **Concurrent usage**: Multiple users use same coupon → Each increments counter

---

## API Reference Quick Guide

### User Operations
```
GET    /api/coupons/validate/{code}       Preview coupon (auth required)
POST   /api/cart/apply-coupon             Apply discount to cart (auth)
DELETE /api/cart/remove-coupon            Remove discount from cart (auth)
```

### Admin Operations
```
GET    /api/admin/coupons                 List all coupons with stats (admin)
POST   /api/admin/coupons                 Create new coupon (admin)
PUT    /api/admin/coupons/{id}            Update coupon (admin)
DELETE /api/admin/coupons/{id}            Delete coupon (admin)
GET    /api/admin/coupons/{id}/usage      View usage statistics (admin)
```

### Authentication
- **User endpoints**: Requires valid JWT token
- **Admin endpoints**: Requires JWT with admin role
- **Header**: `Authorization: Bearer {token}`

---

## Future Enhancements

The current implementation focuses on core functionality. Potential future additions:

### Skipped for Simplicity
1. **Minimum Purchase Requirements**
   - `decimal? MinimumPurchase` property
   - Validate `cart.Subtotal >= coupon.MinimumPurchase`

2. **Product/Category Restrictions**
   - `List<int> ApplicableProductIds` or `List<string> ApplicableGenres`
   - Filter cart items before calculating discount

3. **Per-User Usage Limits**
   - `int? MaxUsesPerUser` property
   - Count `CouponUsages` for current user before applying

4. **Role-Based Coupons**
   - `string? RequiredRole` property (e.g., "Premium", "VIP")
   - Validate `user.Role == coupon.RequiredRole`

5. **Stackable Coupons**
   - Apply multiple coupons simultaneously
   - Define stacking rules and priorities

6. **Auto-Apply Coupons**
   - `bool AutoApply` property
   - Apply best coupon automatically at checkout

7. **Time-Based Promotions**
   - `DateTime? StartDate` property
   - Active only during specific time windows

### Why Simplified?
- **Test automation focus**: Predictable behavior easier to assert
- **Maintainability**: Fewer edge cases and validation rules
- **Learning project**: Core concepts without over-engineering
- **Performance**: Simpler calculations, faster execution

---

## Integration Points

### Cart System
- `Cart.AppliedCouponCode`: Links cart to active coupon
- `Cart.Subtotal`: Base for discount calculation
- `Cart.DiscountAmount`: Stores calculated discount
- `Cart.TotalPrice`: Final amount (Subtotal - DiscountAmount)

### Order System
- Checkout creates `Order` with final `TotalAmount` (after discount)
- `CouponUsage` record links coupon to order
- Order history reflects discounted prices

### User System
- JWT authentication required for all endpoints
- Admin role required for management endpoints
- `CouponUsage.UserId` tracks who used coupons

### Database
- `Database.Coupons`: List<Coupon> collection
- `Database.CouponUsages`: List<CouponUsage> collection
- In-memory storage, cleared on restart

---

## Summary Statistics

### Models
- 3 new classes: `Coupon`, `CouponUsage`, `DiscountType` enum
- 1 extended class: `Cart` (3 new properties)

### Endpoints
- **8 total endpoints**:
  - 3 user endpoints (validate, apply, remove)
  - 5 admin endpoints (list, create, update, delete, usage stats)

### Database
- 2 new collections
- 6 seed coupons
- 0 seed usages (populated at runtime)

### Files Modified/Created
- `Models/Coupon.cs` (created)
- `Models/Cart.cs` (modified)
- `Controllers/CouponsController.cs` (created)
- `Controllers/CartController.cs` (modified)
- `Database.cs` (modified)
- `Tests/Coupons.http` (created)
- `COUPON_SUMMARY.md` (this file)

---

## Quick Start Example

```http
### 1. Login
POST http://localhost:5052/api/users/login
Content-Type: application/json

{
  "username": "User",
  "password": "qazwsxedcrfv12345"
}

### 2. Add items to cart
POST http://localhost:5052/api/cart/items
Authorization: Bearer {token}
Content-Type: application/json

{
  "productId": 1,
  "quantity": 2
}

### 3. Apply coupon
POST http://localhost:5052/api/cart/apply-coupon
Authorization: Bearer {token}
Content-Type: application/json

{
  "code": "SAVE10"
}

### 4. View cart with discount
GET http://localhost:5052/api/cart
Authorization: Bearer {token}

### 5. Checkout
POST http://localhost:5052/api/cart/checkout
Authorization: Bearer {token}
```

**Expected Result**: Order created with 10% discount, usage tracked, cart cleared.

---

*Last Updated: 2025-01-16*  
*Feature Status: ✅ Complete - Models, Controllers, Cart Integration, Usage Tracking, Admin Management*
