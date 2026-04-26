# BeaverTech Tracker

Client SDKs for the **BeaverTech error monitoring service** (Sentry/Elmah-like). Each SDK sends application errors and logs to a BeaverTech server via the `POST /api/error-ingest` endpoint, authenticated with an `X-BT-ApiKey` header.

This monorepo hosts four SDKs published to public registries:

| Language | Package | Registry | Install |
|---|---|---|---|
| .NET | `BeaverTech.Tracker` | NuGet.org | `dotnet add package BeaverTech.Tracker` |
| Node.js | `beavertech-tracker-node` | npm | `npm install beavertech-tracker-node` |
| Java | `io.github.hadagalberto:tracker-java` | Maven Central | see [java/README.md](java/README.md) |
| Laravel | `beavertech/tracker-laravel` | Packagist | `composer require beavertech/tracker-laravel` |

## Quick start

All SDKs share the same configuration model — set the BeaverTech server URL and your application API key, then capture exceptions:

- **Server URL**: env `BEAVERTECH_ERROR_MONITOR_BASE_URL` (e.g. `https://errors.example.com`)
- **API key**: env `BEAVERTECH_ERROR_MONITOR_API_KEY` (provisioned in the BeaverTech admin UI)

See per-language docs:

- [docs/dotnet.md](docs/dotnet.md)
- [docs/node.md](docs/node.md)
- [docs/java.md](docs/java.md)
- [docs/laravel.md](docs/laravel.md)

## Repository layout

```
beavertech-tracker/
├── dotnet/   # BeaverTech.Tracker (NuGet)
├── node/     # beavertech-tracker-node (npm)
├── java/     # io.github.hadagalberto:tracker-java (Maven Central)
├── laravel/  # beavertech/tracker-laravel (Packagist)
└── docs/     # per-language usage guides
```

## Releases

Each SDK is versioned independently via prefixed tags:

- `.NET`: tag `dotnet-vX.Y.Z` → publishes to NuGet
- `Node`: tag `node-vX.Y.Z` → publishes to npm
- `Java`: tag `java-vX.Y.Z` → publishes to Maven Central
- `Laravel`: tag `laravel-vX.Y.Z` → Packagist auto-detects via webhook

See [CHANGELOG.md](CHANGELOG.md) for release notes.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). All four SDKs implement the same HTTP contract — keep payload shapes in sync.

## License

MIT — see [LICENSE](LICENSE).
