# Monolith vs Microservices Architecture Runner
# This script helps you run different combinations of the architecture

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("monitoring", "monolith", "microservices", "monolith-full", "microservices-full", "stop-all", "status")]
    [string]$Command = "status"
)

function Write-Header {
    param([string]$Message)
    Write-Host "`n🚀 $Message" -ForegroundColor Cyan
    Write-Host "=" * 50 -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️  $Message" -ForegroundColor Yellow
}

function Start-Monitoring {
    Write-Header "Starting Monitoring Stack"
    Write-Info "Starting Prometheus + Grafana..."
    
    docker-compose -f docker-compose.monitoring.yml up -d
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Monitoring stack started successfully!"
        Write-Info "Grafana: http://localhost:3000 (admin/admin)"
        Write-Info "Prometheus: http://localhost:9090"
    } else {
        Write-Error "Failed to start monitoring stack"
        exit 1
    }
}

function Start-Monolith {
    Write-Header "Starting Monolith Application"
    Write-Info "Starting monolith with monitoring..."
    
    docker-compose -f docker-compose.monolith.yml up --build -d
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Monolith started successfully!"
        Write-Info "Monolith App: http://localhost:6000"
        Write-Info "Swagger UI: http://localhost:6000/swagger"
        Write-Info "Health Check: http://localhost:6000/health/ping"
        Write-Info "Metrics: http://localhost:6000/metrics"
    } else {
        Write-Error "Failed to start monolith"
        exit 1
    }
}

function Start-Microservices {
    Write-Header "Starting Microservices Stack"
    Write-Info "Starting microservices with monitoring..."
    
    docker-compose -f docker-compose.microservices.yml up --build -d
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Microservices started successfully!"
        Write-Info "API Gateway: http://localhost:7000"
        Write-Info "Health Service: http://localhost:7001"
        Write-Info "Order Service: http://localhost:7002"
        Write-Info "Compute Service: http://localhost:7003"
        Write-Info "Bulk Service: http://localhost:7004"
        Write-Info "Swagger UI: http://localhost:7000/swagger"
    } else {
        Write-Error "Failed to start microservices"
        exit 1
    }
}

function Start-Monolith-Full {
    Write-Header "Starting Full Monolith Stack"
    Write-Info "Starting monitoring + monolith..."
    
    Start-Monitoring
    Start-Sleep -Seconds 5
    Start-Monolith
    
    Write-Success "Full monolith stack is running!"
    Write-Info "Access Grafana: http://localhost:3000"
    Write-Info "Access Monolith: http://localhost:6000/swagger"
}

function Start-Microservices-Full {
    Write-Header "Starting Full Microservices Stack"
    Write-Info "Starting monitoring + microservices..."
    
    Start-Monitoring
    Start-Sleep -Seconds 5
    Start-Microservices
    
    Write-Success "Full microservices stack is running!"
    Write-Info "Access Grafana: http://localhost:3000"
    Write-Info "Access API Gateway: http://localhost:7000/swagger"
}

function Stop-All {
    Write-Header "Stopping All Services"
    Write-Info "Stopping all Docker containers..."
    
    docker-compose -f docker-compose.monitoring.yml down
    docker-compose -f docker-compose.monolith.yml down
    docker-compose -f docker-compose.microservices.yml down
    
    Write-Success "All services stopped!"
}

function Show-Status {
    Write-Header "Current Status"
    
    Write-Info "Checking running containers..."
    docker-compose -f docker-compose.monitoring.yml ps
    docker-compose -f docker-compose.monolith.yml ps
    docker-compose -f docker-compose.microservices.yml ps
    
    Write-Info "`nPort Usage:"
    Write-Host "  Monitoring:" -ForegroundColor Yellow
    Write-Host "    Grafana: http://localhost:3000" -ForegroundColor White
    Write-Host "    Prometheus: http://localhost:9090" -ForegroundColor White
    Write-Host "  Monolith:" -ForegroundColor Yellow
    Write-Host "    App: http://localhost:6000" -ForegroundColor White
    Write-Host "    Swagger: http://localhost:6000/swagger" -ForegroundColor White
    Write-Host "  Microservices:" -ForegroundColor Yellow
    Write-Host "    API Gateway: http://localhost:7000" -ForegroundColor White
    Write-Host "    Health Service: http://localhost:7001" -ForegroundColor White
    Write-Host "    Order Service: http://localhost:7002" -ForegroundColor White
    Write-Host "    Compute Service: http://localhost:7003" -ForegroundColor White
    Write-Host "    Bulk Service: http://localhost:7004" -ForegroundColor White
}

function Show-Help {
    Write-Header "Monolith vs Microservices Architecture Runner"
    Write-Host "Usage: .\run-all.ps1 [command]"
    Write-Host ""
    Write-Host "Commands:" -ForegroundColor Yellow
    Write-Host "  monitoring        Start monitoring stack only (Prometheus + Grafana)"
    Write-Host "  monolith         Start monolith application only"
    Write-Host "  microservices    Start microservices stack only"
    Write-Host "  monolith-full    Start monitoring + monolith (recommended for monolith testing)"
    Write-Host "  microservices-full Start monitoring + microservices (recommended for microservices testing)"
    Write-Host "  stop-all         Stop all services"
    Write-Host "  status           Show current status (default)"
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Cyan
    Write-Host "  .\run-all.ps1 monolith-full"
    Write-Host "  .\run-all.ps1 microservices-full"
    Write-Host "  .\run-all.ps1 stop-all"
    Write-Host ""
    Write-Host "Load Testing:" -ForegroundColor Magenta
    Write-Host "  cd shared/load-testing"
    Write-Host "  k6 run k6-script.js                    # Test monolith"
    Write-Host "  k6 run k6-microservices-script.js     # Test microservices"
}

# Main execution
switch ($Command) {
    "monitoring" { Start-Monitoring }
    "monolith" { Start-Monolith }
    "microservices" { Start-Microservices }
    "monolith-full" { Start-Monolith-Full }
    "microservices-full" { Start-Microservices-Full }
    "stop-all" { Stop-All }
    "status" { Show-Status }
    default { Show-Help }
}

Write-Host "`n🎉 Done!" -ForegroundColor Green
