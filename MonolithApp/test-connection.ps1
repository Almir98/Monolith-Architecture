# Quick test script to verify application is running
Write-Host "Testing MonolithApp connections..." -ForegroundColor Cyan

# Test main application
Write-Host "`n1. Testing application (port 62552 HTTPS)..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "https://localhost:62552/health/ping" -TimeoutSec 5 -SkipCertificateCheck
    if ($response.StatusCode -eq 200) {
        Write-Host "   ✓ Application is running!" -ForegroundColor Green
        Write-Host "   Response: $($response.Content)" -ForegroundColor Gray
    }
} catch {
    Write-Host "   ✗ Application is NOT running!" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test Prometheus
Write-Host "`n2. Testing Prometheus (port 9090)..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:9090/-/healthy" -TimeoutSec 5
    if ($response.StatusCode -eq 200) {
        Write-Host "   ✓ Prometheus is running!" -ForegroundColor Green
    }
} catch {
    Write-Host "   ✗ Prometheus is NOT running!" -ForegroundColor Red
    Write-Host "   Run: docker-compose up -d" -ForegroundColor Yellow
}

# Test Grafana
Write-Host "`n3. Testing Grafana (port 3000)..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:3000/api/health" -TimeoutSec 5
    if ($response.StatusCode -eq 200) {
        Write-Host "   ✓ Grafana is running!" -ForegroundColor Green
    }
} catch {
    Write-Host "   ✗ Grafana is NOT running!" -ForegroundColor Red
    Write-Host "   Run: docker-compose up -d" -ForegroundColor Yellow
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Quick Links:" -ForegroundColor Cyan
Write-Host "  Application: https://localhost:62552/swagger" -ForegroundColor White
Write-Host "  Metrics:     https://localhost:62552/metrics" -ForegroundColor White
Write-Host "  Prometheus:  http://localhost:9090" -ForegroundColor White
Write-Host "  Grafana:     http://localhost:3000" -ForegroundColor White
Write-Host "========================================`n" -ForegroundColor Cyan

