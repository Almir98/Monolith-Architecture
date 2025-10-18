# Quick Start Guide

## 🚀 Running the Application

### Option 1: Using Scripts (Recommended)

**Windows:**
```powershell
.\run.ps1
```

**Linux/macOS:**
```bash
./run.sh
```

### Option 2: Manual Commands

```bash
# Navigate to project directory
cd MonolithApp

# Restore dependencies
dotnet restore

# Run the application
dotnet run
```

## 🧪 Quick Testing

### Test Health Endpoint
```bash
curl http://localhost:5000/health/ping
# Expected: "pong"
```

### Test Orders API
```bash
# Get all orders
curl http://localhost:5000/orders

# Create an order
curl -X POST http://localhost:5000/orders \
  -H "Content-Type: application/json" \
  -d '{"productName": "Test Product", "quantity": 1, "price": 99.99}'
```

### Test Performance Endpoints
```bash
# CPU-intensive computation
curl "http://localhost:5000/compute?n=35"

# Bulk processing
curl -X POST "http://localhost:5000/bulk?count=100"
```

### Check Metrics
```bash
curl http://localhost:5000/metrics
```

## 📊 Monitoring Setup

### Start Monitoring Stack
```bash
docker-compose up -d
```

### Access Dashboards
- **Prometheus**: http://localhost:9090
- **Grafana**: http://localhost:3000 (admin/admin)

## 🧪 Load Testing

### Using k6
```bash
# Install k6 first (Windows: choco install k6)
k6 run load-testing/k6-script.js
```

## 🔧 System Monitoring

### Monitor with dotnet-counters
```bash
# Find the process ID
dotnet-counters ps

# Monitor the application
dotnet-counters monitor --process-name MonolithApp System.Runtime
```

### Monitor with dotnet-monitor
```bash
dotnet-monitor collect --urls http://localhost:52323
```

## 📈 Key Metrics to Watch

1. **Response Times**: `/metrics` endpoint shows `http_request_duration_seconds`
2. **Request Rate**: `/metrics` endpoint shows `http_requests_total`
3. **System Resources**: Use `dotnet-counters` for CPU, memory, GC stats
4. **Error Rates**: Monitor HTTP status codes in Prometheus

## 🚨 Troubleshooting

- **Port 5000 in use**: Change port in `appsettings.json`
- **Database issues**: Check SQLite file permissions
- **Metrics not showing**: Verify Prometheus can reach `http://localhost:5000/metrics`
- **Load test failures**: Ensure API is running and accessible

## 📚 Next Steps

1. Run the application
2. Test all endpoints
3. Set up monitoring stack
4. Run load tests
5. Analyze performance metrics
6. Create custom Grafana dashboards
