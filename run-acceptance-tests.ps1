Write-Host "Running Feature 1.1 Acceptance Tests" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan

# Story 1.1.1 Tests
Write-Host "`nStory 1.1.1: Multi-Tenant Database" -ForegroundColor Yellow
dotnet test --filter "FullyQualifiedName~Story_1_1_1" --logger "console;verbosity=normal"

# Story 1.1.2 Tests  
Write-Host "`nStory 1.1.2: Tenant Context Middleware" -ForegroundColor Yellow
dotnet test --filter "FullyQualifiedName~Story_1_1_2" --logger "console;verbosity=normal"

# Story 1.1.3 Tests
Write-Host "`nStory 1.1.3: Partner Self-Service Registration" -ForegroundColor Yellow
dotnet test --filter "FullyQualifiedName~Story_1_1_3" --logger "console;verbosity=normal"

# Run all acceptance tests
Write-Host "`nRunning ALL Acceptance Tests" -ForegroundColor Green
dotnet test --filter "FullyQualifiedName~AcceptanceTests" --logger "console;verbosity=detailed"
