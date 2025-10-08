# 🚀 Lemon Co API - On-Premise Deployment Quick Reference

## 📋 Pre-Deployment Checklist

### Hardware & Network
- [ ] Windows Server/PC with 8GB+ RAM, 4+ CPU cores
- [ ] Static local IP or DHCP reservation
- [ ] Router admin access for port forwarding
- [ ] Stable internet connection

### Software Downloads
- [ ] SQL Server 2022 Express: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
- [ ] SQL Server Management Studio: https://aka.ms/ssmsfullsetup
- [ ] .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0
- [ ] AutoCount installation files (from client)

---

## ⚡ Quick Installation Steps

### 1. Install SQL Server 2022 Express (10 min)
```powershell
# Run installer, choose "Basic" installation
# Note the instance name: SQLEXPRESS
# Enable SQL Server Browser service
Set-Service -Name "SQLBrowser" -StartupType Automatic
Start-Service -Name "SQLBrowser"
```

### 2. Install .NET 8 SDK (5 min)
```powershell
# Run installer
# Verify installation
dotnet --version  # Should show 8.0.xxx
```

### 3. Install AutoCount (15 min)
```
# Follow AutoCount installation wizard
# Configure database connection to SQL Server
# Create company database
# Note database name and credentials
```

### 4. Deploy API (10 min)
```powershell
# On development machine, build release
cd backend
dotnet publish LemonCo.Api -c Release -o publish

# Copy publish folder to server: C:\LemonCo\API\

# On server, edit configuration
notepad C:\LemonCo\API\appsettings.json
```

**Update these values:**
```json
{
  "AutoCount": {
    "ServerName": "localhost\\SQLEXPRESS",
    "DatabaseName": "YOUR_AUTOCOUNT_DB_NAME",
    "UserId": "sa",
    "Password": "YOUR_SQL_PASSWORD"
  }
}
```

### 5. Configure Firewall (2 min)
```powershell
# Run as Administrator
cd C:\LemonCo
.\scripts\setup-firewall.ps1
```

### 6. Install Windows Service (3 min)
```powershell
# Run as Administrator
cd C:\LemonCo
.\scripts\install-service.ps1
```

### 7. Test API (2 min)
```powershell
# Test locally
Invoke-RestMethod -Uri "http://localhost:5000/health"

# Open Swagger UI
Start-Process "http://localhost:5000/swagger"
```

### 8. Configure Router Port Forwarding (5 min)
```
1. Get server local IP: ipconfig
2. Login to router (usually 192.168.1.1)
3. Add port forwarding rule:
   - External Port: 5000
   - Internal IP: [Server IP]
   - Internal Port: 5000
   - Protocol: TCP
```

### 9. Get Public IP & Test (2 min)
```powershell
# Get public IP
Invoke-RestMethod -Uri "https://api.ipify.org?format=json"

# Test from phone (mobile data):
# http://YOUR_PUBLIC_IP:5000/health
```

### 10. Update Lovable.dev Frontend (5 min)
```typescript
// Update API base URL in your Lovable.dev project
const API_BASE_URL = "http://YOUR_PUBLIC_IP:5000";
```

---

## 🔧 Useful Commands

### Service Management
```powershell
# Start service
Start-Service LemonCoAPI

# Stop service
Stop-Service LemonCoAPI

# Restart service
Restart-Service LemonCoAPI

# Check status
Get-Service LemonCoAPI

# View service logs
Get-EventLog -LogName Application -Source .NET* -Newest 20
```

### Testing
```powershell
# Health check
Invoke-RestMethod -Uri "http://localhost:5000/health"

# Test login
$body = '{"username":"admin","password":"admin123"}'
Invoke-RestMethod -Uri "http://localhost:5000/auth/login" -Method POST -Body $body -ContentType "application/json"

# Open Swagger UI
Start-Process "http://localhost:5000/swagger"
```

### Backup
```powershell
# Run manual backup
C:\LemonCo\scripts\backup-lemonco.ps1

# Schedule daily backup at 2 AM
$action = New-ScheduledTaskAction -Execute "PowerShell.exe" -Argument "-File C:\LemonCo\scripts\backup-lemonco.ps1"
$trigger = New-ScheduledTaskTrigger -Daily -At 2am
Register-ScheduledTask -TaskName "LemonCo Daily Backup" -Action $action -Trigger $trigger -RunLevel Highest
```

### Network Diagnostics
```powershell
# Check if port is listening
netstat -an | findstr :5000

# Get local IP
Get-NetIPAddress -AddressFamily IPv4 | Where-Object {$_.IPAddress -like "192.168.*"}

# Get public IP
Invoke-RestMethod -Uri "https://api.ipify.org?format=json"

# Test port from external
# Use: https://www.yougetsignal.com/tools/open-ports/
```

---

## 🔍 Troubleshooting

### API Won't Start
```powershell
# Run manually to see errors
cd C:\LemonCo\API
.\LemonCo.Api.exe

# Check if port is already in use
netstat -ano | findstr :5000

# Kill process using port
taskkill /F /PID [PID_NUMBER]
```

### Can't Connect to SQL Server
```powershell
# Test connection
sqlcmd -S localhost\SQLEXPRESS -U sa -P YourPassword

# Check service
Get-Service -Name "MSSQL$SQLEXPRESS"

# Restart SQL Server
Restart-Service -Name "MSSQL$SQLEXPRESS"
```

### AutoCount Connection Failed
```
1. Verify database name in appsettings.json
2. Check AutoCount is installed
3. Verify SQL Server credentials
4. Test SQL connection with SSMS
```

### Can't Access from External Network
```
1. Check Windows Firewall rules
2. Verify router port forwarding
3. Check if ISP blocks port 5000
4. Test with phone on mobile data
5. Consider using DDNS service
```

---

## 📁 File Locations

| Item | Location |
|------|----------|
| API Files | `C:\LemonCo\API\` |
| Configuration | `C:\LemonCo\API\appsettings.json` |
| SQLite Database | `C:\LemonCo\API\lemonco.db` |
| Logs | `C:\LemonCo\Logs\` |
| Backups | `C:\LemonCo\Backups\` |
| Scripts | `C:\LemonCo\scripts\` |

---

## 🌐 URLs

| Service | URL |
|---------|-----|
| API Health | `http://localhost:5000/health` |
| Swagger UI | `http://localhost:5000/swagger` |
| Login Endpoint | `http://localhost:5000/auth/login` |
| External API | `http://YOUR_PUBLIC_IP:5000` |
| Lovable Frontend | `https://lemonflow-ops.lovable.app` |

---

## 👥 Demo Users

| Username | Password | Role |
|----------|----------|------|
| admin | admin123 | Admin |
| production | prod123 | Production |
| warehouse | wh123 | Warehouse |

**⚠️ IMPORTANT:** Change these passwords in production!

---

## 📞 Support Resources

- **Full Deployment Guide:** `docs/DEPLOYMENT_ONPREMISE.md`
- **Authentication Guide:** `docs/AUTHENTICATION.md`
- **Quick Start:** `QUICK_START.md`
- **Test Results:** `API_TEST_RESULTS.md`

---

## ⏱️ Total Deployment Time

**Estimated:** 60-90 minutes

- SQL Server installation: 10 min
- .NET SDK installation: 5 min
- AutoCount installation: 15 min
- API deployment: 10 min
- Configuration: 10 min
- Firewall setup: 2 min
- Service installation: 3 min
- Testing: 5 min
- Router configuration: 5 min
- External testing: 5 min
- Frontend update: 5 min
- **Buffer time:** 15-30 min

---

## ✅ Post-Deployment Checklist

- [ ] API service running and auto-starts
- [ ] Health endpoint returns 200 OK
- [ ] AutoCount connection successful
- [ ] Login works with demo users
- [ ] Swagger UI accessible
- [ ] Windows Firewall configured
- [ ] Router port forwarding configured
- [ ] External access tested
- [ ] Lovable.dev frontend connected
- [ ] CORS configured correctly
- [ ] Backup script configured
- [ ] Scheduled backups enabled
- [ ] Demo passwords changed
- [ ] Documentation reviewed
- [ ] Client trained on system

---

## 🎉 Success Criteria

✅ API responds to health checks  
✅ Authentication works  
✅ AutoCount integration functional  
✅ External access from Lovable.dev works  
✅ All endpoints protected with authorization  
✅ Backups running automatically  

**Your Lemon Co Production API is ready for production use!** 🚀

