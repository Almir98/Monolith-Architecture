# Grafana Dashboard Import Guide

## 📊 .NET Memory Usage Dashboard

This dashboard displays comprehensive memory metrics for your MonolithApp .NET application.

---

## 🎯 Dashboard Panels

The dashboard includes 9 panels:

### **1. Working Set Memory (Gauge)**
- Shows current working set memory (physical RAM used by the process)
- Thresholds: Green < 200MB, Yellow < 500MB, Red > 500MB

### **2. .NET Total Memory (Gauge)**
- Shows total managed memory allocated by .NET runtime
- Thresholds: Green < 150MB, Yellow < 400MB, Red > 400MB

### **3. Memory Usage Over Time (Graph)**
- Time series showing Working Set, .NET Total Memory, and Private Memory
- Displays last and max values in the legend

### **4. Garbage Collection Count (Graph)**
- Shows GC collections by generation (Gen 0, Gen 1, Gen 2)
- Higher Gen 2 collections may indicate memory pressure

### **5. GC Heap Size by Generation (Stacked Bar Chart)**
- Displays heap size for each GC generation
- Stacked view shows total heap consumption

### **6. Virtual Memory (Stat)**
- Current virtual memory usage

### **7. Private Memory (Stat)**
- Current private memory (not shared with other processes)

### **8. GC Rate (Stat)**
- Garbage collection rate per minute
- Color-coded: Green < 5/min, Yellow < 10/min, Red > 10/min

### **9. Memory Usage in MB (Graph)**
- Smooth line graph showing memory trends in megabytes
- Includes mean, last, and max calculations

---

## 📥 How to Import

### **Method 1: Grafana UI (Recommended)**

1. **Open Grafana:**
   ```
   http://localhost:3000
   ```

2. **Login:**
   - Username: `admin`
   - Password: `admin`

3. **Navigate to Dashboards:**
   - Click **"+"** (Create) in the left sidebar
   - Select **"Import"**

4. **Import the Dashboard:**
   - Click **"Upload JSON file"**
   - Select: `monitoring/grafana/dashboards/dotnet-memory-dashboard.json`
   - OR paste the JSON content directly

5. **Configure:**
   - Select **Prometheus** as the data source
   - Click **"Import"**

---

### **Method 2: Docker Volume (Auto-provisioning)**

Update your `docker-compose.yml`:

```yaml
grafana:
  image: grafana/grafana:latest
  ports:
    - "3000:3000"
  volumes:
    - grafana-data:/var/lib/grafana
    - ./monitoring/grafana/dashboards:/etc/grafana/provisioning/dashboards:ro
    - ./monitoring/grafana/datasources:/etc/grafana/provisioning/datasources:ro
```

Create provisioning config: `monitoring/grafana/dashboards/dashboard.yml`

```yaml
apiVersion: 1

providers:
  - name: 'Default'
    orgId: 1
    folder: ''
    type: file
    disableDeletion: false
    updateIntervalSeconds: 10
    allowUiUpdates: true
    options:
      path: /etc/grafana/provisioning/dashboards
```

Restart Grafana:
```bash
docker-compose restart grafana
```

---

## 🔍 Metrics Explained

### **Key Prometheus Metrics Used:**

| Metric | Description |
|--------|-------------|
| `process_working_set_bytes` | Physical memory (RAM) used by the process |
| `dotnet_total_memory_bytes` | Total managed memory allocated by .NET |
| `process_private_memory_bytes` | Private memory not shared with other processes |
| `process_virtual_memory_bytes` | Virtual address space reserved |
| `dotnet_collection_count_total` | Number of GC collections by generation |
| `dotnet_gc_heap_size_bytes` | Size of GC heap by generation |

---

## 🎨 Customization

### **Change Time Range:**
- Top right corner → Time picker
- Options: Last 5m, 15m, 1h, 6h, 24h, etc.

### **Adjust Refresh Rate:**
- Default: 5 seconds
- Change in top right: Auto-refresh dropdown

### **Modify Thresholds:**
Edit panel → Field tab → Thresholds
- Example: Change "Yellow" threshold from 200MB to 300MB

### **Add Variables:**
If you have multiple .NET apps, add a variable:
1. Dashboard settings (gear icon)
2. Variables → Add variable
3. Query: `label_values(process_working_set_bytes, job)`
4. Update panels to use: `{job="$job"}`

---

## 📊 What to Monitor

### **Healthy Memory Behavior:**
- ✅ Working Set gradually increases then plateaus
- ✅ Gen 0/1 collections frequent, Gen 2 rare
- ✅ Memory releases after GC cycles
- ✅ No continuous upward trend

### **Memory Issues to Watch:**
- ⚠️ **Memory Leak:** Continuous upward trend without drops
- ⚠️ **Memory Pressure:** Frequent Gen 2 collections (> 10/min)
- ⚠️ **High Working Set:** > 500MB for a simple API
- ⚠️ **OutOfMemoryException:** Check logs if sudden drops

---

## 🧪 Testing the Dashboard

Run a load test to see metrics in action:

```bash
# Start the app
dotnet run

# In another terminal, run k6 test
k6 run load-testing/k6-script.js
```

**Watch the dashboard during the test:**
- Memory should increase during load
- GC collections should trigger to free memory
- After test, memory should stabilize

---

## 🔧 Troubleshooting

### **Issue: No Data Showing**

1. **Check Prometheus is scraping:**
   ```
   http://localhost:9090/targets
   ```
   - Ensure `monolith-app` target is **UP**

2. **Verify metrics endpoint:**
   ```
   https://localhost:62552/metrics
   ```
   - Should see metrics like `process_working_set_bytes`

3. **Check Grafana data source:**
   - Configuration → Data Sources → Prometheus
   - URL should be: `http://prometheus:9090`
   - Click "Save & Test" → Should show green checkmark

### **Issue: Dashboard shows "N/A"**

- App might not be running
- Prometheus might not have scraped yet (wait 5-15 seconds)
- Check job name in queries matches your Prometheus config

### **Issue: Metrics are delayed**

- Prometheus scrape interval is 5 seconds (normal)
- Grafana refresh is 5 seconds (can be reduced)
- Some delay is expected

---

## 📚 Additional Resources

- [Prometheus .NET Client Documentation](https://github.com/prometheus-net/prometheus-net)
- [Grafana Dashboards Guide](https://grafana.com/docs/grafana/latest/dashboards/)
- [.NET Memory Performance Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/)

---

## 🎉 You're All Set!

Your .NET memory dashboard is ready to monitor your application's memory usage in real-time!

**Dashboard URL:** http://localhost:3000/d/dotnet-memory-dashboard

