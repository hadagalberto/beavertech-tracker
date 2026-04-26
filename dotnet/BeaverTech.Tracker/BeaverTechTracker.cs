using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BeaverTech.Tracker;

public static class BeaverTechTracker
{
    private static TrackerClient? _client;

    public static TrackerClient Init(Action<TrackerClientOptions>? configure = null)
    {
        var options = new TrackerClientOptions();
        configure?.Invoke(options);
        _client = new TrackerClient(options);
        _client.Start();
        return _client;
    }

    public static TrackerClient? Client => _client;
}

public static class BeaverTechTrackerLoggingExtensions
{
    public static ILoggingBuilder AddBeaverTechTracker(this ILoggingBuilder builder, Action<TrackerClientOptions>? configure = null)
    {
        var options = new TrackerClientOptions();
        configure?.Invoke(options);

        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton<TrackerClient>();
        builder.Services.AddSingleton<ILoggerProvider, BeaverTechLoggerProvider>();

        return builder;
    }
}
