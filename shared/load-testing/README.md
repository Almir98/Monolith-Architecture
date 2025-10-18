# Load Testing

This directory contains load testing scripts for both monolith and microservices architectures.

## Available Scripts

### Monolith Testing
- **`k6-script.js`**: Tests the monolith application
- **Target**: http://localhost:5000
- **Endpoints**: Health, Orders, Compute, Bulk, Create Order

### Microservices Testing (Future)
- **`k6-microservices-script.js`**: Tests microservices architecture
- **Target**: API Gateway endpoints
- **Endpoints**: All microservice endpoints through gateway

## Test Scenarios

### Load Pattern
- **Ramp up**: 0 → 10 users (30s)
- **Sustain**: 10 users (1m)
- **Ramp up**: 10 → 20 users (30s)
- **Sustain**: 20 users (1m)
- **Ramp down**: 20 → 0 users (30s)

### Performance Thresholds
- **Response Time**: 95% of requests < 500ms
- **Error Rate**: < 10% failure rate
- **Throughput**: Monitor requests per second

## Running Tests

### Prerequisites
```bash
# Install k6
# Windows
choco install k6

# macOS
brew install k6

# Linux
# See https://k6.io/docs/getting-started/installation/
```

### Test Monolith
```bash
# Start monolith application
docker-compose up --build

# Run load test (in another terminal)
cd shared/load-testing
k6 run k6-script.js
```

### Test Microservices (Future)
```bash
# Start microservices
docker-compose -f docker-compose.microservices.yml up --build

# Run microservices load test
cd shared/load-testing
k6 run k6-microservices-script.js
```

## Test Endpoints

### Monolith Endpoints
- `GET /health/ping` - Health check
- `GET /orders` - List orders
- `GET /compute?n=30` - CPU computation
- `POST /bulk?count=50` - Bulk processing
- `POST /orders` - Create order

### Microservices Endpoints (Future)
- `GET /api/health` - Gateway health
- `GET /api/orders` - Orders service
- `GET /api/products` - Products service
- `GET /api/users` - Users service
- `POST /api/orders` - Create order

## Performance Metrics

### Key Metrics
- **Response Time**: Average, P95, P99
- **Throughput**: Requests per second
- **Error Rate**: Failed requests percentage
- **Concurrent Users**: Active virtual users

### Monitoring Integration
- **Prometheus**: Metrics collection during tests
- **Grafana**: Real-time performance visualization
- **Dashboards**: Performance comparison between architectures

## Customizing Tests

### Modify Load Pattern
Edit the `stages` array in the k6 script:
```javascript
stages: [
  { duration: '30s', target: 10 }, // Ramp up
  { duration: '1m', target: 10 },  // Sustain
  { duration: '30s', target: 20 }, // Ramp up
  { duration: '1m', target: 20 },  // Sustain
  { duration: '30s', target: 0 },  // Ramp down
]
```

### Add New Endpoints
Add new HTTP requests in the test function:
```javascript
// Test new endpoint
response = http.get(`${BASE_URL}/new-endpoint`);
check(response, {
  'new endpoint status is 200': (r) => r.status === 200,
});
```

### Adjust Thresholds
Modify the `thresholds` object:
```javascript
thresholds: {
  http_req_duration: ['p(95)<500'], // 95% < 500ms
  http_req_failed: ['rate<0.1'],    // < 10% errors
}
```

## Troubleshooting

### Common Issues
1. **Connection refused**: Ensure application is running
2. **Timeout errors**: Check application performance
3. **High error rates**: Verify endpoint availability
4. **k6 not found**: Install k6 properly

### Debug Commands
```bash
# Check if application is running
curl http://localhost:5000/health

# Test specific endpoint
curl http://localhost:5000/orders

# Check k6 installation
k6 version

# Run with verbose output
k6 run --verbose k6-script.js
```

## Results Analysis

### k6 Output
- **Summary**: Overall test results
- **Metrics**: Detailed performance data
- **Thresholds**: Pass/fail status
- **Timeline**: Performance over time

### Grafana Dashboards
- **Real-time metrics**: Live performance data
- **Historical data**: Performance trends
- **Comparison**: Monolith vs Microservices
- **Alerts**: Performance threshold violations