# Lemon Co Production System - Architecture

## System Overview

The Lemon Co Production Workflow System is a comprehensive manufacturing management solution that integrates with AutoCount Accounting 2.1/2.2 to manage:

- Bill of Materials (BOM) definition and management
- Assembly order creation and posting
- Barcode label generation and printing
- Sales order integration from external platforms
- Role-based access control for different user types

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                     External Systems                         │
│  (Franchise Portals, OMS, ERP, Mobile Apps)                 │
└────────────────────┬────────────────────────────────────────┘
                     │ REST API
                     │ (Sales Orders)
┌────────────────────▼────────────────────────────────────────┐
│                   Frontend (Next.js 14)                      │
│  ┌──────────────┬──────────────┬──────────────┬──────────┐ │
│  │   Admin UI   │ Production UI│ Warehouse UI │ Login    │ │
│  │  - BOM Mgmt  │ - Assembly   │ - Scanning   │          │ │
│  │  - Config    │ - Labels     │ - Labels     │          │ │
│  └──────────────┴──────────────┴──────────────┴──────────┘ │
└────────────────────┬────────────────────────────────────────┘
                     │ HTTPS/REST
                     │
┌────────────────────▼────────────────────────────────────────┐
│              Backend (.NET 8 Minimal API)                    │
│  ┌──────────────────────────────────────────────────────┐  │
│  │                  API Endpoints                        │  │
│  │  /items  /boms  /assembly-orders  /sales-orders      │  │
│  │  /assemblies/post  /labels/print                     │  │
│  └────────────┬─────────────────────────────────────────┘  │
│               │                                              │
│  ┌────────────▼─────────────────────────────────────────┐  │
│  │              Service Layer                            │  │
│  │  ItemService  AssemblyService  SalesOrderService     │  │
│  │  LabelService                                         │  │
│  └────────────┬─────────────────────────────────────────┘  │
│               │                                              │
│  ┌────────────▼─────────────────┬───────────────────────┐  │
│  │   AutoCount Integration      │   Metadata DB         │  │
│  │   - Stock Items              │   (SQLite)            │  │
│  │   - BOM Management           │   - Users/Roles       │  │
│  │   - Assembly Orders          │   - Label Templates   │  │
│  │   - Sales Orders             │   - Config            │  │
│  │   - Stock Movements          │                       │  │
│  └────────────┬─────────────────┴───────────────────────┘  │
└───────────────┼──────────────────────────────────────────────┘
                │
┌───────────────▼──────────────────────────────────────────────┐
│              AutoCount Accounting 2.1/2.2                     │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  SQL Server Database                                  │   │
│  │  - Stock Items                                        │   │
│  │  - Bill of Materials                                  │   │
│  │  - Stock Assembly                                     │   │
│  │  - Sales Orders                                       │   │
│  │  - Stock Movements                                    │   │
│  │  - Customers                                          │   │
│  └──────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────┘
```

## Technology Stack

### Backend
- **.NET 8**: Modern, high-performance framework
- **Minimal API**: Lightweight REST API endpoints
- **AutoCount SDK**: Integration with AutoCount Accounting
- **Entity Framework Core**: ORM for SQLite metadata database
- **Serilog**: Structured logging
- **Polly**: Resilience and retry policies
- **SQLite**: Lightweight metadata storage

### Frontend
- **Next.js 14**: React framework with App Router
- **TypeScript**: Type-safe development
- **Tailwind CSS**: Utility-first CSS framework
- **shadcn/ui**: High-quality React components
- **Axios**: HTTP client for API calls
- **React Hook Form**: Form management
- **Zod**: Schema validation

### Infrastructure
- **Windows Server**: Hosting environment
- **IIS / Windows Service**: Application hosting
- **SQL Server**: AutoCount database
- **Zebra Printers**: ZPL label printing

## Data Flow

### 1. BOM Management Flow
```
Admin UI → POST /boms/{itemCode} → ItemService → AutoCount SDK → SQL Server
                                                                      ↓
                                                              BOM Table Updated
```

### 2. Assembly Order Flow
```
Production UI → POST /assembly-orders → AssemblyService → AutoCount SDK
                                                              ↓
                                                    Stock Assembly Created
                                                              ↓
Production UI → POST /assemblies/post → AssemblyService → AutoCount SDK
                                                              ↓
                                                    ┌─────────┴─────────┐
                                                    ↓                   ↓
                                            Consume Raw Materials   Produce Finished
                                            (Stock Movement)        (Stock Movement)
```

### 3. Sales Order Integration Flow
```
External System → POST /sales-orders → SalesOrderService → Validate Customer
                                                              ↓
                                                         Validate Items
                                                              ↓
                                                    Create Sales Order in AutoCount
                                                              ↓
                                                    Return SO Number & Status
```

### 4. Label Printing Flow
```
UI → POST /labels/print → LabelService → Get Item Details
                                              ↓
                                         Get Label Template (SQLite)
                                              ↓
                                         Generate ZPL/PDF
                                              ↓
                                         Return Label Content
                                              ↓
                                    Send to Zebra Printer / Download PDF
```

## Security Architecture

### Authentication
- Simple token-based authentication (current)
- JWT tokens (recommended for production)
- Role-based access control (Admin, Production, Warehouse)

### Authorization
```
Role Permissions:
├── Admin
│   ├── Full access to all features
│   ├── BOM management
│   ├── Configuration
│   └── User management
├── Production
│   ├── Create assembly orders
│   ├── Post assemblies
│   ├── Print labels
│   └── View production data
└── Warehouse
    ├── Scan operations
    ├── Print labels
    └── View inventory
```

### Data Security
- HTTPS for all communications
- SQL Server authentication
- Encrypted connection strings
- Audit logging for critical operations

## Scalability Considerations

### Current Capacity
- **Concurrent Users**: 50-100
- **API Requests**: 1000/minute
- **Assembly Orders**: 500/day
- **Label Printing**: 10,000/day

### Scaling Options

#### Horizontal Scaling
- Deploy multiple API instances behind load balancer
- Use Redis for distributed caching
- Implement message queue for async operations

#### Vertical Scaling
- Increase server resources (CPU, RAM)
- Optimize SQL Server performance
- Implement connection pooling

#### Database Scaling
- SQL Server Always On for high availability
- Read replicas for reporting
- Partitioning for large tables

## Integration Points

### 1. AutoCount SDK Integration
```csharp
// Connection Management
AutoCount.Data.DBSetting dbSetting;
AutoCount.Data.CompanyDataAccess companyDataAccess;

// Stock Item Operations
AutoCount.Stock.Item.Item item;
item.Load(companyDataAccess, itemCode);

// BOM Operations
AutoCount.Stock.BOM.BOM bom;
bom.Load(companyDataAccess, itemCode);

// Assembly Operations
AutoCount.Stock.Assembly.StockAssembly assembly;
assembly.Post(companyDataAccess);

// Sales Order Operations
AutoCount.Sales.SalesOrder.SalesOrder so;
so.Save(companyDataAccess);
```

### 2. External System Integration
- **REST API**: JSON over HTTPS
- **Webhooks**: Event notifications (future)
- **Batch Import**: CSV/Excel file processing
- **EDI**: Electronic Data Interchange (future)

### 3. Barcode Scanner Integration
- **USB/Serial**: Direct connection
- **Bluetooth**: Wireless scanners
- **Mobile**: Camera-based scanning
- **Format**: Code-128, QR codes

### 4. Label Printer Integration
- **ZPL**: Zebra Programming Language
- **PDF**: Universal format
- **Network**: TCP/IP printing
- **USB**: Direct connection

## Error Handling

### Retry Strategy (Polly)
```csharp
Policy
  .Handle<Exception>()
  .WaitAndRetryAsync(
    retryCount: 3,
    sleepDurationProvider: retryAttempt => 
      TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
  );
```

### Error Logging
- Structured logging with Serilog
- Log levels: Debug, Information, Warning, Error, Fatal
- Log sinks: Console, File, Seq (optional)

### Error Responses
- Standard HTTP status codes
- Detailed error messages in development
- Generic messages in production
- Error tracking with correlation IDs

## Performance Optimization

### Backend
- **Caching**: In-memory cache for frequently accessed data
- **Connection Pooling**: Reuse database connections
- **Async/Await**: Non-blocking I/O operations
- **Compression**: Response compression for large payloads

### Frontend
- **Code Splitting**: Load only required code
- **Image Optimization**: Next.js Image component
- **Lazy Loading**: Load components on demand
- **Caching**: Browser and CDN caching

### Database
- **Indexing**: Proper indexes on frequently queried columns
- **Query Optimization**: Efficient SQL queries
- **Connection Pooling**: Limit concurrent connections
- **Partitioning**: For large tables (future)

## Monitoring & Observability

### Health Checks
- API health endpoint
- AutoCount connection status
- Database connectivity
- Disk space monitoring

### Metrics
- Request rate and latency
- Error rate
- Assembly order throughput
- Label printing volume

### Logging
- Application logs (Serilog)
- IIS/Windows Service logs
- SQL Server logs
- AutoCount logs

### Alerting
- Email notifications for critical errors
- SMS for system downtime
- Dashboard for real-time monitoring

## Disaster Recovery

### Backup Strategy
- **SQLite DB**: Daily backups
- **AutoCount DB**: SQL Server backup schedule
- **Configuration**: Version controlled
- **Logs**: Retained for 30 days

### Recovery Procedures
- Database restore from backup
- Application rollback to previous version
- Configuration restore
- Documented recovery steps

## Future Enhancements

### Phase 2
- [ ] Mobile app for warehouse operations
- [ ] Real-time dashboard with SignalR
- [ ] Advanced reporting and analytics
- [ ] Batch processing for large orders
- [ ] Multi-warehouse support

### Phase 3
- [ ] Machine learning for demand forecasting
- [ ] IoT integration for production monitoring
- [ ] Blockchain for supply chain tracking
- [ ] Advanced workflow automation
- [ ] Integration with ERP systems

## Compliance & Standards

- **Data Privacy**: GDPR compliance (if applicable)
- **Audit Trail**: All critical operations logged
- **Access Control**: Role-based permissions
- **Data Retention**: Configurable retention policies
- **Security Standards**: OWASP Top 10 compliance

