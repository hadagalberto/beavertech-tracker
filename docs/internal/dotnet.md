# BeaverTech Tracker - .NET (publicacao)

Publicacao do pacote `BeaverTech.Tracker`.

## Preparar release

1. Atualize a versao em `sdk/dotnet/BeaverTech.Tracker/BeaverTech.Tracker.csproj`.
2. Revise `sdk/dotnet/README.md`.
3. Faça tag git (ex: `v0.1.0`).

## Gerar pacote

```bash
dotnet pack sdk/dotnet/BeaverTech.Tracker/BeaverTech.Tracker.csproj -c Release
```

## Publicar no NuGet

```bash
dotnet nuget push sdk/dotnet/BeaverTech.Tracker/bin/Release/BeaverTech.Tracker.*.nupkg \
  --api-key SUA_CHAVE --source https://api.nuget.org/v3/index.json
```

## Publicar em feed privado (Azure Artifacts)

```bash
dotnet nuget push sdk/dotnet/BeaverTech.Tracker/bin/Release/BeaverTech.Tracker.*.nupkg \
  --api-key SUA_CHAVE --source https://pkgs.dev.azure.com/ORG/PROJ/_packaging/FEED/nuget/v3/index.json
```
