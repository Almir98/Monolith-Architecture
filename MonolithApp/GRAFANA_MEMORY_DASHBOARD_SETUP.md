# 📊 Grafana .NET Memory Dashboard - Quick Setup

## ✅ What Was Created

I've provisioned a complete Grafana dashboard setup for monitoring your .NET app's memory usage:

### **Files Created:**

1. **`monitoring/grafana/dashboards/dotnet-memory-dashboard.json`**
   - Complete dashboard JSON with 9 memory visualization panels

2. **`monitoring/grafana/dashboards/dashboard-provisioning.yml`**
   - Auto-provisioning configuration

3. **`monitoring/grafana/datasources/prometheus-datasource.yml`**
   - Prometheus data source configuration

4. **`monitoring/grafana/DASHBOARD_IMPORT_GUIDE.md`**
   - Detailed guide with troubleshooting

5. **Updated `docker-compose.yml`**
   - Added volume mounts for auto-provisioning

---

## 🚀 Quick Start (Auto-Import)

### **Step 1: Restart Grafana with new volumes**

```bash
# Stop current containers
docker-compose down

# Start with new configuration (includes dashboard auto-import)
docker-compose up -d
```

### **Step 2: Access Grafana**

```
http://localhost:3000
```

**Login:**
- Username: `admin`
- Password: `admin`

### **Step 3: Find Your Dashboard**

The dashboard will be **automatically imported**!

- Go to: **Dashboards** → **Browse**
- Look for: **".NET Memory Usage Dashboard"**
- Or direct URL: http://localhost:3000/d/dotnet-memory-dashboard

---

## 📊 Dashboard Panels Overview

| Panel | Description | What to Watch |
|-------|-------------|---------------|
| **Working Set Memory** | Physical RAM used | Keep < 500MB |
| **.NET Total Memory** | Managed heap memory | Should stabilize |
| **Memory Over Time** | Historical trends | Look for leaks |
| **GC Count** | Garbage collections | Gen 2 should be rare |
| **GC Heap Size** | Memory per generation | Balanced distribution |
| **Virtual Memory** | Reserved address space | Info only |
| **Private Memory** | Non-shared memory | Info only |
| **GC Rate** | Collections per minute | Keep < 10/min |
| **Memory (MB)** | Memory in megabytes | Easy to read |

---

## 🧪 Test the Dashboard

### **1. Start Your Application**

```bash
# In Visual Studio: Press F5
# OR in terminal:
dotnet run
```

### **2. Generate Some Load**

```bash
# Run k6 load test
k6 run load-testing/k6-script.js
```

### **3. Watch the Dashboard**

Open: http://localhost:3000/d/dotnet-memory-dashboard

**You should see:**
- ✅ Memory usage increasing during load
- ✅ GC collections triggering
- ✅ Memory stabilizing after test
- ✅ All metrics updating every 5 seconds

---

## 📈 Key Metrics Explained

### **1. Working Set Memory**
```
process_working_set_bytes
```
- **What:** Physical RAM used by your app
- **Good:** < 200MB idle, < 500MB under load
- **Bad:** Continuously growing (memory leak)

### **2. .NET Total Memory**
```
dotnet_total_memory_bytes
```
- **What:** Managed memory allocated by .NET
- **Good:** Increases then plateaus
- **Bad:** Never decreases (GC not running)

### **3. GC Collections**
```
dotnet_collection_count_total
```
- **Gen 0:** Frequent (normal)
- **Gen 1:** Occasional (normal)
- **Gen 2:** Rare (if frequent = memory pressure)

---

## 🎨 Customization Tips

### **Change Dashboard Refresh Rate:**
Top right → Auto-refresh dropdown → Select interval (5s, 10s, 30s)

### **Change Time Range:**
Top right → Time picker → Select range (Last 5m, 15m, 1h, etc.)

### **Add Alerts:**
1. Click panel title → Edit
2. Alert tab → Create alert rule
3. Set condition: `Working Set > 500MB for 5 minutes`
4. Save

### **Modify Thresholds:**
1. Click panel title → Edit
2. Field tab → Thresholds
3. Adjust values (e.g., change Yellow from 200MB to 300MB)

---

## 🔧 Troubleshooting

### **Problem: Dashboard not showing after restart**

**Solution:**
```bash
# Check Grafana logs
docker logs grafana

# Verify volume mounts
docker inspect grafana | grep Mounts -A 20

# Restart Grafana
docker-compose restart grafana
```

---

### **Problem: No data in panels**

**Check 1: Is Prometheus scraping?**
```
http://localhost:9090/targets
```
✅ Target `monolith-app` should be **UP**

**Check 2: Are metrics available?**
```
https://localhost:62552/metrics
```
✅ Should see `process_working_set_bytes` and similar

**Check 3: Is app running?**
```bash
# Test health endpoint
curl https://localhost:62552/health/ping -k
```
✅ Should return `pong`

---

### **Problem: "No data source" error**

**Solution:**
1. Go to: Configuration → Data Sources
2. Click "Add data source" → Select "Prometheus"
3. URL: `http://prometheus:9090`
4. Click "Save & Test"
5. Should show green checkmark

---

## 📚 Advanced: Manual Import

If auto-provisioning doesn't work, import manually:

1. Go to: **+** (Create) → **Import**
2. Click: **Upload JSON file**
3. Select: `monitoring/grafana/dashboards/dotnet-memory-dashboard.json`
4. Choose data source: **Prometheus**
5. Click: **Import**

---

## 🎯 What to Monitor During Load Testing

### **Normal Behavior:**
```
1. Start k6 test
2. Memory increases (app processing requests)
3. GC triggers (Gen 0/1 collections increase)
4. Memory drops slightly after GC
5. Test ends
6. Memory stabilizes at baseline
```

### **Memory Leak Behavior:**
```
1. Start k6 test
2. Memory increases
3. GC triggers but memory doesn't drop
4. Test ends
5. Memory stays high (doesn't return to baseline)
6. Repeat test → memory keeps growing
```

---

## 🔍 Useful PromQL Queries

Copy these into Grafana's Explore view (`http://localhost:9090`):

### **Memory Growth Rate:**
```promql
rate(process_working_set_bytes[5m])
```

### **Memory per Request:**
```promql
process_working_set_bytes / rate(http_requests_total[1m])
```

### **GC Efficiency:**
```promql
rate(dotnet_collection_count_total{generation="2"}[5m])
```

### **Memory After GC:**
```promql
dotnet_total_memory_bytes - dotnet_gc_heap_size_bytes
```

---

## 🎉 You're Done!

Your Grafana dashboard is ready to monitor .NET memory usage in real-time!

**Quick Links:**
- Dashboard: http://localhost:3000/d/dotnet-memory-dashboard
- Prometheus: http://localhost:9090
- Metrics: https://localhost:62552/metrics

**Next Steps:**
1. Run your app
2. Open the dashboard
3. Run a load test
4. Watch memory metrics live! 📊

