# BeaverTech Tracker - .NET (pt-BR)

SDK para enviar exceptions e logs ao BeaverTech Tracker.

## Instalacao (local)

```bash
dotnet add reference ../sdk/dotnet/BeaverTech.Tracker/BeaverTech.Tracker.csproj
```

## Instalacao (NuGet)

```bash
dotnet add package BeaverTech.Tracker
```

## Configuracao no ambiente

```
BEAVERTECH_ERROR_MONITOR_BASE_URL=https://seu-dominio.com
BEAVERTECH_ERROR_MONITOR_API_KEY=sua_api_key
ASPNETCORE_ENVIRONMENT=Production
APP_VERSION=2026.1.5
```

Opcional:

```
BEAVERTECH_ERROR_MONITOR_ENVIRONMENT=Production
BEAVERTECH_ERROR_MONITOR_RELEASE=2026.1.5
BEAVERTECH_ERROR_MONITOR_APPLICATION=meu-dotnet-app
```

## Uso basico

```csharp
using BeaverTech.Tracker;

var client = BeaverTechTracker.Init();

try
{
    throw new Exception("boom");
}
catch (Exception ex)
{
    client.CaptureException(ex);
}

client.CaptureLog("warning", "Algo estranho");
```

## Integracao com logging do ASP.NET Core

```csharp
using BeaverTech.Tracker;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddBeaverTechTracker(options =>
{
    options.RequestProvider = () => new TrackerRequestInfo
    {
        Method = "GET",
        Url = "https://example.com/health",
        Ip = "127.0.0.1"
    };
    options.UserProvider = () => new TrackerUserInfo
    {
        Id = "123",
        Email = "user@example.com"
    };
});
```

## Observacoes

- Exceptions capturadas via `UnhandledException` e `UnobservedTaskException`.
- Logs capturados via provider do `ILogger`.
- Se `BEAVERTECH_ERROR_MONITOR_BASE_URL` ou `BEAVERTECH_ERROR_MONITOR_API_KEY` estiverem vazios, nada sera enviado.
