# BeaverTech Tracker - .NET SDK

.NET SDK to send exceptions and logs to the BeaverTech error monitoring API.

## Install (local path)

From your .NET app:

```bash
dotnet add reference ../sdk/dotnet/BeaverTech.Tracker/BeaverTech.Tracker.csproj
```

## Install (NuGet)

```bash
dotnet add package BeaverTech.Tracker
```

## Environment variables

```
BEAVERTECH_ERROR_MONITOR_BASE_URL=https://your-beavertech.app
BEAVERTECH_ERROR_MONITOR_API_KEY=your_api_key
ASPNETCORE_ENVIRONMENT=Production
APP_VERSION=2026.1.5
```

Optional overrides:

```
BEAVERTECH_ERROR_MONITOR_ENVIRONMENT=Production
BEAVERTECH_ERROR_MONITOR_RELEASE=2026.1.5
BEAVERTECH_ERROR_MONITOR_APPLICATION=my-dotnet-app
```

## Basic usage

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

client.CaptureLog("warning", "Something odd");
```

## ASP.NET Core logging

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

## Notes

- Exceptions captured via `UnhandledException` and `UnobservedTaskException`.
- Logs captured via `ILogger` provider.
- If `BEAVERTECH_ERROR_MONITOR_BASE_URL` or `BEAVERTECH_ERROR_MONITOR_API_KEY` is empty, nothing is sent.
