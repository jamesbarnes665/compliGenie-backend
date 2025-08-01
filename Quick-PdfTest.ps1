# Quick PDF Generation Test
Write-Host "Starting API..." -ForegroundColor Yellow
$apiProcess = Start-Process "dotnet" -ArgumentList "run --project .\compligenie-backend.csproj" -PassThru -WindowStyle Hidden

Write-Host "Waiting for API to start (10 seconds)..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

Write-Host "Testing PDF generation..." -ForegroundColor Yellow

try {
    # Create test tenant
    $setupResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/setup/test-tenant" -Method POST -ErrorAction Stop
    Write-Host "✓ Test tenant created" -ForegroundColor Green

    # Test PDF endpoint
    $headers = @{ "X-API-Key" = "test-api-key-123" }
    $policyId = [Guid]::NewGuid()
    $pdfUrl = "http://localhost:5000/api/policies/$policyId/pdf"

    Write-Host "Requesting PDF from: $pdfUrl" -ForegroundColor Yellow
    $response = Invoke-WebRequest -Uri $pdfUrl -Headers $headers -Method GET -ErrorAction Stop
    
    if ($response.StatusCode -eq 200) {
        Write-Host "✓ PDF generation successful!" -ForegroundColor Green
        
        # Create output directory
        New-Item -ItemType Directory -Path ".\test-output" -Force | Out-Null
        
        # Save PDF
        $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
        $pdfPath = ".\test-output\test-policy-$timestamp.pdf"
        [System.IO.File]::WriteAllBytes($pdfPath, $response.Content)
        Write-Host "  PDF saved to: $pdfPath" -ForegroundColor Cyan
        
        # Get file info
        $fileInfo = Get-Item $pdfPath
        Write-Host "  File size: $([math]::Round($fileInfo.Length / 1KB, 2)) KB" -ForegroundColor Cyan
        
        # Open PDF
        Write-Host "`nOpening PDF..." -ForegroundColor Yellow
        Start-Process $pdfPath
    }
}
catch {
    Write-Host "✗ Error: $_" -ForegroundColor Red
    Write-Host "Make sure the API is running on port 5000" -ForegroundColor Yellow
}
finally {
    if ($apiProcess) {
        Write-Host "`nStopping API..." -ForegroundColor Yellow
        Stop-Process -Id $apiProcess.Id -Force -ErrorAction SilentlyContinue
    }
}