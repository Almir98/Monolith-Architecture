# 🏗️ Mikroservisna Arhitektura - Vizuelni Dijagram

## 📐 Kompletan System Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              CLIENT / BROWSER                                │
│                         https://localhost:5001                               │
└────────────────────────────────┬────────────────────────────────────────────┘
                                 │
                                 │ HTTPS
                                 ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                           API GATEWAY (YARP)                                 │
│                         Port: 5000 (HTTP) / 5001 (HTTPS)                    │
│  ┌────────────────────────────────────────────────────────────────────┐    │
│  │  Routes:                                                            │    │
│  │    /api/health/*   → HealthService                                 │    │
│  │    /api/orders/*   → OrderService                                  │    │
│  │    /api/compute/*  → ComputeService                                │    │
│  │    /api/bulk/*     → BulkService                                   │    │
│  │                                                                      │    │
│  │  Features:                                                          │    │
│  │    • Load Balancing                                                 │    │
│  │    • Rate Limiting (100 req/min)                                   │    │
│  │    • CORS                                                           │    │
│  │    • Metrics: gateway_requests_total                               │    │
│  └────────────────────────────────────────────────────────────────────┘    │
└──┬────────┬────────────┬────────────┬───────────────────────────────────────┘
   │        │            │            │
   │        │            │            │
   ▼        ▼            ▼            ▼
┌─────┐ ┌────────┐ ┌──────────┐ ┌─────────┐
│     │ │        │ │          │ │         │
│  1  │ │   2    │ │    3     │ │    4    │
│     │ │        │ │          │ │         │
└─────┘ └────────┘ └──────────┘ └─────────┘

┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
┃  1️⃣  HEALTH SERVICE                                                       ┃
┃  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ┃
┃                                                                             ┃
┃  Port: 5010 (HTTP) / 5011 (HTTPS)                                         ┃
┃                                                                             ┃
┃  Endpoints:                                                                 ┃
┃    • GET /ping         → "pong"                                            ┃
┃    • GET /health       → Health Check                                      ┃
┃    • GET /metrics      → Prometheus Metrics                                ┃
┃                                                                             ┃
┃  Purpose:                                                                   ┃
┃    ✓ Ultra-fast health checks (< 10ms)                                    ┃
┃    ✓ Minimal latency testing                                               ┃
┃    ✓ No database dependencies                                              ┃
┃                                                                             ┃
┃  Metrics:                                                                   ┃
┃    • health_ping_requests_total                                            ┃
┃    • health_ping_duration_seconds                                          ┃
┃                                                                             ┃
┃  Performance Target:                                                        ┃
┃    • Throughput: > 10,000 rps                                              ┃
┃    • Latency: p95 < 10ms, p99 < 20ms                                      ┃
┃                                                                             ┃
┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛


┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
┃  2️⃣  ORDER SERVICE                                                        ┃
┃  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ┃
┃                                                                             ┃
┃  Port: 5020 (HTTP) / 5021 (HTTPS)                                         ┃
┃                                                                             ┃
┃  Endpoints:                                                                 ┃
┃    • GET    /orders           → Get all orders                             ┃
┃    • GET    /orders/{id}      → Get order by ID                            ┃
┃    • POST   /orders           → Create new order                           ┃
┃    • PUT    /orders/{id}      → Update order                               ┃
┃    • DELETE /orders/{id}      → Delete order                               ┃
┃    • GET    /health           → Health Check                               ┃
┃    • GET    /metrics          → Prometheus Metrics                         ┃
┃                                                                             ┃
┃  Database:                                                                  ┃
┃    ┌─────────────────────────────────────────────┐                        ┃
┃    │  SQLite Database (orders.db)                │                        ┃
┃    │  ─────────────────────────────────────────  │                        ┃
┃    │  Table: Orders                              │                        ┃
┃    │    • Id (int, PK)                           │                        ┃
┃    │    • ProductName (string)                   │                        ┃
┃    │    • Quantity (int)                         │                        ┃
┃    │    • Price (decimal)                        │                        ┃
┃    │    • CreatedAt (DateTime)                   │                        ┃
┃    │    • UpdatedAt (DateTime?)                  │                        ┃
┃    │    • Status (string)                        │                        ┃
┃    └─────────────────────────────────────────────┘                        ┃
┃                                                                             ┃
┃  Metrics:                                                                   ┃
┃    • orders_created_total                                                  ┃
┃    • orders_retrieved_total                                                ┃
┃    • orders_updated_total                                                  ┃
┃    • orders_deleted_total                                                  ┃
┃    • orders_total_count (gauge)                                            ┃
┃    • orders_database_query_duration_seconds                                ┃
┃                                                                             ┃
┃  Performance Target:                                                        ┃
┃    • Throughput: > 1,000 rps                                               ┃
┃    • Latency: p95 < 100ms, p99 < 200ms                                    ┃
┃                                                                             ┃
┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛


┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
┃  3️⃣  COMPUTE SERVICE                                                      ┃
┃  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ┃
┃                                                                             ┃
┃  Port: 5030 (HTTP) / 5031 (HTTPS)                                         ┃
┃                                                                             ┃
┃  Endpoints:                                                                 ┃
┃    • GET /compute?n={broj}                → Fibonacci calculation          ┃
┃    • GET /compute/pi?iterations={broj}    → Pi approximation               ┃
┃    • GET /compute/prime?limit={broj}      → Find prime numbers             ┃
┃    • GET /health                          → Health Check                   ┃
┃    • GET /metrics                         → Prometheus Metrics             ┃
┃                                                                             ┃
┃  Purpose:                                                                   ┃
┃    ✓ CPU-intensive operations                                              ┃
┃    ✓ Performance testing under load                                        ┃
┃    ✓ Configurable complexity                                               ┃
┃                                                                             ┃
┃  Operations:                                                                ┃
┃    ┌───────────────────────────────────────┐                              ┃
┃    │  Fibonacci(n):                        │                              ┃
┃    │    • Recursive calculation            │                              ┃
┃    │    • Max n = 50                       │                              ┃
┃    │    • Time: O(2^n)                     │                              ┃
┃    └───────────────────────────────────────┘                              ┃
┃    ┌───────────────────────────────────────┐                              ┃
┃    │  Pi Approximation:                    │                              ┃
┃    │    • Monte Carlo method               │                              ┃
┃    │    • Max iterations = 1,000,000       │                              ┃
┃    └───────────────────────────────────────┘                              ┃
┃    ┌───────────────────────────────────────┐                              ┃
┃    │  Prime Numbers:                       │                              ┃
┃    │    • Sieve of Eratosthenes            │                              ┃
┃    │    • Max limit = 1,000,000            │                              ┃
┃    └───────────────────────────────────────┘                              ┃
┃                                                                             ┃
┃  Metrics:                                                                   ┃
┃    • compute_requests_total{operation="fibonacci|pi|prime"}               ┃
┃    • compute_duration_seconds                                              ┃
┃    • compute_cpu_usage_percent                                             ┃
┃    • compute_complexity{operation="..."}                                   ┃
┃                                                                             ┃
┃  Performance Target:                                                        ┃
┃    • Throughput: > 100 rps                                                 ┃
┃    • Latency: p95 < 500ms, p99 < 1000ms                                   ┃
┃                                                                             ┃
┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛


┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
┃  4️⃣  BULK SERVICE                                                         ┃
┃  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ┃
┃                                                                             ┃
┃  Port: 5040 (HTTP) / 5041 (HTTPS)                                         ┃
┃                                                                             ┃
┃  Endpoints:                                                                 ┃
┃    • POST /bulk?count={broj}                        → Sequential           ┃
┃    • POST /bulk/parallel?count={broj}               → Parallel             ┃
┃    • POST /bulk/batch?count={broj}&batchSize={n}    → Batch                ┃
┃    • GET  /bulk/status/{jobId}                      → Job status           ┃
┃    • GET  /health                                   → Health Check         ┃
┃    • GET  /metrics                                  → Prometheus Metrics   ┃
┃                                                                             ┃
┃  Processing Modes:                                                          ┃
┃                                                                             ┃
┃    📊 Sequential Processing:                                               ┃
┃       Item 1 → Item 2 → Item 3 → ... → Item N                             ┃
┃       (One at a time)                                                       ┃
┃                                                                             ┃
┃    ⚡ Parallel Processing:                                                 ┃
┃       Item 1 ┐                                                              ┃
┃       Item 2 ├─→ Process simultaneously                                    ┃
┃       Item 3 ┘                                                              ┃
┃       (All at once)                                                         ┃
┃                                                                             ┃
┃    📦 Batch Processing:                                                    ┃
┃       [Batch 1: Items 1-10] → [Batch 2: Items 11-20] → ...                ┃
┃       (Groups of items)                                                     ┃
┃                                                                             ┃
┃  Job Tracking:                                                              ┃
┃    ┌──────────────────────────────────┐                                   ┃
┃    │  In-Memory Job Status            │                                   ┃
┃    │  ────────────────────────────     │                                   ┃
┃    │  • JobId (Guid)                  │                                   ┃
┃    │  • TotalItems                    │                                   ┃
┃    │  • ProcessedItems                │                                   ┃
┃    │  • FailedItems                   │                                   ┃
┃    │  • Status (Running/Completed)    │                                   ┃
┃    │  • StartedAt, CompletedAt        │                                   ┃
┃    └──────────────────────────────────┘                                   ┃
┃                                                                             ┃
┃  Metrics:                                                                   ┃
┃    • bulk_jobs_total{type="sequential|parallel|batch"}                    ┃
┃    • bulk_items_processed_total                                            ┃
┃    • bulk_items_failed_total                                               ┃
┃    • bulk_processing_duration_seconds                                      ┃
┃    • bulk_active_jobs (gauge)                                              ┃
┃                                                                             ┃
┃  Performance Target:                                                        ┃
┃    • Throughput: > 50 jobs/minute                                          ┃
┃    • Latency: p95 < 2s, p99 < 5s                                          ┃
┃                                                                             ┃
┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛


                        ╔══════════════════════════════╗
                        ║   MONITORING & OBSERVABILITY  ║
                        ╚══════════════════════════════╝
                                       │
              ┌────────────────────────┼────────────────────────┐
              │                        │                        │
              ▼                        ▼                        ▼
    ┌──────────────────┐    ┌──────────────────┐    ┌──────────────────┐
    │   PROMETHEUS     │    │     GRAFANA      │    │    k6 LOAD       │
    │   Port: 9090     │    │   Port: 3000     │    │    TESTING       │
    ├──────────────────┤    ├──────────────────┤    ├──────────────────┤
    │                  │    │                  │    │                  │
    │ • Scrape all     │───▶│ • Dashboards     │    │ • Scenarios      │
    │   services       │    │ • Visualization  │    │ • Multiple VUs   │
    │ • Store metrics  │    │ • Alerting       │    │ • Thresholds     │
    │ • Query PromQL   │    │ • Real-time      │    │ • Custom metrics │
    │                  │    │                  │    │                  │
    │ Jobs:            │    │ Dashboards:      │    │ Tests:           │
    │  • api-gateway   │    │  • Overview      │    │  • All services  │
    │  • health-svc    │    │  • API Gateway   │    │  • Per service   │
    │  • order-svc     │    │  • Health        │    │  • Via Gateway   │
    │  • compute-svc   │    │  • Order         │    │  • Stress test   │
    │  • bulk-svc      │    │  • Compute       │    │  • Spike test    │
    │                  │    │  • Bulk          │    │                  │
    └──────────────────┘    └──────────────────┘    └──────────────────┘


═══════════════════════════════════════════════════════════════════════════════
                            DATA FLOW EXAMPLE
═══════════════════════════════════════════════════════════════════════════════

Scenario: User creates a new order through API Gateway

1. CLIENT
      │
      │ POST https://localhost:5001/api/orders
      │ Body: { "productName": "Laptop", "quantity": 1, "price": 999.99 }
      ▼
2. API GATEWAY
      │ • Validates request
      │ • Checks rate limit
      │ • Logs request
      │ • Increments gateway_requests_total{service="orders"}
      ▼
3. ORDER SERVICE
      │ • Receives request at POST /orders
      │ • Validates Order model
      │ • Saves to SQLite database
      │ • Increments orders_created_total
      │ • Records orders_database_query_duration_seconds
      │ • Logs: "Order created: ID=123"
      ▼
4. RESPONSE
      │ 201 Created
      │ { "id": 123, "productName": "Laptop", ... }
      ▼
5. METRICS COLLECTION
      │
      ├─▶ PROMETHEUS (scrapes every 5s)
      │     • http_requests_total
      │     • orders_created_total
      │     • http_request_duration_seconds
      │
      └─▶ GRAFANA (visualizes)
            • Updates "Orders Created" graph
            • Updates "Response Time" graph
            • Checks alert rules


═══════════════════════════════════════════════════════════════════════════════
                      KEY METRICS PER SERVICE
═══════════════════════════════════════════════════════════════════════════════

┌─────────────────┬────────────────────────────────────────────────────────┐
│     SERVICE     │                   KEY METRICS                           │
├─────────────────┼────────────────────────────────────────────────────────┤
│ API Gateway     │ • gateway_requests_total{service}                      │
│                 │ • gateway_request_duration_seconds                     │
│                 │ • gateway_errors_total                                 │
│                 │ • downstream_service_health{service}                   │
├─────────────────┼────────────────────────────────────────────────────────┤
│ HealthService   │ • health_ping_requests_total                           │
│                 │ • health_ping_duration_seconds                         │
├─────────────────┼────────────────────────────────────────────────────────┤
│ OrderService    │ • orders_created_total                                 │
│                 │ • orders_retrieved_total                               │
│                 │ • orders_total_count (gauge)                           │
│                 │ • orders_database_query_duration_seconds               │
├─────────────────┼────────────────────────────────────────────────────────┤
│ ComputeService  │ • compute_requests_total{operation}                    │
│                 │ • compute_duration_seconds                             │
│                 │ • compute_cpu_usage_percent                            │
├─────────────────┼────────────────────────────────────────────────────────┤
│ BulkService     │ • bulk_jobs_total{type}                                │
│                 │ • bulk_items_processed_total                           │
│                 │ • bulk_active_jobs (gauge)                             │
│                 │ • bulk_processing_duration_seconds                     │
└─────────────────┴────────────────────────────────────────────────────────┘


═══════════════════════════════════════════════════════════════════════════════
                        DOCKER COMPOSE OVERVIEW
═══════════════════════════════════════════════════════════════════════════════

┌────────────────────────────────────────────────────────────────────────────┐
│                       Docker Network: microservices-network                 │
│                                                                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │ api-gateway  │  │ health-svc   │  │ order-svc    │  │ compute-svc  │  │
│  │ :5000/:5001  │  │ :5010/:5011  │  │ :5020/:5021  │  │ :5030/:5031  │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘  │
│                                                                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                     │
│  │  bulk-svc    │  │ prometheus   │  │  grafana     │                     │
│  │ :5040/:5041  │  │    :9090     │  │    :3000     │                     │
│  └──────────────┘  └──────────────┘  └──────────────┘                     │
│                                                                              │
│  Volumes:                                                                    │
│    • order-data (SQLite database persistence)                               │
│    • prometheus-data (metrics storage)                                      │
│    • grafana-data (dashboards & config)                                     │
└────────────────────────────────────────────────────────────────────────────┘


═══════════════════════════════════════════════════════════════════════════════
                          DEPLOYMENT COMMANDS
═══════════════════════════════════════════════════════════════════════════════

# Build all services
dotnet build MicroservicesArchitecture.sln

# Start all containers
docker-compose up -d

# View logs
docker-compose logs -f

# Check service health
curl https://localhost:5001/api/health/ping

# Run load tests
k6 run load-testing/k6-all-services.js

# View Prometheus
http://localhost:9090

# View Grafana
http://localhost:3000 (admin/admin)

# Stop all
docker-compose down


═══════════════════════════════════════════════════════════════════════════════

