# CompliGenie Quick Start
Write-Host "CompliGenie Development Environment" -ForegroundColor Cyan
Write-Host "===================================" -ForegroundColor Cyan

$action = Read-Host "`nWhat would you like to do? (api/test/both)"

switch ($action.ToLower()) {
    "api" {
        Write-Host "`nStarting API..." -ForegroundColor Yellow
        dotnet run --project compligenie-backend.csproj
    }
    "test" {
        Write-Host "`nRunning tests..." -ForegroundColor Yellow
        cd tests\e2e
        dotnet test
        cd ..\..
    }
    "both" {
        Write-Host "`nStarting API in background..." -ForegroundColor Yellow
        Start-Process "dotnet" -ArgumentList "run --project compligenie-backend.csproj" -WindowStyle Minimized
        Start-Sleep -Seconds 5
        
        Write-Host "Running tests..." -ForegroundColor Yellow
        cd tests\e2e
        dotnet test
        cd ..\..
        
        Write-Host "`nAPI is running in background. Press any key to stop it..." -ForegroundColor Yellow
        $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
        Get-Process "dotnet" | Stop-Process
    }
    default {
        Write-Host "Invalid option. Use 'api', 'test', or 'both'" -ForegroundColor Red
    }
}