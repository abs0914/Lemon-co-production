# Lemon Co API - Windows Service Uninstallation Script
# Run as Administrator

$serviceName = "LemonCoAPI"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Lemon Co API - Service Uninstallation" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    exit 1
}

# Check if service exists
if (-not (Get-Service $serviceName -ErrorAction SilentlyContinue)) {
    Write-Host "Service '$serviceName' not found. Nothing to uninstall." -ForegroundColor Yellow
    exit 0
}

# Stop the service
Write-Host "Stopping service..." -ForegroundColor Yellow
try {
    Stop-Service $serviceName -Force -ErrorAction Stop
    Write-Host "Service stopped successfully!" -ForegroundColor Green
} catch {
    Write-Host "WARNING: Could not stop service: $_" -ForegroundColor Yellow
}

Start-Sleep -Seconds 2

# Remove the service
Write-Host "Removing service..." -ForegroundColor Yellow
sc.exe delete $serviceName

Start-Sleep -Seconds 2

# Verify removal
if (Get-Service $serviceName -ErrorAction SilentlyContinue) {
    Write-Host "ERROR: Service still exists!" -ForegroundColor Red
    exit 1
} else {
    Write-Host "Service removed successfully!" -ForegroundColor Green
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Service Uninstallation Complete!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

