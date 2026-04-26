# BeaverTech Tracker - Node.js (pt-BR)

SDK para enviar exceptions e logs ao BeaverTech Tracker.

## Instalacao (local)

```bash
npm install ./sdk/node
```

## Configuracao no .env

```
BEAVERTECH_ERROR_MONITOR_BASE_URL=https://seu-dominio.com
BEAVERTECH_ERROR_MONITOR_API_KEY=sua_api_key
NODE_ENV=production
APP_VERSION=2026.1.5
```

Opcional:

```
BEAVERTECH_ERROR_MONITOR_ENVIRONMENT=production
BEAVERTECH_ERROR_MONITOR_RELEASE=2026.1.5
BEAVERTECH_ERROR_MONITOR_APPLICATION=meu-node-app
```

## Uso basico

```js
const { init } = require('@beaver-tech/tracker');

init();
```

## Providers (request/user)

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

## Captura manual

```js
const { init } = require('@beaver-tech/tracker');

const client = init();

try {
  throw new Error('boom');
} catch (err) {
  client.captureException(err, { tags: { modulo: 'billing' } });
}

client.captureLog('warning', 'Algo estranho', { extra: { payloadId: 'abc' } });
```

## Observacoes

- Exceptions sao capturadas por `uncaughtException` e `unhandledRejection`.
- Logs sao capturados ao interceptar `console.*` (pode desativar via `BEAVERTECH_ERROR_MONITOR_CAPTURE_CONSOLE=false`).
- Se `BEAVERTECH_ERROR_MONITOR_BASE_URL` ou `BEAVERTECH_ERROR_MONITOR_API_KEY` estiverem vazios, nada sera enviado.
