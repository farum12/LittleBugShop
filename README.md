# 🐛 LittleBugShop

A feature-rich e-commerce API backend designed as a **testing sandbox** for API automation, UI testing, and QA exploration. This project serves as a realistic yet controlled environment to practice testing strategies without the complexity of production systems.

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
- Comprehensive input validation with structured error responses

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

### 🛡️ **Error Handling**
All endpoints return structured error responses:
```json
{
  "code": 401,
  "message": "Authentication required. Please provide a valid JWT token."
}
```

Common error codes:
- **400** - Bad Request (validation errors)
- **401** - Unauthorized (missing/invalid token)
- **403** - Forbidden (insufficient permissions)
- **404** - Not Found (resource doesn't exist)

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
cd Farum.Dev
dotnet run
```

The API will start at `http://localhost:5052`

### Access Swagger UI
Navigate to: `http://localhost:5052/swagger`

### Test Users

The application comes pre-seeded with test data:

**Admin User:**
- Username: `admin`
- Password: `admin123`
- Role: Admin (can manage products, stock, coupons)

**Regular User:**
- Username: `john_doe`
- Password: `password123`
- Role: User (can shop, review, manage cart)

## 📚 API Documentation

### Authentication

#### Register
```http
POST /api/users/register
Content-Type: application/json

{
  "username": "testuser",
  "password": "password123",
  "email": "test@example.com",
  "firstName": "Test",
  "lastName": "User"
}
```

#### Login
```http
POST /api/users/login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin123"
}
```

Response includes JWT token in both response body and HTTP-only cookie.

### Products

#### Get All Products
```http
GET /api/products?searchTerm=fiction&minPrice=10&maxPrice=50&sortBy=price&sortOrder=asc
```

#### Create Product (Admin Only)
```http
POST /api/products
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Test Book",
  "author": "John Doe",
  "genre": "Fiction",
  "isbn": "978-0134685991",
  "price": 29.99,
  "description": "A great book",
  "stockQuantity": 100
}
```

**Product Validation Rules:**
- Name: Required, 2-200 characters
- Author: Required
- Genre: Required
- Description: Required
- Price: Must be > 0 and ≤ 999,999.99
- Stock: Cannot be negative
- ISBN: Optional, must be 10 or 13 digits, must be unique

#### Update Stock (Admin Only)
```http
PUT /api/products/{id}/stock
Authorization: Bearer {token}
Content-Type: application/json

{
  "quantity": 50
}
```

### Shopping Cart

#### Get Cart
```http
GET /api/cart
Authorization: Bearer {token}
```

#### Add to Cart
```http
POST /api/cart/items
Authorization: Bearer {token}
Content-Type: application/json

{
  "productId": 1,
  "quantity": 2
}
```

### Orders & Payments

#### Create Order
```http
POST /api/orders/create
Authorization: Bearer {token}
Content-Type: application/json

{
  "shippingAddressId": 1,
  "billingAddressId": 1,
  "couponCode": "SAVE10"
}
```

#### Process Payment
```http
POST /api/payments/process
Authorization: Bearer {token}
Content-Type: application/json

{
  "orderId": 123,
  "paymentMethodId": 1
}
```

## 🧪 Testing

### HTTP Test Files
Pre-built test files are available in `Farum.Dev/Tests/`:
- `AuthTests.http` - Registration and login
- `ProductManagement.http` - Product CRUD with authorization tests
- `ShoppingCart.http` - Cart operations
- `PlaceOrder.http` - Two-step checkout flow
- `Payments.http` - Payment scenarios with test cards
- `Reviews.http` - Review creation and voting
- `Wishlist.http` - Wishlist management
- `Coupons.http` - Coupon application
- `StockManagement.http` - Admin stock operations

Use VS Code REST Client extension to run these tests.

## 🔐 Security Features

- JWT token authentication with configurable expiration
- Role-based authorization (Admin/User)
- Password validation (minimum 6 characters)
- Protected admin-only endpoints
- Structured error responses (no information leakage)

## 📝 Project Structure

```
LittleBugShop/
├── Farum.Dev/
│   ├── Controllers/        # API endpoints
│   ├── Models/            # Data models and DTOs
│   ├── Services/          # Business logic (JWT, Payment)
│   ├── Tests/             # HTTP test files
│   ├── Database.cs        # In-memory database
│   └── Program.cs         # App configuration
├── README.md
└── .gitignore
```

## 🎓 Learning Resources

This project demonstrates:
- **RESTful API Design** - Proper HTTP methods, status codes, and resource naming
- **Authentication & Authorization** - JWT implementation with role-based access
- **Input Validation** - Comprehensive validation with meaningful error messages
- **Business Logic** - Cart management, stock control, order processing
- **Error Handling** - Structured error responses across all endpoints
- **Testing Patterns** - Pre-built test scenarios for common flows

## 🤝 Contributing

This is a personal testing sandbox, but feel free to fork and customize for your own learning!

## 📄 License

This project is for educational purposes. Use freely for learning and testing.

## 🐛 Found a Bug?

That's the point! This is a testing playground. Document it, reproduce it, and practice your bug reporting skills!

---

**Happy Testing!** 🧪✨
