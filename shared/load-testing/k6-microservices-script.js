import http from 'k6/http';
import { check, sleep, group } from 'k6';

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

const API_GATEWAY_URL = 'http://localhost:8100';

export default function () {
  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  // ===== HEALTH SERVICE TESTS =====
  group('Health Service', () => {
    // Basic ping
    let response = http.get(`${API_GATEWAY_URL}/api/ping`);
    check(response, {
      'ping status is 200': (r) => r.status === 200,
      'ping response is pong': (r) => r.body === 'pong',
    });

    // Ping with delay
    response = http.get(`${API_GATEWAY_URL}/api/ping/delayed?delayMs=10`);
    check(response, {
      'delayed ping status is 200': (r) => r.status === 200,
      'delayed ping response is pong': (r) => r.body === 'pong',
    });

    // Ping stats
    response = http.get(`${API_GATEWAY_URL}/api/ping/stats`);
    check(response, {
      'ping stats status is 200': (r) => r.status === 200,
      'ping stats has service name': (r) => JSON.parse(r.body).ServiceName === 'HealthService',
    });
  });

  // ===== ORDER SERVICE TESTS =====
  group('Order Service', () => {
    // Get all orders
    let response = http.get(`${API_GATEWAY_URL}/api/orders`);
    check(response, {
      'get all orders status is 200': (r) => r.status === 200,
    });

    // Create order
    const orderPayload = JSON.stringify({
      productName: `Test Product ${__VU}-${__ITER}`,
      quantity: Math.floor(Math.random() * 10) + 1,
      price: (Math.random() * 1000).toFixed(2),
      status: 'Pending'
    });

    response = http.post(`${API_GATEWAY_URL}/api/orders`, orderPayload, params);
    let createdOrderId = null;
    const createSuccess = check(response, {
      'create order status is 201': (r) => r.status === 201,
      'created order has id': (r) => JSON.parse(r.body).id !== undefined,
    });

    if (createSuccess && response.status === 201) {
      createdOrderId = JSON.parse(response.body).id;

      // Get order by ID
      response = http.get(`${API_GATEWAY_URL}/api/orders/${createdOrderId}`);
      check(response, {
        'get order by id status is 200': (r) => r.status === 200,
        'order id matches': (r) => JSON.parse(r.body).id === createdOrderId,
      });

      // Update order
      const updatePayload = JSON.stringify({
        productName: `Updated Product ${__VU}-${__ITER}`,
        quantity: Math.floor(Math.random() * 20) + 1,
        price: (Math.random() * 2000).toFixed(2),
        status: 'Processing'
      });

      response = http.put(`${API_GATEWAY_URL}/api/orders/${createdOrderId}`, updatePayload, params);
      check(response, {
        'update order status is 200': (r) => r.status === 200,
      });

      // Delete order
      response = http.del(`${API_GATEWAY_URL}/api/orders/${createdOrderId}`);
      check(response, {
        'delete order status is 204': (r) => r.status === 204,
      });
    }

    // Get orders by status
    response = http.get(`${API_GATEWAY_URL}/api/orders?status=Pending`);
    check(response, {
      'get orders by status is 200': (r) => r.status === 200,
    });

    // Get order stats
    response = http.get(`${API_GATEWAY_URL}/api/orders/stats`);
    check(response, {
      'order stats status is 200': (r) => r.status === 200,
      'order stats has total count': (r) => JSON.parse(r.body).TotalOrders !== undefined,
    });
  });

  // ===== COMPUTE SERVICE TESTS =====
  group('Compute Service', () => {
    // Fibonacci calculation
    let response = http.get(`${API_GATEWAY_URL}/api/compute?n=30`);
    check(response, {
      'fibonacci status is 200': (r) => r.status === 200,
      'fibonacci has result': (r) => JSON.parse(r.body).result !== undefined,
    });

    // Pi approximation
    response = http.get(`${API_GATEWAY_URL}/api/compute/pi?iterations=10000`);
    check(response, {
      'pi approximation status is 200': (r) => r.status === 200,
      'pi has result': (r) => JSON.parse(r.body).result !== undefined,
    });

    // Prime numbers
    response = http.get(`${API_GATEWAY_URL}/api/compute/prime?limit=1000`);
    check(response, {
      'prime numbers status is 200': (r) => r.status === 200,
      'prime has count': (r) => JSON.parse(r.body).count !== undefined,
    });

    // Benchmark
    response = http.get(`${API_GATEWAY_URL}/api/compute/benchmark?iterations=100000`);
    check(response, {
      'benchmark status is 200': (r) => r.status === 200,
      'benchmark has result': (r) => JSON.parse(r.body).result !== undefined,
    });
  });

  // ===== BULK SERVICE TESTS =====
  group('Bulk Service', () => {
    // Sequential processing
    let response = http.post(`${API_GATEWAY_URL}/api/bulk?count=50`);
    check(response, {
      'bulk sequential status is 200': (r) => r.status === 200,
      'bulk sequential has job id': (r) => JSON.parse(r.body).jobId !== undefined,
    });

    // Parallel processing
    response = http.post(`${API_GATEWAY_URL}/api/bulk/parallel?count=50&maxDegree=4`);
    check(response, {
      'bulk parallel status is 200': (r) => r.status === 200,
      'bulk parallel has job id': (r) => JSON.parse(r.body).jobId !== undefined,
    });

    // Batch processing
    response = http.post(`${API_GATEWAY_URL}/api/bulk/batch?count=100&batchSize=20`);
    check(response, {
      'bulk batch status is 200': (r) => r.status === 200,
      'bulk batch has job id': (r) => JSON.parse(r.body).jobId !== undefined,
    });

    // Async job (start and check status)
    response = http.post(`${API_GATEWAY_URL}/api/bulk/async?count=200&type=sequential`);
    const asyncSuccess = check(response, {
      'bulk async started': (r) => r.status === 202,
      'bulk async has job id': (r) => JSON.parse(r.body).jobId !== undefined,
    });

    if (asyncSuccess && response.status === 202) {
      const jobId = JSON.parse(response.body).jobId;
      
      // Check job status
      sleep(1);
      response = http.get(`${API_GATEWAY_URL}/api/bulk/status/${jobId}`);
      check(response, {
        'bulk job status check': (r) => r.status === 200 || r.status === 404,
      });
    }
  });

  sleep(1); // Wait 1 second between iterations
}
