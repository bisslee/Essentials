param(
    [string]$Configuration = "Release",
    [switch]$SkipTests,
    [switch]$SkipPack,
    [string]$Version = "1.0.0"
)

Write-Host "Building Biss Essentials v$Version" -ForegroundColor Green

# Clean
Write-Host "Cleaning..." -ForegroundColor Yellow
dotnet clean --configuration $Configuration

# Restore
Write-Host "Restoring packages..." -ForegroundColor Yellow
dotnet restore

# Build
Write-Host "Building..." -ForegroundColor Yellow
dotnet build --configuration $Configuration --no-restore

if (-not $SkipTests) {
    # Test
    Write-Host "Running tests..." -ForegroundColor Yellow
    dotnet test --configuration $Configuration --no-build --verbosity normal
}

if (-not $SkipPack) {
    # Pack
    Write-Host "Packing..." -ForegroundColor Yellow
    dotnet pack --configuration $Configuration --no-build --output ./artifacts
}

Write-Host "Build completed successfully!" -ForegroundColor Green
