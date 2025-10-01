# Lemon Co Production Workflow System

A comprehensive production workflow system that powers Lemon Co's internal manufacturing operations with AutoCount Accounting 2.1/2.2 integration.

## Features

- **BOM Management**: Define and manage multi-level Bills of Materials
- **Assembly Orders**: Create and post assembly orders to consume raw materials and produce finished goods
- **Barcode & Labels**: Generate and print barcodes (Code-128/QR) with batch tracking
- **Sales Order API**: Integration endpoint for external platforms to push sales orders
- **Role-Based Access**: Admin, Production, and Warehouse user roles
- **AutoCount Integration**: Full integration with AutoCount Accounting 2.1/2.2 SDK

## Architecture

### Backend (Windows)
- **.NET 8 Minimal API** wrapping AutoCount SDK assemblies
- **Serilog** for structured logging
- **Polly** for retry policies
- **SQLite** for metadata storage
- **Windows Service** hosting with Kestrel

### Frontend
- **Next.js 14** with TypeScript
- **Tailwind CSS** + **shadcn/ui** components
- **Role-based UI** for different user types

## Project Structure

```
Lemon-co-production/
├── backend/                    # .NET 8 Backend API
│   ├── LemonCo.Api/           # Minimal API project
│   ├── LemonCo.Core/          # Domain models & interfaces
│   ├── LemonCo.AutoCount/     # AutoCount SDK integration
│   ├── LemonCo.Data/          # SQLite metadata DB
│   └── LemonCo.Tests/         # Unit & integration tests
├── frontend/                   # Next.js 14 Frontend
│   ├── src/
│   │   ├── app/               # App router pages
│   │   ├── components/        # React components
│   │   ├── lib/               # Utilities & API client
│   │   └── types/             # TypeScript types
│   └── public/                # Static assets
└── docs/                       # Documentation
```

## Prerequisites

### Backend
- Windows 10/11 or Windows Server 2019+
- .NET 8 SDK
- AutoCount Accounting 2.1 or 2.2 installed
- AutoCount SDK NuGet packages

### Frontend
- Node.js 18+ and npm/pnpm

## Getting Started

### Backend Setup

1. Navigate to backend directory:
```bash
cd backend
```

2. Restore NuGet packages:
```bash
dotnet restore
```

3. Configure AutoCount connection in `appsettings.json`:
```json
{
  "AutoCount": {
    "ServerName": "YOUR_SQL_SERVER",
    "DatabaseName": "YOUR_AUTOCOUNT_DB",
    "UserId": "sa",
    "Password": "YOUR_PASSWORD"
  }
}
```

4. Run the API:
```bash
cd LemonCo.Api
dotnet run
```

API will be available at `https://localhost:5001`

### Frontend Setup

1. Navigate to frontend directory:
```bash
cd frontend
```

2. Install dependencies:
```bash
npm install
```

3. Configure API endpoint in `.env.local`:
```
NEXT_PUBLIC_API_URL=https://localhost:5001
```

4. Run development server:
```bash
npm run dev
```

Frontend will be available at `http://localhost:3000`

## API Endpoints

### Health
- `GET /health` - Health check

### Items & BOM
- `GET /items?search={query}` - Search items
- `POST /items` - Create new item
- `GET /boms/{itemCode}` - Get BOM for item
- `POST /boms/{itemCode}` - Create/update BOM

### Assembly Orders
- `POST /assembly-orders` - Create assembly order
- `POST /assemblies/post` - Post assembly (consume & produce)

### Sales Orders (Integration)
- `POST /sales-orders` - Create sales order from external platform

### Labels
- `POST /labels/print` - Generate barcode labels (ZPL/PDF)

## Core Use Cases

### 1. Define BOM
Admin creates a BOM for "Strawberry 500ml" with raw materials and quantities.

### 2. Create Assembly Order
Production creates an assembly order for batch production.

### 3. Post Assembly
System consumes raw materials and produces finished goods in AutoCount.

### 4. Print Labels
Generate barcode labels with batch, mfg/exp dates for warehouse tracking.

### 5. External Sales Order Integration
External platforms push validated sales orders via REST API.

## User Roles

- **Admin**: Full access to BOM management, configuration
- **Production**: Create and post assembly orders, print labels
- **Warehouse**: Scan operations, label reprinting

## Development

### Run Tests
```bash
cd backend
dotnet test
```

### Build for Production
```bash
# Backend
cd backend/LemonCo.Api
dotnet publish -c Release -o ./publish

# Frontend
cd frontend
npm run build
```

## Deployment

### Backend as Windows Service
See `docs/windows-service-setup.md` for detailed instructions.

### Frontend
Deploy to Vercel, Netlify, or IIS with Node.js hosting.

## License

Proprietary - Lemon Co Internal Use Only

