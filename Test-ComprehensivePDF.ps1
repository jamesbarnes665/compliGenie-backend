# Comprehensive PDF Test
Write-Host "Starting API for comprehensive PDF test..." -ForegroundColor Yellow
$api = Start-Process "dotnet" -ArgumentList "run --project .\compligenie-backend.csproj" -PassThru -WindowStyle Hidden

Write-Host "Waiting for API to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 8

try {
    Write-Host "Generating comprehensive policy PDF..." -ForegroundColor Yellow
    $headers = @{ "X-API-Key" = "test-api-key-123" }
    $pdfPath = ".\comprehensive-policy-$(Get-Date -Format 'yyyyMMdd-HHmmss').pdf"
    
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/policies/$([Guid]::NewGuid())/pdf" `
        -Headers $headers -OutFile $pdfPath -PassThru
    
    if (Test-Path $pdfPath) {
        $fileInfo = Get-Item $pdfPath
        Write-Host "✓ PDF generated successfully!" -ForegroundColor Green
        Write-Host "  File: $pdfPath" -ForegroundColor Cyan
        Write-Host "  Size: $([math]::Round($fileInfo.Length / 1KB, 2)) KB" -ForegroundColor Cyan
        Write-Host "`nOpening PDF..." -ForegroundColor Yellow
        Start-Process $pdfPath
    }
}
catch {
    Write-Host "✗ Error generating PDF: $_" -ForegroundColor Red
}
finally {
    Write-Host "`nPress any key to stop the API..." -ForegroundColor Yellow
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    Stop-Process -Id $api.Id -Force -ErrorAction SilentlyContinue
}