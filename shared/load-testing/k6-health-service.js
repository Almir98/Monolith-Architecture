import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Counter } from 'k6/metrics';

const errorRate = new Rate('errors');
const pingCounter = new Counter('ping_requests');

export const options = {
  stages: [
    { duration: '30s', target: 50 },
    { duration: '1m', target: 100 },
    { duration: '30s', target: 200 },
    { duration: '1m', target: 200 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    'http_req_duration': ['p(95)<50', 'p(99)<100'],
    'http_req_failed': ['rate<0.01'],
    'errors': ['rate<0.01'],
  },
};

const BASE_URL = 'http://localhost:5010';

export default function () {
  // Test ping endpoint
  const pingRes = http.get(`${BASE_URL}/ping`);
  
  const pingCheck = check(pingRes, {
    'ping status is 200': (r) => r.status === 200,
    'ping response is pong': (r) => r.body === 'pong',
    'ping response time < 50ms': (r) => r.timings.duration < 50,
  });
  
  pingCounter.add(1);
  errorRate.add(!pingCheck);
  
  // Test stats endpoint
  const statsRes = http.get(`${BASE_URL}/ping/stats`);
  
  check(statsRes, {
    'stats status is 200': (r) => r.status === 200,
    'stats has service name': (r) => JSON.parse(r.body).ServiceName === 'HealthService',
  });
  
  sleep(0.1);
}

