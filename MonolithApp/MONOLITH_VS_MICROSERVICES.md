# ⚖️ Monolith vs Microservices - Detaljna Komparacija

## 📊 Brza Komparacija

| Aspekt | MonolithApp | MicroservicesArchitecture |
|--------|-------------|---------------------------|
| **Broj Servisa** | 1 aplikacija | 5 mikroservisa + Gateway |
| **Portovi** | 62552 (HTTPS), 62553 (HTTP) | 5000-5041 (12 portova) |
| **Projekti** | 1 .csproj | 6 .csproj + Shared |
| **Database** | 1 SQLite (orders.db) | 1 SQLite (samo OrderService) |
| **Deployment** | Single unit | Nezavisno po servisu |
| **Skaliranje** | Vertikalno | Horizontalno |
| **Development** | Jednostavno | Kompleksno |
| **Testing** | Unit + Integration | Unit + Integration + E2E |
| **Monitoring** | 1 Prometheus job | 6 Prometheus jobs |
| **Docker Kontejneri** | 3 (app, prometheus, grafana) | 8+ kontejnera |
| **Startup Time** | ~3-5 sekundi | ~20-30 sekundi (svi servisi) |
| **Memory Usage** | ~100-150 MB | ~500-800 MB (total) |
| **Latency** | Direktan poziv | +5-10ms (gateway overhead) |
| **Fault Isolation** | ❌ Cijela app pada | ✅ Samo affected service |
| **Technology Stack** | Jedinstven | Različit po servisu |
| **Team Size** | 1-5 developera | 5-20+ developera |
| **CI/CD** | 1 pipeline | 6+ pipelines |

---

## 🏗️ Arhitektura

### MonolithApp (Monolit)

```
┌────────────────────────────────────────┐
│          MonolithApp                   │
│                                        │
│  ┌──────────────────────────────┐    │
│  │ Controllers                   │    │
│  │  • HealthController           │    │
│  │  • OrdersController           │    │
│  │  • ComputeController          │    │
│  │  • BulkController             │    │
│  └──────────────────────────────┘    │
│                │                       │
│                ▼                       │
│  ┌──────────────────────────────┐    │
│  │ Services                      │    │
│  │  • OrderService               │    │
│  └──────────────────────────────┘    │
│                │                       │
│                ▼                       │
│  ┌──────────────────────────────┐    │
│  │ Data Layer                    │    │
│  │  • ApplicationDbContext       │    │
│  │  • SQLite Database            │    │
│  └──────────────────────────────┘    │
│                                        │
│  Port: 62552 (HTTPS)                  │
└────────────────────────────────────────┘
```

**Karakteristike:**
- ✅ Sve u jednom procesu
- ✅ Direktan poziv funkcija
- ✅ Jednostavno za debug
- ❌ Svi failure-i ruše cijelu app
- ❌ Teško skaliranje individualnih komponenti

---

### MicroservicesArchitecture (Mikroservisi)

```
                      ┌─────────────────┐
                      │  API Gateway    │
                      │   Port: 5001    │
                      └────────┬────────┘
                               │
        ┌──────────────────────┼──────────────────────┐
        │                      │                      │
        ▼                      ▼                      ▼
┌──────────────┐      ┌──────────────┐      ┌──────────────┐
│ HealthService│      │ OrderService │      │ComputeService│
│  Port: 5011  │      │  Port: 5021  │      │  Port: 5031  │
│              │      │     ┌────┐   │      │              │
│  No DB       │      │     │ DB │   │      │  No DB       │
│              │      │     └────┘   │      │              │
└──────────────┘      └──────────────┘      └──────────────┘
                               
                      ┌──────────────┐
                      │ BulkService  │
                      │  Port: 5041  │
                      │              │
                      │  No DB       │
                      └──────────────┘
```

**Karakteristike:**
- ✅ Nezavisno skaliranje
- ✅ Fault isolation
- ✅ Technology flexibility
- ✅ Team autonomy
- ❌ Kompleksniji deployment
- ❌ Network latency
- ❌ Distributed tracing needed

---

## 📝 Kod Struktura

### MonolithApp

```
MonolithApp/
├── Controllers/
│   ├── HealthController.cs      (30 lines)
│   ├── OrdersController.cs      (150 lines)
│   ├── ComputeController.cs     (60 lines)
│   └── BulkController.cs        (54 lines)
├── Services/
│   ├── IOrderService.cs         (12 lines)
│   └── OrderService.cs          (65 lines)
├── Models/
│   └── Order.cs                 (15 lines)
├── Data/
│   └── ApplicationDbContext.cs  (23 lines)
├── Program.cs                   (193 lines)
└── MonolithApp.csproj           (21 lines)

TOTAL: ~623 lines of code
```

---

### MicroservicesArchitecture

```
src/
├── ApiGateway/
│   ├── Program.cs               (~150 lines)
│   ├── ocelot.json              (~200 lines)
│   └── ApiGateway.csproj
│
├── HealthService/
│   ├── Controllers/
│   │   └── PingController.cs    (~40 lines)
│   ├── Program.cs               (~80 lines)
│   └── HealthService.csproj
│
├── OrderService/
│   ├── Controllers/
│   │   └── OrdersController.cs  (~180 lines)
│   ├── Services/
│   │   ├── IOrderService.cs     (~15 lines)
│   │   └── OrderService.cs      (~80 lines)
│   ├── Models/
│   │   └── Order.cs             (~20 lines)
│   ├── Data/
│   │   └── OrderDbContext.cs    (~30 lines)
│   ├── Program.cs               (~120 lines)
│   └── OrderService.csproj
│
├── ComputeService/
│   ├── Controllers/
│   │   └── ComputeController.cs (~100 lines)
│   ├── Services/
│   │   ├── IComputeService.cs   (~15 lines)
│   │   └── ComputeService.cs    (~80 lines)
│   ├── Program.cs               (~100 lines)
│   └── ComputeService.csproj
│
├── BulkService/
│   ├── Controllers/
│   │   └── BulkController.cs    (~120 lines)
│   ├── Services/
│   │   ├── IBulkService.cs      (~15 lines)
│   │   └── BulkService.cs       (~100 lines)
│   ├── Models/
│   │   └── BulkItem.cs          (~30 lines)
│   ├── Program.cs               (~100 lines)
│   └── BulkService.csproj
│
└── Shared/
    ├── Middleware/
    │   ├── MetricsMiddleware.cs (~80 lines)
    │   └── LoggingMiddleware.cs (~60 lines)
    ├── Extensions/
    │   └── ServiceExtensions.cs (~100 lines)
    └── Shared.csproj

TOTAL: ~2,500+ lines of code
```

**Razlika:** ~4x više koda u mikroservisnoj arhitekturi

---

## 🚀 Performance Komparacija

### Request Flow Latency

#### MonolithApp (Monolit)
```
Client → MonolithApp → Response
         └─ 10-50ms ─┘

Total Latency: 10-50ms
```

#### Microservices
```
Client → API Gateway → OrderService → Response
         └─ 5ms ─┘     └─ 10-50ms ─┘

Total Latency: 15-60ms (gateway overhead: +5-10ms)
```

---

### Resource Usage

| Metrika | MonolithApp | Microservices (Total) |
|---------|-------------|----------------------|
| **CPU (idle)** | 0.5% | 2-3% |
| **Memory** | 100-150 MB | 500-800 MB |
| **Startup Time** | 3-5 sek | 20-30 sek |
| **Docker Images** | 1 image (~200 MB) | 5 images (~1 GB total) |

---

### Throughput

#### Scenario: Create 1000 Orders

**MonolithApp:**
```
Direct database access
├─ Average: 100ms per request
├─ Throughput: ~600 rps
└─ Total time: ~10 seconds
```

**Microservices:**
```
Gateway → OrderService → Database
├─ Average: 110ms per request (gateway overhead)
├─ Throughput: ~540 rps
└─ Total time: ~11 seconds
```

**Winner:** Monolit (10% brži)

---

## 🔧 Development Experience

### Local Development Setup

#### MonolithApp
```bash
# 1. Clone repo
git clone ...

# 2. Restore packages
dotnet restore

# 3. Run app
dotnet run

# 4. Start monitoring (optional)
docker-compose up -d

TOTAL TIME: ~30 seconds
```

#### Microservices
```bash
# 1. Clone repo
git clone ...

# 2. Restore ALL projects
dotnet restore MicroservicesArchitecture.sln

# 3. Start monitoring
docker-compose up -d prometheus grafana

# 4. Start ALL services (6 terminal windows or Docker)
dotnet run --project src/ApiGateway
dotnet run --project src/HealthService
dotnet run --project src/OrderService
dotnet run --project src/ComputeService
dotnet run --project src/BulkService

# OR with Docker
docker-compose up -d

TOTAL TIME: ~5 minutes (first time)
```

**Winner:** Monolit (10x brže setup)

---

### Debugging

#### MonolithApp
```
✅ Single breakpoint u Visual Studio
✅ Step-through kroz cijeli flow
✅ Jedan log stream
✅ Jedan process za attach debugger
```

#### Microservices
```
⚠️ Multiple Visual Studio instances (ili Rider)
⚠️ 5 log streams (Docker logs)
⚠️ Distributed tracing needed (Jaeger/Zipkin)
⚠️ Network calls teže za debug
```

**Winner:** Monolit (jednostavnije)

---

### Testing Strategy

#### MonolithApp
```csharp
// Unit Tests
[Test]
public void OrderService_CreateOrder_ShouldReturnOrder()
{
    // Test service directly
}

// Integration Tests
[Test]
public async Task OrdersController_CreateOrder_ShouldReturn201()
{
    // Test with TestServer
    var client = _factory.CreateClient();
    var response = await client.PostAsync("/orders", ...);
}
```

**Test Types:**
- Unit Tests (Controllers, Services)
- Integration Tests (Full app)
- k6 Load Tests

---

#### Microservices
```csharp
// Unit Tests (per service)
[Test]
public void OrderService_CreateOrder_ShouldReturnOrder()
{
    // Same as monolith
}

// Integration Tests (per service)
[Test]
public async Task OrderService_CreateOrder_ShouldReturn201()
{
    // Test OrderService in isolation
}

// E2E Tests (through Gateway)
[Test]
public async Task ApiGateway_CreateOrder_ShouldRouteCorrectly()
{
    // Test Gateway → OrderService flow
}

// Contract Tests
[Test]
public void OrderService_ShouldMatchContract()
{
    // Verify API contract (Pact)
}
```

**Test Types:**
- Unit Tests (per service)
- Integration Tests (per service)
- Contract Tests (service boundaries)
- E2E Tests (through Gateway)
- k6 Load Tests (per service + Gateway)

**Winner:** Monolit (manje test scenarija)

---

## 📈 Monitoring & Observability

### MonolithApp

**Prometheus Jobs:** 1
```yaml
- job_name: 'monolith-app'
  static_configs:
    - targets: ['host.docker.internal:62552']
```

**Grafana Dashboards:** 1
- MonolithApp Overview

**Log Aggregation:** Jednostavno
- Jedan log stream (Serilog → Console)

**Metrics:** ~20-30 custom metrics

---

### Microservices

**Prometheus Jobs:** 6
```yaml
- job_name: 'api-gateway'
- job_name: 'health-service'
- job_name: 'order-service'
- job_name: 'compute-service'
- job_name: 'bulk-service'
- job_name: 'prometheus' (self-monitoring)
```

**Grafana Dashboards:** 6+
- Microservices Overview
- API Gateway
- HealthService
- OrderService
- ComputeService
- BulkService

**Log Aggregation:** Kompleksno
- 6 log streams
- Correlation IDs needed
- ELK Stack preporučen (Elasticsearch, Logstash, Kibana)

**Distributed Tracing:** Potreban
- Jaeger ili Zipkin
- Trace requests across services

**Metrics:** ~60-100 custom metrics

**Winner:** Microservices (bolji insights, ali kompleksnije)

---

## 💰 Cost Analysis

### MonolithApp

**Infrastructure (Cloud):**
- 1 VM instance (2 vCPU, 4GB RAM): **$40/mj**
- Monitoring (Prometheus/Grafana): **$20/mj**
- **TOTAL: ~$60/mj**

**Development:**
- 1-2 developera
- Salary: **$5,000-$10,000/mj**

**TOTAL COST: ~$5,060-$10,060/mj**

---

### Microservices

**Infrastructure (Cloud):**
- 5 service instances (1 vCPU, 2GB RAM each): **$100/mj**
- API Gateway (Load Balancer): **$30/mj**
- Monitoring (Prometheus/Grafana): **$50/mj**
- Log aggregation (ELK Stack): **$100/mj**
- Distributed tracing (Jaeger): **$30/mj**
- **TOTAL: ~$310/mj**

**Development:**
- 3-6 developera (multiple teams)
- Salary: **$15,000-$30,000/mj**

**TOTAL COST: ~$15,310-$30,310/mj**

**Razlika:** 3-5x skuplje (infrastructure + development)

---

## 🎯 Use Cases - Kada koristiti šta?

### Koristite MonolithApp ako:

✅ **Početak projekta** - Nepoznati requirements  
✅ **Mali tim** - 1-5 developera  
✅ **Brz time-to-market** - MVP u nedeljama  
✅ **Ograničen budžet** - Startup faza  
✅ **Jednostavan domain** - CRUD aplikacija  
✅ **Low traffic** - < 1,000 users  
✅ **Jedan tech stack** - .NET end-to-end  

**Primjeri:**
- Startup MVP
- Interni alati
- B2B SaaS (mali broj klijenata)
- Proof of concept

---

### Koristite Microservices ako:

✅ **Zreo proizvod** - Stabilni requirements  
✅ **Veliki tim** - 10+ developera  
✅ **Različiti tech stackovi** - .NET + Node.js + Python  
✅ **Visoka dostupnost** - 99.99% uptime  
✅ **High traffic** - > 10,000 users  
✅ **Nezavisno skaliranje** - Neki servisi opterećeniji  
✅ **Team autonomy** - Feature teams  
✅ **Fault isolation** - Partial failure OK  

**Primjeri:**
- Netflix, Amazon, Uber
- Enterprise aplikacije
- Multi-tenant SaaS (veliki broj klijenata)
- E-commerce platforme

---

## 🔄 Migracija: Monolit → Mikroservisi

### Strategija: Strangler Fig Pattern

```
Phase 1: Monolit (sve)
┌────────────────────────────────┐
│       MonolithApp              │
│  Health | Orders | Compute     │
└────────────────────────────────┘

Phase 2: Izvuci Health (prvi mikroservis)
┌─────────────┐  ┌────────────────┐
│HealthService│  │  MonolithApp   │
│             │  │ Orders|Compute │
└─────────────┘  └────────────────┘

Phase 3: Izvuci Orders
┌─────────────┐  ┌─────────────┐  ┌────────────┐
│HealthService│  │OrderService │  │ MonolithApp│
│             │  │             │  │  Compute   │
└─────────────┘  └─────────────┘  └────────────┘

Phase 4: Izvuci Compute
┌─────────────┐  ┌─────────────┐  ┌──────────────┐
│HealthService│  │OrderService │  │ComputeService│
└─────────────┘  └─────────────┘  └──────────────┘

Phase 5: Dodaj API Gateway
               ┌──────────────┐
               │  API Gateway │
               └──────┬───────┘
          ┌───────────┼───────────┐
          ▼           ▼           ▼
    ┌─────────┐ ┌─────────┐ ┌─────────┐
    │ Health  │ │ Order   │ │ Compute │
    └─────────┘ └─────────┘ └─────────┘
```

**Timeline:** 3-6 mjeseci  
**Team size:** 3-5 developera  
**Risk:** Srednji (postepena migracija)

---

## 📊 Decision Matrix

| Faktor | Važnost | Monolit | Mikroservisi |
|--------|---------|---------|--------------|
| **Time to Market** | ⭐⭐⭐⭐⭐ | ✅ Brzo | ❌ Sporo |
| **Scalability** | ⭐⭐⭐⭐ | ⚠️ Vertikalno | ✅ Horizontalno |
| **Development Speed** | ⭐⭐⭐⭐ | ✅ Brzo | ⚠️ Sporo |
| **Fault Isolation** | ⭐⭐⭐ | ❌ Ne | ✅ Da |
| **Technology Flexibility** | ⭐⭐⭐ | ❌ Jedan stack | ✅ Različiti |
| **Team Autonomy** | ⭐⭐⭐ | ⚠️ Ograničeno | ✅ Puno |
| **Operational Complexity** | ⭐⭐⭐⭐ | ✅ Jednostavno | ❌ Kompleksno |
| **Cost** | ⭐⭐⭐⭐⭐ | ✅ Nisko | ❌ Visoko |
| **Debugging** | ⭐⭐⭐⭐ | ✅ Jednostavno | ❌ Teško |
| **Monitoring** | ⭐⭐⭐ | ✅ Jednostavno | ⚠️ Potreban setup |

### Scoring:
- **Monolit:** 8/10 ✅ (za većinu projekata)
- **Mikroservisi:** 6/10 ⚠️ (za velike, kompleksne aplikacije)

---

## 🎓 Zaključak

### MonolithApp je bolji za:
- 🚀 **Brz start** - MVP za nedjelje
- 💰 **Ograničen budžet** - 3-5x jeftiniji
- 👨‍💻 **Mali tim** - 1-5 developera
- 🔧 **Jednostavno održavanje** - jedan deploy
- 🐛 **Lakše debugging** - direktan flow

### Microservices su bolji za:
- 📈 **Horizontalno skaliranje** - nezavisno po servisu
- 🛡️ **Fault isolation** - partial failure
- 👥 **Veliki tim** - autonomija po timu
- 🔀 **Technology diversity** - različiti stackovi
- 🌐 **Enterprise scale** - 10,000+ users

### Zlatno Pravilo:
> **"Start with a monolith, migrate to microservices when you have a reason to."**
> 
> — Martin Fowler

**Razlog za migraciju:**
1. ✅ Tim > 10 developera
2. ✅ Deployment bottleneck (redeploy cijele app svakih par dana)
3. ✅ Scalability issues (neki dijelovi opterećeniji)
4. ✅ Technology constraints (različiti stackovi)
5. ✅ Fault isolation kritičan (99.99% uptime)

**Ako nemaš ove probleme → OSTANI NA MONOLITU!** ✅

---

**TL;DR:** MonolithApp je 90% slučajeva bolji izbor. Mikroservisi su potrebni samo za velike, kompleksne sisteme sa velikim timovima.

