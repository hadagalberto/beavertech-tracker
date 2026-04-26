# BeaverTech Tracker - Laravel (pt-BR)

SDK para enviar exceptions e logs ao BeaverTech Tracker.

## Instalação (local)

```bash
composer config repositories.beavertech-tracker path ./sdk/laravel
composer require beavertech/tracker-laravel
```

Publicar a configuração:

```bash
php artisan vendor:publish --tag=beavertech-error-monitor-config
```

## Configuração no .env

```
BEAVERTECH_ERROR_MONITOR_BASE_URL=https://seu-dominio.com
BEAVERTECH_ERROR_MONITOR_API_KEY=sua_api_key
APP_VERSION=2026.1.5
```

Opcional:

```
BEAVERTECH_ERROR_MONITOR_ENVIRONMENT=production
BEAVERTECH_ERROR_MONITOR_RELEASE=2026.1.5
```

## Como funciona

- Exceptions: capturadas pelo evento `ExceptionReported`.
- Logs: capturados via `Log::listen`.
- Environment: usa `APP_ENV` por padrão.
- Release: usa `APP_VERSION` por padrão.

## Ajustes de log level

Edite `config/beavertech_error_monitor.php`:

```php
'log_levels' => ['error', 'warning', 'info'],
```

## Observações

- Se `BEAVERTECH_ERROR_MONITOR_BASE_URL` ou `BEAVERTECH_ERROR_MONITOR_API_KEY` estiverem vazios, nada sera enviado.
- Contexto de request e usuario e coletado automaticamente quando disponivel.
