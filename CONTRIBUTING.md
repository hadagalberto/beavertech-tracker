# Contributing

Thanks for your interest in BeaverTech Tracker! This monorepo hosts four client SDKs sharing a single HTTP contract with the BeaverTech error monitoring server.

## Repository layout

```
beavertech-tracker/
├── dotnet/   # .NET SDK
├── node/     # Node.js SDK
├── java/     # Java SDK
├── laravel/  # Laravel SDK
└── docs/     # Per-language usage guides
```

## HTTP contract

All SDKs send `POST /api/error-ingest` with header `X-BT-ApiKey: <key>` and a JSON body matching the shape documented in the BeaverTech server `ERROR_MONITORING_DESIGN.md`. Any change to the contract must be propagated to all four SDKs in the same pull request.

## Versioning

Each SDK is released independently via prefixed git tags:

| SDK | Tag prefix | Publishes to |
|---|---|---|
| .NET | `dotnet-vX.Y.Z` | NuGet.org |
| Node | `node-vX.Y.Z` | npm |
| Java | `java-vX.Y.Z` | Maven Central |
| Laravel | `laravel-vX.Y.Z` | Packagist (via webhook) |

Push the tag from `main` after the corresponding metadata file (`csproj` / `package.json` / `pom.xml` / `composer.json`) has been bumped.

```bash
git tag dotnet-v0.1.1
git push origin dotnet-v0.1.1
```

The matching GitHub Actions workflow under `.github/workflows/publish-*.yml` handles the actual publish.

## Required secrets (org/repo settings)

| Secret | Used by |
|---|---|
| `NUGET_API_KEY` | `publish-dotnet.yml` |
| `NPM_TOKEN` | `publish-node.yml` (Granular Access Token scoped to `@beaver-tech`, with 2FA bypass enabled) |
| `SPLIT_REPO_TOKEN` | `sync-laravel.yml` (PAT with `repo` scope, write access to `hadagalberto/beavertech-tracker-laravel`) |
| `OSSRH_USERNAME`, `OSSRH_TOKEN`, `GPG_PRIVATE_KEY`, `GPG_PASSPHRASE` | `publish-java.yml` (when Java publishing is enabled) |

> **Future**: switch `publish-node.yml` to [npm Trusted Publishing](https://docs.npmjs.com/trusted-publishers) (OIDC) once the org has the feature available — that removes the need for `NPM_TOKEN` entirely. The workflow already requests `id-token: write` and uses `--provenance`, so the migration only requires changing the env block to drop `NODE_AUTH_TOKEN` and configuring a trusted publisher on npmjs.com.

## Laravel split repo

The `laravel/` subtree is mirrored to a dedicated repo [hadagalberto/beavertech-tracker-laravel](https://github.com/hadagalberto/beavertech-tracker-laravel) so Packagist can register the package (Packagist requires `composer.json` at repo root).

The mirroring is automatic via [`.github/workflows/sync-laravel.yml`](.github/workflows/sync-laravel.yml):

- Pushes to `main` that touch `laravel/**` or `LICENSE` → split repo `main` is force-updated.
- Tag `laravel-vX.Y.Z` on the monorepo → split repo gets a matching `vX.Y.Z` tag (Packagist auto-detects).

Do **not** edit the split repo directly — changes will be overwritten by the next sync.

## Local development

- **.NET**: `dotnet build dotnet/BeaverTech.Tracker/BeaverTech.Tracker.csproj`
- **Node**: `cd node && npm install && npm test` (when tests exist)
- **Java**: `cd java && mvn -B verify`
- **Laravel**: `cd laravel && composer install`

## Pull requests

- Keep changes scoped to one SDK per PR when possible. Contract changes that span all four SDKs are an exception.
- Update `CHANGELOG.md` under the relevant `Unreleased` section.
- Bump the version in the metadata file when preparing a release.
