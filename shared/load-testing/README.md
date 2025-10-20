# Load Testing

Ovaj folder sadrži k6 skripte za load testiranje monolitne i mikroservisne arhitekture.

## 📋 Dostupne Skripte

### 1️⃣ `k6-monolith-script.js` - Testiranje Monolita
- **Target**: http://localhost:8000
- **Svrha**: Testira monolitnu aplikaciju
- **Endpoints**: Health, Orders (CRUD), Compute, Bulk

### 2️⃣ `k6-microservices-script.js` - Testiranje Mikroservisa
- **Target**: http://localhost:8100 (API Gateway)
- **Svrha**: Testira mikroservise kroz API Gateway
- **Endpoints**: Svi mikroservisni endpointi kroz gateway (Health, Orders, Compute, Bulk)

### 3️⃣ `k6-comparison.js` - Poređenje Oba Sistema
- **Target**: Oba sistema istovremeno
- **Svrha**: Paralelno testiranje monolita i mikroservisa za direktno poređenje performansi
- **Output**: Prikazuje komparativne metrike (response time, error rate, itd.)

---

## 🚀 Kako Pokrenuti Testove

### Preduslov: Instalacija k6

```bash
# Windows
choco install k6

# macOS
brew install k6

# Linux
# See https://k6.io/docs/getting-started/installation/
```

---

### Test 1: Monolith

```bash
# 1. Pokreni monolith aplikaciju
docker-compose -f docker-compose.monolith.yml up --build

# 2. U drugom terminalu, pokreni test
cd shared/load-testing
k6 run k6-monolith-script.js
```

---

### Test 2: Mikroservisi

```bash
# 1. Pokreni mikroservise
docker-compose -f docker-compose.microservices.yml up --build

# 2. U drugom terminalu, pokreni test
cd shared/load-testing
k6 run k6-microservices-script.js
```

---

### Test 3: Poređenje (Comparison)

**VAŽNO**: Oba sistema moraju biti pokrenuta!

```bash
# 1. Pokreni monolith (Terminal 1)
docker-compose -f docker-compose.monolith.yml up --build

# 2. Pokreni mikroservise (Terminal 2)
docker-compose -f docker-compose.microservices.yml up --build

# 3. Pokreni comparison test (Terminal 3)
cd shared/load-testing
k6 run k6-comparison.js
```

**Output**: Na kraju testa, dobićeš komparativni prikaz:
```
=====================================
MONOLITH vs MICROSERVICES COMPARISON
=====================================

Response Times:
  Monolith Average: 123.45 ms
  Microservices Average: 156.78 ms

Error Counts:
  Monolith Errors: 2
  Microservices Errors: 1

=====================================
```

---

## 📊 Test Scenario (Load Pattern)

Sve skripte koriste isti load pattern:

```javascript
stages: [
  { duration: '30s', target: 10 },  // Ramp up: 0 → 10 users
  { duration: '1m', target: 10 },   // Sustain: 10 users
  { duration: '30s', target: 20 },  // Ramp up: 10 → 20 users
  { duration: '1m', target: 20 },   // Sustain: 20 users
  { duration: '30s', target: 0 },   // Ramp down: 20 → 0 users
]
```

**Ukupno trajanje**: 3 minuta i 30 sekundi

---

## 🎯 Testirani Endpointi

### Monolith (k6-monolith-script.js)

| Endpoint | Method | Opis |
|----------|--------|------|
| `/health/ping` | GET | Health check |
| `/orders` | GET | Lista svih orders |
| `/orders` | POST | Kreiranje novog order-a |
| `/orders/{id}` | GET | Preuzimanje order-a po ID-u |
| `/orders/{id}` | PUT | Ažuriranje order-a |
| `/orders/{id}` | DELETE | Brisanje order-a |
| `/compute?n=30` | GET | CPU-intensive operacija (Fibonacci) |
| `/bulk?count=50` | POST | Bulk processing operacija |

### Mikroservisi (k6-microservices-script.js)

**Health Service:**
| Endpoint | Method | Opis |
|----------|--------|------|
| `/api/ping` | GET | Basic health check |
| `/api/ping/delayed?delayMs=X` | GET | Ping sa kašnjenjem |
| `/api/ping/stats` | GET | Statistika health servisa |

**Order Service:**
| Endpoint | Method | Opis |
|----------|--------|------|
| `/api/orders` | GET | Lista svih orders |
| `/api/orders` | POST | Kreiranje novog order-a |
| `/api/orders/{id}` | GET | Preuzimanje order-a po ID-u |
| `/api/orders/{id}` | PUT | Ažuriranje order-a |
| `/api/orders/{id}` | DELETE | Brisanje order-a |
| `/api/orders?status=X` | GET | Filtriranje po statusu |
| `/api/orders/stats` | GET | Statistika order-a |

**Compute Service:**
| Endpoint | Method | Opis |
|----------|--------|------|
| `/api/compute?n=X` | GET | Fibonacci kalkulacija |
| `/api/compute/pi?iterations=X` | GET | Pi aproksimacija |
| `/api/compute/prime?limit=X` | GET | Pronalaženje prostih brojeva |
| `/api/compute/benchmark?iterations=X` | GET | CPU benchmark |

**Bulk Service:**
| Endpoint | Method | Opis |
|----------|--------|------|
| `/api/bulk?count=X` | POST | Sequential processing |
| `/api/bulk/parallel?count=X&maxDegree=Y` | POST | Parallel processing |
| `/api/bulk/batch?count=X&batchSize=Y` | POST | Batch processing |
| `/api/bulk/async?count=X&type=Y` | POST | Pokretanje async job-a |
| `/api/bulk/status/{jobId}` | GET | Provera statusa job-a |

### Comparison Test (k6-comparison.js)

Testira osnovne endpointe na oba sistema (Health, Orders GET/POST, Compute, Bulk) za pravedno poređenje.

---

## 📈 Performance Thresholds

Sve skripte imaju definisane pragove:

- **Response Time**: 95% zahteva < 500ms
- **Error Rate**: < 10% neuspešnih zahteva

---

## 🔍 Prikazane Metrike

k6 automatski prikazuje:

- **http_req_duration**: Trajanje zahteva (avg, min, max, p90, p95)
- **http_req_failed**: Procenat neuspešnih zahteva
- **http_reqs**: Ukupan broj zahteva
- **vus**: Broj aktivnih virtual users
- **iterations**: Broj završenih iteracija

---

## 💡 Prilagođavanje Testova

### Promena Load Pattern-a

Edituj `stages` niz u skripti (bilo kojoj: k6-monolith-script.js, k6-microservices-script.js, k6-comparison.js):

```javascript
stages: [
  { duration: '1m', target: 50 },   // Više korisnika
  { duration: '5m', target: 50 },   // Duže testiranje
  { duration: '1m', target: 0 },
]
```

### Dodavanje Novih Endpointa

Dodaj novi HTTP zahtev u test funkciju:

```javascript
response = http.get(`${BASE_URL}/new-endpoint`);
check(response, {
  'new endpoint status is 200': (r) => r.status === 200,
});
```

### Prilagođavanje Thresholds

Promeni `thresholds` objekat:

```javascript
thresholds: {
  http_req_duration: ['p(95)<200'],  // Stroži prag
  http_req_failed: ['rate<0.05'],     // Manji error rate
}
```

---

## 🐛 Troubleshooting

### Problem: "Connection refused"
**Rešenje**: Proveri da li je aplikacija pokrenuta na odgovarajućem portu
```bash
# Proveri monolit
curl http://localhost:8000/health/ping

# Proveri mikroservise
curl http://localhost:8100/health/ping
```

### Problem: "Timeout errors"
**Rešenje**: Aplikacija je možda preopterećena, smanji broj virtual users

### Problem: "High error rates"
**Rešenje**: Proveri logove aplikacije za detalje o greškama
```bash
docker-compose logs -f
```

### Problem: "k6 not found"
**Rešenje**: Instaliraj k6 i proveri instalaciju
```bash
k6 version
```

---

## 📊 Integracija sa Monitoring-om

Tokom testiranja možeš pratiti performanse u realnom vremenu:

1. **Prometheus**: Prikuplja metrike (`http://localhost:9090`)
2. **Grafana**: Prikazuje dashboard-e (`http://localhost:3000`)

```bash
# Pokreni monitoring stack
docker-compose -f docker-compose.monitoring.yml up -d

# Pristup:
# - Grafana: http://localhost:3000 (admin/admin)
# - Prometheus: http://localhost:9090
```

---

## 📝 Primeri Rezultata

### Uspešan Test
```
✓ ping status is 200
✓ orders status is 200
✓ compute has result
✓ bulk processed count
✓ create order status is 201

checks.........................: 100.00% ✓ 500  ✗ 0
http_req_duration..............: avg=145ms  p(95)=320ms
http_req_failed................: 0.00%   ✓ 0    ✗ 500
http_reqs......................: 500     16.6/s
vus............................: 20      min=0  max=20
```

### Test sa Greškama
```
✗ create order status is 201
  ↳  95% — ✓ 475 / ✗ 25

checks.........................: 95.00%  ✓ 475  ✗ 25
http_req_failed................: 5.00%   ✓ 25   ✗ 475
```

---

## 🎓 Best Practices

1. **Zagrij sistem**: Pokreni kratki test pre glavnog testa
2. **Prati resurse**: Koristi Grafana tokom testiranja
3. **Testiraj realistične scenarije**: Prilagodi load pattern tvojim potrebama
4. **Analiziraj rezultate**: Ne gledaj samo proseke, već i p95/p99
5. **Ponavljaj testove**: Jedan test nije dovoljan za zaključke

---

## 📞 Dodatna Dokumentacija

- **k6 Docs**: https://k6.io/docs/
- **k6 Examples**: https://k6.io/docs/examples/
- **Best Practices**: https://k6.io/docs/testing-guides/

