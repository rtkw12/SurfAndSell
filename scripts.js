import http from 'k6/http';
import { sleep } from 'k6';

export let options = {
    insecureSkipTLSVerify: true,
    noConnectionReuse: false,
    stages: [
        { duration: '15s', target: 100 },
        { duration: '15s', target: 100 },
        { duration: '30s', target: 1000 },
        { duration: '30s', target: 1000 },
        { duration: '2m', target: 2500 },
        { duration: '1m', target: 2500 },
        { duration: '1m', target: 3000 },
        { duration: '30s', target: 2500 },
        { duration: '2m', target: 0 },
    ]
}

const API_BASE_URL = "https://localhost:7922"

export default function () {
  http.get(API_BASE_URL + "/stores");
  sleep(1);
}