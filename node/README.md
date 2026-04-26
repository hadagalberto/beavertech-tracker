# BeaverTech Tracker - Node.js SDK

Node.js SDK to send exceptions and logs to the BeaverTech error monitoring API.

## Install (local path)

From your Node app:

```bash
npm install ./sdk/node
```

## Environment variables

```
BEAVERTECH_ERROR_MONITOR_BASE_URL=https://your-beavertech.app
BEAVERTECH_ERROR_MONITOR_API_KEY=your_api_key
APP_VERSION=2026.1.5
NODE_ENV=production
```

Optional overrides:

```
BEAVERTECH_ERROR_MONITOR_ENVIRONMENT=production
BEAVERTECH_ERROR_MONITOR_RELEASE=2026.1.5
BEAVERTECH_ERROR_MONITOR_APPLICATION=my-node-app
```

## Basic usage

```js
const { init } = require('@beaver-tech/tracker');

init({
  requestProvider: () => ({
    method: 'GET',
    url: '/health',
    ip: '127.0.0.1'
  }),
  userProvider: () => ({
    id: '123',
    email: 'user@example.com'
  })
});
```

## Capture manually

```js
const { init } = require('@beaver-tech/tracker');

const client = init();

try {
  throw new Error('boom');
} catch (err) {
  client.captureException(err, { tags: { module: 'billing' } });
}

client.captureLog('warning', 'Something odd', { extra: { payloadId: 'abc' } });
```

## Options

```js
init({
  enabled: true,
  captureExceptions: true,
  captureLogs: true,
  captureConsole: true,
  timeout: 2000,
  logLevels: ['error', 'warning', 'info'],
  environment: 'production',
  release: '2026.1.5',
  application: 'node-app',
  tags: { tenant: 'alpha' },
  extra: { service: 'api' }
});
```

## Notes

- Exceptions are captured via `uncaughtException` and `unhandledRejection`.
- Logs are captured by patching `console.*` when `captureConsole` is enabled.
- If `BEAVERTECH_ERROR_MONITOR_BASE_URL` or `BEAVERTECH_ERROR_MONITOR_API_KEY` is empty, nothing is sent.
