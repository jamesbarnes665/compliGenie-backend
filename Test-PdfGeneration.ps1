# PDF Generation Test Script
param(
    [string]$ApiUrl = "http://localhost:5000",
    [string]$ApiKey = "test-api-key-123"
)

Write-Host "Testing PDF Generation..." -ForegroundColor Cyan

# First ensure we have a test tenant
Write-Host "Setting up test tenant..." -ForegroundColor Yellow
$setupResponse = Invoke-RestMethod -Uri "$ApiUrl/api/setup/test-tenant" -Method POST
Write-Host "✓ Test tenant ready" -ForegroundColor Green

# Generate a test policy first
Write-Host "Generating test policy..." -ForegroundColor Yellow
$policyRequest = @{
    clientName = "PDF Test Company LLC"
    industry = "legal"
    companySize = 50
    aiTools = @("ChatGPT", "Claude")
    jurisdictions = @("California", "New York")
} | ConvertTo-Json

$headers = @{
    "X-API-Key" = $ApiKey
    "Content-Type" = "application/json"
}

try {
    # Generate policy
    $policyResponse = Invoke-RestMethod -Uri "$ApiUrl/api/policies/generate" `
        -Method POST -Body $policyRequest -Headers $headers
    
    Write-Host "✓ Policy generated: $($policyResponse.policyId)" -ForegroundColor Green
    
    # Generate PDF
    Write-Host "Generating PDF..." -ForegroundColor Yellow
    $pdfUrl = "$ApiUrl/api/policies/$($policyResponse.policyId)/pdf"
    
    $pdfPath = ".\test-output\AI_Policy_$(Get-Date -Format 'yyyyMMdd_HHmmss').pdf"
    New-Item -ItemType Directory -Path ".\test-output" -Force | Out-Null
    
    Invoke-WebRequest -Uri $pdfUrl -Headers @{"X-API-Key" = $ApiKey} -OutFile $pdfPath
    
    Write-Host "✓ PDF generated successfully!" -ForegroundColor Green
    Write-Host "  File saved to: $pdfPath" -ForegroundColor Cyan
    
    # Open PDF for review
    if (Test-Path $pdfPath) {
        $fileInfo = Get-Item $pdfPath
        Write-Host "  File size: $([math]::Round($fileInfo.Length / 1KB, 2)) KB" -ForegroundColor Cyan
        
        $openPdf = Read-Host "Open PDF for review? (Y/N)"
        if ($openPdf -eq 'Y') {
            Start-Process $pdfPath
        }
    }
}
catch {
    Write-Host "✗ PDF generation failed: $_" -ForegroundColor Red
}