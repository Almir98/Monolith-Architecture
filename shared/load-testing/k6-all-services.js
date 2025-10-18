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

const BASE_URL = 'http://localhost:5000/api';

export function testHealth() {
  const res = http.get(`${BASE_URL}/ping`, {
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
    productName: `Test Product ${__VU}-${__ITER}`,
    quantity: Math.floor(Math.random() * 10) + 1,
    price: Math.random() * 1000,
    status: 'Pending'
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
  });
  
  bulkTrend.add(res.timings.duration);
  
  sleep(2);
}

