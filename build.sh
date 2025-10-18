#!/bin/bash

echo "Building Travel Portal Backend..."

# Restore packages
echo "Restoring packages..."
dotnet restore

# Build solution
echo "Building solution..."
dotnet build --configuration Release --no-restore

# Run tests
echo "Running tests..."
dotnet test --configuration Release --no-build --verbosity normal

echo "Build completed successfully!"

