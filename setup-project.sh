#!/bin/bash

# Setup Script for __PROJECT_NAME__ Template
# This script replaces all placeholders with your actual project name

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored output
print_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if project name is provided
if [ -z "$1" ]; then
    print_error "Project name is required!"
    echo "Usage: ./setup-project.sh YourProjectName"
    exit 1
fi

PROJECT_NAME=$1

# Validate project name (alphanumeric and underscores only)
if ! [[ "$PROJECT_NAME" =~ ^[a-zA-Z][a-zA-Z0-9_]*$ ]]; then
    print_error "Invalid project name! Use only letters, numbers, and underscores. Must start with a letter."
    exit 1
fi

print_info "Setting up project: $PROJECT_NAME"
print_info "Current directory: $(pwd)"

# Backup original files
print_info "Creating backup..."
BACKUP_DIR="backup_$(date +%Y%m%d_%H%M%S)"
mkdir -p "$BACKUP_DIR"
cp -r . "$BACKUP_DIR/" 2>/dev/null || true
print_info "Backup created at: $BACKUP_DIR"

# Function to replace in file
replace_in_file() {
    local file=$1
    if [ -f "$file" ]; then
        # macOS uses BSD sed, Linux uses GNU sed
        if [[ "$OSTYPE" == "darwin"* ]]; then
            sed -i '' "s/__PROJECT_NAME__/$PROJECT_NAME/g" "$file"
            sed -i '' "s/Api___PROJECT_NAME__/Api_${PROJECT_NAME}/g" "$file"
        else
            sed -i "s/__PROJECT_NAME__/$PROJECT_NAME/g" "$file"
            sed -i "s/Api___PROJECT_NAME__/Api_${PROJECT_NAME}/g" "$file"
        fi
        print_info "✓ Updated: $file"
    fi
}

# Replace placeholders in all relevant files
print_info "Replacing placeholders in files..."

# Configuration files
replace_in_file "appsettings.json"
replace_in_file "Api-__PROJECT_NAME__/appsettings.json"
replace_in_file "Api-__PROJECT_NAME__/appsettings.Development.json"
replace_in_file "Api-__PROJECT_NAME__/appsettings.Production.json"
replace_in_file "Api-__PROJECT_NAME__/appsettings.qa.json"

# Documentation files
replace_in_file "README.md"
replace_in_file "TEMPLATE_SETUP.md"
replace_in_file "ARCHITECTURE.md"

# C# files
find . -name "*.cs" -type f | while read -r file; do
    replace_in_file "$file"
done

# Project files
find . -name "*.csproj" -type f | while read -r file; do
    replace_in_file "$file"
done

# Solution file
replace_in_file "__PROJECT_NAME__.sln"

# package.json and package-lock.json
replace_in_file "package.json"
replace_in_file "package-lock.json"

# Rename directories
print_info "Renaming project directories..."

if [ -d "Api-__PROJECT_NAME__" ]; then
    mv "Api-__PROJECT_NAME__" "Api-${PROJECT_NAME}"
    print_info "✓ Renamed: Api-__PROJECT_NAME__ → Api-${PROJECT_NAME}"
fi

# Update solution file to reflect directory rename
if [ -f "__PROJECT_NAME__.sln" ]; then
    if [[ "$OSTYPE" == "darwin"* ]]; then
        sed -i '' "s/Api-__PROJECT_NAME__/Api-${PROJECT_NAME}/g" "__PROJECT_NAME__.sln"
        sed -i '' "s/__PROJECT_NAME__/${PROJECT_NAME}/g" "__PROJECT_NAME__.sln"
    else
        sed -i "s/Api-__PROJECT_NAME__/Api-${PROJECT_NAME}/g" "__PROJECT_NAME__.sln"
        sed -i "s/__PROJECT_NAME__/${PROJECT_NAME}/g" "__PROJECT_NAME__.sln"
    fi
    mv "__PROJECT_NAME__.sln" "${PROJECT_NAME}.sln"
    print_info "✓ Renamed: __PROJECT_NAME__.sln → ${PROJECT_NAME}.sln"
fi

# Update namespace references in csproj files
find . -name "*.csproj" -type f | while read -r file; do
    if [[ "$OSTYPE" == "darwin"* ]]; then
        sed -i '' "s/<RootNamespace>Api___PROJECT_NAME__<\/RootNamespace>/<RootNamespace>Api_${PROJECT_NAME}<\/RootNamespace>/g" "$file"
    else
        sed -i "s/<RootNamespace>Api___PROJECT_NAME__<\/RootNamespace>/<RootNamespace>Api_${PROJECT_NAME}<\/RootNamespace>/g" "$file"
    fi
done

print_info ""
print_info "================================================"
print_info "Project setup completed successfully!"
print_info "================================================"
print_info ""
print_warning "Next steps:"
echo "  1. Update connection strings in appsettings.json"
echo "  2. Configure JWT secret (minimum 32 characters)"
echo "  3. Set up email/SMS providers (if needed)"
echo "  4. Run: cd Api-${PROJECT_NAME} && dotnet restore"
echo "  5. Create initial migration: dotnet ef migrations add InitialCreate --project ../CC.Infrastructure"
echo "  6. Update database: dotnet ef database update --project ../CC.Infrastructure"
echo "  7. Run the application: dotnet run"
echo ""
print_info "Backup saved at: $BACKUP_DIR"
print_info "Review TEMPLATE_SETUP.md for detailed configuration instructions"
echo ""
