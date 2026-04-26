using Microsoft.Extensions.Logging;

namespace BeaverTech.Tracker;

public sealed class BeaverTechLoggerProvider : ILoggerProvider
{
    private readonly TrackerClient _client;
    private readonly TrackerClientOptions _options;

    public BeaverTechLoggerProvider(TrackerClient client, TrackerClientOptions options)
    {
        _client = client;
        _options = options;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new BeaverTechLogger(categoryName, _client, _options);
    }

    public void Dispose()
    {
    }
}

internal sealed class BeaverTechLogger : ILogger
{
    private readonly string _categoryName;
    private readonly TrackerClient _client;
    private readonly TrackerClientOptions _options;

    public BeaverTechLogger(string categoryName, TrackerClient client, TrackerClientOptions options)
    {
        _categoryName = categoryName;
        _client = client;
        _options = options;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return NullScope.Instance;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        if (!_options.CaptureLogs)
        {
            return false;
        }

        if (_categoryName.StartsWith("BeaverTech.Tracker", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return _options.LogLevels.Contains(logLevel);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);
        var tags = new Dictionary<string, string>
        {
            ["log.category"] = _categoryName,
            ["log.level"] = logLevel.ToString().ToLowerInvariant()
        };

        var extra = new Dictionary<string, object?>
        {
            ["eventId"] = eventId.Id,
            ["eventName"] = eventId.Name,
            ["state"] = state?.ToString()
        };

        if (exception != null)
        {
            _client.CaptureException(exception, tags, extra);
            return;
        }

        _client.CaptureLog(logLevel, message, tags, extra);
    }
}

internal sealed class NullScope : IDisposable
{
    public static readonly NullScope Instance = new();

    public void Dispose()
    {
    }
}
