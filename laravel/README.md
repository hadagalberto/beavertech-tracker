# BeaverTech Tracker - Laravel SDK

Laravel SDK to send exceptions and logs to the BeaverTech error monitoring API.

## Install (local path)

From your Laravel app:

```bash
composer config repositories.beavertech-monitor path ./sdk/laravel
composer require beavertech/tracker-laravel
```

Publish config:

```bash
php artisan vendor:publish --tag=beavertech-error-monitor-config
```

## .env configuration

```
BEAVERTECH_ERROR_MONITOR_BASE_URL=https://your-beavertech.app
BEAVERTECH_ERROR_MONITOR_API_KEY=your_api_key
APP_VERSION=2026.1.5
```

Optional overrides:

```
BEAVERTECH_ERROR_MONITOR_ENVIRONMENT=production
BEAVERTECH_ERROR_MONITOR_RELEASE=2026.1.5
```

## Behavior

- Exceptions: captured via `ExceptionReported` event.
- Logs: captured via `Log::listen` with level filtering.
- Environment: defaults to `APP_ENV`.
- Release: defaults to `APP_VERSION`.

## Log level filtering

Edit `config/beavertech_error_monitor.php`:

```php
'log_levels' => ['error', 'warning', 'info'],
```

## Notes

- If `BEAVERTECH_ERROR_MONITOR_BASE_URL` or `BEAVERTECH_ERROR_MONITOR_API_KEY` is empty,
  the SDK does not send anything.
- Request and user context are collected automatically when available.
