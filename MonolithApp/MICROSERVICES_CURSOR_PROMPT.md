# 🎯 CURSOR PROMPT: Mikroservisna Arhitektura sa Monitoring & Load Testing

## 📋 ZADATAK

Kreiraj **kompletnu mikroservisnu arhitekturu** (.NET 8) sa API Gateway-em, 4 nezavisna mikroservisa, monitoring stack-om (Prometheus + Grafana), i load testing setup-om (k6). Arhitektura mora biti production-ready sa detaljnim metrics-ima, logging-om, health checks-ima i Docker kontejnerizacijom.

---

## 🏗️ ARHITEKTURA

### Struktura Projekta:

```
MicroservicesArchitecture/
│
├── src/
│   ├── ApiGateway/                    # API Gateway (Ocelot/YARP)
│   │   ├── Program.cs
│   │   ├── ocelot.json                # Routing configuration
│   │   ├── appsettings.json
│   │   └── ApiGateway.csproj
│   │
│   ├── HealthService/                 # Mikroservis za health checks
│   │   ├── Controllers/
│   │   │   └── PingController.cs      # GET /ping endpoint
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── HealthService.csproj
│   │
│   ├── OrderService/                  # Mikroservis za upravljanje narudžbinama
│   │   ├── Controllers/
│   │   │   └── OrdersController.cs    # CRUD operacije za Orders
│   │   ├── Services/
│   │   │   ├── IOrderService.cs
│   │   │   └── OrderService.cs
│   │   ├── Models/
│   │   │   └── Order.cs
│   │   ├── Data/
│   │   │   └── OrderDbContext.cs
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── OrderService.csproj
│   │
│   ├── ComputeService/                # Mikroservis za CPU-intenzivne zadatke
│   │   ├── Controllers/
│   │   │   └── ComputeController.cs   # GET /compute?n={broj}
│   │   ├── Services/
│   │   │   ├── IComputeService.cs
│   │   │   └── ComputeService.cs
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── ComputeService.csproj
│   │
│   ├── BulkService/                   # Mikroservis za bulk operacije
│   │   ├── Controllers/
│   │   │   └── BulkController.cs      # POST /bulk?count={broj}
│   │   ├── Services/
│   │   │   ├── IBulkService.cs
│   │   │   └── BulkService.cs
│   │   ├── Models/
│   │   │   └── BulkItem.cs
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── BulkService.csproj
│   │
│   └── Shared/                        # Zajednički kod za sve servise
│       ├── Middleware/
│       │   ├── MetricsMiddleware.cs   # Custom Prometheus metrics
│       │   └── LoggingMiddleware.cs   # Request/Response logging
│       ├── Extensions/
│       │   ├── ServiceExtensions.cs   # Dependency injection helpers
│       │   └── SwaggerExtensions.cs   # Swagger configuration
│       └── Shared.csproj
│
├── monitoring/
│   ├── prometheus/
│   │   ├── prometheus.yml             # Konfiguracija za sve mikroservise
│   │   └── alerts.yml                 # Alerting rules
│   └── grafana/
│       ├── dashboards/
│       │   ├── microservices-overview.json
│       │   ├── api-gateway-dashboard.json
│       │   ├── health-service-dashboard.json
│       │   ├── order-service-dashboard.json
│       │   ├── compute-service-dashboard.json
│       │   └── bulk-service-dashboard.json
│       └── datasources/
│           └── prometheus.yml
│
├── load-testing/
│   ├── k6-all-services.js             # Test svih servisa
│   ├── k6-health-service.js           # Specifičan test za HealthService
│   ├── k6-order-service.js            # Specifičan test za OrderService
│   ├── k6-compute-service.js          # Specifičan test za ComputeService
│   ├── k6-bulk-service.js             # Specifičan test za BulkService
│   ├── k6-api-gateway.js              # Test kroz API Gateway
│   └── README.md
│
├── docker-compose.yml                 # Orchestracija svih servisa
├── docker-compose.override.yml        # Development overrides
├── .dockerignore
├── README.md
├── SETUP_GUIDE.md
└── MicroservicesArchitecture.sln

```

---

## 🎯 DETALJNE SPECIFIKACIJE PO MIKROSERVISU

### 1. **API Gateway** (Port: 5000/5001)

**Tehnologije:** YARP (Yet Another Reverse Proxy) ili Ocelot

**Funkcionalnost:**
- Routing ka svim mikroservisima
- Load balancing
- Rate limiting
- Request/response logging
- Centralizovani Swagger UI (agregacija svih API dokumentacija)
- Prometheus metrics za gateway traffic
- Health checks za sve downstream servise

**Routing:**
```
/api/health/*     → HealthService   (port 5010)
/api/orders/*     → OrderService    (port 5020)
/api/compute/*    → ComputeService  (port 5030)
/api/bulk/*       → BulkService     (port 5040)
```

**Key Metrics:**
- `gateway_requests_total{service="health|orders|compute|bulk"}`
- `gateway_request_duration_seconds`
- `gateway_errors_total`
- `downstream_service_health{service="..."}`

---

### 2. **HealthService** (Port: 5010)

**Endpointi:**
- `GET /ping` - Vraća "pong" (test minimalne latencije)
- `GET /health` - ASP.NET health check
- `GET /metrics` - Prometheus metrics

**Funkcionalnost:**
- Ultra-brz odgovor (< 5ms)
- Logging svakog ping zahtjeva
- Counter metrika za broj pingova
- Response time histogram

**Models:** Nema database

**Dependencies:**
- `Microsoft.AspNetCore.App`
- `prometheus-net.AspNetCore`
- `Serilog.AspNetCore`
- `Swashbuckle.AspNetCore`

**Key Metrics:**
- `health_ping_requests_total`
- `health_ping_duration_seconds`

**Config (appsettings.json):**
```json
{
  "ServiceName": "HealthService",
  "ServicePort": 5010,
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

---

### 3. **OrderService** (Port: 5020)

**Endpointi:**
- `GET /orders` - Vraća sve narudžbine
- `GET /orders/{id}` - Vraća specifičnu narudžbinu
- `POST /orders` - Kreira novu narudžbinu
- `PUT /orders/{id}` - Update postojeće narudžbine
- `DELETE /orders/{id}` - Briše narudžbinu
- `GET /health` - Health check
- `GET /metrics` - Prometheus metrics

**Funkcionalnost:**
- Full CRUD operacije
- Entity Framework Core sa SQLite bazom
- Async/await pattern
- Validation (FluentValidation)
- Exception handling middleware
- Database seeding sa dummy podacima

**Model: Order**
```csharp
public class Order
{
    public int Id { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string Status { get; set; } // "Pending", "Processing", "Completed", "Cancelled"
}
```

**Database:** SQLite (`orders.db`)

**Dependencies:**
- `Microsoft.EntityFrameworkCore.Sqlite`
- `Microsoft.EntityFrameworkCore.Design`
- `prometheus-net.AspNetCore`
- `Serilog.AspNetCore`
- `Swashbuckle.AspNetCore`
- `FluentValidation.AspNetCore`

**Key Metrics:**
- `orders_created_total`
- `orders_retrieved_total`
- `orders_updated_total`
- `orders_deleted_total`
- `orders_database_query_duration_seconds`
- `orders_total_count` (gauge)

---

### 4. **ComputeService** (Port: 5030)

**Endpointi:**
- `GET /compute?n={broj}` - Fibonacci kalkulacija (CPU-intensive)
- `GET /compute/pi?iterations={broj}` - Pi aproksimacija
- `GET /compute/prime?limit={broj}` - Pronalaženje prostih brojeva
- `GET /health` - Health check
- `GET /metrics` - Prometheus metrics

**Funkcionalnost:**
- CPU-intenzivni zadaci za load testing
- Configurable complexity (parametar `n`, `iterations`, `limit`)
- Response time tracking
- CPU usage metrics
- Memory allocation tracking
- Throttling/Rate limiting za prevenciju abuse-a

**Dependencies:**
- `Microsoft.AspNetCore.App`
- `prometheus-net.AspNetCore`
- `Serilog.AspNetCore`
- `Swashbuckle.AspNetCore`

**Key Metrics:**
- `compute_requests_total{operation="fibonacci|pi|prime"}`
- `compute_duration_seconds`
- `compute_cpu_usage_percent`
- `compute_complexity{operation="..."}`

**Config:**
```json
{
  "ServiceName": "ComputeService",
  "ServicePort": 5030,
  "ComputeLimits": {
    "MaxFibonacci": 50,
    "MaxPiIterations": 1000000,
    "MaxPrimeLimit": 1000000
  }
}
```

---

### 5. **BulkService** (Port: 5040)

**Endpointi:**
- `POST /bulk?count={broj}` - Bulk data processing
- `POST /bulk/parallel?count={broj}` - Parallel processing
- `POST /bulk/batch?count={broj}&batchSize={broj}` - Batch processing
- `GET /bulk/status/{jobId}` - Status bulk job-a
- `GET /health` - Health check
- `GET /metrics` - Prometheus metrics

**Funkcionalnost:**
- Simultana obrada velikog broja podataka
- Sequential vs Parallel processing options
- Batch processing sa configurable batch size
- In-memory job tracking
- Progress reporting
- Error handling i partial success

**Model: BulkItem**
```csharp
public class BulkItem
{
    public Guid Id { get; set; }
    public string Data { get; set; }
    public DateTime ProcessedAt { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}

public class BulkJobStatus
{
    public Guid JobId { get; set; }
    public int TotalItems { get; set; }
    public int ProcessedItems { get; set; }
    public int FailedItems { get; set; }
    public string Status { get; set; } // "Running", "Completed", "Failed"
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
```

**Dependencies:**
- `Microsoft.AspNetCore.App`
- `prometheus-net.AspNetCore`
- `Serilog.AspNetCore`
- `Swashbuckle.AspNetCore`
- `System.Threading.Tasks.Dataflow` (za parallel processing)

**Key Metrics:**
- `bulk_jobs_total{type="sequential|parallel|batch"}`
- `bulk_items_processed_total`
- `bulk_items_failed_total`
- `bulk_processing_duration_seconds`
- `bulk_batch_size`
- `bulk_active_jobs` (gauge)

---

## 🔧 SHARED KOMPONENTE

### Shared/Middleware/MetricsMiddleware.cs

**Funkcionalnost:**
- Automatski tracking HTTP request-a
- Request/response duration
- Status code distribution
- Endpoint-specific metrics
- Custom labels (service name, method, path)

**Metrics:**
```csharp
- http_requests_total{service, method, endpoint, status}
- http_request_duration_seconds{service, method, endpoint}
- http_requests_in_progress{service}
- http_response_size_bytes{service, endpoint}
```

### Shared/Middleware/LoggingMiddleware.cs

**Funkcionalnost:**
- Structured logging (Serilog)
- Request/Response logging
- Correlation ID tracking
- Error logging sa stack trace
- Performance logging (slow requests > 1s)

### Shared/Extensions/ServiceExtensions.cs

**Funkcionalnost:**
```csharp
public static IServiceCollection AddCommonServices(this IServiceCollection services, IConfiguration configuration)
{
    // Health checks
    services.AddHealthChecks();
    
    // Swagger
    services.AddSwaggerGen(/* config */);
    
    // CORS
    services.AddCors(/* config */);
    
    // Prometheus
    services.AddControllers();
    
    return services;
}
```

---

## 📊 PROMETHEUS KONFIGURACIJA

### monitoring/prometheus/prometheus.yml

```yaml
global:
  scrape_interval: 15s
  evaluation_interval: 15s
  external_labels:
    cluster: 'microservices-dev'
    environment: 'development'

# Alerting configuration
alerting:
  alertmanagers:
    - static_configs:
        - targets:
          # - alertmanager:9093

# Load rules files
rule_files:
  - "alerts.yml"

# Scrape configurations
scrape_configs:
  # API Gateway
  - job_name: 'api-gateway'
    scheme: 'https'
    tls_config:
      insecure_skip_verify: true
    static_configs:
      - targets: ['host.docker.internal:5001']
    metrics_path: '/metrics'
    scrape_interval: 5s
    scrape_timeout: 5s

  # HealthService
  - job_name: 'health-service'
    scheme: 'https'
    tls_config:
      insecure_skip_verify: true
    static_configs:
      - targets: ['host.docker.internal:5011']
    metrics_path: '/metrics'
    scrape_interval: 5s

  # OrderService
  - job_name: 'order-service'
    scheme: 'https'
    tls_config:
      insecure_skip_verify: true
    static_configs:
      - targets: ['host.docker.internal:5021']
    metrics_path: '/metrics'
    scrape_interval: 5s

  # ComputeService
  - job_name: 'compute-service'
    scheme: 'https'
    tls_config:
      insecure_skip_verify: true
    static_configs:
      - targets: ['host.docker.internal:5031']
    metrics_path: '/metrics'
    scrape_interval: 5s

  # BulkService
  - job_name: 'bulk-service'
    scheme: 'https'
    tls_config:
      insecure_skip_verify: true
    static_configs:
      - targets: ['host.docker.internal:5041']
    metrics_path: '/metrics'
    scrape_interval: 5s
```

### monitoring/prometheus/alerts.yml

```yaml
groups:
  - name: microservices_alerts
    interval: 30s
    rules:
      # High error rate
      - alert: HighErrorRate
        expr: |
          (rate(http_requests_total{status=~"5.."}[5m]) / rate(http_requests_total[5m])) > 0.05
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "High error rate detected"
          description: "Service {{ $labels.service }} has error rate above 5%"

      # Slow response time
      - alert: SlowResponseTime
        expr: |
          histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m])) > 1
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "Slow response time detected"
          description: "95th percentile response time is above 1 second"

      # Service down
      - alert: ServiceDown
        expr: up == 0
        for: 1m
        labels:
          severity: critical
        annotations:
          summary: "Service is down"
          description: "{{ $labels.job }} is down"
```

---

## 🐳 DOCKER COMPOSE KONFIGURACIJA

### docker-compose.yml

```yaml
version: '3.8'

services:
  # API Gateway
  api-gateway:
    build:
      context: .
      dockerfile: src/ApiGateway/Dockerfile
    container_name: api-gateway
    ports:
      - "5000:80"
      - "5001:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=https://+:443;http://+:80
      - ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
      - ASPNETCORE_Kestrel__Certificates__Default__Password=SecurePassword123
    volumes:
      - ~/.aspnet/https:/https:ro
    depends_on:
      - health-service
      - order-service
      - compute-service
      - bulk-service
    networks:
      - microservices-network

  # HealthService
  health-service:
    build:
      context: .
      dockerfile: src/HealthService/Dockerfile
    container_name: health-service
    ports:
      - "5010:80"
      - "5011:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    networks:
      - microservices-network

  # OrderService
  order-service:
    build:
      context: .
      dockerfile: src/OrderService/Dockerfile
    container_name: order-service
    ports:
      - "5020:80"
      - "5021:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Data Source=/data/orders.db
    volumes:
      - order-data:/data
    networks:
      - microservices-network

  # ComputeService
  compute-service:
    build:
      context: .
      dockerfile: src/ComputeService/Dockerfile
    container_name: compute-service
    ports:
      - "5030:80"
      - "5031:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    networks:
      - microservices-network

  # BulkService
  bulk-service:
    build:
      context: .
      dockerfile: src/BulkService/Dockerfile
    container_name: bulk-service
    ports:
      - "5040:80"
      - "5041:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    networks:
      - microservices-network

  # Prometheus
  prometheus:
    image: prom/prometheus:latest
    container_name: prometheus
    ports:
      - "9090:9090"
    volumes:
      - ./monitoring/prometheus/prometheus.yml:/etc/prometheus/prometheus.yml
      - ./monitoring/prometheus/alerts.yml:/etc/prometheus/alerts.yml
      - prometheus-data:/prometheus
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
      - '--storage.tsdb.path=/prometheus'
      - '--web.console.libraries=/etc/prometheus/console_libraries'
      - '--web.console.templates=/etc/prometheus/consoles'
      - '--web.enable-lifecycle'
    extra_hosts:
      - "host.docker.internal:host-gateway"
    networks:
      - microservices-network

  # Grafana
  grafana:
    image: grafana/grafana:latest
    container_name: grafana
    ports:
      - "3000:3000"
    environment:
      - GF_SECURITY_ADMIN_USER=admin
      - GF_SECURITY_ADMIN_PASSWORD=admin
      - GF_USERS_ALLOW_SIGN_UP=false
    volumes:
      - grafana-data:/var/lib/grafana
      - ./monitoring/grafana/dashboards:/etc/grafana/provisioning/dashboards
      - ./monitoring/grafana/datasources:/etc/grafana/provisioning/datasources
    depends_on:
      - prometheus
    networks:
      - microservices-network

networks:
  microservices-network:
    driver: bridge

volumes:
  order-data:
  prometheus-data:
  grafana-data:
```

---

## 🧪 K6 LOAD TESTING

### load-testing/k6-all-services.js

```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');
const healthTrend = new Trend('health_duration');
const orderTrend = new Trend('order_duration');
const computeTrend = new Trend('compute_duration');
const bulkTrend = new Trend('bulk_duration');

export const options = {
  scenarios: {
    // Health service test
    health_load: {
      executor: 'constant-vus',
      exec: 'testHealth',
      vus: 50,
      duration: '2m',
    },
    // Order service test
    order_load: {
      executor: 'ramping-vus',
      exec: 'testOrders',
      startVUs: 0,
      stages: [
        { duration: '30s', target: 10 },
        { duration: '1m', target: 20 },
        { duration: '30s', target: 0 },
      ],
    },
    // Compute service test
    compute_load: {
      executor: 'constant-arrival-rate',
      exec: 'testCompute',
      rate: 10,
      timeUnit: '1s',
      duration: '2m',
      preAllocatedVUs: 20,
    },
    // Bulk service test
    bulk_load: {
      executor: 'shared-iterations',
      exec: 'testBulk',
      vus: 5,
      iterations: 50,
    },
  },
  thresholds: {
    'http_req_duration': ['p(95)<500'],
    'http_req_failed': ['rate<0.1'],
    'errors': ['rate<0.1'],
  },
};

const BASE_URL = 'https://localhost:5001/api';

export function testHealth() {
  const res = http.get(`${BASE_URL}/health/ping`, {
    tags: { name: 'HealthPing' },
  });
  
  const success = check(res, {
    'health status is 200': (r) => r.status === 200,
    'health response is pong': (r) => r.body === 'pong',
  });
  
  errorRate.add(!success);
  healthTrend.add(res.timings.duration);
  
  sleep(0.1);
}

export function testOrders() {
  // Get all orders
  let res = http.get(`${BASE_URL}/orders`, {
    tags: { name: 'GetOrders' },
  });
  
  check(res, {
    'get orders status is 200': (r) => r.status === 200,
  });
  
  orderTrend.add(res.timings.duration);
  
  // Create new order
  const payload = JSON.stringify({
    productName: 'Test Product',
    quantity: Math.floor(Math.random() * 10) + 1,
    price: Math.random() * 1000,
  });
  
  res = http.post(`${BASE_URL}/orders`, payload, {
    headers: { 'Content-Type': 'application/json' },
    tags: { name: 'CreateOrder' },
  });
  
  check(res, {
    'create order status is 201': (r) => r.status === 201,
  });
  
  orderTrend.add(res.timings.duration);
  
  sleep(1);
}

export function testCompute() {
  const n = Math.floor(Math.random() * 20) + 10;
  const res = http.get(`${BASE_URL}/compute?n=${n}`, {
    tags: { name: 'Compute' },
  });
  
  check(res, {
    'compute status is 200': (r) => r.status === 200,
    'compute has result': (r) => JSON.parse(r.body).result !== undefined,
  });
  
  computeTrend.add(res.timings.duration);
  
  sleep(0.5);
}

export function testBulk() {
  const count = Math.floor(Math.random() * 50) + 10;
  const res = http.post(`${BASE_URL}/bulk?count=${count}`, null, {
    tags: { name: 'Bulk' },
  });
  
  check(res, {
    'bulk status is 200': (r) => r.status === 200,
    'bulk processed count matches': (r) => JSON.parse(r.body).processedCount === count,
  });
  
  bulkTrend.add(res.timings.duration);
  
  sleep(2);
}
```

### load-testing/k6-api-gateway.js

```javascript
import http from 'k6/http';
import { check, group, sleep } from 'k6';

export const options = {
  insecureSkipTLSVerify: true,
  stages: [
    { duration: '1m', target: 10 },
    { duration: '2m', target: 50 },
    { duration: '1m', target: 100 },
    { duration: '2m', target: 100 },
    { duration: '1m', target: 0 },
  ],
  thresholds: {
    'http_req_duration': ['p(95)<1000', 'p(99)<2000'],
    'http_req_failed': ['rate<0.05'],
    'checks': ['rate>0.95'],
  },
};

const BASE_URL = 'https://localhost:5001/api';

export default function () {
  group('Health Check via Gateway', () => {
    const res = http.get(`${BASE_URL}/health/ping`);
    check(res, {
      'gateway to health status 200': (r) => r.status === 200,
      'gateway to health response time < 50ms': (r) => r.timings.duration < 50,
    });
  });

  group('Orders via Gateway', () => {
    const res = http.get(`${BASE_URL}/orders`);
    check(res, {
      'gateway to orders status 200': (r) => r.status === 200,
    });
  });

  group('Compute via Gateway', () => {
    const res = http.get(`${BASE_URL}/compute?n=25`);
    check(res, {
      'gateway to compute status 200': (r) => r.status === 200,
    });
  });

  group('Bulk via Gateway', () => {
    const res = http.post(`${BASE_URL}/bulk?count=30`);
    check(res, {
      'gateway to bulk status 200': (r) => r.status === 200,
    });
  });

  sleep(1);
}
```

---

## 📈 GRAFANA DASHBOARDS

### Potrebni Dashboards:

1. **Microservices Overview Dashboard**
   - Total requests per second (svi servisi)
   - Error rate distribution
   - Response time p95/p99 per service
   - Service health status
   - Active connections gauge

2. **API Gateway Dashboard**
   - Request routing breakdown
   - Downstream service health
   - Gateway error rate
   - Response time per route

3. **Per-Service Dashboards** (Health, Order, Compute, Bulk)
   - Request rate
   - Error rate
   - Response time percentiles
   - Active requests
   - Resource usage (CPU, memory)
   - Service-specific metrics

### Key PromQL Queries:

```promql
# Total request rate across all services
sum(rate(http_requests_total[5m])) by (service)

# Error rate per service
sum(rate(http_requests_total{status=~"5.."}[5m])) by (service) / sum(rate(http_requests_total[5m])) by (service)

# P95 response time per service
histogram_quantile(0.95, sum(rate(http_request_duration_seconds_bucket[5m])) by (service, le))

# Service health (1 = UP, 0 = DOWN)
up{job=~".*-service"}

# Gateway routing breakdown
sum(rate(gateway_requests_total[5m])) by (service)

# Orders created per minute
rate(orders_created_total[1m]) * 60

# Compute average complexity
avg(compute_complexity) by (operation)

# Bulk processing throughput
rate(bulk_items_processed_total[1m]) * 60
```

---

## 📝 DODATNI ZAHTJEVI

### 1. **Logging sa Serilog**

Svi servisi moraju koristiti:
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "ServiceNameHere")
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();
```

### 2. **Health Checks**

Svaki servis mora imati:
- `/health` endpoint (ASP.NET health check)
- `/health/ready` endpoint (readiness probe)
- `/health/live` endpoint (liveness probe)

### 3. **Swagger Dokumentacija**

Svi API-ji moraju imati:
- OpenAPI/Swagger dokumentaciju
- XML komentari za sve endpointe
- Request/response primjeri
- Error response dokumentaciju

### 4. **Error Handling**

Globalni exception handler middleware:
```csharp
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        
        var error = context.Features.Get<IExceptionHandlerFeature>();
        if (error != null)
        {
            var ex = error.Error;
            Log.Error(ex, "Unhandled exception");
            
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Internal server error",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    });
});
```

### 5. **CORS Configuration**

API Gateway mora imati CORS za sve origin-e (development):
```csharp
services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
```

### 6. **Rate Limiting**

API Gateway mora imati rate limiting:
- 100 requests per minute per IP (default)
- 1000 requests per minute per IP (authenticated)

### 7. **Dockerfile za svaki servis**

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ServiceName/ServiceName.csproj", "ServiceName/"]
RUN dotnet restore "ServiceName/ServiceName.csproj"
COPY . .
WORKDIR "/src/ServiceName"
RUN dotnet build "ServiceName.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ServiceName.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServiceName.dll"]
```

---

## 📚 DOCUMENTATION

### README.md mora sadržavati:

1. **Arhitektura dijagram** (ASCII ili Mermaid)
2. **Quick start guide**
3. **Endpoints dokumentacija**
4. **Monitoring setup**
5. **Load testing instrukcije**
6. **Troubleshooting sekcija**
7. **Performance benchmarks**

### SETUP_GUIDE.md mora sadržavati:

1. **Prerequisites**
2. **Step-by-step setup** (lokalno i Docker)
3. **Prometheus konfiguracija**
4. **Grafana dashboard import**
5. **k6 installation i usage**
6. **Common issues i solutions**

---

## 🎯 PERFORMANCE TARGETS

### HealthService:
- Response time: p95 < 10ms, p99 < 20ms
- Throughput: > 10,000 rps
- Error rate: < 0.01%

### OrderService:
- Response time: p95 < 100ms, p99 < 200ms
- Throughput: > 1,000 rps
- Error rate: < 0.5%

### ComputeService:
- Response time: p95 < 500ms, p99 < 1000ms
- Throughput: > 100 rps
- Error rate: < 1%

### BulkService:
- Response time: p95 < 2s, p99 < 5s
- Throughput: > 50 jobs/minute
- Error rate: < 2%

### API Gateway:
- Additional latency: < 10ms
- Throughput: > 5,000 rps
- Error rate: < 0.1%

---

## ✅ CHECKLIST

Prije nego što kažeš da je gotovo, osiguraj:

- [ ] Svi mikroservisi se builduju bez errora
- [ ] Svi Dockerfile-ovi su ispravni
- [ ] docker-compose.yml pokreće sve servise
- [ ] Prometheus scrape-uje sve servise (targets su UP)
- [ ] Grafana dashboards prikazuju metrike
- [ ] k6 svi testovi prolaze
- [ ] Swagger UI je dostupan za sve servise
- [ ] Health checks rade za sve servise
- [ ] Logging piše strukturirane logove
- [ ] API Gateway rutira ka svim servisima
- [ ] Database inicijalizacija radi (OrderService)
- [ ] Svi endpointi vraćaju ispravne response-e
- [ ] Error handling radi globalno
- [ ] README.md i SETUP_GUIDE.md su kompletni

---

## 🚀 DODATNE NAPOMENE

1. **Koristiti .NET 8** (najnovija verzija)
2. **Async/await** svugdje gdje je moguće
3. **Dependency Injection** za sve servise
4. **Clean Code** principi
5. **SOLID** principi
6. **Error handling** na svim nivoima
7. **Logging** za svaku važnu operaciju
8. **Comments** samo gdje je potrebno (self-documenting code)

---

## 🎬 KRAJ PROMPTA

Generisanje ove arhitekture je kompleksan zadatak. Kreiraj sve fajlove, konfiguracije, i dokumentaciju. Fokusiraj se na **production-ready kod** sa detaljnim monitoring-om i testiranjem.

**POČNI SA:**
1. Kreiranje folder strukture
2. Solution file (.sln)
3. Shared projekt (prvo)
4. API Gateway
5. Mikroservisi (HealthService → OrderService → ComputeService → BulkService)
6. Docker konfiguracija
7. Prometheus/Grafana setup
8. k6 testovi
9. Dokumentacija

**Good luck!** 🚀

