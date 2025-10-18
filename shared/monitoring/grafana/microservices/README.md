# Microservices Monitoring Configuration

This directory contains Grafana dashboards and datasources specifically configured for the microservices architecture.

## 📁 Directory Structure

```
microservices/
├── dashboards/
│   ├── dashboard-provisioning.yml          # Dashboard provisioning config
│   ├── aspnet-core-dashboard.json         # ASP.NET Core dashboard (shared)
│   ├── dotnet-memory-dashboard.json       # .NET Memory dashboard (shared)
│   ├── microservices-overview-dashboard.json    # Microservices overview
│   └── microservices-services-dashboard.json    # Individual services performance
└── datasources/
    └── prometheus-datasource.yml          # Prometheus datasource config
```

## 🎯 Microservices-Specific Dashboards

### 1. **Microservices Overview Dashboard**
- **Purpose**: High-level view of all microservices
- **Metrics**: Request rates, response times, error rates across all services
- **Use Case**: Overall system health monitoring

### 2. **Microservices Services Dashboard**
- **Purpose**: Individual service performance
- **Metrics**: Per-service request rates, response times, error rates
- **Use Case**: Service-specific troubleshooting and optimization

### 3. **Shared Dashboards**
- **ASP.NET Core Dashboard**: Application metrics (works for both monolith and microservices)
- **.NET Memory Dashboard**: Memory usage and GC metrics

## 🔧 Configuration Details

### Dashboard Provisioning
- **Folder**: `Microservices` (separate from main dashboards)
- **Auto-import**: Dashboards are automatically imported on Grafana startup
- **Updates**: Dashboards can be updated through Grafana UI

### Data Source Configuration
- **Name**: `Prometheus-Microservices`
- **UID**: `prometheus-microservices`
- **URL**: `http://prometheus:9090`
- **Default**: Set as default datasource for microservices dashboards

## 📊 Available Metrics

### Service-Level Metrics
- **API Gateway**: Request routing and load balancing metrics
- **Health Service**: Health check endpoints
- **Order Service**: Order processing and database operations
- **Compute Service**: CPU-intensive computations
- **Bulk Service**: Bulk processing operations

### Key Performance Indicators
- **Request Rate**: Requests per second per service
- **Response Time**: P50, P95, P99 response times
- **Error Rate**: 4XX and 5XX error rates
- **Throughput**: Total requests processed
- **Availability**: Service uptime and health

## 🚀 Usage

### Automatic Setup
The microservices dashboards are automatically provisioned when you run:

```bash
# Start monitoring with microservices support
docker-compose -f docker-compose.monitoring.yml up -d

# Start microservices with monitoring
docker-compose -f docker-compose.microservices.yml up --build
```

### Manual Access
1. **Grafana**: http://localhost:3000 (admin/admin)
2. **Navigate to**: Dashboards → Browse → Microservices folder
3. **Available Dashboards**:
   - Microservices Overview
   - Microservices Services Performance
   - ASP.NET Core (shared)
   - .NET Memory (shared)

## 🔍 Dashboard Features

### Microservices Overview Dashboard
- **Service Comparison**: Side-by-side metrics for all services
- **Request Distribution**: How traffic is distributed across services
- **System Health**: Overall system performance indicators
- **Alerting**: Visual indicators for performance thresholds

### Microservices Services Dashboard
- **Individual Service Metrics**: Detailed performance per service
- **Service Dependencies**: How services interact with each other
- **Performance Trends**: Historical performance data
- **Troubleshooting**: Service-specific issue identification

## 📈 Performance Monitoring

### Key Metrics to Monitor
1. **Request Rate**: Should be consistent across services
2. **Response Time**: P95 should be under 500ms
3. **Error Rate**: Should be under 1%
4. **Memory Usage**: Should be stable and not growing
5. **CPU Usage**: Should correlate with request load

### Alerting Thresholds
- **High Response Time**: P95 > 1 second
- **High Error Rate**: > 5% errors
- **Service Down**: No requests for 5 minutes
- **Memory Leak**: Memory usage growing continuously

## 🛠️ Troubleshooting

### Common Issues
1. **No Data**: Check if Prometheus is scraping microservices
2. **Missing Services**: Verify all services are running and healthy
3. **Dashboard Errors**: Check datasource configuration
4. **Performance Issues**: Use service-specific dashboards for detailed analysis

### Debug Steps
1. **Check Prometheus Targets**: http://localhost:9090/targets
2. **Verify Service Health**: Check individual service endpoints
3. **Review Grafana Logs**: Check for configuration errors
4. **Test Data Source**: Verify Prometheus connectivity

## 🔄 Updates and Maintenance

### Adding New Services
1. Update Prometheus configuration to scrape new service
2. Add service-specific queries to dashboards
3. Update service lists in dashboard queries
4. Test new service metrics

### Dashboard Customization
1. **Edit Dashboards**: Use Grafana UI to modify dashboards
2. **Add Panels**: Create new panels for specific metrics
3. **Set Alerts**: Configure alerting rules for critical metrics
4. **Export/Import**: Share dashboard configurations

## 📚 Related Documentation

- **Main README**: Overall repository documentation
- **Load Testing**: Performance testing with k6
- **Docker Compose**: Service orchestration
- **Prometheus**: Metrics collection and storage
