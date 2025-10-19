# Simple Monolith vs Microservices Runner
param(
    [string]$Command = "help"
)

Write-Host "`n🚀 Monolith vs Microservices Architecture Runner" -ForegroundColor Cyan
Write-Host "=" * 50 -ForegroundColor Cyan

switch ($Command.ToLower()) {
    "monolith-full" {
        Write-Host "Starting full monolith stack..." -ForegroundColor Yellow
        docker-compose -f docker-compose.monitoring.yml up -d
        Start-Sleep -Seconds 5
        docker-compose -f docker-compose.monolith.yml up --build -d
        Write-Host "✅ Monolith stack started!" -ForegroundColor Green
        Write-Host "Grafana: http://localhost:9000" -ForegroundColor White
        Write-Host "Monolith: http://localhost:8000/swagger" -ForegroundColor White
    }
    "microservices-full" {
        Write-Host "Starting full microservices stack..." -ForegroundColor Yellow
        docker-compose -f docker-compose.monitoring.yml up -d
        Start-Sleep -Seconds 5
        docker-compose -f docker-compose.microservices.yml up --build -d
        Write-Host "✅ Microservices stack started!" -ForegroundColor Green
        Write-Host "Grafana: http://localhost:9000" -ForegroundColor White
        Write-Host "API Gateway: http://localhost:8100/swagger" -ForegroundColor White
    }
    "stop-all" {
        Write-Host "Stopping all services..." -ForegroundColor Yellow
        docker-compose -f docker-compose.monitoring.yml down
        docker-compose -f docker-compose.monolith.yml down
        docker-compose -f docker-compose.microservices.yml down
        Write-Host "✅ All services stopped!" -ForegroundColor Green
    }
    "status" {
        Write-Host "Current status:" -ForegroundColor Yellow
        docker-compose -f docker-compose.monitoring.yml ps
        docker-compose -f docker-compose.monolith.yml ps
        docker-compose -f docker-compose.microservices.yml ps
    }
    default {
        Write-Host "Usage: .\run.ps1 [command]" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Commands:" -ForegroundColor Cyan
        Write-Host "  monolith-full     Start monitoring + monolith"
        Write-Host "  microservices-full Start monitoring + microservices"
        Write-Host "  stop-all         Stop all services"
        Write-Host "  status           Show current status"
        Write-Host ""
        Write-Host "Examples:" -ForegroundColor Green
        Write-Host "  .\run.ps1 monolith-full"
        Write-Host "  .\run.ps1 microservices-full"
        Write-Host "  .\run.ps1 stop-all"
    }
}

Write-Host "`n🎉 Done!" -ForegroundColor Green
