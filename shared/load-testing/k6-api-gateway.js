import http from 'k6/http';
import { check, group, sleep } from 'k6';

export const options = {
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

const BASE_URL = 'http://localhost:5000/api';

export default function () {
  group('Health Check via Gateway', () => {
    const res = http.get(`${BASE_URL}/ping`);
    check(res, {
      'gateway to health status 200': (r) => r.status === 200,
      'gateway to health response time < 50ms': (r) => r.timings.duration < 50,
    });
  });

  group('Orders via Gateway', () => {
    // Get orders
    const getRes = http.get(`${BASE_URL}/orders`);
    check(getRes, {
      'gateway to orders status 200': (r) => r.status === 200,
    });
    
    // Create order
    const payload = JSON.stringify({
      productName: `Gateway Test Product ${__VU}-${__ITER}`,
      quantity: Math.floor(Math.random() * 20) + 1,
      price: (Math.random() * 500).toFixed(2),
      status: 'Pending'
    });
    
    const createRes = http.post(`${BASE_URL}/orders`, payload, {
      headers: { 'Content-Type': 'application/json' },
    });
    
    check(createRes, {
      'gateway create order status 201': (r) => r.status === 201,
    });
  });

  group('Compute via Gateway', () => {
    const n = Math.floor(Math.random() * 30) + 10;
    const res = http.get(`${BASE_URL}/compute?n=${n}`);
    check(res, {
      'gateway to compute status 200': (r) => r.status === 200,
      'compute response time < 500ms': (r) => r.timings.duration < 500,
    });
  });

  group('Bulk via Gateway', () => {
    const count = Math.floor(Math.random() * 40) + 20;
    const res = http.post(`${BASE_URL}/bulk?count=${count}`);
    check(res, {
      'gateway to bulk status 200': (r) => r.status === 200,
    });
  });

  sleep(1);
}

