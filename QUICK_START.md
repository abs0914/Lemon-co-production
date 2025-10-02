# 🚀 Quick Start Guide - Lemon Co Production API

## ⚡ Get Started in 5 Minutes

### Step 1: Configure Supabase (2 minutes)

1. Go to your Supabase project dashboard
2. Navigate to **Settings → API**
3. Copy these values to `backend/LemonCo.Api/appsettings.json`:

```json
{
  "Supabase": {
    "Url": "PASTE_PROJECT_URL_HERE",
    "AnonKey": "PASTE_ANON_KEY_HERE",
    "JwtSecret": "PASTE_JWT_SECRET_HERE",
    "JwtIssuer": "PASTE_PROJECT_URL_HERE/auth/v1"
  }
}
```

### Step 2: Generate JWT Secret (1 minute)

Run in PowerShell:
```powershell
-join ((65..90) + (97..122) + (48..57) | Get-Random -Count 32 | % {[char]$_})
```

Paste result in `appsettings.json`:
```json
{
  "Jwt": {
    "Secret": "PASTE_GENERATED_SECRET_HERE"
  }
}
```

### Step 3: Update CORS (1 minute)

In `backend/LemonCo.Api/Program.cs`, line ~65, replace:
```csharp
"https://your-app.lovable.app"
```

With your actual Lovable.dev domain.

### Step 4: Run the API (1 minute)

```bash
cd backend
dotnet run --project LemonCo.Api
```

Wait for:
```
Now listening on: https://localhost:5001
```

### Step 5: Test It! (30 seconds)

Open browser: `https://localhost:5001/swagger`

Click **Authorize**, login with:
- Username: `admin`
- Password: `admin123`

Try any endpoint!

---

## 🎯 Quick Test Commands

### Test Login
```bash
curl -X POST https://localhost:5001/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

### Test Protected Endpoint
```bash
# Replace YOUR_TOKEN with token from login response
curl -X GET https://localhost:5001/items \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## 👥 Demo Users

| Username   | Password  | Role       |
|------------|-----------|------------|
| admin      | admin123  | Admin      |
| production | prod123   | Production |
| warehouse  | wh123     | Warehouse  |

---

## 🔗 Integration with Lovable.dev

### Option 1: Local JWT Authentication

```typescript
// In your Lovable.dev app
const login = async (username: string, password: string) => {
  const response = await fetch('https://localhost:5001/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password })
  })
  
  const { token, user } = await response.json()
  localStorage.setItem('auth_token', token)
  return { token, user }
}

// Use in API calls
const fetchItems = async () => {
  const token = localStorage.getItem('auth_token')
  const response = await fetch('https://localhost:5001/items', {
    headers: { 'Authorization': `Bearer ${token}` }
  })
  return response.json()
}
```

### Option 2: Supabase Authentication

```typescript
// In your Lovable.dev app with Supabase
import { createClient } from '@supabase/supabase-js'

const supabase = createClient(SUPABASE_URL, SUPABASE_ANON_KEY)

// Login via Supabase
const { data, error } = await supabase.auth.signInWithPassword({
  email: 'user@example.com',
  password: 'password123'
})

// Get token
const { data: { session } } = await supabase.auth.getSession()
const token = session?.access_token

// Use in API calls
const fetchItems = async () => {
  const response = await fetch('https://localhost:5001/items', {
    headers: { 'Authorization': `Bearer ${token}` }
  })
  return response.json()
}
```

---

## 📋 Available Endpoints

### Authentication (Public)
- `POST /auth/login` - Login
- `POST /auth/validate-token` - Validate token
- `GET /auth/me` - Current user

### Items & BOM (Protected)
- `GET /items` - Search items
- `GET /items/{code}` - Get item
- `POST /items` - Create item
- `GET /boms/{code}` - Get BOM
- `POST /boms/{code}` - Save BOM

### Assembly Orders (Protected)
- `POST /assembly-orders` - Create order
- `GET /assembly/orders/open` - List open orders
- `POST /assemblies/post` - Post assembly

### Labels (Protected)
- `POST /labels/print` - Print label
- `GET /labels/barcode-config` - Get config
- `POST /labels/parse-barcode` - Parse barcode

### Sales Orders (Protected)
- `POST /sales-orders` - Create order
- `GET /sales-orders/validate-customer/{code}` - Validate customer

### Health (Public)
- `GET /health` - Health check

---

## 🛠️ Troubleshooting

### "401 Unauthorized"
- Check token is in `Authorization: Bearer <token>` header
- Login again to get fresh token

### "CORS Error"
- Update CORS in `Program.cs` with your domain
- Restart API

### "Connection Failed"
- Check AutoCount SQL Server is running
- Verify connection string in `appsettings.json`

### "Database Error"
- Delete `lemonco.db` and restart (will recreate with demo users)

---

## 📚 Full Documentation

- **Complete Guide**: `BACKEND_UPDATE_COMPLETE.md`
- **Authentication**: `docs/AUTHENTICATION.md`
- **Setup**: `docs/SETUP.md`
- **Architecture**: `docs/ARCHITECTURE.md`

---

## 🎉 You're Ready!

Your backend is configured and ready to integrate with your Lovable.dev frontend. Start building! 🚀

