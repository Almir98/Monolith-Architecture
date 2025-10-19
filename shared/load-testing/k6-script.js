import http from 'k6/http';
import { check, sleep } from 'k6';

// Test configuration
export const options = {
  stages: [
    { duration: '30s', target: 10 }, // Ramp up to 10 users
    { duration: '1m', target: 10 },  // Stay at 10 users
    { duration: '30s', target: 20 }, // Ramp up to 20 users
    { duration: '1m', target: 20 },  // Stay at 20 users
    { duration: '30s', target: 0 },  // Ramp down to 0 users
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'], // 95% of requests must complete below 500ms
    http_req_failed: ['rate<0.1'],    // Error rate must be below 10%
  },
};

const BASE_URL = 'http://localhost:8000';

export default function () {
  // Test health endpoint
  let response = http.get(`${BASE_URL}/health/ping`);
  check(response, {
    'ping status is 200': (r) => r.status === 200,
    'ping response is pong': (r) => r.body === 'pong',
  });

  // Test orders endpoint
  response = http.get(`${BASE_URL}/orders`);
  check(response, {
    'orders status is 200': (r) => r.status === 200,
  });

  // Test compute endpoint
  response = http.get(`${BASE_URL}/compute?n=30`);
  check(response, {
    'compute status is 200': (r) => r.status === 200,
    'compute has result': (r) => JSON.parse(r.body).result !== undefined,
  });

  // Test bulk endpoint
  response = http.post(`${BASE_URL}/bulk?count=50`);
  check(response, {
    'bulk status is 200': (r) => r.status === 200,
    'bulk processed count': (r) => JSON.parse(r.body).processedCount === 50,
  });

  // Test creating a new order
  const orderPayload = JSON.stringify({
    productName: 'Test Product',
    quantity: 1,
    price: 99.99
  });

  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  response = http.post(`${BASE_URL}/orders`, orderPayload, params);
  check(response, {
    'create order status is 201': (r) => r.status === 201,
  });

  sleep(1); // Wait 1 second between iterations
}
