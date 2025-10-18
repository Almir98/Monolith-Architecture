@echo off
REM Monolith vs Microservices Architecture Runner
REM Simple batch script that runs Docker Compose commands

setlocal enabledelayedexpansion

if "%1"=="" goto help
if "%1"=="help" goto help
if "%1"=="-h" goto help
if "%1"=="--help" goto help

if "%1"=="monitoring" goto monitoring
if "%1"=="monolith" goto monolith
if "%1"=="microservices" goto microservices
if "%1"=="monolith-full" goto monolith_full
if "%1"=="microservices-full" goto microservices_full
if "%1"=="all" goto all
if "%1"=="stop-all" goto stop_all
if "%1"=="status" goto status

:help
echo Usage: run.bat [command]
echo.
echo Commands:
echo   monitoring        Start monitoring stack only
echo   monolith         Start monolith application only
echo   microservices    Start microservices stack only
echo   monolith-full    Start monitoring + monolith
echo   microservices-full Start monitoring + microservices
echo   all              Start everything (monitoring + monolith + microservices)
echo   stop-all         Stop all services
echo   status           Show current status
echo   help             Show this help
echo.
echo Examples:
echo   run.bat all
echo   run.bat monolith-full
echo   run.bat microservices-full
echo   run.bat stop-all
goto end

:monitoring
echo Starting monitoring stack...
docker-compose -f docker-compose.monitoring.yml up -d
if %errorlevel% equ 0 (
    echo ✅ Monitoring started! Grafana: http://localhost:3000, Prometheus: http://localhost:9090
) else (
    echo ❌ Failed to start monitoring stack
    exit /b 1
)
goto end

:monolith
echo Starting monolith...
docker-compose -f docker-compose.monolith.yml up --build -d
if %errorlevel% equ 0 (
    echo ✅ Monolith started! App: http://localhost:6000
) else (
    echo ❌ Failed to start monolith
    exit /b 1
)
goto end

:microservices
echo Starting microservices...
docker-compose -f docker-compose.microservices.yml up --build -d
if %errorlevel% equ 0 (
    echo ✅ Microservices started! API Gateway: http://localhost:7000
) else (
    echo ❌ Failed to start microservices
    exit /b 1
)
goto end

:monolith_full
echo Starting monitoring + monolith...
docker-compose -f docker-compose.monitoring.yml up -d
if %errorlevel% neq 0 (
    echo ❌ Failed to start monitoring
    exit /b 1
)
timeout /t 3 /nobreak >nul
docker-compose -f docker-compose.monolith.yml up --build -d
if %errorlevel% equ 0 (
    echo ✅ Full monolith stack running! Grafana: http://localhost:3000, App: http://localhost:6000
) else (
    echo ❌ Failed to start monolith
    exit /b 1
)
goto end

:microservices_full
echo Starting monitoring + microservices...
docker-compose -f docker-compose.monitoring.yml up -d
if %errorlevel% neq 0 (
    echo ❌ Failed to start monitoring
    exit /b 1
)
timeout /t 3 /nobreak >nul
docker-compose -f docker-compose.microservices.yml up --build -d
if %errorlevel% equ 0 (
    echo ✅ Full microservices stack running! Grafana: http://localhost:3000, API Gateway: http://localhost:7000
) else (
    echo ❌ Failed to start microservices
    exit /b 1
)
goto end

:all
echo Starting everything (monitoring + monolith + microservices)...
docker-compose -f docker-compose.monitoring.yml up -d
if %errorlevel% neq 0 (
    echo ❌ Failed to start monitoring
    exit /b 1
)
timeout /t 3 /nobreak >nul
docker-compose -f docker-compose.monolith.yml up --build -d
if %errorlevel% neq 0 (
    echo ❌ Failed to start monolith
    exit /b 1
)
timeout /t 3 /nobreak >nul
docker-compose -f docker-compose.microservices.yml up --build -d
if %errorlevel% equ 0 (
    echo ✅ Everything started! Grafana: http://localhost:3000, Monolith: http://localhost:6000, API Gateway: http://localhost:7000
) else (
    echo ❌ Failed to start microservices
    exit /b 1
)
goto end

:stop_all
echo Stopping all services...
docker-compose -f docker-compose.monitoring.yml down
docker-compose -f docker-compose.monolith.yml down
docker-compose -f docker-compose.microservices.yml down
echo ✅ All services stopped!
goto end

:status
echo Current status:
docker-compose -f docker-compose.monitoring.yml ps
docker-compose -f docker-compose.monolith.yml ps
docker-compose -f docker-compose.microservices.yml ps
goto end

:end
