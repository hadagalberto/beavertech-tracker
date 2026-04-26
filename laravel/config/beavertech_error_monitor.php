<?php

return [
    'enabled' => env('BEAVERTECH_ERROR_MONITOR_ENABLED', true),
    'base_url' => env('BEAVERTECH_ERROR_MONITOR_BASE_URL', 'http://localhost'),
    'api_key' => env('BEAVERTECH_ERROR_MONITOR_API_KEY'),
    'timeout' => env('BEAVERTECH_ERROR_MONITOR_TIMEOUT', 2),
    'capture_exceptions' => env('BEAVERTECH_ERROR_MONITOR_CAPTURE_EXCEPTIONS', true),
    'capture_logs' => env('BEAVERTECH_ERROR_MONITOR_CAPTURE_LOGS', true),
    'log_levels' => [
        'debug',
        'info',
        'notice',
        'warning',
        'error',
        'critical',
        'alert',
        'emergency'
    ],
    'environment' => env('BEAVERTECH_ERROR_MONITOR_ENVIRONMENT', env('APP_ENV', 'production')),
    'release' => env('BEAVERTECH_ERROR_MONITOR_RELEASE', env('APP_VERSION')),
    'tags' => [],
];
