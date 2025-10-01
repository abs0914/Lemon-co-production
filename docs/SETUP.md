# Lemon Co Production System - Setup Guide

## Prerequisites

### Backend Requirements
- **Windows 10/11** or **Windows Server 2019+**
- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **AutoCount Accounting 2.1 or 2.2** installed
- **SQL Server** (for AutoCount database)
- **Visual Studio 2022** or **VS Code** (recommended)

### Frontend Requirements
- **Node.js 18+** - [Download](https://nodejs.org/)
- **npm** or **pnpm** package manager

## Backend Setup

### Step 1: Install AutoCount SDK

The AutoCount SDK assemblies need to be referenced in the project. You have two options:

#### Option A: NuGet Packages (if available)
```bash
cd backend/LemonCo.AutoCount
dotnet add package AutoCount.Accounting --version 2.2.0
dotnet add package AutoCount.Stock --version 2.2.0
dotnet add package AutoCount.Sales --version 2.2.0
```

#### Option B: Local DLL References
If AutoCount SDK is not available via NuGet, add local references:

1. Locate AutoCount installation directory (usually `C:\Program Files\AutoCount\AutoCount Accounting 2.2\`)
2. Copy these DLLs to `backend/lib/`:
   - `AutoCount.dll`
   - `AutoCount.Data.dll`
   - `AutoCount.Stock.dll`
   - `AutoCount.Sales.dll`
   - `AutoCount.BOM.dll`

3. Update `LemonCo.AutoCount.csproj`:
```xml
<ItemGroup>
  <Reference Include="AutoCount">
    <HintPath>..\..\lib\AutoCount.dll</HintPath>
  </Reference>
  <!-- Add other DLLs similarly -->
</ItemGroup>
```

### Step 2: Configure Database Connection

Edit `backend/LemonCo.Api/appsettings.json`:

```json
{
  "AutoCount": {
    "ServerName": "YOUR_SQL_SERVER",
    "DatabaseName": "YOUR_AUTOCOUNT_DB",
    "UserId": "sa",
    "Password": "YOUR_PASSWORD",
    "UseWindowsAuth": false,
    "CompanyCode": "LEMONCO",
    "AutoCountUser": "admin",
    "AutoCountPassword": "admin"
  }
}
```

**For Windows Authentication:**
```json
{
  "AutoCount": {
    "ServerName": "localhost\\SQLEXPRESS",
    "DatabaseName": "AutoCountDB",
    "UseWindowsAuth": true,
    "CompanyCode": "LEMONCO"
  }
}
```

### Step 3: Build and Run Backend

```bash
cd backend
dotnet restore
dotnet build

# Run the API
cd LemonCo.Api
dotnet run
```

The API will start at `https://localhost:5001`

### Step 4: Verify Backend

Open browser and navigate to:
- Swagger UI: `https://localhost:5001/swagger`
- Health Check: `https://localhost:5001/health`

Expected health check response:
```json
{
  "status": "healthy",
  "timestamp": "2025-01-30T...",
  "autoCountConnected": true
}
```

## Frontend Setup

### Step 1: Install Dependencies

```bash
cd frontend
npm install
```

### Step 2: Configure Environment

Create `.env.local` file:
```bash
cp .env.local.example .env.local
```

Edit `.env.local`:
```
NEXT_PUBLIC_API_URL=https://localhost:5001
```

### Step 3: Run Development Server

```bash
npm run dev
```

Frontend will be available at `http://localhost:3000`

### Step 4: Login

Use these demo credentials:
- **Admin**: `admin` / `admin123`
- **Production**: `production` / `prod123`
- **Warehouse**: `warehouse` / `wh123`

## Database Initialization

The SQLite metadata database will be created automatically on first run at:
```
backend/LemonCo.Api/lemonco.db
```

Default data includes:
- Admin user (username: `admin`, password: `admin123`)
- Default barcode configuration
- Default ZPL label template

## Testing the System

### 1. Test Health Check
```bash
curl https://localhost:5001/health
```

### 2. Test Items API
```bash
curl https://localhost:5001/items
```

### 3. Create a BOM (Example: Strawberry 500ml)

```bash
curl -X POST https://localhost:5001/boms/STR-500ML \
  -H "Content-Type: application/json" \
  -d '[
    {
      "componentCode": "RAW-STR",
      "qtyPer": 0.3,
      "uom": "KG",
      "description": "Raw Strawberry Extract",
      "sequence": 1
    },
    {
      "componentCode": "PKG-BTL-500",
      "qtyPer": 1,
      "uom": "PCS",
      "description": "500ml Bottle",
      "sequence": 2
    }
  ]'
```

### 4. Create Assembly Order

```bash
curl -X POST https://localhost:5001/assembly-orders \
  -H "Content-Type: application/json" \
  -d '{
    "itemCode": "STR-500ML",
    "quantity": 100,
    "productionDate": "2025-01-30",
    "remarks": "Batch 001"
  }'
```

### 5. Post Assembly

```bash
curl -X POST https://localhost:5001/assemblies/post \
  -H "Content-Type: application/json" \
  -d '{
    "orderDocNo": "ASM-20250130-1001"
  }'
```

## Troubleshooting

### Backend Issues

**Issue: AutoCount connection failed**
- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Ensure AutoCount database exists
- Test SQL connection with SQL Server Management Studio

**Issue: DLL not found errors**
- Ensure AutoCount SDK DLLs are referenced correctly
- Check DLL versions match your AutoCount installation
- Copy DLLs to output directory if needed

**Issue: Port already in use**
- Change port in `launchSettings.json`:
```json
{
  "applicationUrl": "https://localhost:5002;http://localhost:5003"
}
```

### Frontend Issues

**Issue: API connection refused**
- Ensure backend is running
- Check `NEXT_PUBLIC_API_URL` in `.env.local`
- Verify CORS settings in backend

**Issue: Module not found**
```bash
rm -rf node_modules package-lock.json
npm install
```

**Issue: Build errors**
```bash
npm run lint
npm run build
```

## Next Steps

1. **Configure AutoCount**: Set up your AutoCount company database
2. **Import Master Data**: Import items, customers, and BOMs
3. **Configure Barcode Printer**: Set up Zebra printer for ZPL labels
4. **Test Production Flow**: Create and post assembly orders
5. **External Integration**: Test Sales Order API with external systems

## Production Deployment

See `docs/DEPLOYMENT.md` for production deployment instructions.

## Support

For issues and questions:
- Check logs in `backend/LemonCo.Api/logs/`
- Review API documentation at `/swagger`
- Contact system administrator

