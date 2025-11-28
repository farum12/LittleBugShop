# User Profile Management - Implementation Summary

## Overview
Complete user profile management system implemented with self-service profile editing and admin user management capabilities.

## Features Implemented

### 1. User Model Extensions
Extended `User` model with profile information:
- Email (nullable)
- FirstName (nullable)
- LastName (nullable)
- PhoneNumber (nullable)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)
- AddressIds (List<int>)

### 2. Address Management
Created `Address` model with:
- AddressType enum (Shipping, Billing, Both)
- Street, City, State, PostalCode, Country
- IsDefault flag for user's preferred address
- UserId for linking to user

### 3. Seed Data
Updated seed data with 3 users and 4 addresses:

**Users:**
- Admin (admin@littlebugshop.com) - 1 address (both shipping/billing)
- User (user@example.com) - 2 addresses (home shipping, work billing)
- User2 (user2@example.com) - 1 address (both shipping/billing)

All users have realistic names, phone numbers, and timestamps.

### 4. User Profile Endpoints (Self-Service)
Created `ProfileController` with endpoints for users to manage their own profiles:

#### GET /api/users/profile
- Returns current user's profile with all addresses
- Requires authentication
- Shows: Id, Username, Email, FirstName, LastName, PhoneNumber, Role, CreatedAt, UpdatedAt, Addresses

#### PUT /api/users/profile
- Update own profile information
- Can update: Email, FirstName, LastName, PhoneNumber
- Automatically updates UpdatedAt timestamp
- Requires authentication

#### POST /api/users/profile/addresses
- Add new address to own profile
- Specify AddressType (Shipping=0, Billing=1, Both=2)
- Can set as default (automatically unsets other defaults)
- Requires authentication

#### PUT /api/users/profile/addresses/{id}
- Update existing address
- Validates ownership (can only update own addresses)
- Can change default status
- Requires authentication

#### DELETE /api/users/profile/addresses/{id}
- Delete address
- Validates ownership (can only delete own addresses)
- Removes from user's AddressIds list
- Requires authentication

#### PUT /api/users/profile/addresses/{id}/set-default
- Set specific address as default
- Automatically unsets other default addresses
- Validates ownership
- Requires authentication

#### PUT /api/users/profile/change-password
- Change own password
- Validates old password
- Requires new password to be at least 6 characters
- Updates UpdatedAt timestamp
- Requires authentication

### 5. Admin User Management Endpoints
Extended `UsersController` with admin-only endpoints:

#### GET /api/users/admin/users
- List all users with profile summaries
- Shows: Id, Username, Email, FirstName, LastName, PhoneNumber, Role, CreatedAt, UpdatedAt, AddressCount
- Requires Admin role

#### GET /api/users/admin/users/{id}
- Get detailed user information
- Includes all addresses and order statistics
- Shows TotalSpent calculation
- Requires Admin role

#### PUT /api/users/admin/users/{id}
- Update any user's profile
- Can update: Email, FirstName, LastName, PhoneNumber, Role
- Requires Admin role

#### POST /api/users/admin/users/{id}/reset-password
- Reset user's password without requiring old password
- Admin privilege for password recovery
- Validates new password (min 6 characters)
- Requires Admin role

### 6. Security Features
- **Authentication**: All endpoints require JWT authentication
- **Authorization**: Admin endpoints check for Admin role
- **Ownership Validation**: Address endpoints validate user owns the address
- **Password Validation**: Minimum 6 characters for password changes
- **Automatic Timestamps**: CreatedAt and UpdatedAt managed automatically

### 7. Updated Registration
Modified user registration to initialize new fields:
- Sets CreatedAt and UpdatedAt to current time
- Initializes empty AddressIds list
- Assigns default "User" role if not specified

## Testing
Created comprehensive test file `Tests/UserProfile.http` with scenarios:

### User Profile Tests
- Get own profile
- Update profile information
- Clear optional fields (phone number)

### Address Management Tests
- Add new addresses
- Update existing addresses
- Set default address
- Delete addresses
- Test ownership validation (can't modify other users' addresses)

### Password Management Tests
- Successful password change
- Wrong old password (should fail)
- Too short password (should fail)
- Change password back

### Admin Operations Tests
- Get all users
- Get specific user details
- Update user profile
- Reset user password
- Password validation

### Authorization Tests
- Regular user accessing admin endpoints (should fail)
- No authentication (should fail)

### Address Type Tests
- Add Shipping address (type 0)
- Add Billing address (type 1)
- Add Both address (type 2)
- Verify multiple addresses for single user

## API Reference

### Address Types
```csharp
public enum AddressType
{
    Shipping = 0,
    Billing = 1,
    Both = 2
}
```

### Request Models

**UpdateProfileRequest:**
```json
{
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1-555-0100"
}
```

**AddAddressRequest:**
```json
{
  "addressType": 0,
  "street": "123 Main St",
  "city": "New York",
  "state": "NY",
  "postalCode": "10001",
  "country": "USA",
  "isDefault": true
}
```

**ChangePasswordRequest:**
```json
{
  "oldPassword": "currentpassword",
  "newPassword": "newpassword123"
}
```

**AdminUpdateUserRequest:**
```json
{
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1-555-0100",
  "role": "User"
}
```

**ResetPasswordRequest:**
```json
{
  "newPassword": "resetpassword123"
}
```

## Usage Example

### User Flow
1. Login to get JWT token
2. Get profile: `GET /api/users/profile`
3. Update profile: `PUT /api/users/profile`
4. Add shipping address: `POST /api/users/profile/addresses`
5. Add billing address: `POST /api/users/profile/addresses`
6. Set default address: `PUT /api/users/profile/addresses/{id}/set-default`
7. Change password: `PUT /api/users/profile/change-password`

### Admin Flow
1. Login as admin to get JWT token
2. List all users: `GET /api/users/admin/users`
3. View user details: `GET /api/users/admin/users/{id}`
4. Update user info: `PUT /api/users/admin/users/{id}`
5. Reset password: `POST /api/users/admin/users/{id}/reset-password`

## Database Collections
Added to `Database.cs`:
- `List<Address> Addresses` - Stores all user addresses

Updated existing collections:
- `List<User> Users` - Extended with profile fields

## Files Modified/Created

### Created:
- `Models/Address.cs` - Address model with AddressType enum
- `Controllers/ProfileController.cs` - User profile management
- `Tests/UserProfile.http` - Comprehensive test scenarios

### Modified:
- `Models/User.cs` - Added Email, FirstName, LastName, PhoneNumber, CreatedAt, UpdatedAt, AddressIds
- `Controllers/UsersController.cs` - Added admin user management endpoints
- `Database.cs` - Added Addresses collection and updated user seed data

## Next Steps for Integration
1. **Order Checkout**: Update order placement to select shipping/billing addresses from user's address list
2. **Address Validation**: Add address validation rules (postal code format, etc.)
3. **Profile Pictures**: Add profile picture upload/storage
4. **Email Verification**: Add email verification workflow
5. **Password Reset via Email**: Implement forgot password flow
6. **User Deactivation**: Add soft delete for user accounts
7. **Audit Log**: Track profile changes for security

## Session Endpoint

### GET /api/session
Returns current authenticated user's session details.

**Response:**
```json
{
  "session": {
    "isAuthenticated": true,
    "tokenExpiration": "2025-11-21T15:30:00Z",
    "tokenExpiresIn": 45.5,
    "jwt": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  },
  "user": {
    "id": 2,
    "username": "User",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "fullName": "John Doe",
    "phoneNumber": "+1-555-0101",
    "role": "User",
    "createdAt": "2025-08-21T10:00:00Z",
    "updatedAt": "2025-08-21T10:00:00Z"
  },
  "stats": {
    "addressCount": 2,
    "orderCount": 5
  }
}
```

**Features:**
- Returns JWT token from Authorization header or cookie
- Calculates token expiration time and minutes remaining
- Shows full user profile including full name
- Includes user statistics (address count, order count)
- Requires authentication

**Use Cases:**
- Check if user is still logged in
- Display user info in UI header
- Verify token validity
- Show session expiration warning
- Display user role for UI customization

## Build Status
✅ Project builds successfully
✅ Application running on http://localhost:5052
✅ All endpoints ready for testing
✅ Session endpoint available at GET /api/session
