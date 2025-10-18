import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { Rate, Counter } from 'k6/metrics';

const errorRate = new Rate('errors');
const computeOps = new Counter('compute_operations');

export const options = {
  scenarios: {
    fibonacci_test: {
      executor: 'constant-vus',
      exec: 'testFibonacci',
      vus: 10,
      duration: '1m',
    },
    pi_test: {
      executor: 'constant-vus',
      exec: 'testPi',
      vus: 5,
      duration: '1m',
      startTime: '30s',
    },
    prime_test: {
      executor: 'ramping-vus',
      exec: 'testPrime',
      startVUs: 0,
      stages: [
        { duration: '30s', target: 5 },
        { duration: '1m', target: 10 },
        { duration: '30s', target: 0 },
      ],
    },
  },
  thresholds: {
    'http_req_duration': ['p(95)<1000', 'p(99)<2000'],
    'http_req_failed': ['rate<0.01'],
  },
};

const BASE_URL = 'http://localhost:5030/compute';

export function testFibonacci() {
  const n = Math.floor(Math.random() * 40) + 10;
  const res = http.get(`${BASE_URL}?n=${n}`);
  
  const success = check(res, {
    'fibonacci status is 200': (r) => r.status === 200,
    'fibonacci has result': (r) => JSON.parse(r.body).result !== undefined,
    'fibonacci response time < 500ms': (r) => r.timings.duration < 500,
  });
  
  computeOps.add(1);
  errorRate.add(!success);
  
  sleep(0.5);
}

export function testPi() {
  const iterations = Math.floor(Math.random() * 50000) + 10000;
  const res = http.get(`${BASE_URL}/pi?iterations=${iterations}`);
  
  const success = check(res, {
    'pi status is 200': (r) => r.status === 200,
    'pi has result': (r) => JSON.parse(r.body).result !== undefined,
    'pi approximation is reasonable': (r) => {
      const data = JSON.parse(r.body);
      return Math.abs(data.result - Math.PI) < 0.1;
    },
  });
  
  computeOps.add(1);
  errorRate.add(!success);
  
  sleep(1);
}

export function testPrime() {
  const limit = Math.floor(Math.random() * 5000) + 1000;
  const res = http.get(`${BASE_URL}/prime?limit=${limit}`);
  
  const success = check(res, {
    'prime status is 200': (r) => r.status === 200,
    'prime has count': (r) => JSON.parse(r.body).count !== undefined,
  });
  
  computeOps.add(1);
  errorRate.add(!success);
  
  sleep(0.7);
}

export default function () {
  group('Mixed Compute Load', () => {
    const operation = Math.floor(Math.random() * 3);
    
    if (operation === 0) {
      testFibonacci();
    } else if (operation === 1) {
      testPi();
    } else {
      testPrime();
    }
  });
}

