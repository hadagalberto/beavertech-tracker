namespace BeaverTech.Tracker;

public sealed class TrackerPayload
{
    public string? Application { get; set; }
    public string? Environment { get; set; }
    public string? Release { get; set; }
    public DateTime Timestamp { get; set; }
    public TrackerExceptionInfo? Exception { get; set; }
    public TrackerRequestInfo? Request { get; set; }
    public TrackerUserInfo? User { get; set; }
    public Dictionary<string, string> Tags { get; set; } = new();
    public Dictionary<string, object?> Extra { get; set; } = new();
}

public sealed class TrackerExceptionInfo
{
    public string? Type { get; set; }
    public string? Message { get; set; }
    public string? StackTrace { get; set; }
}

public sealed class TrackerRequestInfo
{
    public string? Method { get; set; }
    public string? Url { get; set; }
    public string? Ip { get; set; }
}

public sealed class TrackerUserInfo
{
    public string? Id { get; set; }
    public string? Email { get; set; }
}
