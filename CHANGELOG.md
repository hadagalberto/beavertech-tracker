# Changelog

All notable changes to BeaverTech Tracker SDKs are documented here. Each SDK is versioned independently.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and each SDK adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [dotnet-v0.1.0] / [node-v0.1.0] / [java-v0.1.0] / [laravel-v0.1.0] — Initial release

### Added

- .NET SDK (`BeaverTech.Tracker`): `TrackerClient`, `BeaverTechTracker` static entry point, `BeaverTechLoggerProvider`, env-based configuration via `BEAVERTECH_ERROR_MONITOR_*`.
- Node.js SDK (`beavertech-tracker-node`): error capture with unhandled exception/rejection hooks.
- Java SDK (`io.github.hadagalberto:tracker-java`): error capture using Jackson for JSON serialization.
- Laravel SDK (`beavertech/tracker-laravel`): service provider auto-discovery, exception handler integration.
- Shared HTTP contract: `POST /api/error-ingest` with `X-BT-ApiKey` header, JSON payload (application/environment/release/exception/request/user/tags/extra).
- PII redaction in tags/extra for keys matching `password`, `token`, `authorization`, `secret`, `apikey`, `access`.
