import http from 'k6/http';
import { check, sleep, group } from 'k6';

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
  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  // ===== HEALTH TESTS =====
  group('Health Check', () => {
    let response = http.get(`${BASE_URL}/health/ping`);
    check(response, {
      'ping status is 200': (r) => r.status === 200,
      'ping response is pong': (r) => r.body === 'pong',
    });
  });

  // ===== ORDER TESTS =====
  group('Order Operations', () => {
    // Get all orders
    let response = http.get(`${BASE_URL}/orders`);
    check(response, {
      'get all orders status is 200': (r) => r.status === 200,
    });

    // Create order
    const orderPayload = JSON.stringify({
      productName: `Test Product ${__VU}-${__ITER}`,
      quantity: Math.floor(Math.random() * 10) + 1,
      price: (Math.random() * 1000).toFixed(2)
    });

    response = http.post(`${BASE_URL}/orders`, orderPayload, params);
    let createdOrderId = null;
    const createSuccess = check(response, {
      'create order status is 201': (r) => r.status === 201,
      'created order has id': (r) => JSON.parse(r.body).id !== undefined,
    });

    if (createSuccess && response.status === 201) {
      createdOrderId = JSON.parse(response.body).id;

      // Get order by ID
      response = http.get(`${BASE_URL}/orders/${createdOrderId}`);
      check(response, {
        'get order by id status is 200': (r) => r.status === 200,
        'order id matches': (r) => JSON.parse(r.body).id === createdOrderId,
      });

      // Update order
      const updatePayload = JSON.stringify({
        productName: `Updated Product ${__VU}-${__ITER}`,
        quantity: Math.floor(Math.random() * 20) + 1,
        price: (Math.random() * 2000).toFixed(2)
      });

      response = http.put(`${BASE_URL}/orders/${createdOrderId}`, updatePayload, params);
      check(response, {
        'update order status is 200': (r) => r.status === 200,
      });

      // Delete order
      response = http.del(`${BASE_URL}/orders/${createdOrderId}`);
      check(response, {
        'delete order status is 204': (r) => r.status === 204,
      });
    }
  });

  // ===== COMPUTE TESTS =====
  group('Compute Operations', () => {
    // Fibonacci calculation
    let response = http.get(`${BASE_URL}/compute?n=30`);
    check(response, {
      'compute status is 200': (r) => r.status === 200,
      'compute has result': (r) => JSON.parse(r.body).result !== undefined,
    });
  });

  // ===== BULK TESTS =====
  group('Bulk Operations', () => {
    // Bulk processing
    let response = http.post(`${BASE_URL}/bulk?count=50`);
    check(response, {
      'bulk status is 200': (r) => r.status === 200,
      'bulk processed count': (r) => JSON.parse(r.body).processedCount === 50,
    });
  });

  sleep(1); // Wait 1 second between iterations
}

