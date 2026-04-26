const http = require('http');
const https = require('https');
const { URL } = require('url');

const DEFAULT_LOG_LEVELS = [
  'debug',
  'info',
  'notice',
  'warn',
  'warning',
  'error',
  'critical',
  'alert',
  'emergency'
];

class ErrorMonitoringClient {
  constructor(options = {}) {
    const env = process.env;
    this.enabled = resolveBoolean(options.enabled, env.BEAVERTECH_ERROR_MONITOR_ENABLED, true);
    this.baseUrl = trimSlash(
      options.baseUrl || env.BEAVERTECH_ERROR_MONITOR_BASE_URL || ''
    );
    this.apiKey = options.apiKey || env.BEAVERTECH_ERROR_MONITOR_API_KEY || '';
    this.timeout = resolveNumber(options.timeout, env.BEAVERTECH_ERROR_MONITOR_TIMEOUT, 2000);
    this.captureExceptions = resolveBoolean(
      options.captureExceptions,
      env.BEAVERTECH_ERROR_MONITOR_CAPTURE_EXCEPTIONS,
      true
    );
    this.captureLogs = resolveBoolean(
      options.captureLogs,
      env.BEAVERTECH_ERROR_MONITOR_CAPTURE_LOGS,
      true
    );
    this.captureConsole = resolveBoolean(
      options.captureConsole,
      env.BEAVERTECH_ERROR_MONITOR_CAPTURE_CONSOLE,
      true
    );
    this.logLevels = normalizeLevels(options.logLevels || DEFAULT_LOG_LEVELS);
    this.environment =
      options.environment ||
      env.BEAVERTECH_ERROR_MONITOR_ENVIRONMENT ||
      env.NODE_ENV ||
      'production';
    this.release =
      options.release ||
      env.BEAVERTECH_ERROR_MONITOR_RELEASE ||
      env.APP_VERSION ||
      env.npm_package_version ||
      null;
    this.application =
      options.application ||
      env.BEAVERTECH_ERROR_MONITOR_APPLICATION ||
      env.npm_package_name ||
      'node-app';
    this.tags = options.tags || {};
    this.extra = options.extra || {};
    this.requestProvider = options.requestProvider || null;
    this.userProvider = options.userProvider || null;
    this.beforeSend = options.beforeSend || null;
    this._sending = false;
    this._consolePatched = false;
    this._originalConsole = {};
  }

  start() {
    if (!this.enabled) {
      return;
    }

    if (this.captureExceptions) {
      process.on('uncaughtException', (err) => {
        this.captureException(err);
      });

      process.on('unhandledRejection', (reason) => {
        if (reason instanceof Error) {
          this.captureException(reason);
        } else {
          this.captureMessage('UnhandledRejection', { reason: reason });
        }
      });
    }

    if (this.captureLogs && this.captureConsole) {
      this.patchConsole();
    }
  }

  stop() {
    this.restoreConsole();
  }

  captureException(error, context = {}) {
    if (!this.canSend() || !this.captureExceptions || this.isBusy()) {
      return;
    }

    const payload = this.buildPayload({
      exception: {
        type: error && error.name ? error.name : 'Error',
        message: error && error.message ? error.message : 'Error',
        stackTrace: error && error.stack ? error.stack : ''
      },
      tags: mergeTags(this.tags, context.tags),
      extra: mergeExtra(this.extra, context.extra || context)
    });

    this.send(payload);
  }

  captureMessage(message, context = {}) {
    if (!this.canSend() || this.isBusy()) {
      return;
    }

    const payload = this.buildPayload({
      exception: {
        type: 'Log',
        message: message || 'Log',
        stackTrace: ''
      },
      tags: mergeTags(this.tags, context.tags),
      extra: mergeExtra(this.extra, context.extra || context)
    });

    this.send(payload);
  }

  captureLog(level, message, context = {}) {
    if (!this.canSend() || !this.captureLogs || this.isBusy()) {
      return;
    }

    if (!this.shouldCaptureLevel(level)) {
      return;
    }

    if (context && context.error instanceof Error) {
      this.captureException(context.error, context);
      return;
    }

    const payload = this.buildPayload({
      exception: {
        type: 'Log',
        message: message || '',
        stackTrace: ''
      },
      tags: mergeTags(this.tags, {
        'log.level': level
      }, context.tags),
      extra: mergeExtra(this.extra, context.extra || context)
    });

    this.send(payload);
  }

  patchConsole() {
    if (this._consolePatched) {
      return;
    }

    const methods = ['debug', 'info', 'log', 'warn', 'error'];
    methods.forEach((method) => {
      if (typeof console[method] !== 'function') {
        return;
      }

      this._originalConsole[method] = console[method];
      console[method] = (...args) => {
        this._originalConsole[method](...args);

        const error = findError(args);
        if (error) {
          this.captureException(error, { tags: { 'log.level': method } });
          return;
        }

        const message = formatMessage(args);
        this.captureLog(method, message, { args: safeArgs(args) });
      };
    });

    this._consolePatched = true;
  }

  restoreConsole() {
    if (!this._consolePatched) {
      return;
    }

    Object.keys(this._originalConsole).forEach((method) => {
      console[method] = this._originalConsole[method];
    });

    this._consolePatched = false;
  }

  buildPayload(data) {
    const payload = {
      application: this.application,
      environment: this.environment,
      release: this.release,
      timestamp: new Date().toISOString(),
      request: this.resolveRequest(),
      user: this.resolveUser(),
      exception: data.exception,
      tags: data.tags || {},
      extra: data.extra || {}
    };

    if (typeof this.beforeSend === 'function') {
      const modified = this.beforeSend(payload);
      return modified === null ? null : modified;
    }

    return payload;
  }

  resolveRequest() {
    if (typeof this.requestProvider === 'function') {
      try {
        return this.requestProvider() || null;
      } catch (err) {
        return null;
      }
    }
    return null;
  }

  resolveUser() {
    if (typeof this.userProvider === 'function') {
      try {
        return this.userProvider() || null;
      } catch (err) {
        return null;
      }
    }
    return null;
  }

  shouldCaptureLevel(level) {
    if (!level) {
      return true;
    }
    return this.logLevels.includes(String(level).toLowerCase());
  }

  canSend() {
    return this.enabled && this.baseUrl && this.apiKey;
  }

  isBusy() {
    return this._sending;
  }

  send(payload) {
    if (!payload) {
      return;
    }
    if (!this.canSend()) {
      return;
    }

    const url = new URL('/api/error-ingest', this.baseUrl);
    const data = Buffer.from(JSON.stringify(payload));
    const transport = url.protocol === 'https:' ? https : http;

    const options = {
      method: 'POST',
      hostname: url.hostname,
      port: url.port || (url.protocol === 'https:' ? 443 : 80),
      path: url.pathname + url.search,
      headers: {
        'Content-Type': 'application/json',
        'Content-Length': data.length,
        'X-BT-ApiKey': this.apiKey
      }
    };

    this._sending = true;
    try {
      const req = transport.request(options, (res) => {
        res.on('data', () => {});
        res.on('end', () => {
          this._sending = false;
        });
      });

      req.on('error', () => {
        this._sending = false;
      });

      req.setTimeout(this.timeout, () => {
        req.destroy();
        this._sending = false;
      });

      req.write(data);
      req.end();
    } catch (err) {
      this._sending = false;
    }
  }
}

function resolveBoolean(value, envValue, fallback) {
  if (typeof value === 'boolean') {
    return value;
  }
  if (typeof envValue === 'string') {
    return !['false', '0', 'no'].includes(envValue.toLowerCase());
  }
  return fallback;
}

function resolveNumber(value, envValue, fallback) {
  if (typeof value === 'number' && !Number.isNaN(value)) {
    return value;
  }
  if (typeof envValue === 'string') {
    const parsed = parseInt(envValue, 10);
    if (!Number.isNaN(parsed)) {
      return parsed;
    }
  }
  return fallback;
}

function normalizeLevels(levels) {
  if (!Array.isArray(levels)) {
    return DEFAULT_LOG_LEVELS;
  }
  return levels.map((level) => String(level).toLowerCase());
}

function trimSlash(value) {
  return String(value).replace(/\/+$/, '');
}

function mergeTags(base, ...items) {
  return Object.assign({}, base || {}, ...items.filter(Boolean));
}

function mergeExtra(base, extra) {
  return Object.assign({}, base || {}, normalizeExtra(extra));
}

function normalizeExtra(extra) {
  if (!extra || typeof extra !== 'object') {
    return {};
  }
  const normalized = {};
  Object.keys(extra).forEach((key) => {
    normalized[key] = normalizeValue(extra[key]);
  });
  return normalized;
}

function normalizeValue(value) {
  if (value === null || value === undefined) {
    return value;
  }
  if (typeof value === 'string' || typeof value === 'number' || typeof value === 'boolean') {
    return value;
  }
  if (value instanceof Error) {
    return {
      type: value.name,
      message: value.message
    };
  }
  if (Array.isArray(value)) {
    return value.map((item) => normalizeValue(item));
  }
  if (typeof value === 'object') {
    const normalized = {};
    Object.keys(value).forEach((key) => {
      normalized[key] = normalizeValue(value[key]);
    });
    return normalized;
  }
  return String(value);
}

function formatMessage(args) {
  if (!Array.isArray(args)) {
    return '';
  }
  return args
    .map((arg) => {
      if (typeof arg === 'string') {
        return arg;
      }
      if (arg instanceof Error) {
        return arg.message;
      }
      try {
        return JSON.stringify(arg);
      } catch (err) {
        return String(arg);
      }
    })
    .join(' ');
}

function safeArgs(args) {
  return args.map((arg) => normalizeValue(arg));
}

function findError(args) {
  return args.find((arg) => arg instanceof Error);
}

module.exports = {
  ErrorMonitoringClient
};
