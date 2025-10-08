# Lemon Co API - Backup Script
# Backs up SQLite database, configuration, and SQL Server database

param(
    [string]$BackupPath = "C:\LemonCo\Backups",
    [string]$AutoCountDB = "YOUR_AUTOCOUNT_DATABASE_NAME",
    [string]$SQLInstance = "localhost\SQLEXPRESS",
    [int]$RetentionDays = 30
)

$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$backupFolder = Join-Path $BackupPath "backup_$timestamp"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Lemon Co API - Backup Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Backup Time: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor White
Write-Host "Backup Location: $backupFolder" -ForegroundColor White
Write-Host ""

# Create backup directory
try {
    New-Item -Path $backupFolder -ItemType Directory -Force | Out-Null
    Write-Host "[1/4] Created backup directory" -ForegroundColor Green
} catch {
    Write-Host "[1/4] ERROR: Failed to create backup directory: $_" -ForegroundColor Red
    exit 1
}

# Backup SQLite database
try {
    $sqliteSource = "C:\LemonCo\API\lemonco.db"
    if (Test-Path $sqliteSource) {
        Copy-Item $sqliteSource $backupFolder -Force
        Write-Host "[2/4] Backed up SQLite database (lemonco.db)" -ForegroundColor Green
    } else {
        Write-Host "[2/4] WARNING: SQLite database not found at $sqliteSource" -ForegroundColor Yellow
    }
} catch {
    Write-Host "[2/4] ERROR: Failed to backup SQLite database: $_" -ForegroundColor Red
}

# Backup configuration files
try {
    $configSource = "C:\LemonCo\API\appsettings.json"
    if (Test-Path $configSource) {
        Copy-Item $configSource $backupFolder -Force
        Write-Host "[3/4] Backed up configuration (appsettings.json)" -ForegroundColor Green
    } else {
        Write-Host "[3/4] WARNING: Configuration file not found at $configSource" -ForegroundColor Yellow
    }
} catch {
    Write-Host "[3/4] ERROR: Failed to backup configuration: $_" -ForegroundColor Red
}

# Backup SQL Server database (AutoCount)
try {
    if ($AutoCountDB -ne "YOUR_AUTOCOUNT_DATABASE_NAME") {
        $sqlBackupPath = Join-Path $backupFolder "autocount_$timestamp.bak"
        $query = "BACKUP DATABASE [$AutoCountDB] TO DISK='$sqlBackupPath' WITH FORMAT, COMPRESSION"
        
        sqlcmd -S $SQLInstance -Q $query -b
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "[4/4] Backed up SQL Server database ($AutoCountDB)" -ForegroundColor Green
        } else {
            Write-Host "[4/4] ERROR: SQL Server backup failed with exit code $LASTEXITCODE" -ForegroundColor Red
        }
    } else {
        Write-Host "[4/4] SKIPPED: AutoCount database name not configured" -ForegroundColor Yellow
        Write-Host "      Update the script with your AutoCount database name" -ForegroundColor Yellow
    }
} catch {
    Write-Host "[4/4] ERROR: Failed to backup SQL Server database: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Backup Complete!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Calculate backup size
$backupSize = (Get-ChildItem $backupFolder -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host "Backup Size: $([math]::Round($backupSize, 2)) MB" -ForegroundColor White
Write-Host "Backup Location: $backupFolder" -ForegroundColor White
Write-Host ""

# Clean up old backups
Write-Host "Cleaning up old backups (older than $RetentionDays days)..." -ForegroundColor Yellow
$cutoffDate = (Get-Date).AddDays(-$RetentionDays)
$oldBackups = Get-ChildItem $BackupPath -Directory | Where-Object { $_.Name -like "backup_*" -and $_.CreationTime -lt $cutoffDate }

if ($oldBackups) {
    foreach ($oldBackup in $oldBackups) {
        try {
            Remove-Item $oldBackup.FullName -Recurse -Force
            Write-Host "  Removed: $($oldBackup.Name)" -ForegroundColor Gray
        } catch {
            Write-Host "  ERROR: Could not remove $($oldBackup.Name): $_" -ForegroundColor Red
        }
    }
    Write-Host "Cleaned up $($oldBackups.Count) old backup(s)" -ForegroundColor Green
} else {
    Write-Host "No old backups to clean up" -ForegroundColor Gray
}

Write-Host ""
Write-Host "Backup completed successfully!" -ForegroundColor Green
Write-Host ""

