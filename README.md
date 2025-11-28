# 🐛 LittleBugShop

A feature-rich e-commerce API backend designed as a **testing sandbox** for API automation, UI testing, and QA exploration. This project was 99% vibe-coded for educational purposes, providing a realistic yet controlled environment to practice testing strategies without the complexity of production systems.

## 🎯 Purpose

LittleBugShop serves as a **private playground** for:
- **API Testing Practice** - Test REST endpoints, authentication flows, and complex business logic
- **Test Automation Development** - Build and validate automation frameworks
- **QA Skill Building** - Explore edge cases, failure scenarios, and validation strategies
- **UI Testing Integration** - Backend ready for frontend test automation
- **Learning & Experimentation** - Safe environment to break things and learn

The name "LittleBugShop" is a playful nod to its purpose: a shop full of intentional complexity where you can discover and practice finding bugs! 🐛

## ✨ Features

### 🛒 **E-Commerce Core**
- Product browsing with advanced filtering (search, genre, author, price range)
- Shopping cart management (add, update, remove items)
- Real-time stock availability checking
- Two-step checkout flow (order creation + payment processing)
- Order history and status tracking

### 💳 **Payment System**
- Multiple payment methods per user (Credit/Debit cards, PayPal)
- **Test card patterns** for deterministic outcomes:
  - `xxxx0000` → Success
  - `xxxx1111` → Insufficient Funds
  - `xxxx2222` → Timeout
  - `xxxx3333` → Fraud Detection
  - `xxxx4444` → Card Expired
  - `xxxx5555` → Invalid CVV
  - `xxxx6666` → Declined
- Payment retry logic (cart preserved on failure)
- Full/partial refund support
- Transaction audit trail

### 👤 **User Management**
- JWT authentication (cookie + Authorization header)
- Role-based authorization (Admin/User)
- User profiles with personal information
- Multiple shipping/billing addresses
- Default address management

### ⭐ **Reviews & Social**
- Product reviews with 1-5 star ratings
- Verified purchase badges
- Review helpfulness voting (helpful/not helpful)
- Admin moderation (hide/unhide reviews)
- Review statistics (average rating, count)

### 💝 **Wishlist**
- Add products to wishlist
- Move wishlist items directly to cart
- Track when items were added

### 🎟️ **Coupon System**
- Percentage and fixed-amount discounts
- Minimum purchase requirements
- Usage limits (max uses per coupon)
- Expiration dates
- Admin usage reports

### 📦 **Stock Management**
- Automatic stock reservation on order creation
- Stock restoration on cancellation/refund
- Low stock warnings
- Admin stock control (set, increase, decrease)
- Order expiration (15 minutes for pending orders)

## 🏗️ Technology Stack

- **.NET 8.0** / ASP.NET Core Web API
- **In-Memory Database** - No external dependencies, instant startup
- **JWT Bearer Authentication** - Industry-standard auth
- **Swagger/OpenAPI** - Interactive API documentation
- **C# 12** - Modern language features

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK or later

### Run the Application

```powershell
cd WebApplication1
dotnet run
```

The API will start at `http://localhost:5052`

### Access Swagger UI
Navigate to: `http://localhost:5052/swagger`

### Test Users

The application comes pre-seeded with test data:

| Username | Password | Role | Email |
|----------|----------|------|-------|
| admin | admin123 | Admin | admin@littlebugshop.com |
| user | user123 | User | user@littlebugshop.com |
| user2 | user123 | User | user2@littlebugshop.com |

## 📚 API Overview

### Authentication (3 endpoints)
- `POST /api/users/register` - Create new account
- `POST /api/users/login` - Get JWT token
- `POST /api/users/logout` - Clear auth cookie

### Products (6 endpoints)
- `GET /api/products` - Browse with filters (search, genre, author, price)
- `GET /api/products/{id}` - Get product details
- `GET /api/products/{id}/availability` - Check stock status
- `PUT /api/products/{id}/stock` - **[Admin]** Set exact stock
- `POST /api/products/{id}/stock/increase` - **[Admin]** Add stock
- `POST /api/products/{id}/stock/decrease` - **[Admin]** Remove stock

### Shopping Cart (5 endpoints)
- `GET /api/cart` - View current cart
- `POST /api/cart/items` - Add item to cart
- `PUT /api/cart/items/{id}` - Update quantity
- `DELETE /api/cart/items/{id}` - Remove item
- `DELETE /api/cart` - Clear cart

### Orders (9 endpoints)
- `POST /api/orders/create` - **Step 1:** Create pending order (reserves stock)
- `GET /api/orders/pending` - View pending orders with expiration
- `DELETE /api/orders/{id}/cancel` - Cancel pending order
- `POST /api/cart/checkout` - Legacy one-step checkout
- `POST /api/orders/place` - Alternative order creation
- `GET /api/orders/my-orders` - User's order history
- `GET /api/orders/{id}` - Order details
- `GET /api/orders` - **[Admin]** All orders
- `PUT /api/orders/{id}/status` - **[Admin]** Update status

### Payment Methods (5 endpoints)
- `GET /api/payment-methods` - List user's payment methods
- `GET /api/payment-methods/{id}` - Get specific method
- `POST /api/payment-methods` - Add card/PayPal
- `PUT /api/payment-methods/{id}/set-default` - Set default method
- `DELETE /api/payment-methods/{id}` - Remove method

### Payments (6 endpoints)
- `POST /api/payments/process` - **Step 2:** Process payment for pending order
- `GET /api/payments/transactions` - User's transaction history
- `GET /api/payments/transactions/{id}` - Transaction details
- `POST /api/payments/refund` - **[Admin]** Issue refund
- `GET /api/payments/admin/transactions` - **[Admin]** All transactions
- `GET /api/payments/admin/statistics` - **[Admin]** Revenue stats

### User Profile (8 endpoints)
- `GET /api/profile` - Get profile
- `PUT /api/profile` - Update profile
- `POST /api/profile/addresses` - Add address
- `PUT /api/profile/addresses/{id}` - Update address
- `DELETE /api/profile/addresses/{id}` - Remove address
- `PUT /api/profile/addresses/{id}/set-default-shipping` - Set default shipping
- `PUT /api/profile/addresses/{id}/set-default-billing` - Set default billing

### Reviews (13 endpoints)
- `GET /api/products/{productId}/reviews` - List reviews
- `GET /api/products/{productId}/reviews/summary` - Statistics
- `GET /api/products/{productId}/reviews/{id}` - Single review
- `POST /api/products/{productId}/reviews` - Write review
- `PUT /api/products/{productId}/reviews/{id}` - Update own review
- `DELETE /api/products/{productId}/reviews/{id}` - Delete own review
- `POST /api/products/{productId}/reviews/{id}/helpful` - Vote helpful/not helpful
- `PUT /api/products/{productId}/reviews/{id}/hide` - **[Admin]** Hide review
- `PUT /api/products/{productId}/reviews/{id}/unhide` - **[Admin]** Unhide review
- `GET /api/products/{productId}/reviews/user/{userId}` - User's reviews for product
- `GET /api/products/{productId}/reviews/helpful` - Most helpful reviews
- `GET /api/products/{productId}/reviews/rating/{rating}` - Filter by rating
- `GET /api/products/{productId}/reviews/verified` - Verified purchase reviews only

### Wishlist (4 endpoints)
- `GET /api/wishlist` - View wishlist
- `POST /api/wishlist` - Add to wishlist
- `DELETE /api/wishlist/{id}` - Remove from wishlist
- `POST /api/wishlist/{id}/move-to-cart` - Move to cart

### Coupons (6 endpoints)
- `POST /api/coupons/validate` - Validate coupon code
- `GET /api/coupons` - **[Admin]** List all coupons
- `POST /api/coupons` - **[Admin]** Create coupon
- `PUT /api/coupons/{id}` - **[Admin]** Update coupon
- `DELETE /api/coupons/{id}` - **[Admin]** Delete coupon
- `GET /api/coupons/usage-report` - **[Admin]** Usage statistics

### Session (1 endpoint)
- `GET /api/session/user` - Get current user info

**Total: 80+ endpoints** across 11 feature areas

## 🧪 Testing Features

### Test Card Patterns
Use these card numbers (last 4 digits determine outcome):

```
4532015112340000 → ✅ Success
4532015112341111 → ❌ Insufficient Funds
4532015112342222 → ❌ Timeout
4532015112343333 → ❌ Fraud Detection
4532015112344444 → ❌ Card Expired
4532015112345555 → ❌ Invalid CVV
4532015112346666 → ❌ Declined
```

### HTTP Test Files
Pre-built test scenarios in `Tests/` directory:
- `Payments.http` - 60+ payment scenarios
- `Reviews.http` - 40+ review scenarios
- `Wishlist.http` - Wishlist operations
- `Coupons.http` - Coupon validation
- `UserProfile.http` - Profile management
- `ShoppingCart.http` - Cart operations
- `OrderStatus.http` - Order lifecycle
- `StockManagement.http` - Admin stock control

### Seed Data
- **15 products** across 5 genres (Fiction, Non-Fiction, Fantasy, Science, Mystery)
- **3 users** with different roles and addresses
- **9 payment methods** with various test cards
- **5 active coupons** with different configurations
- **Multiple addresses** per user for shipping/billing scenarios

## 📖 Documentation

- **[APPLICATION_FLOWS.md](APPLICATION_FLOWS.md)** - Complete flow diagrams and data models
- **[PAYMENT_SUMMARY.md](PAYMENT_SUMMARY.md)** - Payment system documentation
- **[USER_PROFILE_SUMMARY.md](USER_PROFILE_SUMMARY.md)** - Profile & address features
- **[WISHLIST_SUMMARY.md](WISHLIST_SUMMARY.md)** - Wishlist functionality
- **[COUPON_SUMMARY.md](COUPON_SUMMARY.md)** - Coupon system guide

## 🎓 Learning Opportunities

This project is perfect for practicing:

### API Testing
- ✅ Request/response validation
- ✅ Authentication & authorization flows
- ✅ Error handling and edge cases
- ✅ Status code verification
- ✅ JSON schema validation
- ✅ State management across requests

### Test Scenarios
- ✅ Happy path workflows (browse → cart → checkout → payment)
- ✅ Failure scenarios (payment failures, stock issues, invalid data)
- ✅ Boundary testing (stock limits, coupon expiration, price ranges)
- ✅ Race conditions (concurrent stock updates)
- ✅ State transitions (order status, payment status)
- ✅ Data validation (email formats, required fields, data types)

### Automation Practice
- ✅ REST API automation frameworks
- ✅ Data-driven testing (multiple test cards, users, products)
- ✅ Test data setup and teardown
- ✅ Assertion libraries
- ✅ Reporting and logging
- ✅ CI/CD integration

## 🎨 Design Philosophy

**Vibe-Coded for Learning** - This isn't production-ready code. It's intentionally:
- Simple enough to understand quickly
- Complex enough to be interesting
- Realistic enough to practice real scenarios
- Forgiving enough to experiment safely

**No Database Setup** - Everything runs in-memory, so you can:
- Start testing immediately
- Reset state by restarting the app
- No migration headaches
- No cleanup scripts needed

**Deterministic Behavior** - Test card patterns and seed data ensure:
- Repeatable test results
- Predictable failure scenarios
- Easy debugging
- Reliable automation

## 🔧 Configuration

The application runs on `http://localhost:5052` by default. To change:

Edit `Properties/launchSettings.json`:
```json
"applicationUrl": "http://localhost:YOUR_PORT"
```

## 🤝 Contributing

This is a personal sandbox project for testing practice. Feel free to:
- Fork it and experiment
- Use it for learning
- Modify it for your needs
- Break it intentionally to practice debugging

## 📝 License

Educational use only. Do what you want with it! 🎓

## 🐛 Found a Bug?

That's the point! Practice writing a bug report:
- What endpoint did you call?
- What was the request body?
- What did you expect?
- What actually happened?
- Can you reproduce it?

---

**Happy Testing! May your assertions be true and your bugs be easily reproducible.** ✨🐛
