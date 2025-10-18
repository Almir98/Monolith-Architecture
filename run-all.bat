@echo off
REM Monolith vs Microservices Architecture Runner
REM Simple batch file for Windows users

if "%1"=="" goto help
if "%1"=="help" goto help
if "%1"=="monitoring" goto monitoring
if "%1"=="monolith" goto monolith
if "%1"=="microservices" goto microservices
if "%1"=="monolith-full" goto monolith-full
if "%1"=="microservices-full" goto microservices-full
if "%1"=="stop-all" goto stop-all
if "%1"=="status" goto status
goto help

:monitoring
echo.
echo 🚀 Starting Monitoring Stack
echo ==========================================
echo Starting Prometheus + Grafana...
docker-compose -f docker-compose.monitoring.yml up -d
if %errorlevel% equ 0 (
    echo ✅ Monitoring stack started successfully!
    echo ℹ️  Grafana: http://localhost:3000 (admin/admin)
    echo ℹ️  Prometheus: http://localhost:9090
) else (
    echo ❌ Failed to start monitoring stack
    exit /b 1
)
goto end

:monolith
echo.
echo 🚀 Starting Monolith Application
echo ==========================================
echo Starting monolith with monitoring...
docker-compose -f docker-compose.monolith.yml up --build -d
if %errorlevel% equ 0 (
    echo ✅ Monolith started successfully!
    echo ℹ️  Monolith App: http://localhost:6000
    echo ℹ️  Swagger UI: http://localhost:6000/swagger
    echo ℹ️  Health Check: http://localhost:6000/health/ping
    echo ℹ️  Metrics: http://localhost:6000/metrics
) else (
    echo ❌ Failed to start monolith
    exit /b 1
)
goto end

:microservices
echo.
echo 🚀 Starting Microservices Stack
echo ==========================================
echo Starting microservices with monitoring...
docker-compose -f docker-compose.microservices.yml up --build -d
if %errorlevel% equ 0 (
    echo ✅ Microservices started successfully!
    echo ℹ️  API Gateway: http://localhost:7000
    echo ℹ️  Health Service: http://localhost:7001
    echo ℹ️  Order Service: http://localhost:7002
    echo ℹ️  Compute Service: http://localhost:7003
    echo ℹ️  Bulk Service: http://localhost:7004
    echo ℹ️  Swagger UI: http://localhost:7000/swagger
) else (
    echo ❌ Failed to start microservices
    exit /b 1
)
goto end

:monolith-full
echo.
echo 🚀 Starting Full Monolith Stack
echo ==========================================
echo Starting monitoring + monolith...
call :monitoring
timeout /t 5 /nobreak >nul
call :monolith
echo ✅ Full monolith stack is running!
echo ℹ️  Access Grafana: http://localhost:3000
echo ℹ️  Access Monolith: http://localhost:6000/swagger
goto end

:microservices-full
echo.
echo 🚀 Starting Full Microservices Stack
echo ==========================================
echo Starting monitoring + microservices...
call :monitoring
timeout /t 5 /nobreak >nul
call :microservices
echo ✅ Full microservices stack is running!
echo ℹ️  Access Grafana: http://localhost:3000
echo ℹ️  Access API Gateway: http://localhost:7000/swagger
goto end

:stop-all
echo.
echo 🚀 Stopping All Services
echo ==========================================
echo Stopping all Docker containers...
docker-compose -f docker-compose.monitoring.yml down
docker-compose -f docker-compose.monolith.yml down
docker-compose -f docker-compose.microservices.yml down
echo ✅ All services stopped!
goto end

:status
echo.
echo 🚀 Current Status
echo ==========================================
echo Checking running containers...
docker-compose -f docker-compose.monitoring.yml ps
docker-compose -f docker-compose.monolith.yml ps
docker-compose -f docker-compose.microservices.yml ps
echo.
echo ℹ️  Port Usage:
echo   Monitoring:
echo     Grafana: http://localhost:3000
echo     Prometheus: http://localhost:9090
echo   Monolith:
echo     App: http://localhost:6000
echo     Swagger: http://localhost:6000/swagger
echo   Microservices:
echo     API Gateway: http://localhost:7000
echo     Health Service: http://localhost:7001
echo     Order Service: http://localhost:7002
echo     Compute Service: http://localhost:7003
echo     Bulk Service: http://localhost:7004
goto end

:help
echo.
echo 🚀 Monolith vs Microservices Architecture Runner
echo ==========================================
echo Usage: run-all.bat [command]
echo.
echo Commands:
echo   monitoring        Start monitoring stack only (Prometheus + Grafana)
echo   monolith         Start monolith application only
echo   microservices    Start microservices stack only
echo   monolith-full    Start monitoring + monolith (recommended for monolith testing)
echo   microservices-full Start monitoring + microservices (recommended for microservices testing)
echo   stop-all         Stop all services
echo   status           Show current status (default)
echo.
echo Examples:
echo   run-all.bat monolith-full
echo   run-all.bat microservices-full
echo   run-all.bat stop-all
echo.
echo Load Testing:
echo   cd shared/load-testing
echo   k6 run k6-script.js                    # Test monolith
echo   k6 run k6-microservices-script.js     # Test microservices
goto end

:end
echo.
echo 🎉 Done!
