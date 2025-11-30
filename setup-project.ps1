# Setup Script for __PROJECT_NAME__ Template
# This script replaces all placeholders with your actual project name

param(
    [Parameter(Mandatory=$true)]
    [string]$ProjectName
)

# Colors for output
function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

# Validate project name
if ($ProjectName -notmatch '^[a-zA-Z][a-zA-Z0-9_]*$') {
    Write-Error "Invalid project name! Use only letters, numbers, and underscores. Must start with a letter."
    exit 1
}

Write-Info "Setting up project: $ProjectName"
Write-Info "Current directory: $(Get-Location)"

# Create backup
Write-Info "Creating backup..."
$backupDir = "backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
Copy-Item -Path .\* -Destination $backupDir -Recurse -Force -ErrorAction SilentlyContinue
Write-Info "Backup created at: $backupDir"

# Function to replace in file
function Replace-InFile {
    param(
        [string]$FilePath
    )
    
    if (Test-Path $FilePath) {
        $content = Get-Content $FilePath -Raw
        $content = $content -replace '__PROJECT_NAME__', $ProjectName
        $content = $content -replace 'Api___PROJECT_NAME__', "Api_$ProjectName"
        Set-Content -Path $FilePath -Value $content -NoNewline
        Write-Info "✓ Updated: $FilePath"
    }
}

# Replace placeholders in all relevant files
Write-Info "Replacing placeholders in files..."

# Configuration files
Replace-InFile "appsettings.json"
Replace-InFile "Api-__PROJECT_NAME__\appsettings.json"
Replace-InFile "Api-__PROJECT_NAME__\appsettings.Development.json"
Replace-InFile "Api-__PROJECT_NAME__\appsettings.Production.json"
Replace-InFile "Api-__PROJECT_NAME__\appsettings.qa.json"

# Documentation files
Replace-InFile "README.md"
Replace-InFile "TEMPLATE_SETUP.md"
Replace-InFile "ARCHITECTURE.md"

# C# files
Get-ChildItem -Path . -Filter *.cs -Recurse | ForEach-Object {
    Replace-InFile $_.FullName
}

# Project files
Get-ChildItem -Path . -Filter *.csproj -Recurse | ForEach-Object {
    Replace-InFile $_.FullName
}

# Solution file
Replace-InFile "__PROJECT_NAME__.sln"

# package.json files
Replace-InFile "package.json"
Replace-InFile "package-lock.json"

# Rename directories
Write-Info "Renaming project directories..."

if (Test-Path "Api-__PROJECT_NAME__") {
    Rename-Item -Path "Api-__PROJECT_NAME__" -NewName "Api-$ProjectName"
    Write-Info "✓ Renamed: Api-__PROJECT_NAME__ → Api-$ProjectName"
}

# Update solution file to reflect directory rename
if (Test-Path "__PROJECT_NAME__.sln") {
    $slnContent = Get-Content "__PROJECT_NAME__.sln" -Raw
    $slnContent = $slnContent -replace 'Api-__PROJECT_NAME__', "Api-$ProjectName"
    $slnContent = $slnContent -replace '__PROJECT_NAME__', $ProjectName
    Set-Content -Path "__PROJECT_NAME__.sln" -Value $slnContent -NoNewline
    Rename-Item -Path "__PROJECT_NAME__.sln" -NewName "$ProjectName.sln"
    Write-Info "✓ Renamed: __PROJECT_NAME__.sln → $ProjectName.sln"
}

# Update namespace references in csproj files
Get-ChildItem -Path . -Filter *.csproj -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $content = $content -replace '<RootNamespace>Api___PROJECT_NAME__</RootNamespace>', "<RootNamespace>Api_$ProjectName</RootNamespace>"
    Set-Content -Path $_.FullName -Value $content -NoNewline
}

Write-Info ""
Write-Info "================================================"
Write-Info "Project setup completed successfully!"
Write-Info "================================================"
Write-Info ""
Write-Warning "Next steps:"
Write-Host "  1. Update connection strings in appsettings.json"
Write-Host "  2. Configure JWT secret (minimum 32 characters)"
Write-Host "  3. Set up email/SMS providers (if needed)"
Write-Host "  4. Run: cd Api-$ProjectName; dotnet restore"
Write-Host "  5. Create initial migration: dotnet ef migrations add InitialCreate --project ..\CC.Infrastructure"
Write-Host "  6. Update database: dotnet ef database update --project ..\CC.Infrastructure"
Write-Host "  7. Run the application: dotnet run"
Write-Host ""
Write-Info "Backup saved at: $backupDir"
Write-Info "Review TEMPLATE_SETUP.md for detailed configuration instructions"
Write-Host ""
