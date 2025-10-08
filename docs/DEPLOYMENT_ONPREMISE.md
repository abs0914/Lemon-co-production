# 🖥️ On-Premise Deployment Guide - Windows Server + SQL Server 2022 Express

## 📋 Overview

This guide will help you deploy the Lemon Co Production API on an **on-premise Windows Server** with **SQL Server 2022 Express** and **AutoCount**.

---

## 🎯 Server Requirements

### Hardware (Minimum)
- **CPU:** 4 cores (Intel/AMD)
- **RAM:** 8GB (16GB recommended)
- **Storage:** 100GB SSD
- **Network:** Static IP or DDNS, Port forwarding capability

### Software
- **OS:** Windows Server 2019/2022 or Windows 10/11 Pro
- **SQL Server:** SQL Server 2022 Express (Free)
- **.NET:** .NET 8 SDK
- **AutoCount:** Client's AutoCount installation

### Network Requirements
- **Static IP or DDNS:** For external access
- **Router Access:** To configure port forwarding
- **Firewall:** Ports 5000, 5001 open
- **Internet:** Stable connection for API access

---

## 📦 Step 1: Prepare Windows Server

### 1.1 Update Windows
```powershell
# Run Windows Update
# Settings > Update & Security > Windows Update > Check for updates
```

### 1.2 Enable Remote Desktop (Optional)
```powershell
# For remote management
Set-ItemProperty -Path 'HKLM:\System\CurrentControlSet\Control\Terminal Server' -Name "fDenyTSConnections" -Value 0
Enable-NetFirewallRule -DisplayGroup "Remote Desktop"
```

### 1.3 Create Application Directory
```powershell
# Create directories
New-Item -Path "C:\LemonCo" -ItemType Directory
New-Item -Path "C:\LemonCo\API" -ItemType Directory
New-Item -Path "C:\LemonCo\Logs" -ItemType Directory
New-Item -Path "C:\LemonCo\Backups" -ItemType Directory
```

---

## 📦 Step 2: Install SQL Server 2022 Express

### 2.1 Download SQL Server 2022 Express
- **URL:** https://www.microsoft.com/en-us/sql-server/sql-server-downloads
- **Version:** SQL Server 2022 Express (Free)
- **Download:** Click "Download now" under Express edition

### 2.2 Install SQL Server
```
1. Run the installer: SQLServer2022-SSEI-Expr.exe
2. Choose: "Basic" installation
3. Accept license terms
4. Choose installation location: C:\Program Files\Microsoft SQL Server
5. Click "Install"
6. Wait for installation to complete
7. Note the instance name: Usually "SQLEXPRESS"
```

### 2.3 Install SQL Server Management Studio (SSMS)
```
1. Download SSMS from: https://aka.ms/ssmsfullsetup
2. Run installer: SSMS-Setup-ENU.exe
3. Click "Install"
4. Restart computer after installation
```

### 2.4 Configure SQL Server
```powershell
# Enable SQL Server Browser service
Set-Service -Name "SQLBrowser" -StartupType Automatic
Start-Service -Name "SQLBrowser"

# Enable SQL Server service
Set-Service -Name "MSSQL`$SQLEXPRESS" -StartupType Automatic
Start-Service -Name "MSSQL`$SQLEXPRESS"
```

### 2.5 Enable SQL Server Authentication
```
1. Open SQL Server Management Studio (SSMS)
2. Connect to: localhost\SQLEXPRESS (Windows Authentication)
3. Right-click server name > Properties
4. Select "Security" page
5. Choose "SQL Server and Windows Authentication mode"
6. Click OK
7. Restart SQL Server service:
```

```powershell
Restart-Service -Name "MSSQL`$SQLEXPRESS"
```

### 2.6 Create SQL Server Login
```sql
-- In SSMS, open New Query and run:

-- Create login for API
CREATE LOGIN lemonco_api WITH PASSWORD = 'YourStrongPassword123!';

-- Create database for AutoCount (if not exists)
-- Note: AutoCount usually creates its own database
-- This is just for reference

-- Grant permissions
USE master;
GRANT VIEW SERVER STATE TO lemonco_api;
```

---

## 📦 Step 3: Install .NET 8 SDK

### 3.1 Download .NET 8 SDK
- **URL:** https://dotnet.microsoft.com/download/dotnet/8.0
- **Download:** .NET 8.0 SDK (x64) - Windows Installer

### 3.2 Install .NET 8
```
1. Run installer: dotnet-sdk-8.0.xxx-win-x64.exe
2. Follow installation wizard
3. Click "Install"
4. Wait for completion
```

### 3.3 Verify Installation
```powershell
# Open PowerShell and verify
dotnet --version
# Should show: 8.0.xxx
```

---

## 📦 Step 4: Install AutoCount

### 4.1 Install AutoCount Software
```
1. Use client's AutoCount installation files
2. Follow AutoCount installation wizard
3. Configure AutoCount database connection to SQL Server
4. Create company database (e.g., "LEMONCO")
5. Note the database name and credentials
```

### 4.2 Verify AutoCount Database
```sql
-- In SSMS, verify AutoCount database exists
SELECT name FROM sys.databases WHERE name LIKE '%AutoCount%';

-- Check AutoCount tables
USE [YourAutoCountDB];
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES;
```

---

## 📦 Step 5: Deploy Lemon Co API

### 5.1 Build the Application
```powershell
# On your development machine, build the release version
cd C:\Users\USER\Documents\augment-projects\Lemon-co-production\backend
dotnet publish LemonCo.Api -c Release -o publish
```

### 5.2 Copy Files to Server
```powershell
# Copy the publish folder to the server
# Use USB drive, network share, or remote desktop

# On the server:
# Copy contents of publish folder to C:\LemonCo\API\
```

### 5.3 Configure appsettings.json
```powershell
# Edit C:\LemonCo\API\appsettings.json
notepad C:\LemonCo\API\appsettings.json
```

Update with your settings:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=C:\\LemonCo\\API\\lemonco.db"
  },
  "AutoCount": {
    "ServerName": "localhost\\SQLEXPRESS",
    "DatabaseName": "YOUR_AUTOCOUNT_DATABASE_NAME",
    "UserId": "lemonco_api",
    "Password": "YourStrongPassword123!",
    "UseWindowsAuth": false,
    "CompanyCode": "LEMONCO",
    "AutoCountUser": "admin",
    "AutoCountPassword": "your_autocount_password"
  },
  "Supabase": {
    "Url": "https://pukezienbcenozlqmunf.supabase.co",
    "AnonKey": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InB1a2V6aWVuYmNlbm96bHFtdW5mIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTkxOTYyMDQsImV4cCI6MjA3NDc3MjIwNH0.WlMo4tKLwQi_FuAQ15LqmXz6UXHHnnEJqNojxG0PnY0",
    "JwtSecret": "OhYO147mYGunXEVYGmZ3cYnUb6qyH0E8cvhhoogELnF+uyLuHCNhjEBQpzUgNRqlfSNzf4WzPhZk5gONyxRj7w==",
    "JwtIssuer": "https://pukezienbcenozlqmunf.supabase.co/auth/v1"
  },
  "Jwt": {
    "Secret": "OhYO147mYGunXEVYGmZ3cYnUb6qyH0E8cvhhoogELnF+uyLuHCNhjEBQpzUgNRqlfSNzf4WzPhZk5gONyxRj7w==",
    "Issuer": "LemonCoProductionAPI",
    "Audience": "LemonCoFrontend",
    "ExpiryMinutes": 480
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5000"
      }
    }
  }
}
```

**Important:** Replace:
- `YOUR_AUTOCOUNT_DATABASE_NAME` - Your AutoCount database name
- `YourStrongPassword123!` - Your SQL Server password
- `your_autocount_password` - Your AutoCount password

---

## 📦 Step 6: Configure Windows Firewall

### 6.1 Allow API Ports
```powershell
# Allow port 5000 (HTTP)
New-NetFirewallRule -DisplayName "Lemon Co API HTTP" -Direction Inbound -LocalPort 5000 -Protocol TCP -Action Allow

# Allow port 5001 (HTTPS) - for future use
New-NetFirewallRule -DisplayName "Lemon Co API HTTPS" -Direction Inbound -LocalPort 5001 -Protocol TCP -Action Allow
```

### 6.2 Verify Firewall Rules
```powershell
Get-NetFirewallRule -DisplayName "Lemon Co API*" | Format-Table DisplayName, Enabled, Direction, Action
```

---

## 📦 Step 7: Test the API

### 7.1 Run API Manually (First Test)
```powershell
# Navigate to API directory
cd C:\LemonCo\API

# Run the API
.\LemonCo.Api.exe

# You should see:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: http://0.0.0.0:5000
```

### 7.2 Test from Server
```powershell
# Open another PowerShell window
Invoke-RestMethod -Uri "http://localhost:5000/health"

# Should return:
# status        : healthy/unhealthy
# timestamp     : 2025-10-02T...
# autoCountConnected : true/false
```

### 7.3 Test from Another Computer
```powershell
# Replace SERVER_IP with your server's IP address
Invoke-RestMethod -Uri "http://SERVER_IP:5000/health"
```

---

## 📦 Step 8: Install as Windows Service

### 8.1 Create Service Installation Script
```powershell
# Create install-service.ps1
@"
`$serviceName = "LemonCoAPI"
`$displayName = "Lemon Co Production API"
`$description = "Lemon Co Production Workflow API with AutoCount Integration"
`$exePath = "C:\LemonCo\API\LemonCo.Api.exe"

# Stop service if exists
if (Get-Service `$serviceName -ErrorAction SilentlyContinue) {
    Stop-Service `$serviceName
    sc.exe delete `$serviceName
    Start-Sleep -Seconds 2
}

# Create service
New-Service -Name `$serviceName ``
    -BinaryPathName `$exePath ``
    -DisplayName `$displayName ``
    -Description `$description ``
    -StartupType Automatic

# Start service
Start-Service `$serviceName

# Verify
Get-Service `$serviceName
"@ | Out-File -FilePath "C:\LemonCo\install-service.ps1" -Encoding UTF8
```

### 8.2 Install the Service
```powershell
# Run as Administrator
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process
C:\LemonCo\install-service.ps1
```

### 8.3 Verify Service
```powershell
# Check service status
Get-Service -Name "LemonCoAPI"

# Check if API is responding
Invoke-RestMethod -Uri "http://localhost:5000/health"
```

---

## 📦 Step 9: Configure Router (Port Forwarding)

### 9.1 Get Server Local IP
```powershell
# Get server's local IP address
Get-NetIPAddress -AddressFamily IPv4 | Where-Object {$_.IPAddress -like "192.168.*"} | Select-Object IPAddress
```

### 9.2 Configure Router Port Forwarding
```
1. Login to your router admin panel (usually 192.168.1.1 or 192.168.0.1)
2. Find "Port Forwarding" or "Virtual Server" section
3. Add new rule:
   - Service Name: Lemon Co API
   - External Port: 5000
   - Internal IP: [Your server's local IP from step 9.1]
   - Internal Port: 5000
   - Protocol: TCP
4. Save and apply
```

### 9.3 Get Public IP
```powershell
# Get your public IP address
Invoke-RestMethod -Uri "https://api.ipify.org?format=json"
```

### 9.4 Test External Access
```
From your phone (disconnect from WiFi, use mobile data):
Open browser: http://YOUR_PUBLIC_IP:5000/health
```

---

## 📦 Step 10: Update Lovable.dev Frontend

### 10.1 Update API Base URL

In your Lovable.dev project, update the API URL:

```typescript
// Use your public IP or domain
const API_BASE_URL = "http://YOUR_PUBLIC_IP:5000";

// Or if you have a domain with DDNS:
const API_BASE_URL = "http://lemonco.yourdomain.com:5000";
```

### 10.2 Update CORS in API

On the server, edit `C:\LemonCo\API\appsettings.json` or update `Program.cs`:

```powershell
# Stop the service
Stop-Service -Name "LemonCoAPI"

# Edit Program.cs or appsettings.json to add your public IP
# (Already configured with your Lovable.dev domain)

# Restart service
Start-Service -Name "LemonCoAPI"
```

---

## 📦 Step 11: Set Up DDNS (Optional but Recommended)

### Why DDNS?
- Your public IP may change
- DDNS gives you a permanent domain name

### Popular Free DDNS Providers:
- **No-IP:** https://www.noip.com (Free)
- **DuckDNS:** https://www.duckdns.org (Free)
- **Dynu:** https://www.dynu.com (Free)

### Setup Example (No-IP):
```
1. Create account at https://www.noip.com
2. Create hostname: lemonco.ddns.net
3. Download No-IP DUC (Dynamic Update Client)
4. Install on your server
5. Configure with your hostname
6. Use http://lemonco.ddns.net:5000 as your API URL
```

---

## 📦 Step 12: Backup Configuration

### 12.1 Create Backup Script
```powershell
# Create backup-lemonco.ps1
@"
`$backupPath = "C:\LemonCo\Backups\backup_`$(Get-Date -Format 'yyyyMMdd_HHmmss')"
New-Item -Path `$backupPath -ItemType Directory

# Backup SQLite database
Copy-Item "C:\LemonCo\API\lemonco.db" `$backupPath

# Backup configuration
Copy-Item "C:\LemonCo\API\appsettings.json" `$backupPath

# Backup SQL Server (AutoCount database)
`$sqlBackupPath = "`$backupPath\autocount.bak"
sqlcmd -S localhost\SQLEXPRESS -Q "BACKUP DATABASE [YOUR_AUTOCOUNT_DB] TO DISK='`$sqlBackupPath'"

Write-Host "Backup completed: `$backupPath"
"@ | Out-File -FilePath "C:\LemonCo\backup-lemonco.ps1" -Encoding UTF8
```

### 12.2 Schedule Daily Backups
```powershell
# Create scheduled task for daily backup at 2 AM
$action = New-ScheduledTaskAction -Execute "PowerShell.exe" -Argument "-File C:\LemonCo\backup-lemonco.ps1"
$trigger = New-ScheduledTaskTrigger -Daily -At 2am
Register-ScheduledTask -TaskName "LemonCo Daily Backup" -Action $action -Trigger $trigger -RunLevel Highest
```

---

## ✅ Deployment Checklist

- [ ] Windows Server/PC prepared
- [ ] SQL Server 2022 Express installed
- [ ] SQL Server Management Studio installed
- [ ] SQL Server authentication enabled
- [ ] SQL Server login created
- [ ] .NET 8 SDK installed
- [ ] AutoCount installed and configured
- [ ] AutoCount database created
- [ ] API files copied to C:\LemonCo\API
- [ ] appsettings.json configured
- [ ] Windows Firewall rules added
- [ ] API tested locally
- [ ] Windows Service installed
- [ ] Service starts automatically
- [ ] Router port forwarding configured
- [ ] External access tested
- [ ] Lovable.dev frontend updated with API URL
- [ ] CORS configured for Lovable.dev domain
- [ ] DDNS configured (optional)
- [ ] Backup script created
- [ ] Scheduled backups configured
- [ ] Demo user passwords changed

---

## 🔧 Troubleshooting

### API Won't Start
```powershell
# Check service status
Get-Service -Name "LemonCoAPI"

# Check event logs
Get-EventLog -LogName Application -Source "LemonCoAPI" -Newest 10

# Run manually to see errors
cd C:\LemonCo\API
.\LemonCo.Api.exe
```

### Can't Connect to SQL Server
```powershell
# Test SQL Server connection
sqlcmd -S localhost\SQLEXPRESS -U lemonco_api -P YourPassword

# Check SQL Server service
Get-Service -Name "MSSQL`$SQLEXPRESS"

# Restart SQL Server
Restart-Service -Name "MSSQL`$SQLEXPRESS"
```

### Can't Access from External Network
```powershell
# Check firewall rules
Get-NetFirewallRule -DisplayName "Lemon Co API*"

# Test if port is listening
netstat -an | findstr :5000

# Check router port forwarding configuration
```

### AutoCount Connection Failed
```
1. Verify AutoCount database name in appsettings.json
2. Check AutoCount credentials
3. Ensure AutoCount is installed and database exists
4. Check SQL Server permissions for AutoCount database
```

---

## 📞 Support

If you encounter issues:
1. Check logs in `C:\LemonCo\Logs`
2. Check Windows Event Viewer
3. Test API manually: `cd C:\LemonCo\API && .\LemonCo.Api.exe`
4. Verify all services are running

---

## 🎉 Deployment Complete!

Your Lemon Co Production API is now running on-premise with:
- ✅ Windows Server
- ✅ SQL Server 2022 Express
- ✅ AutoCount Integration
- ✅ .NET 8 API
- ✅ Windows Service (auto-start)
- ✅ External access configured
- ✅ Automated backups

**API URL:** `http://YOUR_PUBLIC_IP:5000` or `http://your-ddns-domain:5000`  
**Swagger UI:** `http://YOUR_PUBLIC_IP:5000/swagger`

Your Lovable.dev frontend can now connect to your on-premise API! 🚀

