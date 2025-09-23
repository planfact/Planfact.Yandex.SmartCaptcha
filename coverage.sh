#!/bin/bash

# Script for local code coverage report generation for Yandex.SmartCaptcha

set -e  # Stop execution on error

echo "🧪 Running Yandex.SmartCaptcha tests with coverage generation..."

# Clean previous results
rm -rf ./TestResults ./CoverageReport

# Run tests for the entire solution with coverage
dotnet test Yandex.SmartCaptcha.sln \
    --configuration Release \
    --collect:"XPlat Code Coverage" \
    --results-directory ./TestResults \
    --verbosity minimal

echo "📊 Generating HTML report..."

# Install report generation tool (if not installed)
dotnet tool restore > /dev/null 2>&1
if ! command -v reportgenerator &> /dev/null; then
    echo "🔧 Installing reportgenerator..."
    dotnet tool install --global dotnet-reportgenerator-globaltool
fi

# Collect all coverage files
COVERAGE_FILES=$(find ./TestResults -name "coverage.cobertura.xml" -type f)

if [ -z "$COVERAGE_FILES" ]; then
    echo "❌ Coverage files not found!"
    echo "Check that tests executed successfully and coverlet.collector is installed."
    exit 1
fi

echo "📁 Found coverage files:"
echo "$COVERAGE_FILES" | sed 's/^/  - /'

# Create string with all coverage files for reportgenerator
COVERAGE_REPORTS=$(echo "$COVERAGE_FILES" | tr '\n' ';' | sed 's/;$//')

# Generate HTML report with all projects combined
reportgenerator \
    -reports:"$COVERAGE_REPORTS" \
    -targetdir:"./CoverageReport" \
    -reporttypes:"Html;TextSummary" \
    -assemblyfilters:"+Yandex.SmartCaptcha*;-*.Tests*" \
    -classfilters:"-*.Tests*"

echo ""
echo "✅ HTML report created at ./CoverageReport/index.html"
echo "🌐 Open the file in browser to view:"
echo "   file://$(pwd)/CoverageReport/index.html"

# Show brief statistics
if [ -f "./CoverageReport/Summary.txt" ]; then
    echo ""
    echo "📈 Brief coverage statistics:"
    echo "======================================="
    cat ./CoverageReport/Summary.txt
    echo "======================================="
else
    echo "⚠️  Could not retrieve brief statistics"
fi

echo ""
echo "🎯 Tip: Add ./TestResults and ./CoverageReport to .gitignore"
