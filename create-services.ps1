# PowerShell script to create service structure
# This is a reference script - the actual services are created below

$services = @(
    "CatalogService",
    "BookingService",
    "PaymentService",
    "NotificationService",
    "ReportingService"
)

foreach ($service in $services) {
    Write-Host "Creating $service structure..."
    
    # Create directories
    $layers = @("Domain", "Contracts", "Application", "Infrastructure", "API")
    foreach ($layer in $layers) {
        $path = "src/Services/$service/$service.$layer"
        New-Item -ItemType Directory -Force -Path $path
    }
}

Write-Host "Service structure created successfully!"

