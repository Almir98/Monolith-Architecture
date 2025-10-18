import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { Rate, Counter } from 'k6/metrics';

const errorRate = new Rate('errors');
const bulkJobs = new Counter('bulk_jobs');

export const options = {
  scenarios: {
    sequential_test: {
      executor: 'constant-vus',
      exec: 'testSequential',
      vus: 5,
      duration: '1m',
    },
    parallel_test: {
      executor: 'constant-vus',
      exec: 'testParallel',
      vus: 3,
      duration: '1m',
      startTime: '30s',
    },
    batch_test: {
      executor: 'ramping-vus',
      exec: 'testBatch',
      startVUs: 0,
      stages: [
        { duration: '20s', target: 2 },
        { duration: '40s', target: 5 },
        { duration: '20s', target: 0 },
      ],
    },
  },
  thresholds: {
    'http_req_duration': ['p(95)<5000'],
    'http_req_failed': ['rate<0.05'],
  },
};

const BASE_URL = 'http://localhost:5040/bulk';

export function testSequential() {
  const count = Math.floor(Math.random() * 50) + 10;
  const res = http.post(`${BASE_URL}?count=${count}`, null);
  
  const success = check(res, {
    'sequential status is 200': (r) => r.status === 200,
    'sequential has job id': (r) => JSON.parse(r.body).jobId !== undefined,
    'sequential processed count matches': (r) => JSON.parse(r.body).totalCount === count,
  });
  
  bulkJobs.add(1);
  errorRate.add(!success);
  
  sleep(2);
}

export function testParallel() {
  const count = Math.floor(Math.random() * 100) + 20;
  const maxDegree = Math.floor(Math.random() * 4) + 2;
  const res = http.post(`${BASE_URL}/parallel?count=${count}&maxDegree=${maxDegree}`, null);
  
  const success = check(res, {
    'parallel status is 200': (r) => r.status === 200,
    'parallel has job id': (r) => JSON.parse(r.body).jobId !== undefined,
    'parallel processed count matches': (r) => JSON.parse(r.body).totalCount === count,
  });
  
  bulkJobs.add(1);
  errorRate.add(!success);
  
  sleep(3);
}

export function testBatch() {
  const count = Math.floor(Math.random() * 100) + 50;
  const batchSize = Math.floor(Math.random() * 30) + 20;
  const res = http.post(`${BASE_URL}/batch?count=${count}&batchSize=${batchSize}`, null);
  
  const success = check(res, {
    'batch status is 200': (r) => r.status === 200,
    'batch has job id': (r) => JSON.parse(r.body).jobId !== undefined,
    'batch processed count matches': (r) => JSON.parse(r.body).totalCount === count,
  });
  
  bulkJobs.add(1);
  errorRate.add(!success);
  
  sleep(2);
}

export default function () {
  group('Async Bulk Job', () => {
    const count = Math.floor(Math.random() * 200) + 100;
    const type = ['sequential', 'parallel', 'batch'][Math.floor(Math.random() * 3)];
    
    const startRes = http.post(`${BASE_URL}/async?count=${count}&type=${type}`, null);
    
    const success = check(startRes, {
      'async job started': (r) => r.status === 202,
      'async job has id': (r) => JSON.parse(r.body).jobId !== undefined,
    });
    
    if (success) {
      const jobId = JSON.parse(startRes.body).jobId;
      bulkJobs.add(1);
      
      // Poll job status a few times
      for (let i = 0; i < 3; i++) {
        sleep(2);
        const statusRes = http.get(`${BASE_URL}/status/${jobId}`);
        check(statusRes, {
          'status check is 200 or 404': (r) => r.status === 200 || r.status === 404,
        });
      }
    }
    
    errorRate.add(!success);
  });
  
  sleep(3);
}

