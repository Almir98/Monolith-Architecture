import http from 'k6/http';
import { check, sleep } from 'k6';

// Test configuration for microservices
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

const API_GATEWAY_URL = 'http://localhost:7000';

export default function () {
  // Test API Gateway health
  let response = http.get(`${API_GATEWAY_URL}/health/ping`);
  check(response, {
    'gateway ping status is 200': (r) => r.status === 200,
    'gateway ping response is pong': (r) => r.body === 'pong',
  });

  // Test orders through API Gateway
  response = http.get(`${API_GATEWAY_URL}/orders`);
  check(response, {
    'orders through gateway status is 200': (r) => r.status === 200,
  });

  // Test compute through API Gateway
  response = http.get(`${API_GATEWAY_URL}/compute?n=30`);
  check(response, {
    'compute through gateway status is 200': (r) => r.status === 200,
    'compute through gateway has result': (r) => JSON.parse(r.body).result !== undefined,
  });

  // Test bulk through API Gateway
  response = http.post(`${API_GATEWAY_URL}/bulk?count=50`);
  check(response, {
    'bulk through gateway status is 200': (r) => r.status === 200,
    'bulk through gateway processed count': (r) => JSON.parse(r.body).processedCount === 50,
  });

  // Test creating a new order through API Gateway
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

  response = http.post(`${API_GATEWAY_URL}/orders`, orderPayload, params);
  check(response, {
    'create order through gateway status is 201': (r) => r.status === 201,
  });

  sleep(1); // Wait 1 second between iterations
}
