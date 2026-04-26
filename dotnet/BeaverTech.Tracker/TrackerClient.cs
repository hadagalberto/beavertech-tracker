using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace BeaverTech.Tracker;

public sealed class TrackerClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly TrackerClientOptions _options;
    private readonly HttpClient _httpClient;
    private bool _isSending;

    public TrackerClient(TrackerClientOptions? options = null)
    {
        _options = options ?? new TrackerClientOptions();
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMilliseconds(_options.TimeoutMs)
        };
    }

    public void Start()
    {
        if (!_options.Enabled)
        {
            return;
        }

        if (_options.CaptureExceptions)
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }
    }

    public void Stop()
    {
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
    }

    public void CaptureException(Exception exception, IDictionary<string, string>? tags = null, IDictionary<string, object?>? extra = null)
    {
        if (exception == null || !CanSend() || !_options.CaptureExceptions || IsBusy())
        {
            return;
        }

        var payload = BuildPayload(new TrackerExceptionInfo
        {
            Type = exception.GetType().FullName,
            Message = exception.Message,
            StackTrace = exception.StackTrace
        }, tags, extra);

        _ = SendAsync(payload);
    }

    public void CaptureLog(LogLevel level, string message, IDictionary<string, string>? tags = null, IDictionary<string, object?>? extra = null)
    {
        if (!CanSend() || !_options.CaptureLogs || IsBusy())
        {
            return;
        }

        if (!_options.LogLevels.Contains(level))
        {
            return;
        }

        var mergedTags = MergeTags(tags, new Dictionary<string, string>
        {
            ["log.level"] = level.ToString().ToLowerInvariant()
        });

        var payload = BuildPayload(new TrackerExceptionInfo
        {
            Type = "Log",
            Message = message,
            StackTrace = string.Empty
        }, mergedTags, extra);

        _ = SendAsync(payload);
    }

    public void CaptureLog(string level, string message, IDictionary<string, string>? tags = null, IDictionary<string, object?>? extra = null)
    {
        var parsed = ParseLogLevel(level);
        CaptureLog(parsed, message, tags, extra);
    }

    private async Task SendAsync(TrackerPayload? payload)
    {
        if (payload == null || !CanSend())
        {
            return;
        }

        if (_options.BeforeSend != null)
        {
            payload = _options.BeforeSend(payload);
            if (payload == null)
            {
                return;
            }
        }

        var url = BuildUrl();
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        _isSending = true;
        try
        {
            var json = JsonSerializer.Serialize(payload, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            request.Headers.Add("X-BT-ApiKey", _options.ApiKey);
            using var response = await _httpClient.SendAsync(request);
        }
        catch
        {
            // swallow
        }
        finally
        {
            _isSending = false;
        }
    }

    private string? BuildUrl()
    {
        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            return null;
        }

        return $"{_options.BaseUrl.TrimEnd('/')}/api/error-ingest";
    }

    private TrackerPayload BuildPayload(TrackerExceptionInfo exception, IDictionary<string, string>? tags, IDictionary<string, object?>? extra)
    {
        return new TrackerPayload
        {
            Application = _options.Application,
            Environment = _options.Environment,
            Release = _options.Release,
            Timestamp = DateTime.UtcNow,
            Exception = exception,
            Request = ResolveRequest(),
            User = ResolveUser(),
            Tags = MergeTags(tags),
            Extra = MergeExtra(extra)
        };
    }

    private TrackerRequestInfo? ResolveRequest()
    {
        if (_options.RequestProvider == null)
        {
            return null;
        }

        try
        {
            return _options.RequestProvider();
        }
        catch
        {
            return null;
        }
    }

    private TrackerUserInfo? ResolveUser()
    {
        if (_options.UserProvider == null)
        {
            return null;
        }

        try
        {
            return _options.UserProvider();
        }
        catch
        {
            return null;
        }
    }

    private Dictionary<string, string> MergeTags(IDictionary<string, string>? tags, IDictionary<string, string>? extra = null)
    {
        var merged = new Dictionary<string, string>(_options.Tags);
        if (tags != null)
        {
            foreach (var item in tags)
            {
                merged[item.Key] = item.Value;
            }
        }

        if (extra != null)
        {
            foreach (var item in extra)
            {
                merged[item.Key] = item.Value;
            }
        }

        return merged;
    }

    private Dictionary<string, object?> MergeExtra(IDictionary<string, object?>? extra)
    {
        var merged = new Dictionary<string, object?>();
        foreach (var item in _options.Extra)
        {
            merged[item.Key] = NormalizeExtraValue(item.Value);
        }

        if (extra != null)
        {
            foreach (var item in extra)
            {
                merged[item.Key] = NormalizeExtraValue(item.Value);
            }
        }

        return merged;
    }

    private static object? NormalizeExtraValue(object? value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is string || value is int || value is long || value is double || value is decimal || value is bool)
        {
            return value;
        }

        if (value is Exception ex)
        {
            return new Dictionary<string, string?>
            {
                ["type"] = ex.GetType().FullName,
                ["message"] = ex.Message
            };
        }

        if (value is IEnumerable<object?> list)
        {
            return list.Select(NormalizeExtraValue).ToList();
        }

        if (value is IDictionary<string, object?> map)
        {
            return map.ToDictionary(k => k.Key, v => NormalizeExtraValue(v.Value));
        }

        return value.ToString();
    }

    private bool CanSend()
    {
        return _options.Enabled
               && !string.IsNullOrWhiteSpace(_options.BaseUrl)
               && !string.IsNullOrWhiteSpace(_options.ApiKey);
    }

    private bool IsBusy()
    {
        return _isSending;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        if (args.ExceptionObject is Exception ex)
        {
            CaptureException(ex, new Dictionary<string, string>
            {
                ["handled"] = args.IsTerminating ? "false" : "true",
                ["source"] = "unhandled"
            });
        }
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs args)
    {
        CaptureException(args.Exception, new Dictionary<string, string>
        {
            ["source"] = "unobserved"
        });
    }

    private static LogLevel ParseLogLevel(string level)
    {
        if (string.IsNullOrWhiteSpace(level))
        {
            return LogLevel.Information;
        }

        var normalized = level.Trim().ToLowerInvariant();
        if (normalized == "warn")
        {
            return LogLevel.Warning;
        }

        if (normalized == "fatal")
        {
            return LogLevel.Critical;
        }

        if (Enum.TryParse<LogLevel>(level, true, out var parsed))
        {
            return parsed;
        }

        return LogLevel.Information;
    }
}
