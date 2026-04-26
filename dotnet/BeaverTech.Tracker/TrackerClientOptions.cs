using System.Reflection;
using Microsoft.Extensions.Logging;

namespace BeaverTech.Tracker;

public sealed class TrackerClientOptions
{
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutMs { get; set; } = 2000;
    public bool CaptureExceptions { get; set; } = true;
    public bool CaptureLogs { get; set; } = true;
    public HashSet<LogLevel> LogLevels { get; set; } = new();
    public string Environment { get; set; } = "Production";
    public string? Release { get; set; }
    public string Application { get; set; } = "dotnet-app";
    public Dictionary<string, string> Tags { get; set; } = new();
    public Dictionary<string, object?> Extra { get; set; } = new();
    public Func<TrackerRequestInfo?>? RequestProvider { get; set; }
    public Func<TrackerUserInfo?>? UserProvider { get; set; }
    public Func<TrackerPayload, TrackerPayload?>? BeforeSend { get; set; }

    public TrackerClientOptions()
    {
        Enabled = ResolveBool("BEAVERTECH_ERROR_MONITOR_ENABLED", true);
        BaseUrl = System.Environment.GetEnvironmentVariable("BEAVERTECH_ERROR_MONITOR_BASE_URL") ?? string.Empty;
        ApiKey = System.Environment.GetEnvironmentVariable("BEAVERTECH_ERROR_MONITOR_API_KEY") ?? string.Empty;
        TimeoutMs = ResolveInt("BEAVERTECH_ERROR_MONITOR_TIMEOUT", 2000);
        CaptureExceptions = ResolveBool("BEAVERTECH_ERROR_MONITOR_CAPTURE_EXCEPTIONS", true);
        CaptureLogs = ResolveBool("BEAVERTECH_ERROR_MONITOR_CAPTURE_LOGS", true);
        Environment = ResolveEnvironment();
        Release = ResolveRelease();
        Application = ResolveApplication();
        LogLevels = new HashSet<LogLevel>(new[]
        {
            LogLevel.Trace,
            LogLevel.Debug,
            LogLevel.Information,
            LogLevel.Warning,
            LogLevel.Error,
            LogLevel.Critical
        });
    }

    private static bool ResolveBool(string name, bool fallback)
    {
        var raw = System.Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return fallback;
        }

        return !raw.Equals("false", StringComparison.OrdinalIgnoreCase)
               && !raw.Equals("0", StringComparison.OrdinalIgnoreCase)
               && !raw.Equals("no", StringComparison.OrdinalIgnoreCase);
    }

    private static int ResolveInt(string name, int fallback)
    {
        var raw = System.Environment.GetEnvironmentVariable(name);
        if (int.TryParse(raw, out var value))
        {
            return value;
        }

        return fallback;
    }

    private static string ResolveEnvironment()
    {
        return System.Environment.GetEnvironmentVariable("BEAVERTECH_ERROR_MONITOR_ENVIRONMENT")
               ?? System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
               ?? System.Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
               ?? "Production";
    }

    private static string? ResolveRelease()
    {
        var release = System.Environment.GetEnvironmentVariable("BEAVERTECH_ERROR_MONITOR_RELEASE")
                      ?? System.Environment.GetEnvironmentVariable("APP_VERSION");
        if (!string.IsNullOrWhiteSpace(release))
        {
            return release;
        }

        var entry = Assembly.GetEntryAssembly();
        var info = entry?.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        return info?.InformationalVersion ?? entry?.GetName().Version?.ToString();
    }

    private static string ResolveApplication()
    {
        return System.Environment.GetEnvironmentVariable("BEAVERTECH_ERROR_MONITOR_APPLICATION")
               ?? Assembly.GetEntryAssembly()?.GetName().Name
               ?? "dotnet-app";
    }
}
