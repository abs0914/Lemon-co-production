# Lemon Co API - Windows Service Installation Script
# Run as Administrator

$serviceName = "LemonCoAPI"
$displayName = "Lemon Co Production API"
$description = "Lemon Co Production Workflow API with AutoCount Integration"
$exePath = "C:\LemonCo\API\LemonCo.Api.exe"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Lemon Co API - Service Installation" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    exit 1
}

# Check if executable exists
if (-not (Test-Path $exePath)) {
    Write-Host "ERROR: API executable not found at: $exePath" -ForegroundColor Red
    Write-Host "Please ensure the API is deployed to C:\LemonCo\API\" -ForegroundColor Yellow
    exit 1
}

# Stop and remove existing service if it exists
if (Get-Service $serviceName -ErrorAction SilentlyContinue) {
    Write-Host "Stopping existing service..." -ForegroundColor Yellow
    Stop-Service $serviceName -Force
    Start-Sleep -Seconds 2
    
    Write-Host "Removing existing service..." -ForegroundColor Yellow
    sc.exe delete $serviceName
    Start-Sleep -Seconds 2
}

# Create new service
Write-Host "Creating Windows Service..." -ForegroundColor Green
try {
    New-Service -Name $serviceName `
        -BinaryPathName $exePath `
        -DisplayName $displayName `
        -Description $description `
        -StartupType Automatic `
        -ErrorAction Stop
    
    Write-Host "Service created successfully!" -ForegroundColor Green
} catch {
    Write-Host "ERROR: Failed to create service: $_" -ForegroundColor Red
    exit 1
}

# Configure service recovery options (restart on failure)
Write-Host "Configuring service recovery options..." -ForegroundColor Green
sc.exe failure $serviceName reset= 86400 actions= restart/60000/restart/60000/restart/60000

# Start the service
Write-Host "Starting service..." -ForegroundColor Green
try {
    Start-Service $serviceName -ErrorAction Stop
    Start-Sleep -Seconds 3
    
    $service = Get-Service $serviceName
    if ($service.Status -eq "Running") {
        Write-Host "Service started successfully!" -ForegroundColor Green
    } else {
        Write-Host "WARNING: Service is not running. Status: $($service.Status)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "ERROR: Failed to start service: $_" -ForegroundColor Red
    Write-Host "Check Event Viewer for details" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Service Installation Complete!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Service Name: $serviceName" -ForegroundColor White
Write-Host "Display Name: $displayName" -ForegroundColor White
Write-Host "Status: $($(Get-Service $serviceName).Status)" -ForegroundColor White
Write-Host ""
Write-Host "Testing API..." -ForegroundColor Yellow
Start-Sleep -Seconds 2

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/health" -TimeoutSec 5
    Write-Host "API Health Check: SUCCESS" -ForegroundColor Green
    Write-Host "Status: $($response.status)" -ForegroundColor White
    Write-Host "AutoCount Connected: $($response.autoCountConnected)" -ForegroundColor White
} catch {
    Write-Host "API Health Check: FAILED" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Yellow
    Write-Host "Check logs and configuration" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Useful Commands:" -ForegroundColor Cyan
Write-Host "  Start Service:   Start-Service $serviceName" -ForegroundColor White
Write-Host "  Stop Service:    Stop-Service $serviceName" -ForegroundColor White
Write-Host "  Restart Service: Restart-Service $serviceName" -ForegroundColor White
Write-Host "  Check Status:    Get-Service $serviceName" -ForegroundColor White
Write-Host "  View Logs:       Get-EventLog -LogName Application -Source .NET* -Newest 20" -ForegroundColor White
Write-Host ""

