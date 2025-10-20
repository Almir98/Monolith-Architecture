import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { Trend, Counter } from 'k6/metrics';

// Custom metrics for comparison
const monolithDuration = new Trend('monolith_duration');
const microservicesDuration = new Trend('microservices_duration');
const monolithErrors = new Counter('monolith_errors');
const microservicesErrors = new Counter('microservices_errors');

export const options = {
  scenarios: {
    // Test monolith
    monolith_load: {
      executor: 'ramping-vus',
      exec: 'testMonolith',
      startVUs: 0,
      stages: [
        { duration: '30s', target: 10 },
        { duration: '1m', target: 10 },
        { duration: '30s', target: 20 },
        { duration: '1m', target: 20 },
        { duration: '30s', target: 0 },
      ],
      tags: { architecture: 'monolith' },
    },
    // Test microservices
    microservices_load: {
      executor: 'ramping-vus',
      exec: 'testMicroservices',
      startVUs: 0,
      stages: [
        { duration: '30s', target: 10 },
        { duration: '1m', target: 10 },
        { duration: '30s', target: 20 },
        { duration: '1m', target: 20 },
        { duration: '30s', target: 0 },
      ],
      tags: { architecture: 'microservices' },
    },
  },
  thresholds: {
    'http_req_duration{architecture:monolith}': ['p(95)<500'],
    'http_req_duration{architecture:microservices}': ['p(95)<500'],
    'http_req_failed{architecture:monolith}': ['rate<0.1'],
    'http_req_failed{architecture:microservices}': ['rate<0.1'],
  },
};

const MONOLITH_URL = 'http://localhost:8000';
const MICROSERVICES_URL = 'http://localhost:8100';

const params = {
  headers: {
    'Content-Type': 'application/json',
  },
};

// Test Monolith
export function testMonolith() {
  group('Monolith - Health', () => {
    let start = Date.now();
    let response = http.get(`${MONOLITH_URL}/health/ping`, {
      tags: { endpoint: 'health', architecture: 'monolith' },
    });
    monolithDuration.add(Date.now() - start);
    
    let success = check(response, {
      'monolith ping status is 200': (r) => r.status === 200,
      'monolith ping response is pong': (r) => r.body === 'pong',
    });
    if (!success) monolithErrors.add(1);
  });

  group('Monolith - Orders', () => {
    // Get orders
    let start = Date.now();
    let response = http.get(`${MONOLITH_URL}/orders`, {
      tags: { endpoint: 'orders-get', architecture: 'monolith' },
    });
    monolithDuration.add(Date.now() - start);
    
    let success = check(response, {
      'monolith get orders status is 200': (r) => r.status === 200,
    });
    if (!success) monolithErrors.add(1);

    // Create order
    const orderPayload = JSON.stringify({
      productName: `Monolith Test ${__VU}-${__ITER}`,
      quantity: Math.floor(Math.random() * 10) + 1,
      price: (Math.random() * 1000).toFixed(2)
    });

    start = Date.now();
    response = http.post(`${MONOLITH_URL}/orders`, orderPayload, params);
    monolithDuration.add(Date.now() - start);
    
    success = check(response, {
      'monolith create order status is 201': (r) => r.status === 201,
    });
    if (!success) monolithErrors.add(1);
  });

  group('Monolith - Compute', () => {
    let start = Date.now();
    let response = http.get(`${MONOLITH_URL}/compute?n=30`, {
      tags: { endpoint: 'compute', architecture: 'monolith' },
    });
    monolithDuration.add(Date.now() - start);
    
    let success = check(response, {
      'monolith compute status is 200': (r) => r.status === 200,
      'monolith compute has result': (r) => JSON.parse(r.body).result !== undefined,
    });
    if (!success) monolithErrors.add(1);
  });

  group('Monolith - Bulk', () => {
    let start = Date.now();
    let response = http.post(`${MONOLITH_URL}/bulk?count=50`, null, {
      tags: { endpoint: 'bulk', architecture: 'monolith' },
    });
    monolithDuration.add(Date.now() - start);
    
    let success = check(response, {
      'monolith bulk status is 200': (r) => r.status === 200,
      'monolith bulk processed count': (r) => JSON.parse(r.body).processedCount === 50,
    });
    if (!success) monolithErrors.add(1);
  });

  sleep(1);
}

// Test Microservices
export function testMicroservices() {
  group('Microservices - Health', () => {
    let start = Date.now();
    let response = http.get(`${MICROSERVICES_URL}/api/ping`, {
      tags: { endpoint: 'health', architecture: 'microservices' },
    });
    microservicesDuration.add(Date.now() - start);
    
    let success = check(response, {
      'microservices ping status is 200': (r) => r.status === 200,
      'microservices ping response is pong': (r) => r.body === 'pong',
    });
    if (!success) microservicesErrors.add(1);
  });

  group('Microservices - Orders', () => {
    // Get orders
    let start = Date.now();
    let response = http.get(`${MICROSERVICES_URL}/api/orders`, {
      tags: { endpoint: 'orders-get', architecture: 'microservices' },
    });
    microservicesDuration.add(Date.now() - start);
    
    let success = check(response, {
      'microservices get orders status is 200': (r) => r.status === 200,
    });
    if (!success) microservicesErrors.add(1);

    // Create order
    const orderPayload = JSON.stringify({
      productName: `Microservices Test ${__VU}-${__ITER}`,
      quantity: Math.floor(Math.random() * 10) + 1,
      price: (Math.random() * 1000).toFixed(2),
      status: 'Pending'
    });

    start = Date.now();
    response = http.post(`${MICROSERVICES_URL}/api/orders`, orderPayload, params);
    microservicesDuration.add(Date.now() - start);
    
    success = check(response, {
      'microservices create order status is 201': (r) => r.status === 201,
    });
    if (!success) microservicesErrors.add(1);
  });

  group('Microservices - Compute', () => {
    let start = Date.now();
    let response = http.get(`${MICROSERVICES_URL}/api/compute?n=30`, {
      tags: { endpoint: 'compute', architecture: 'microservices' },
    });
    microservicesDuration.add(Date.now() - start);
    
    let success = check(response, {
      'microservices compute status is 200': (r) => r.status === 200,
      'microservices compute has result': (r) => JSON.parse(r.body).result !== undefined,
    });
    if (!success) microservicesErrors.add(1);
  });

  group('Microservices - Bulk', () => {
    let start = Date.now();
    let response = http.post(`${MICROSERVICES_URL}/api/bulk?count=50`, null, {
      tags: { endpoint: 'bulk', architecture: 'microservices' },
    });
    microservicesDuration.add(Date.now() - start);
    
    let success = check(response, {
      'microservices bulk status is 200': (r) => r.status === 200,
      'microservices bulk has job id': (r) => JSON.parse(r.body).jobId !== undefined,
    });
    if (!success) microservicesErrors.add(1);
  });

  sleep(1);
}

export function handleSummary(data) {
  console.log('\n=====================================');
  console.log('MONOLITH vs MICROSERVICES COMPARISON');
  console.log('=====================================\n');
  
  // Calculate overall metrics
  const monolithMetrics = {
    avgDuration: data.metrics.monolith_duration?.values.avg?.toFixed(2) || 'N/A',
    p95Duration: data.metrics['http_req_duration{architecture:monolith}']?.values['p(95)']?.toFixed(2) || 'N/A',
    errors: data.metrics.monolith_errors?.values.count || 0,
    requests: data.metrics['http_reqs{architecture:monolith}']?.values.count || 0,
  };

  const microservicesMetrics = {
    avgDuration: data.metrics.microservices_duration?.values.avg?.toFixed(2) || 'N/A',
    p95Duration: data.metrics['http_req_duration{architecture:microservices}']?.values['p(95)']?.toFixed(2) || 'N/A',
    errors: data.metrics.microservices_errors?.values.count || 0,
    requests: data.metrics['http_reqs{architecture:microservices}']?.values.count || 0,
  };
  
  console.log('Response Times (Custom Metrics):');
  console.log('  Monolith Average:       ', monolithMetrics.avgDuration, 'ms');
  console.log('  Microservices Average:  ', microservicesMetrics.avgDuration, 'ms');
  
  console.log('\nResponse Times P95 (HTTP Metrics):');
  console.log('  Monolith P95:           ', monolithMetrics.p95Duration, 'ms');
  console.log('  Microservices P95:      ', microservicesMetrics.p95Duration, 'ms');
  
  console.log('\nError Counts:');
  console.log('  Monolith Errors:        ', monolithMetrics.errors);
  console.log('  Microservices Errors:   ', microservicesMetrics.errors);
  
  console.log('\nTotal Requests:');
  console.log('  Monolith Requests:      ', monolithMetrics.requests);
  console.log('  Microservices Requests: ', microservicesMetrics.requests);
  
  // Calculate winner
  if (monolithMetrics.avgDuration !== 'N/A' && microservicesMetrics.avgDuration !== 'N/A') {
    const monoDuration = parseFloat(monolithMetrics.avgDuration);
    const microDuration = parseFloat(microservicesMetrics.avgDuration);
    const difference = Math.abs(monoDuration - microDuration).toFixed(2);
    const percentDiff = ((difference / Math.min(monoDuration, microDuration)) * 100).toFixed(1);
    
    console.log('\nPerformance Comparison:');
    if (monoDuration < microDuration) {
      console.log(`  ✅ Monolith is FASTER by ${difference}ms (${percentDiff}%)`);
    } else if (microDuration < monoDuration) {
      console.log(`  ✅ Microservices is FASTER by ${difference}ms (${percentDiff}%)`);
    } else {
      console.log('  ⚖️  Both architectures have EQUAL performance');
    }
  }
  
  console.log('\n=====================================\n');
  
  return {
    'stdout': JSON.stringify(data, null, 2),
  };
}
