# Lemon Co API - Firewall Configuration Script
# Run as Administrator

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Lemon Co API - Firewall Configuration" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    exit 1
}

# Remove existing rules if they exist
Write-Host "Removing existing firewall rules (if any)..." -ForegroundColor Yellow
Remove-NetFirewallRule -DisplayName "Lemon Co API HTTP" -ErrorAction SilentlyContinue
Remove-NetFirewallRule -DisplayName "Lemon Co API HTTPS" -ErrorAction SilentlyContinue

# Create firewall rule for HTTP (port 5000)
Write-Host "Creating firewall rule for HTTP (port 5000)..." -ForegroundColor Green
try {
    New-NetFirewallRule -DisplayName "Lemon Co API HTTP" `
        -Direction Inbound `
        -LocalPort 5000 `
        -Protocol TCP `
        -Action Allow `
        -Profile Any `
        -Description "Allow inbound HTTP traffic for Lemon Co Production API" `
        -ErrorAction Stop
    Write-Host "  HTTP rule created successfully!" -ForegroundColor Green
} catch {
    Write-Host "  ERROR: Failed to create HTTP rule: $_" -ForegroundColor Red
}

# Create firewall rule for HTTPS (port 5001)
Write-Host "Creating firewall rule for HTTPS (port 5001)..." -ForegroundColor Green
try {
    New-NetFirewallRule -DisplayName "Lemon Co API HTTPS" `
        -Direction Inbound `
        -LocalPort 5001 `
        -Protocol TCP `
        -Action Allow `
        -Profile Any `
        -Description "Allow inbound HTTPS traffic for Lemon Co Production API" `
        -ErrorAction Stop
    Write-Host "  HTTPS rule created successfully!" -ForegroundColor Green
} catch {
    Write-Host "  ERROR: Failed to create HTTPS rule: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Firewall Configuration Complete!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Display created rules
Write-Host "Created Firewall Rules:" -ForegroundColor White
Get-NetFirewallRule -DisplayName "Lemon Co API*" | Format-Table DisplayName, Enabled, Direction, Action, Profile

Write-Host ""
Write-Host "Testing port accessibility..." -ForegroundColor Yellow

# Test if ports are listening
$port5000 = Test-NetConnection -ComputerName localhost -Port 5000 -InformationLevel Quiet -WarningAction SilentlyContinue
$port5001 = Test-NetConnection -ComputerName localhost -Port 5001 -InformationLevel Quiet -WarningAction SilentlyContinue

if ($port5000) {
    Write-Host "  Port 5000 (HTTP): LISTENING" -ForegroundColor Green
} else {
    Write-Host "  Port 5000 (HTTP): NOT LISTENING (API may not be running)" -ForegroundColor Yellow
}

if ($port5001) {
    Write-Host "  Port 5001 (HTTPS): LISTENING" -ForegroundColor Green
} else {
    Write-Host "  Port 5001 (HTTPS): NOT LISTENING (HTTPS not configured yet)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "  1. Ensure the API service is running" -ForegroundColor White
Write-Host "  2. Configure port forwarding on your router" -ForegroundColor White
Write-Host "  3. Test external access from outside your network" -ForegroundColor White
Write-Host ""

