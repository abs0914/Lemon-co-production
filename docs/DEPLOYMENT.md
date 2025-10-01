# Production Deployment Guide

## Windows Service Deployment (Backend)

### Step 1: Publish the Application

```bash
cd backend/LemonCo.Api
dotnet publish -c Release -o ./publish
```

### Step 2: Install as Windows Service

#### Option A: Using sc.exe

```cmd
sc create LemonCoProductionApi binPath="C:\LemonCo\LemonCo.Api.exe" start=auto
sc description LemonCoProductionApi "Lemon Co Production Workflow API"
sc start LemonCoProductionApi
```

#### Option B: Using NSSM (Non-Sucking Service Manager)

1. Download NSSM from https://nssm.cc/download
2. Install service:

```cmd
nssm install LemonCoProductionApi "C:\LemonCo\LemonCo.Api.exe"
nssm set LemonCoProductionApi AppDirectory "C:\LemonCo"
nssm set LemonCoProductionApi DisplayName "Lemon Co Production API"
nssm set LemonCoProductionApi Description "Production workflow system with AutoCount integration"
nssm set LemonCoProductionApi Start SERVICE_AUTO_START
nssm start LemonCoProductionApi
```

### Step 3: Configure Firewall

```powershell
New-NetFirewallRule -DisplayName "Lemon Co API" -Direction Inbound -LocalPort 5001 -Protocol TCP -Action Allow
```

### Step 4: Configure HTTPS Certificate

#### Development Certificate
```bash
dotnet dev-certs https --trust
```

#### Production Certificate
1. Obtain SSL certificate from CA
2. Install certificate in Windows Certificate Store
3. Update `appsettings.Production.json`:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://*:5001",
        "Certificate": {
          "Subject": "lemonco.yourdomain.com",
          "Store": "My",
          "Location": "LocalMachine"
        }
      }
    }
  }
}
```

### Step 5: Configure Production Settings

Create `appsettings.Production.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "LemonCoDb": "Data Source=C:\\LemonCo\\Data\\lemonco.db"
  },
  "AutoCount": {
    "ServerName": "PROD-SQL-SERVER",
    "DatabaseName": "AutoCountDB_PROD",
    "UserId": "lemonco_api",
    "Password": "SECURE_PASSWORD_HERE",
    "UseWindowsAuth": false,
    "CompanyCode": "LEMONCO",
    "AutoCountUser": "api_user",
    "AutoCountPassword": "SECURE_PASSWORD_HERE"
  },
  "Serilog": {
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "C:\\LemonCo\\Logs\\lemonco-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

### Step 6: Verify Service

```powershell
# Check service status
Get-Service LemonCoProductionApi

# View logs
Get-Content C:\LemonCo\Logs\lemonco-*.log -Tail 50

# Test API
Invoke-WebRequest -Uri https://localhost:5001/health
```

---

## IIS Deployment (Alternative)

### Step 1: Install Prerequisites

1. Install IIS with ASP.NET Core Module
2. Install .NET 8 Hosting Bundle from https://dotnet.microsoft.com/download/dotnet/8.0

### Step 2: Publish Application

```bash
dotnet publish -c Release -o C:\inetpub\lemonco-api
```

### Step 3: Create IIS Site

1. Open IIS Manager
2. Add Application Pool:
   - Name: `LemonCoApi`
   - .NET CLR Version: `No Managed Code`
   - Managed Pipeline Mode: `Integrated`

3. Add Website:
   - Site Name: `LemonCo Production API`
   - Application Pool: `LemonCoApi`
   - Physical Path: `C:\inetpub\lemonco-api`
   - Binding: `https` on port `5001`

### Step 4: Configure web.config

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\LemonCo.Api.dll" 
                  stdoutLogEnabled="true" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess" />
    </system.webServer>
  </location>
</configuration>
```

---

## Frontend Deployment

### Option 1: Static Export (Recommended)

```bash
cd frontend
npm run build
```

Deploy the `out/` directory to:
- IIS
- Nginx
- Apache
- CDN (Cloudflare, AWS CloudFront)

### Option 2: Node.js Server

```bash
cd frontend
npm run build
npm start
```

Run as Windows Service using NSSM:

```cmd
nssm install LemonCoFrontend "C:\Program Files\nodejs\node.exe"
nssm set LemonCoFrontend AppParameters "C:\LemonCo\Frontend\.next\standalone\server.js"
nssm set LemonCoFrontend AppDirectory "C:\LemonCo\Frontend"
nssm start LemonCoFrontend
```

### Option 3: Vercel/Netlify

```bash
# Install Vercel CLI
npm i -g vercel

# Deploy
cd frontend
vercel --prod
```

---

## Database Backup

### SQLite Metadata Database

```powershell
# Backup script
$date = Get-Date -Format "yyyyMMdd-HHmmss"
Copy-Item "C:\LemonCo\Data\lemonco.db" "C:\LemonCo\Backups\lemonco-$date.db"
```

Schedule with Task Scheduler:
```powershell
$action = New-ScheduledTaskAction -Execute "PowerShell.exe" -Argument "-File C:\LemonCo\Scripts\backup.ps1"
$trigger = New-ScheduledTaskTrigger -Daily -At 2am
Register-ScheduledTask -TaskName "LemonCo DB Backup" -Action $action -Trigger $trigger
```

### AutoCount Database

Use SQL Server backup:
```sql
BACKUP DATABASE AutoCountDB_PROD
TO DISK = 'C:\Backups\AutoCount\AutoCountDB_PROD.bak'
WITH FORMAT, COMPRESSION;
```

---

## Monitoring

### Health Check Monitoring

Create PowerShell script `health-check.ps1`:

```powershell
$url = "https://localhost:5001/health"
try {
    $response = Invoke-WebRequest -Uri $url -UseBasicParsing
    $health = $response.Content | ConvertFrom-Json
    
    if ($health.status -ne "healthy") {
        # Send alert email
        Send-MailMessage -To "admin@lemonco.com" `
                        -From "monitor@lemonco.com" `
                        -Subject "LemonCo API Health Check Failed" `
                        -Body "API is unhealthy: $($health.status)" `
                        -SmtpServer "smtp.office365.com"
    }
} catch {
    # Send alert email
    Write-Error "Health check failed: $_"
}
```

Schedule every 5 minutes with Task Scheduler.

### Log Monitoring

Use tools like:
- **Seq** - https://datalust.co/seq
- **Elasticsearch + Kibana**
- **Application Insights** (Azure)

Configure Serilog sink:
```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://localhost:5341"
        }
      }
    ]
  }
}
```

---

## Security Checklist

- [ ] Change default admin password
- [ ] Use strong SQL Server passwords
- [ ] Enable HTTPS with valid certificate
- [ ] Configure firewall rules
- [ ] Implement API authentication (JWT)
- [ ] Enable CORS only for trusted origins
- [ ] Regular security updates
- [ ] Encrypt sensitive configuration
- [ ] Implement rate limiting
- [ ] Enable audit logging
- [ ] Regular database backups
- [ ] Disaster recovery plan

---

## Performance Tuning

### Backend

1. **Connection Pooling**
```json
{
  "AutoCount": {
    "ConnectionTimeout": 30,
    "CommandTimeout": 60,
    "MaxPoolSize": 100
  }
}
```

2. **Caching**
```csharp
services.AddMemoryCache();
services.AddResponseCaching();
```

3. **Compression**
```csharp
services.AddResponseCompression(options => {
    options.EnableForHttps = true;
});
```

### Frontend

1. **Image Optimization**
2. **Code Splitting**
3. **CDN for Static Assets**
4. **Browser Caching**

---

## Troubleshooting

### Service Won't Start

1. Check Windows Event Viewer
2. Review logs in `C:\LemonCo\Logs\`
3. Verify permissions on directories
4. Test manually: `C:\LemonCo\LemonCo.Api.exe`

### AutoCount Connection Issues

1. Test SQL connection
2. Verify AutoCount user permissions
3. Check firewall rules
4. Review AutoCount logs

### Performance Issues

1. Check SQL Server performance
2. Review API logs for slow queries
3. Monitor memory usage
4. Check network latency

---

## Rollback Procedure

1. Stop service
```cmd
sc stop LemonCoProductionApi
```

2. Restore previous version
```cmd
xcopy /E /Y C:\LemonCo\Backups\v1.0.0\* C:\LemonCo\
```

3. Restore database
```powershell
Copy-Item "C:\LemonCo\Backups\lemonco-backup.db" "C:\LemonCo\Data\lemonco.db"
```

4. Start service
```cmd
sc start LemonCoProductionApi
```

---

## Support Contacts

- **System Administrator**: admin@lemonco.com
- **AutoCount Support**: support@autocount.com
- **Emergency Hotline**: +1-XXX-XXX-XXXX

