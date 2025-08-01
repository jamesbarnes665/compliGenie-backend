# Policy Generation Integration Test
param(
    [string]$ApiUrl = "http://localhost:5000",
    [string]$ApiKey = "test-api-key-123"
)

Write-Host "Testing Policy Generation API..." -ForegroundColor Cyan

# Test request
$request = @{
    clientName = "Test Law Firm LLP"
    industry = "legal"
    companySize = 50
    aiTools = @("ChatGPT", "Claude", "CaseText")
    jurisdictions = @("California", "New York")
    complianceFrameworks = @("SOC2", "ISO27001")
} | ConvertTo-Json

$headers = @{
    "X-API-Key" = $ApiKey
    "Content-Type" = "application/json"
}

try {
    $response = Invoke-RestMethod -Uri "$ApiUrl/api/policies/generate" `
        -Method POST -Body $request -Headers $headers
    
    Write-Host "✓ Policy generation successful!" -ForegroundColor Green
    Write-Host "  Policy ID: $($response.policyId)"
    Write-Host "  Page Count: $($response.pageCount)"
    Write-Host "  Sections: $($response.sections)"
    Write-Host "  Duration: $($response.duration)s"
    
    if ($response.pageCount -ge 8 -and $response.pageCount -le 12) {
        Write-Host "✓ Page count within acceptable range (8-12)" -ForegroundColor Green
    }
    
    if ($response.duration -lt 10) {
        Write-Host "✓ Generation completed within time limit (<10s)" -ForegroundColor Green
    }
}
catch {
    Write-Host "✗ Policy generation failed: $_" -ForegroundColor Red
}
