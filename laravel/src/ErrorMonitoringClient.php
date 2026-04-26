<?php

namespace BeaverTech\ErrorMonitoring;

use Illuminate\Log\Events\MessageLogged;
use Illuminate\Support\Facades\Auth;
use Illuminate\Support\Facades\Http;
use Throwable;

class ErrorMonitoringClient
{
    private const IngestPath = '/api/error-ingest';
    private static bool $sending = false;

    private bool $enabled;
    private string $baseUrl;
    private ?string $apiKey;
    private int $timeout;
    private bool $captureExceptions;
    private bool $captureLogs;
    private array $logLevels;
    private ?string $environment;
    private ?string $release;
    private array $tags;
    private ?string $application;

    public function __construct(array $config)
    {
        $this->enabled = (bool) ($config['enabled'] ?? true);
        $this->baseUrl = rtrim((string) ($config['base_url'] ?? ''), '/');
        $this->apiKey = $config['api_key'] ?? null;
        $this->timeout = (int) ($config['timeout'] ?? 2);
        $this->captureExceptions = (bool) ($config['capture_exceptions'] ?? true);
        $this->captureLogs = (bool) ($config['capture_logs'] ?? true);
        $this->logLevels = array_map('strtolower', $config['log_levels'] ?? []);
        $this->environment = $config['environment'] ?? null;
        $this->release = $config['release'] ?? null;
        $this->tags = $config['tags'] ?? [];
        $this->application = config('app.name');
    }

    public function captureException(Throwable $exception, array $context = []): void
    {
        if (!$this->enabled || !$this->captureExceptions || $this->isBusy()) {
            return;
        }

        $payload = $this->buildPayloadFromException($exception, $context);
        $this->send($payload);
    }

    public function captureLog(MessageLogged $event): void
    {
        if (!$this->enabled || !$this->captureLogs || $this->isBusy()) {
            return;
        }

        if (!$this->shouldCaptureLevel($event->level)) {
            return;
        }

        if (isset($event->context['exception']) && $event->context['exception'] instanceof Throwable) {
            $this->captureException($event->context['exception'], $event->context);
            return;
        }

        $payload = $this->buildPayloadFromLog($event);
        $this->send($payload);
    }

    private function buildPayloadFromException(Throwable $exception, array $context = []): array
    {
        $payload = $this->buildBasePayload();
        $payload['exception'] = [
            'type' => get_class($exception),
            'message' => $exception->getMessage(),
            'stackTrace' => $exception->getTraceAsString()
        ];

        $payload['tags'] = $this->mergeTags($context);
        $payload['extra'] = $this->normalizeContext($context);

        return $payload;
    }

    private function buildPayloadFromLog(MessageLogged $event): array
    {
        $payload = $this->buildBasePayload();
        $payload['exception'] = [
            'type' => 'Log',
            'message' => $event->message,
            'stackTrace' => ''
        ];

        $tags = [
            'log.level' => strtolower($event->level),
            'log.channel' => $event->channel ?? 'default'
        ];

        $payload['tags'] = $this->mergeTags($event->context, $tags);
        $payload['extra'] = $this->normalizeContext($event->context);

        return $payload;
    }

    private function buildBasePayload(): array
    {
        return [
            'application' => $this->application,
            'environment' => $this->environment,
            'release' => $this->release,
            'timestamp' => now()->toIso8601String(),
            'request' => $this->resolveRequest(),
            'user' => $this->resolveUser()
        ];
    }

    private function resolveRequest(): ?array
    {
        if (!app()->bound('request')) {
            return null;
        }

        $request = app('request');
        if ($request === null) {
            return null;
        }

        return [
            'method' => $request->method(),
            'url' => $request->fullUrl(),
            'ip' => $request->ip()
        ];
    }

    private function resolveUser(): ?array
    {
        try {
            $user = Auth::user();
            if ($user === null) {
                return null;
            }

            return [
                'id' => (string) ($user->id ?? ''),
                'email' => (string) ($user->email ?? '')
            ];
        } catch (Throwable $e) {
            return null;
        }
    }

    private function mergeTags(array $context, array $extra = []): array
    {
        $tags = array_merge($this->tags, $extra);
        if (isset($context['tags']) && is_array($context['tags'])) {
            $tags = array_merge($tags, $context['tags']);
        }

        return $tags;
    }

    private function shouldCaptureLevel(string $level): bool
    {
        if (empty($this->logLevels)) {
            return true;
        }

        return in_array(strtolower($level), $this->logLevels, true);
    }

    private function send(array $payload): void
    {
        if (empty($this->baseUrl) || empty($this->apiKey)) {
            return;
        }

        $url = $this->baseUrl . self::IngestPath;

        self::$sending = true;
        try {
            Http::timeout($this->timeout)
                ->withHeaders(['X-BT-ApiKey' => $this->apiKey])
                ->post($url, $payload);
        } catch (Throwable $e) {
            // swallow
        } finally {
            self::$sending = false;
        }
    }

    private function isBusy(): bool
    {
        return self::$sending;
    }

    private function normalizeContext(array $context): array
    {
        $normalized = [];
        foreach ($context as $key => $value) {
            $normalized[$key] = $this->normalizeValue($value);
        }

        return $normalized;
    }

    private function normalizeValue(mixed $value): mixed
    {
        if (is_null($value) || is_scalar($value)) {
            return $value;
        }

        if (is_array($value)) {
            $items = [];
            foreach ($value as $key => $item) {
                $items[$key] = $this->normalizeValue($item);
            }
            return $items;
        }

        if ($value instanceof Throwable) {
            return [
                'type' => get_class($value),
                'message' => $value->getMessage()
            ];
        }

        if (method_exists($value, '__toString')) {
            return (string) $value;
        }

        return get_class($value);
    }
}
