# PowerShell-Skript zum Beenden aller hängenden Test-Prozesse
# Verwendung: .\kill-test-processes.ps1

Write-Host "Suche nach hängenden Test-Prozessen..." -ForegroundColor Yellow

# Finde alle testhost Prozesse
$testhostProcesses = Get-Process -Name "testhost" -ErrorAction SilentlyContinue

if ($testhostProcesses) {
    Write-Host "Gefunden: $($testhostProcesses.Count) testhost Prozess(e)" -ForegroundColor Cyan
    $testhostProcesses | ForEach-Object {
        Write-Host "  Beende testhost (PID: $($_.Id), RAM: $([math]::Round($_.WorkingSet64/1MB, 2)) MB)" -ForegroundColor Gray
        Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
    }
    Write-Host "✓ Alle testhost Prozesse beendet" -ForegroundColor Green
} else {
    Write-Host "Keine testhost Prozesse gefunden" -ForegroundColor Green
}

# Finde alle dotnet Prozesse (außer dem aktuellen)
$dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object { $_.Id -ne $PID }

if ($dotnetProcesses) {
    Write-Host "Gefunden: $($dotnetProcesses.Count) dotnet Prozess(e)" -ForegroundColor Cyan
    $dotnetProcesses | ForEach-Object {
        Write-Host "  Beende dotnet (PID: $($_.Id), RAM: $([math]::Round($_.WorkingSet64/1MB, 2)) MB)" -ForegroundColor Gray
        Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
    }
    Write-Host "✓ Alle dotnet Prozesse beendet" -ForegroundColor Green
} else {
    Write-Host "Keine dotnet Prozesse gefunden" -ForegroundColor Green
}

Write-Host ""
Write-Host "Fertig! Alle Test-Prozesse wurden beendet." -ForegroundColor Green
Write-Host ""
Write-Host "Tipp: Verwenden Sie 'dotnet test --settings .runsettings' um das Problem zu vermeiden." -ForegroundColor Yellow
