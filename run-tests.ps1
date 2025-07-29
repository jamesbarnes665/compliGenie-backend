# CompliGenie Test Runner
param(
    [string]$Filter = "",
    [switch]$RunApi
)

Write-Host "CompliGenie Test Runner" -ForegroundColor Cyan
Write-Host "======================" -ForegroundColor Cyan

if ($RunApi) {
    Write-Host "`nStarting API in background..." -ForegroundColor Yellow
    Start-Process "dotnet" -ArgumentList "run", "--project", "compligenie-backend.csproj" -WorkingDirectory $PSScriptRoot
    Write-Host "Waiting for API to start..." -ForegroundColor Yellow
    Start-Sleep -Seconds 5
}

Write-Host "`nRunning tests..." -ForegroundColor Yellow
cd "$PSScriptRoot\tests\e2e"

if ($Filter) {
    dotnet test --filter $Filter
} else {
    dotnet test
}

if ($RunApi) {
    Write-Host "`nPress any key to stop the API..." -ForegroundColor Yellow
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    Get-Process "dotnet" | Where-Object { $_.MainWindowTitle -like "*compligenie*" } | Stop-Process
}