# 🚀 QUICK MICROSERVICES PROMPT (Kratka Verzija)

## 📋 TL;DR

Kreiraj mikroservisnu arhitekturu sa:
- **5 mikroservisa** (Health, Order, Compute, Bulk) + **API Gateway**
- **Prometheus** za metrics
- **Grafana** za vizualizaciju
- **k6** za load testing
- **Docker Compose** za orchestration

---

## 🏗️ Servisi i Portovi

| Servis | Port (HTTP/HTTPS) | Endpoint | Funkcija |
|--------|-------------------|----------|----------|
| API Gateway | 5000/5001 | `/api/*` | Routing |
| HealthService | 5010/5011 | `/ping` | Health check |
| OrderService | 5020/5021 | `/orders` | CRUD operacije |
| ComputeService | 5030/5031 | `/compute` | CPU tasks |
| BulkService | 5040/5041 | `/bulk` | Bulk processing |
| Prometheus | 9090 | - | Metrics |
| Grafana | 3000 | - | Dashboards |

---

## 🎯 Ključne Komponente

### **1. HealthService**
```csharp
[HttpGet("ping")]
public IActionResult Ping() => Ok("pong");
```
- Ultra-brz (< 10ms)
- Bez database
- Metrics: `health_ping_requests_total`

### **2. OrderService**
```csharp
[HttpGet] GetOrders()
[HttpPost] CreateOrder(Order order)
[HttpPut("{id}")] UpdateOrder(int id, Order order)
[HttpDelete("{id}")] DeleteOrder(int id)
```
- SQLite database
- EF Core
- Metrics: `orders_created_total`, `orders_total_count`

### **3. ComputeService**
```csharp
[HttpGet("compute")]
public IActionResult Compute(int n) // Fibonacci
```
- CPU-intensive
- Configurable complexity
- Metrics: `compute_duration_seconds`, `compute_cpu_usage_percent`

### **4. BulkService**
```csharp
[HttpPost("bulk")]
public IActionResult BulkProcess(int count)
```
- Parallel processing
- Job tracking
- Metrics: `bulk_items_processed_total`, `bulk_active_jobs`

### **5. API Gateway (YARP)**
```json
{
  "ReverseProxy": {
    "Routes": {
      "health-route": {
        "ClusterId": "health-cluster",
        "Match": { "Path": "/api/health/{**catch-all}" }
      }
    }
  }
}
```

---

## 🐳 Docker Compose (Minimalni)

```yaml
services:
  api-gateway:
    build: ./src/ApiGateway
    ports: ["5000:80", "5001:443"]

  health-service:
    build: ./src/HealthService
    ports: ["5010:80"]

  order-service:
    build: ./src/OrderService
    ports: ["5020:80"]

  compute-service:
    build: ./src/ComputeService
    ports: ["5030:80"]

  bulk-service:
    build: ./src/BulkService
    ports: ["5040:80"]

  prometheus:
    image: prom/prometheus
    ports: ["9090:9090"]
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml

  grafana:
    image: grafana/grafana
    ports: ["3000:3000"]
```

---

## 📊 Prometheus Config

```yaml
scrape_configs:
  - job_name: 'api-gateway'
    static_configs:
      - targets: ['host.docker.internal:5001']
    tls_config:
      insecure_skip_verify: true

  - job_name: 'health-service'
    static_configs:
      - targets: ['host.docker.internal:5011']

  - job_name: 'order-service'
    static_configs:
      - targets: ['host.docker.internal:5021']

  - job_name: 'compute-service'
    static_configs:
      - targets: ['host.docker.internal:5031']

  - job_name: 'bulk-service'
    static_configs:
      - targets: ['host.docker.internal:5041']
```

---

## 🧪 k6 Test (Osnovni)

```javascript
import http from 'k6/http';
import { check } from 'k6';

export const options = {
  vus: 50,
  duration: '2m',
};

const BASE = 'https://localhost:5001/api';

export default function () {
  // Test Health
  check(http.get(`${BASE}/health/ping`), {
    'ping OK': (r) => r.status === 200,
  });

  // Test Orders
  check(http.get(`${BASE}/orders`), {
    'orders OK': (r) => r.status === 200,
  });

  // Test Compute
  check(http.get(`${BASE}/compute?n=25`), {
    'compute OK': (r) => r.status === 200,
  });

  // Test Bulk
  check(http.post(`${BASE}/bulk?count=50`), {
    'bulk OK': (r) => r.status === 200,
  });
}
```

---

## ⚡ Shared Components

### **Metrics Middleware**
```csharp
app.Use(async (context, next) =>
{
    var method = context.Request.Method;
    var path = context.Request.Path;
    
    Metrics.CreateCounter("http_requests_total", "Total requests",
        new[] { "method", "path" })
        .WithLabels(method, path).Inc();
    
    var sw = Stopwatch.StartNew();
    await next();
    sw.Stop();
    
    Metrics.CreateHistogram("http_request_duration_seconds", "Request duration",
        new[] { "method", "path" })
        .WithLabels(method, path).Observe(sw.Elapsed.TotalSeconds);
});
```

### **Serilog Config**
```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.WithProperty("ServiceName", "ServiceNameHere")
    .WriteTo.Console()
    .CreateLogger();
```

---

## 📦 NuGet Packages (sve servise)

```xml
<PackageReference Include="prometheus-net.AspNetCore" Version="8.0.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
```

**OrderService dodatno:**
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />
```

**API Gateway dodatno:**
```xml
<PackageReference Include="Yarp.ReverseProxy" Version="2.1.0" />
```

---

## ✅ Quick Checklist

- [ ] Folder struktura kreirana
- [ ] Svi .csproj fajlovi postoje
- [ ] Program.cs za svaki servis
- [ ] API Gateway routing config
- [ ] Prometheus.yml sa svim job-ovima
- [ ] docker-compose.yml sa svim servisima
- [ ] k6 test scriptovi
- [ ] README.md

---

## 🎯 Test Flow

```bash
# 1. Build all
dotnet build MicroservicesArchitecture.sln

# 2. Start monitoring
docker-compose up -d prometheus grafana

# 3. Start services (izbor):
# Opcija A: Lokalno
dotnet run --project src/HealthService
dotnet run --project src/OrderService
# ...

# Opcija B: Docker
docker-compose up -d

# 4. Test
curl https://localhost:5001/api/health/ping
curl https://localhost:5001/api/orders

# 5. Load test
k6 run load-testing/k6-all-services.js

# 6. View metrics
# Prometheus: http://localhost:9090
# Grafana: http://localhost:3000
```

---

## 🚀 KORISTI OVU VERZIJU AKO ŽELIŠ:

- Brzu implementaciju
- Manje detalja
- Osnovne funkcionalnosti
- Proof of concept

## 📖 KORISTI FULL VERSION (`MICROSERVICES_CURSOR_PROMPT.md`) AKO ŽELIŠ:

- Production-ready kod
- Sve metrics i monitoring
- Kompletne k6 scenarije
- Error handling
- Rate limiting
- Detaljne dashboards
- Alerting rules
- Punu dokumentaciju

---

**HAPPY CODING!** 🎉

