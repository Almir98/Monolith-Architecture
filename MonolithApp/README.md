# MonolithApp - .NET 8 Web API with Monitoring & Load Testing

A comprehensive .NET 8 Web API monolithic application with built-in monitoring, metrics collection, and load testing capabilities.

## 🏗️ Project Structure

```
MonolithApp/
├── Controllers/          # API Controllers
├── Services/           # Business Logic Services
├── Models/              # Data Models
├── Data/                # Entity Framework Context
├── load-testing/        # Load Testing Scripts
├── Program.cs           # Application Entry Point
├── appsettings.json     # Configuration
├── prometheus.yml       # Prometheus Configuration
├── docker-compose.yml   # Monitoring Stack
└── README.md           # This File
```

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- Docker (for monitoring stack)
- k6 (for load testing) - [Install k6](https://k6.io/docs/getting-started/installation/)

### Running the Application

1. **Navigate to the project directory:**
   ```bash
   cd MonolithApp
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Run the application:**
   ```bash
   dotnet run
   ```

4. **The API will be available at:**
   - HTTP: `http://localhost:5000`
   - HTTPS: `https://localhost:5001`

## 📊 API Endpoints

### Health Check
- **GET** `/health/ping` - Returns "pong" for latency testing

### Orders Management
- **GET** `/orders` - Get all orders
- **GET** `/orders/{id}` - Get order by ID
- **POST** `/orders` - Create new order
- **PUT** `/orders/{id}` - Update order
- **DELETE** `/orders/{id}` - Delete order

### Performance Testing
- **GET** `/compute?n=35` - CPU-intensive Fibonacci calculation
- **POST** `/bulk?count=100` - Bulk data processing simulation

### Monitoring
- **GET** `/metrics` - Prometheus metrics endpoint
- **GET** `/health` - Health check endpoint

## 🔧 Monitoring Setup

### 1. Start Monitoring Stack

```bash
# Start Prometheus and Grafana
docker-compose up -d
```

### 2. Access Monitoring Dashboards

- **Prometheus**: http://localhost:9090
- **Grafana**: http://localhost:3000 (admin/admin)

### 3. Configure Grafana

1. Login to Grafana (admin/admin)
2. Add Prometheus as data source:
   - URL: `http://prometheus:9090`
   - Access: Server (default)
3. Import dashboard or create custom dashboards

## 🧪 Load Testing

### Using k6

1. **Install k6:**
   ```bash
   # Windows (using Chocolatey)
   choco install k6
   
   # macOS (using Homebrew)
   brew install k6
   
   # Linux
   sudo gpg --no-default-keyring --keyring /usr/share/keyrings/k6-archive-keyring.gpg --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
   echo "deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
   sudo apt-get update
   sudo apt-get install k6
   ```

2. **Run k6 load test:**
   ```bash
   k6 run load-testing/k6-script.js
   ```

## 📈 Metrics Collection

The application exposes the following metrics:

### HTTP Metrics
- `http_requests_total` - Total HTTP requests by method and endpoint
- `http_request_duration_seconds` - HTTP request duration histogram
- `http_active_connections` - Number of active connections

### System Metrics (via dotnet-counters)
- CPU usage
- Memory consumption
- GC activity
- Thread counts

### Using dotnet-counters

```bash
# Monitor the running application
dotnet-counters monitor --process-id <PID> System.Runtime

# Or monitor by process name
dotnet-counters monitor --process-name MonolithApp System.Runtime
```

### Using dotnet-monitor

```bash
# Start dotnet-monitor for real-time metrics
dotnet-monitor collect --urls http://localhost:52323
```

## 🔍 Performance Monitoring

### Key Metrics to Monitor

1. **Response Times**
   - Average response time
   - 95th percentile response time
   - Maximum response time

2. **Throughput**
   - Requests per second
   - Successful vs failed requests

3. **Resource Usage**
   - CPU utilization
   - Memory consumption
   - Database connection pool

4. **Error Rates**
   - HTTP error rates
   - Exception rates
   - Timeout rates

### Sample Grafana Queries

```promql
# Request rate
rate(http_requests_total[5m])

# Average response time
rate(http_request_duration_seconds_sum[5m]) / rate(http_request_duration_seconds_count[5m])

# Error rate
rate(http_requests_total{status=~"5.."}[5m]) / rate(http_requests_total[5m])
```

## 🛠️ Development

### Adding New Endpoints

1. Create controller in `Controllers/` directory
2. Add service interface and implementation in `Services/`
3. Register service in `Program.cs`
4. Add metrics collection as needed

### Database

The application uses SQLite by default. To change to a different database:

1. Update connection string in `appsettings.json`
2. Change provider in `Program.cs`
3. Run migrations if needed

### Logging

The application uses Serilog for structured logging. Configuration is in `appsettings.json`.

## 📝 Example API Usage

### Create an Order
```bash
curl -X POST http://localhost:5000/orders \
  -H "Content-Type: application/json" \
  -d '{"productName": "Laptop", "quantity": 1, "price": 999.99}'
```

### Get All Orders
```bash
curl http://localhost:5000/orders
```

### Test CPU Performance
```bash
curl "http://localhost:5000/compute?n=40"
```

### Test Bulk Processing
```bash
curl -X POST "http://localhost:5000/bulk?count=1000"
```

## 🚨 Troubleshooting

### Common Issues

1. **Port conflicts**: Change ports in `appsettings.json` or `docker-compose.yml`
2. **Database issues**: Ensure SQLite file permissions
3. **Metrics not showing**: Check Prometheus configuration and target health
4. **Load test failures**: Verify API is running and accessible

### Debug Mode

Run with detailed logging:
```bash
dotnet run --environment Development
```

## 📚 Additional Resources

- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/core/)
- [Prometheus Documentation](https://prometheus.io/docs/)
- [Grafana Documentation](https://grafana.com/docs/)
- [k6 Documentation](https://k6.io/docs/)

---

## 🎯 Microservices Migration Guide

Želiš da migriraš na mikroservisnu arhitekturu? Imamo kompletnu dokumentaciju:

### 📄 Dostupni Resursi:

1. **[MICROSERVICES_CURSOR_PROMPT.md](MICROSERVICES_CURSOR_PROMPT.md)** ⭐
   - **31,000+ riječi detaljni prompt** za Cursor AI
   - Kompletna specifikacija 5 mikroservisa + API Gateway
   - Prometheus, Grafana, k6 setup
   - Docker Compose orchestration
   - Performance targets i checklist
   - **Use this:** Za production-ready mikroservise

2. **[MICROSERVICES_QUICK_PROMPT.md](MICROSERVICES_QUICK_PROMPT.md)** 🚀
   - Skraćena verzija (Quick reference)
   - Osnovne komponente i setup
   - Minimalni Docker Compose
   - **Use this:** Za brzu implementaciju ili PoC

3. **[MICROSERVICES_ARCHITECTURE_DIAGRAM.md](MICROSERVICES_ARCHITECTURE_DIAGRAM.md)** 📐
   - Vizuelni dijagrami arhitekture
   - Data flow primjeri
   - Metrics overview
   - Deployment commands
   - **Use this:** Za razumijevanje arhitekture

4. **[MONOLITH_VS_MICROSERVICES.md](MONOLITH_VS_MICROSERVICES.md)** ⚖️
   - Detaljna komparacija
   - Performance analiza
   - Cost analysis
   - Decision matrix
   - Migration strategy (Strangler Fig Pattern)
   - **Use this:** Za odluku monolit vs mikroservisi

### 🚀 Kako koristiti prompte:

```bash
# Opcija 1: Copy/Paste u Cursor
# Otvori MICROSERVICES_CURSOR_PROMPT.md
# Kopiraj CIJELI sadržaj → Paste u Cursor chat
# Pričekaj 10-15 minuta da AI generiše sve

# Opcija 2: Segment po Segment
# Kopiraj jedan po jedan servis iz prompta

# Opcija 3: Postepena Migracija
# Prati Strangler Fig Pattern iz MONOLITH_VS_MICROSERVICES.md
```

### 📊 Quick Comparison

| Aspekt | MonolithApp | MicroservicesArchitecture |
|--------|-------------|---------------------------|
| **Time to Market** | ✅ Brzo (dani) | ⚠️ Sporo (sedmice) |
| **Cost** | ✅ $60/mj | ❌ $310/mj |
| **Team Size** | ✅ 1-5 dev | ⚠️ 5-20+ dev |
| **Scalability** | ⚠️ Vertikalno | ✅ Horizontalno |
| **Fault Isolation** | ❌ Ne | ✅ Da |
| **Complexity** | ✅ Niska | ❌ Visoka |

**Preporuka:** Start with MonolithApp, migrate to Microservices when needed!

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License.
