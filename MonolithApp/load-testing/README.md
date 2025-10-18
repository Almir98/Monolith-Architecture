# Load Testing

This directory contains load testing scripts for the MonolithApp using k6.

## k6 Load Testing

- **File**: `k6-script.js`
- **Status**: ✅ Working
- **Usage**: `k6 run k6-script.js`

## Quick Start

### Install k6
```bash
# Windows
choco install k6

# macOS
brew install k6

# Linux
# See https://k6.io/docs/getting-started/installation/
```

### Run the Load Test
```bash
k6 run k6-script.js
```

## Test Scenarios

The k6 test covers:
- Health check endpoint (`/health/ping`)
- Orders API (`/orders`)
- CPU-intensive computation (`/compute`)
- Bulk processing (`/bulk`)
- Order creation (`POST /orders`)

## Performance Metrics

The test measures:
- Response times (avg, min, max, p95, p99)
- Success/failure rates
- Throughput (requests per second)
- Error rates
- HTTP request duration

## Troubleshooting

If you encounter issues:
1. Ensure the MonolithApp is running on `http://localhost:5000` or `http://localhost:62553`
2. Check that all endpoints are accessible
3. Verify k6 installation: `k6 version`
4. Check if the base URL in the script matches your running application
