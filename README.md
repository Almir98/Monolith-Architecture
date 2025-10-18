# Architecture Comparison Repository

This repository demonstrates and compares **Monolith** vs **Microservices** architectures using .NET applications with comprehensive monitoring and load testing.

## 🎯 Three Docker Compose Files

### 1. **Shared Monitoring** (`docker-compose.monitoring.yml`)
- **Purpose**: Single monitoring stack for both architectures
- **Use Case**: Run once, monitor everything
- **Access**: Grafana (http://localhost:3000), Prometheus (http://localhost:9090)

### 2. **Monolith** (`docker-compose.monolith.yml`)
- **Purpose**: Monolith application only
- **Use Case**: Testing monolith architecture
- **Access**: App (http://localhost:6000) + shared monitoring

### 3. **Microservices** (`docker-compose.microservices.yml`)
- **Purpose**: Microservices with API Gateway
- **Use Case**: Testing microservices architecture
- **Access**: API Gateway (http://localhost:7000) + individual services + shared monitoring

## 🚀 Quick Start

### **Option 1: Use the Run Scripts (Recommended)**

#### **PowerShell (Windows)**
```powershell
# Start everything (monitoring + monolith + microservices)
.\run.ps1 all

# Start full monolith stack (monitoring + monolith)
.\run.ps1 monolith-full

# Start full microservices stack (monitoring + microservices)
.\run.ps1 microservices-full

# Stop all services
.\run.ps1 stop-all

# Check status
.\run.ps1 status
```

#### **Batch File (Windows)**
```cmd
# Start everything (monitoring + monolith + microservices)
run.bat all

# Start full monolith stack
run.bat monolith-full

# Start full microservices stack
run.bat microservices-full

# Stop all services
run.bat stop-all

# Check status
run.bat status
```

#### **Bash Script (Linux/macOS/WSL)**
```bash
# Make executable (first time only)
chmod +x run.sh

# Start everything (monitoring + monolith + microservices)
./run.sh all

# Start full monolith stack
./run.sh monolith-full

# Start full microservices stack
./run.sh microservices-full

# Stop all services
./run.sh stop-all

# Check status
./run.sh status
```

### **Option 2: Manual Docker Compose**

#### **1. Start Shared Monitoring (Run Once)**
```bash
# Start the shared monitoring stack
docker-compose -f docker-compose.monitoring.yml up -d
```

#### **2. Choose Your Architecture**

##### **Option A: Test Monolith**
```bash
# Start monolith (connects to shared monitoring)
docker-compose -f docker-compose.monolith.yml up --build
```

##### **Option B: Test Microservices**
```bash
# Start microservices (connects to shared monitoring)
docker-compose -f docker-compose.microservices.yml up --build
```

## 📊 Monitoring & Dashboards

### Access Points
- **Grafana**: http://localhost:3000 (admin/admin)
- **Prometheus**: http://localhost:9090

### Available Dashboards
- **ASP.NET Core Dashboard**: Application metrics for both architectures
- **.NET Memory Dashboard**: Memory usage and GC metrics
- **Service-specific metrics**: Individual service performance

### Metrics Collected
- **Monolith**: Single application metrics
- **Microservices**: API Gateway + 5 individual services
- **Shared**: Prometheus collects from all services

## 🧪 Load Testing

### Install k6
```bash
# Windows
choco install k6

# macOS
brew install k6

# Linux
# See https://k6.io/docs/getting-started/installation/
```

### Test Monolith
```bash
# Start monolith first
docker-compose -f docker-compose.monitoring.yml up -d
docker-compose -f docker-compose.monolith.yml up --build

# Run monolith load test
cd shared/load-testing
k6 run k6-script.js
```

### Test Microservices
```bash
# Start microservices first
docker-compose -f docker-compose.monitoring.yml up -d
docker-compose -f docker-compose.microservices.yml up --build

# Run microservices load test
cd shared/load-testing
k6 run k6-microservices-script.js
```

## 🏗️ Architecture Comparison

### Monolith Architecture
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   monolith-app  │    │    prometheus   │    │     grafana     │
│   (Port 6000)   │───▶│   (Port 9090)   │───▶│   (Port 3000)   │
│   .NET 8 API    │    │   Metrics Store  │    │  Visualization  │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

### Microservices Architecture
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   api-gateway   │    │    prometheus   │    │     grafana     │
│   (Port 7000)   │───▶│   (Port 9090)   │───▶│   (Port 3000)   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │
         ▼
┌─────────────────┐
│  health-service │ (Port 7001)
│  order-service  │ (Port 7002)
│ compute-service │ (Port 7003)
│  bulk-service   │ (Port 7004)
└─────────────────┘
```

## 📁 Repository Structure

```
├── apps/
│   ├── monolith/              # Monolith application
│   │   ├── Controllers/       # API controllers
│   │   ├── Services/          # Business logic
│   │   ├── Models/            # Data models
│   │   ├── Data/              # Database context
│   │   ├── Program.cs         # Entry point
│   │   ├── MonolithApp.csproj
│   │   └── Dockerfile
│   └── microservices/         # Microservices (API Gateway + Services)
│       ├── src/
│       │   ├── ApiGateway/     # API Gateway service
│       │   ├── HealthService/  # Health service
│       │   ├── OrderService/   # Order service
│       │   ├── ComputeService/ # Compute service
│       │   ├── BulkService/    # Bulk service
│       │   └── Shared/         # Shared libraries
│       └── MicroservicesArchitecture.sln
├── shared/
│   ├── monitoring/            # Shared monitoring (Prometheus + Grafana)
│   │   ├── grafana/          # Dashboards & configs
│   │   └── prometheus.yml     # Prometheus configuration
│   └── load-testing/          # Shared load testing (k6 scripts)
│       ├── k6-script.js      # Monolith load test
│       ├── k6-microservices-script.js # Microservices load test
│       └── README.md          # Load testing documentation
├── docker-compose.monitoring.yml    # Monitoring only
├── docker-compose.monolith.yml      # Monolith + monitoring
├── docker-compose.microservices.yml # Microservices + monitoring
└── data/                      # SQLite databases
```

## 🔧 Development Workflow

### 1. Start Shared Monitoring (One Time Setup)
```bash
# Start the shared monitoring stack (run once)
docker-compose -f docker-compose.monitoring.yml up -d
```

### 2. Develop & Test Monolith
```bash
# Start monolith (connects to existing monitoring)
docker-compose -f docker-compose.monolith.yml up --build

# Run load tests
cd shared/load-testing
k6 run k6-script.js

# View metrics in Grafana
# http://localhost:3000
```

### 3. Develop & Test Microservices
```bash
# Stop monolith first
docker-compose -f docker-compose.monolith.yml down

# Start microservices (connects to existing monitoring)
docker-compose -f docker-compose.microservices.yml up --build

# Run load tests
cd shared/load-testing
k6 run k6-microservices-script.js

# View metrics in Grafana
# http://localhost:3000
```

### 4. Switch Between Architectures
```bash
# Stop current architecture
docker-compose -f docker-compose.monolith.yml down
# OR
docker-compose -f docker-compose.microservices.yml down

# Start different architecture
docker-compose -f docker-compose.microservices.yml up --build
```

## 🐳 Docker Commands Reference

### Start Services
```bash
# Monitoring only
docker-compose -f docker-compose.monitoring.yml up -d

# Monolith with monitoring
docker-compose -f docker-compose.monolith.yml up --build

# Microservices with monitoring
docker-compose -f docker-compose.microservices.yml up --build
```

### Stop Services
```bash
# Stop specific stack
docker-compose -f docker-compose.monolith.yml down
docker-compose -f docker-compose.microservices.yml down

# Stop monitoring
docker-compose -f docker-compose.monitoring.yml down

# Stop everything
docker-compose -f docker-compose.monitoring.yml -f docker-compose.monolith.yml down
```

### Clean Up
```bash
# Remove all containers and volumes
docker-compose -f docker-compose.monitoring.yml down -v
docker-compose -f docker-compose.monolith.yml down -v
docker-compose -f docker-compose.microservices.yml down -v
```

## 🔍 Troubleshooting

### Common Issues

1. **Port Conflicts**
   - Monolith: 5000, 3000, 9090
   - Microservices: 5000, 5010, 5020, 5030, 5040, 3000, 9090

2. **Network Issues**
   - Ensure monitoring network is created first
   - Check if monitoring stack is running

3. **Metrics Not Appearing**
   - Verify Prometheus can reach services
   - Check service health endpoints

### Debug Commands
```bash
# Check running containers
docker-compose ps

# View logs
docker-compose logs monolith-app
docker-compose logs api-gateway

# Test endpoints
curl http://localhost:6000/health
curl http://localhost:6000/metrics
```

## 📈 Performance Comparison

### Load Testing Results
- **Monolith**: Single application under load
- **Microservices**: Distributed load across services
- **Metrics**: Compare response times, throughput, error rates

### Monitoring Benefits
- **Unified View**: Same Grafana dashboards for both architectures
- **Performance Comparison**: Side-by-side metrics
- **Scalability Analysis**: Resource usage patterns

## 🎯 Next Steps

1. **Start with monitoring**: `docker-compose -f docker-compose.monitoring.yml up -d`
2. **Test monolith**: `docker-compose -f docker-compose.monolith.yml up --build`
3. **Test microservices**: `docker-compose -f docker-compose.microservices.yml up --build`
4. **Compare results**: Use Grafana dashboards to analyze performance
5. **Run load tests**: Use k6 scripts to stress test both architectures

## 🏛️ Architecture Comparison

### Monolith Architecture
- **Single deployable unit**
- **Shared database**
- **Simpler deployment**
- **Easier debugging**
- **Tight coupling**

### Microservices Architecture
- **Multiple deployable units**
- **Database per service**
- **Complex deployment**
- **Distributed debugging**
- **Loose coupling**

## 🔧 Development

### Monolith Development
```bash
cd apps/monolith
dotnet run
```

### Microservices Development
```bash
cd apps/microservices
dotnet run --project src/ApiGateway
```

## 📈 Performance Testing

The k6 load testing scripts test:
- Health check endpoints
- Orders API
- CPU-intensive computation
- Bulk processing
- Order creation

## 🛠️ Troubleshooting

### Common Issues
1. **Port conflicts**: Ensure ports are available
2. **Database issues**: Check SQLite file permissions
3. **Metrics not appearing**: Verify Prometheus can reach services
4. **Dashboard errors**: Check datasource configuration

### Debug Commands
```bash
# Check container status
docker-compose ps

# View application logs
docker-compose logs monolith-app

# Check Prometheus targets
curl http://localhost:9090/api/v1/targets

# Test application health
curl http://localhost:5000/health

# Test metrics endpoint
curl http://localhost:5000/metrics
```

## 🎯 Future Enhancements

- [ ] Service mesh integration
- [ ] Advanced monitoring dashboards
- [ ] Automated performance testing
- [ ] CI/CD pipeline integration
- [ ] Security scanning integration