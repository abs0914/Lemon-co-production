# 🚀 Quick Start Guide - Lemon Co Production System

Get up and running in 10 minutes!

## Prerequisites Check

Before starting, ensure you have:
- [ ] Windows 10/11 or Windows Server
- [ ] .NET 8 SDK installed
- [ ] Node.js 18+ installed
- [ ] AutoCount Accounting 2.1/2.2 installed (optional for initial testing)
- [ ] SQL Server running (optional for initial testing)

## Step 1: Clone/Download Project

```bash
cd C:\Projects
# If using git:
git clone <repository-url> Lemon-co-production
cd Lemon-co-production
```

## Step 2: Backend Setup (5 minutes)

### 2.1 Configure AutoCount Connection

Edit `backend/LemonCo.Api/appsettings.json`:

```json
{
  "AutoCount": {
    "ServerName": "localhost\\SQLEXPRESS",
    "DatabaseName": "AutoCountDB",
    "UserId": "sa",
    "Password": "YourPassword",
    "UseWindowsAuth": false,
    "CompanyCode": "LEMONCO"
  }
}
```

**Note**: For initial testing without AutoCount, the mock data will work fine!

### 2.2 Build and Run Backend

```bash
cd backend
dotnet restore
cd LemonCo.Api
dotnet run
```

You should see:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### 2.3 Verify Backend

Open browser: `https://localhost:5001/swagger`

You should see the Swagger UI with all API endpoints!

Test health check: `https://localhost:5001/health`

Expected response:
```json
{
  "status": "healthy",
  "timestamp": "2025-01-30T...",
  "autoCountConnected": true
}
```

## Step 3: Frontend Setup (3 minutes)

### 3.1 Install Dependencies

Open a **new terminal** (keep backend running):

```bash
cd frontend
npm install
```

### 3.2 Configure Environment

Create `.env.local`:

```bash
# Windows PowerShell
Copy-Item .env.local.example .env.local

# Or manually create with content:
NEXT_PUBLIC_API_URL=https://localhost:5001
```

### 3.3 Run Frontend

```bash
npm run dev
```

You should see:
```
  ▲ Next.js 14.2.0
  - Local:        http://localhost:3000
  - Ready in 2.3s
```

## Step 4: Login and Explore (2 minutes)

### 4.1 Open Application

Navigate to: `http://localhost:3000`

### 4.2 Login

Use demo credentials:
- **Username**: `admin`
- **Password**: `admin123`

### 4.3 Explore Dashboard

You should see:
- Dashboard with metrics
- Open assembly orders
- Quick action buttons

## Step 5: Test Core Features

### Test 1: View Items

1. Open Swagger UI: `https://localhost:5001/swagger`
2. Try `GET /items` endpoint
3. Click "Try it out" → "Execute"
4. You should see mock items (STR-500ML, RAW-STR, etc.)

### Test 2: View BOM

1. In Swagger, try `GET /boms/STR-500ML`
2. You should see the BOM with components:
   - RAW-STR (0.3 KG)
   - PKG-BTL-500 (1 PCS)
   - RAW-SUGAR (0.05 KG)

### Test 3: Create Assembly Order

1. In Swagger, try `POST /assembly-orders`
2. Use this request body:
```json
{
  "itemCode": "STR-500ML",
  "quantity": 100,
  "productionDate": "2025-01-30",
  "remarks": "Test batch"
}
```
3. You should get a response with `docNo` like "ASM-20250130-1001"

### Test 4: View in UI

1. Go to frontend: `http://localhost:3000/dashboard/production`
2. You should see the assembly order you just created!

### Test 5: Post Assembly

1. In the Production page, click "Post Assembly" button
2. Confirm the action
3. You should see cost breakdown in the alert

### Test 6: Generate Label

1. In Swagger, try `POST /labels/print`
2. Use this request body:
```json
{
  "itemCode": "STR-500ML",
  "batchNo": "B001",
  "mfgDate": "2025-01-30",
  "expDate": "2025-07-30",
  "copies": 1,
  "format": "ZPL"
}
```
3. You should get ZPL code in the response

### Test 7: Sales Order Integration

1. In Swagger, try `POST /sales-orders`
2. Use this request body:
```json
{
  "customerCode": "CUST001",
  "lines": [
    {
      "itemCode": "STR-500ML",
      "qty": 50,
      "unitPrice": 15.00
    }
  ],
  "remarks": "Test order",
  "externalRef": "TEST-001"
}
```
3. You should get a response with `soNo` and `totalAmount`

## 🎉 Success!

You now have a fully functional production workflow system running!

## Next Steps

### For Development
1. Explore the code in `backend/` and `frontend/`
2. Modify mock data in service implementations
3. Add new features or customize existing ones
4. Review documentation in `docs/` folder

### For Production Use
1. **Install AutoCount SDK**
   - Add DLL references to `LemonCo.AutoCount` project
   - Update service implementations to use real AutoCount SDK

2. **Configure Real Database**
   - Update `appsettings.json` with production SQL Server
   - Test connection with AutoCount

3. **Set Up Barcode Printer**
   - Configure Zebra printer
   - Test ZPL label printing

4. **Deploy to Production**
   - Follow `docs/DEPLOYMENT.md`
   - Set up as Windows Service
   - Configure HTTPS certificate

## Common Issues & Solutions

### Issue: Backend won't start

**Error**: "Port 5001 already in use"

**Solution**: 
```bash
# Change port in launchSettings.json or kill process
netstat -ano | findstr :5001
taskkill /PID <process_id> /F
```

### Issue: Frontend can't connect to API

**Error**: "Network Error" or "CORS error"

**Solution**:
1. Verify backend is running: `https://localhost:5001/health`
2. Check `.env.local` has correct API URL
3. Restart frontend: `npm run dev`

### Issue: AutoCount connection failed

**Error**: "AutoCount connection test failed"

**Solution**:
1. This is expected if AutoCount is not installed
2. Mock data will still work for testing
3. To fix: Install AutoCount and configure connection string

### Issue: Database errors

**Error**: "SQLite Error 14: unable to open database file"

**Solution**:
```bash
# Ensure directory exists
mkdir backend/LemonCo.Api/bin/Debug/net8.0
# Database will be created automatically on first run
```

### Issue: npm install fails

**Error**: "EACCES: permission denied"

**Solution**:
```bash
# Run as administrator or fix npm permissions
npm cache clean --force
npm install
```

## Testing Checklist

- [ ] Backend starts without errors
- [ ] Swagger UI loads at /swagger
- [ ] Health check returns "healthy"
- [ ] Frontend loads at localhost:3000
- [ ] Can login with demo credentials
- [ ] Dashboard shows metrics
- [ ] Can view items via API
- [ ] Can create assembly order
- [ ] Can view order in UI
- [ ] Can post assembly
- [ ] Can generate label
- [ ] Can create sales order

## Getting Help

### Documentation
- **Setup Guide**: `docs/SETUP.md`
- **API Reference**: `docs/API.md`
- **Deployment**: `docs/DEPLOYMENT.md`
- **Architecture**: `docs/ARCHITECTURE.md`

### Logs
- Backend logs: `backend/LemonCo.Api/logs/`
- Browser console: F12 in browser
- Windows Event Viewer (for service issues)

### Support
- Check Swagger UI for API testing
- Review error messages in logs
- Verify configuration files
- Test with Postman/curl

## What's Next?

Now that you have the system running, you can:

1. **Customize for Your Needs**
   - Modify item types and categories
   - Adjust BOM structure
   - Customize label templates
   - Add new fields to forms

2. **Integrate with AutoCount**
   - Install AutoCount SDK
   - Configure production database
   - Test with real data
   - Verify stock movements

3. **Extend Functionality**
   - Add more UI pages
   - Implement additional reports
   - Create mobile app
   - Add notifications

4. **Deploy to Production**
   - Set up Windows Service
   - Configure SSL certificate
   - Set up monitoring
   - Train users

## 🎊 Congratulations!

You've successfully set up the Lemon Co Production Workflow System!

The system is now ready for:
- ✅ Development and testing
- ✅ Customization for your needs
- ✅ Integration with AutoCount
- ✅ Production deployment

Happy coding! 🍋

