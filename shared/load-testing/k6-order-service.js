import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { Rate, Counter } from 'k6/metrics';

const errorRate = new Rate('errors');
const ordersCreated = new Counter('orders_created');
const ordersRetrieved = new Counter('orders_retrieved');

export const options = {
  stages: [
    { duration: '30s', target: 10 },
    { duration: '1m', target: 20 },
    { duration: '30s', target: 30 },
    { duration: '1m', target: 30 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    'http_req_duration': ['p(95)<200', 'p(99)<500'],
    'http_req_failed': ['rate<0.05'],
  },
};

const BASE_URL = 'http://localhost:5020';

export default function () {
  group('Get Orders', () => {
    const res = http.get(`${BASE_URL}/orders`);
    
    check(res, {
      'get orders status is 200': (r) => r.status === 200,
      'response is array': (r) => Array.isArray(JSON.parse(r.body)),
    });
    
    ordersRetrieved.add(1);
  });
  
  group('Create Order', () => {
    const payload = JSON.stringify({
      productName: `Load Test Product ${__VU}-${__ITER}`,
      quantity: Math.floor(Math.random() * 100) + 1,
      price: (Math.random() * 1000).toFixed(2),
      status: ['Pending', 'Processing', 'Completed'][Math.floor(Math.random() * 3)]
    });
    
    const res = http.post(`${BASE_URL}/orders`, payload, {
      headers: { 'Content-Type': 'application/json' },
    });
    
    const success = check(res, {
      'create order status is 201': (r) => r.status === 201,
      'order has id': (r) => JSON.parse(r.body).id !== undefined,
    });
    
    if (success) {
      ordersCreated.add(1);
      
      const order = JSON.parse(res.body);
      
      // Get the created order
      group('Get Order by ID', () => {
        const getRes = http.get(`${BASE_URL}/orders/${order.id}`);
        check(getRes, {
          'get order by id status is 200': (r) => r.status === 200,
        });
      });
      
      // Update the order
      group('Update Order', () => {
        const updatePayload = JSON.stringify({
          productName: order.productName,
          quantity: order.quantity + 1,
          price: order.price,
          status: 'Completed'
        });
        
        const updateRes = http.put(`${BASE_URL}/orders/${order.id}`, updatePayload, {
          headers: { 'Content-Type': 'application/json' },
        });
        
        check(updateRes, {
          'update order status is 200': (r) => r.status === 200,
        });
      });
    }
    
    errorRate.add(!success);
  });
  
  group('Get Orders by Status', () => {
    const status = ['Pending', 'Processing', 'Completed'][Math.floor(Math.random() * 3)];
    const res = http.get(`${BASE_URL}/orders?status=${status}`);
    
    check(res, {
      'get orders by status is 200': (r) => r.status === 200,
    });
  });
  
  sleep(1);
}

