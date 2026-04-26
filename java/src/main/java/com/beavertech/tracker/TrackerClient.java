package com.beavertech.tracker;

import com.fasterxml.jackson.databind.ObjectMapper;

import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.time.Instant;
import java.util.HashMap;
import java.util.Map;

public class TrackerClient {
    private static final ObjectMapper mapper = new ObjectMapper();
    private final TrackerConfig config;
    private boolean sending;

    public TrackerClient() {
        this(new TrackerConfig());
    }

    public TrackerClient(TrackerConfig config) {
        this.config = config;
    }

    public void start() {
        if (!config.isEnabled()) {
            return;
        }
        if (config.isCaptureExceptions()) {
            Thread.setDefaultUncaughtExceptionHandler((thread, throwable) -> {
                captureException(throwable, null, null);
            });
        }
    }

    public void captureException(Throwable throwable, Map<String, String> tags, Map<String, Object> extra) {
        if (!canSend() || !config.isCaptureExceptions() || isBusy()) {
            return;
        }

        TrackerPayload payload = buildPayloadFromException(throwable, tags, extra);
        send(payload);
    }

    public void captureLog(String level, String message, Map<String, String> tags, Map<String, Object> extra) {
        if (!canSend() || !config.isCaptureLogs() || isBusy()) {
            return;
        }

        if (!shouldCaptureLevel(level)) {
            return;
        }

        TrackerPayload payload = buildPayloadFromLog(level, message, tags, extra);
        send(payload);
    }

    private TrackerPayload buildPayloadFromException(Throwable throwable, Map<String, String> tags, Map<String, Object> extra) {
        TrackerPayload payload = basePayload();
        TrackerPayload.TrackerException exception = new TrackerPayload.TrackerException();
        exception.setType(throwable.getClass().getName());
        exception.setMessage(throwable.getMessage());
        exception.setStackTrace(stackTraceToString(throwable));
        payload.setException(exception);
        payload.setTags(mergeTags(tags));
        payload.setExtra(mergeExtra(extra));
        return payload;
    }

    private TrackerPayload buildPayloadFromLog(String level, String message, Map<String, String> tags, Map<String, Object> extra) {
        TrackerPayload payload = basePayload();
        TrackerPayload.TrackerException exception = new TrackerPayload.TrackerException();
        exception.setType("Log");
        exception.setMessage(message);
        exception.setStackTrace("");
        payload.setException(exception);
        Map<String, String> merged = mergeTags(tags);
        merged.put("log.level", level.toLowerCase());
        payload.setTags(merged);
        payload.setExtra(mergeExtra(extra));
        return payload;
    }

    private TrackerPayload basePayload() {
        TrackerPayload payload = new TrackerPayload();
        payload.setApplication(config.getApplication());
        payload.setEnvironment(config.getEnvironment());
        payload.setRelease(config.getRelease());
        payload.setTimestamp(Instant.now().toString());
        payload.setRequest(resolveRequest());
        payload.setUser(resolveUser());
        return payload;
    }

    private TrackerPayload.TrackerRequest resolveRequest() {
        if (config.getRequestProvider() == null) {
            return null;
        }
        try {
            return config.getRequestProvider().get();
        } catch (Exception ex) {
            return null;
        }
    }

    private TrackerPayload.TrackerUser resolveUser() {
        if (config.getUserProvider() == null) {
            return null;
        }
        try {
            return config.getUserProvider().get();
        } catch (Exception ex) {
            return null;
        }
    }

    private Map<String, String> mergeTags(Map<String, String> tags) {
        Map<String, String> merged = new HashMap<>(config.getTags());
        if (tags != null) {
            merged.putAll(tags);
        }
        return merged;
    }

    private Map<String, Object> mergeExtra(Map<String, Object> extra) {
        Map<String, Object> merged = new HashMap<>(config.getExtra());
        if (extra != null) {
            merged.putAll(extra);
        }
        return merged;
    }

    private boolean shouldCaptureLevel(String level) {
        if (level == null || level.trim().isEmpty()) {
            return true;
        }
        return config.getLogLevels().contains(level.toLowerCase());
    }

    private void send(TrackerPayload payload) {
        if (payload == null) {
            return;
        }

        if (config.getBeforeSend() != null) {
            payload = config.getBeforeSend().apply(payload);
            if (payload == null) {
                return;
            }
        }

        String url = buildUrl();
        if (url == null || url.isEmpty()) {
            return;
        }

        sending = true;
        HttpURLConnection connection = null;
        try {
            URL endpoint = new URL(url);
            connection = (HttpURLConnection) endpoint.openConnection();
            connection.setConnectTimeout(config.getTimeoutMs());
            connection.setReadTimeout(config.getTimeoutMs());
            connection.setRequestMethod("POST");
            connection.setRequestProperty("Content-Type", "application/json");
            connection.setRequestProperty("X-BT-ApiKey", config.getApiKey());
            connection.setDoOutput(true);

            String json = mapper.writeValueAsString(payload);
            byte[] data = json.getBytes("UTF-8");
            try (OutputStream os = connection.getOutputStream()) {
                os.write(data);
            }
            connection.getResponseCode();
        } catch (Exception ex) {
            // swallow
        } finally {
            if (connection != null) {
                connection.disconnect();
            }
            sending = false;
        }
    }

    private String buildUrl() {
        if (config.getBaseUrl() == null || config.getBaseUrl().trim().isEmpty()) {
            return null;
        }
        String base = config.getBaseUrl().trim();
        while (base.endsWith("/")) {
            base = base.substring(0, base.length() - 1);
        }
        return base + "/api/error-ingest";
    }

    private boolean canSend() {
        return config.isEnabled()
            && config.getBaseUrl() != null
            && !config.getBaseUrl().trim().isEmpty()
            && config.getApiKey() != null
            && !config.getApiKey().trim().isEmpty();
    }

    private boolean isBusy() {
        return sending;
    }

    private String stackTraceToString(Throwable throwable) {
        StringBuilder builder = new StringBuilder();
        builder.append(throwable.toString()).append("\n");
        for (StackTraceElement element : throwable.getStackTrace()) {
            builder.append("    at ").append(element.toString()).append("\n");
        }
        return builder.toString();
    }
}
