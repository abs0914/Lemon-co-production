# Authentication & Authorization Guide

## Overview

The Lemon Co Production API now supports **dual authentication** methods:
1. **Local JWT Authentication** - Username/password authentication with locally generated JWT tokens
2. **Supabase JWT Authentication** - Token validation from Supabase (for Lovable.dev frontend)

## Configuration

### 1. Update `appsettings.json`

Add the following configuration sections:

```json
{
  "Supabase": {
    "Url": "https://your-project.supabase.co",
    "AnonKey": "your-anon-key-here",
    "JwtSecret": "your-jwt-secret-here",
    "JwtIssuer": "https://your-project.supabase.co/auth/v1"
  },
  "Jwt": {
    "Secret": "your-local-jwt-secret-min-32-chars-long",
    "Issuer": "LemonCoProductionAPI",
    "Audience": "LemonCoFrontend",
    "ExpiryMinutes": 480
  }
}
```

### 2. Get Supabase Configuration

From your Supabase project dashboard:
- **Url**: Project Settings → API → Project URL
- **AnonKey**: Project Settings → API → anon public key
- **JwtSecret**: Project Settings → API → JWT Secret
- **JwtIssuer**: Usually `https://your-project.supabase.co/auth/v1`

### 3. Generate Local JWT Secret

Generate a secure random string (minimum 32 characters):

```bash
# PowerShell
-join ((65..90) + (97..122) + (48..57) | Get-Random -Count 32 | % {[char]$_})

# Or use an online generator
```

## API Endpoints

### POST /auth/login

Authenticate with username and password.

**Request:**
```json
{
  "username": "admin",
  "password": "admin123"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "username": "admin",
    "fullName": "System Administrator",
    "role": "Admin",
    "email": "admin@lemonco.com"
  }
}
```

**Response (401 Unauthorized):**
```json
{
  "success": false,
  "errorMessage": "Invalid username or password"
}
```

### POST /auth/validate-token

Validate a Supabase JWT token.

**Request:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response (200 OK):**
```json
{
  "isValid": true,
  "userId": "uuid-here",
  "email": "user@example.com",
  "role": "Admin"
}
```

### GET /auth/me

Get current authenticated user information.

**Headers:**
```
Authorization: Bearer <your-jwt-token>
```

**Response (200 OK):**
```json
{
  "id": 1,
  "username": "admin",
  "fullName": "System Administrator",
  "role": "Admin",
  "email": "admin@lemonco.com"
}
```

## Using Authentication

### From Frontend (Lovable.dev with Supabase)

```typescript
// 1. User logs in via Supabase
const { data, error } = await supabase.auth.signInWithPassword({
  email: 'user@example.com',
  password: 'password123'
})

// 2. Get Supabase session token
const { data: { session } } = await supabase.auth.getSession()
const token = session?.access_token

// 3. Use token in API calls
const response = await fetch('https://api.lemonco.com/items', {
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
})
```

### From Frontend (Local Authentication)

```typescript
// 1. Login with username/password
const response = await fetch('https://api.lemonco.com/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    username: 'admin',
    password: 'admin123'
  })
})

const { token, user } = await response.json()

// 2. Store token
localStorage.setItem('auth_token', token)
localStorage.setItem('user', JSON.stringify(user))

// 3. Use token in subsequent requests
const items = await fetch('https://api.lemonco.com/items', {
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
})
```

### From Postman/Insomnia

1. **Login:**
   - Method: POST
   - URL: `https://localhost:5001/auth/login`
   - Body (JSON):
     ```json
     {
       "username": "admin",
       "password": "admin123"
     }
     ```
   - Copy the `token` from response

2. **Use Token:**
   - Add header to all requests:
     - Key: `Authorization`
     - Value: `Bearer <paste-token-here>`

## Role-Based Access Control

### Roles

- **Admin**: Full access to all endpoints
- **Production**: Items, BOMs, Assembly Orders, Labels
- **Warehouse**: Items (read-only), Labels, Barcode scanning

### Protected Endpoints

All endpoints except `/health` and `/auth/*` require authentication.

**Example:**
```csharp
// Require any authenticated user
.RequireAuthorization()

// Require specific role
.RequireAuthorization(policy => policy.RequireRole("Admin"))

// Require one of multiple roles
.RequireAuthorization(policy => policy.RequireRole("Admin", "Production"))
```

## Demo Users

The system seeds three demo users on first run:

| Username   | Password  | Role       | Email                    |
|------------|-----------|------------|--------------------------|
| admin      | admin123  | Admin      | admin@lemonco.com        |
| production | prod123   | Production | production@lemonco.com   |
| warehouse  | wh123     | Warehouse  | warehouse@lemonco.com    |

**⚠️ IMPORTANT:** Change these passwords in production!

## Security Best Practices

### 1. Use HTTPS in Production

Always use HTTPS to protect tokens in transit.

### 2. Secure JWT Secrets

- Use strong, random secrets (minimum 32 characters)
- Store secrets in environment variables or Azure Key Vault
- Never commit secrets to source control

### 3. Token Expiry

- Default: 480 minutes (8 hours)
- Adjust based on security requirements
- Implement token refresh for better UX

### 4. CORS Configuration

Update CORS in `Program.cs` with your actual domains:

```csharp
policy.WithOrigins(
    "https://your-app.lovable.app",
    "https://app.lemonco.com"
)
```

### 5. Password Hashing

Passwords are hashed using BCrypt with automatic salt generation.

## Troubleshooting

### 401 Unauthorized

**Cause:** Invalid or expired token

**Solution:**
- Check token is included in `Authorization` header
- Verify token format: `Bearer <token>`
- Login again to get a new token

### Token Validation Failed

**Cause:** JWT secret mismatch

**Solution:**
- Verify `Jwt:Secret` in appsettings.json
- Ensure secret is at least 32 characters
- Check Supabase JWT secret is correct

### CORS Error

**Cause:** Frontend domain not in CORS policy

**Solution:**
- Add your domain to `WithOrigins()` in Program.cs
- Restart the API

## Testing Authentication

### Test Login Endpoint

```bash
curl -X POST https://localhost:5001/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

### Test Protected Endpoint

```bash
curl -X GET https://localhost:5001/items \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### Test Swagger UI

1. Navigate to `https://localhost:5001/swagger`
2. Click "Authorize" button (top right)
3. Enter: `Bearer <your-token>`
4. Click "Authorize"
5. Try any endpoint

## Migration from Old System

If you have existing users without BCrypt hashes:

1. Users will need to reset passwords
2. Or run a migration script to hash existing passwords
3. Update `User.PasswordHash` with BCrypt hashed values

## Next Steps

1. ✅ Configure Supabase credentials
2. ✅ Update CORS with production domains
3. ✅ Change demo user passwords
4. ✅ Test authentication flow
5. ✅ Deploy to production
6. ✅ Update frontend to use new auth endpoints

