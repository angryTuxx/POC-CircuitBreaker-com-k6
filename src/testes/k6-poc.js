import http from 'k6/http';
import { Trend, Rate, Counter } from 'k6/metrics';
import { check } from 'k6';

const latency = new Trend('latency');
const successRate = new Rate('http_2xx');
const failRate = new Rate('http_non_2xx');
const timeoutCount = new Counter('timeout_count');
const serverErrorCount = new Counter('server_error_count');
const highLatencyCount = new Counter('high_latency_count');

const TIMEOUT_THRESHOLD = 800; // ms

const PATH_TESTE = __ENV.PATH
const BASE_URL_POC = 'http://localhost:4000/api/poc';
const BASE_URL_POC_RECEIVER = 'http://localhost:5000/api/poc';

export const options = {
  scenarios: {
    ramp1: {
      executor: 'ramping-vus',
      exec: 'call',
      startVUs: 0,
      stages: [
        { duration: '10s', target: 1 },
        { duration: '10s', target: 0 },
      ],
    },

    prepareInstability: {
      executor: 'per-vu-iterations',
      exec: 'triggerInstability',
      vus: 1,
      iterations: 1,
      startTime: '20s',
    },

    ramp2: {
      executor: 'ramping-vus',
      exec: 'call',
      startVUs: 0,
      startTime: '25s',
      stages: [
        { duration: '10s', target: 1 },
        { duration: '10s', target: 0 },
      ],
    },

    prepareUnavaible: {
      executor: 'per-vu-iterations',
      exec: 'triggerUnavaible',
      vus: 1,
      iterations: 1,
      startTime: '45s',
    },

    ramp3: {
      executor: 'ramping-vus',
      exec: 'call',
      startVUs: 0,
      startTime: '50s',
      stages: [
        { duration: '10s', target: 1 },
        { duration: '10s', target: 0 },
      ],    
    },

     clearErrorStuffs: {
      executor: 'per-vu-iterations',
      exec: 'triggerClear',
      vus: 1,
      iterations: 1,
      startTime: '1m',
    },

    ramp4: {
      executor: 'ramping-vus',
      exec: 'call',
      startVUs: 0,
      startTime: '1m05s',
      stages: [
        { duration: '10s', target: 1 },
        { duration: '10s', target: 0 },
      ],    
    },
  },

  thresholds: {
    latency: ['p(95)<100', 'p(99)<150'],
    'http_2xx': ['rate>0.9'],
    'http_non_2xx': ['rate<0.1'],
  },
};

export function call() {
  let res;
  try {
    res = http.get(`${BASE_URL_POC}/${PATH_TESTE}`, { timeout: '5s' });
  } catch (e) {
    timeoutCount.add(1);
    failRate.add(1);
    return;
  }

  latency.add(res.timings.duration);

  if (res.status === 200) {
    successRate.add(1);
  } else {
    failRate.add(1);
    if (res.status >= 500 && res.status < 600) {
      serverErrorCount.add(1);
    }
  }

  if (res.timings.duration > TIMEOUT_THRESHOLD) {
    highLatencyCount.add(1);
  }

  check(res, {
    'status é 200': (r) => r.status === 200,
  });
}

export function triggerInstability() {
  console.log('instabilidade simulada...');
  http.get(`${BASE_URL_POC_RECEIVER}/instable`);
}

export function triggerUnavaible(){
  console.log('indisponibilidade simulada...');
  http.get(`${BASE_URL_POC_RECEIVER}/instable`);
  http.get(`${BASE_URL_POC_RECEIVER}/unavailable`);
}

export function triggerClear(){
  console.log('resetando cenarios de erro...');
  http.get(`${BASE_URL_POC_RECEIVER}/clear`);
}

function recordMetrics(res) {
  latency.add(res.timings.duration);
  successRate.add(res.status === 200);
  failRate.add(res.status !== 200);

  check(res, { 'status é 200': (r) => r.status === 200 });
}

export function teardown() {
  console.log('resetando sandbox errors...');
  http.get(`${BASE_URL_POC_RECEIVER}/clear`);
}