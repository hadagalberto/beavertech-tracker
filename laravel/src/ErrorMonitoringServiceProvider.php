<?php

namespace BeaverTech\ErrorMonitoring;

use Illuminate\Foundation\Exceptions\Events\ExceptionReported;
use Illuminate\Log\Events\MessageLogged;
use Illuminate\Support\Facades\Event;
use Illuminate\Support\Facades\Log;
use Illuminate\Support\ServiceProvider;

class ErrorMonitoringServiceProvider extends ServiceProvider
{
    public function register(): void
    {
        $this->mergeConfigFrom(__DIR__ . '/../config/beavertech_error_monitor.php', 'beavertech_error_monitor');

        $this->app->singleton(ErrorMonitoringClient::class, function () {
            return new ErrorMonitoringClient(config('beavertech_error_monitor'));
        });
    }

    public function boot(): void
    {
        $this->publishes([
            __DIR__ . '/../config/beavertech_error_monitor.php' => config_path('beavertech_error_monitor.php')
        ], 'beavertech-error-monitor-config');

        $client = $this->app->make(ErrorMonitoringClient::class);

        if (config('beavertech_error_monitor.capture_exceptions') && class_exists(ExceptionReported::class)) {
            Event::listen(ExceptionReported::class, function (ExceptionReported $event) use ($client) {
                $client->captureException($event->exception);
            });
        }

        if (config('beavertech_error_monitor.capture_logs')) {
            Log::listen(function (MessageLogged $event) use ($client) {
                $client->captureLog($event);
            });
        }
    }
}
