package com.beavertech.tracker;

import java.util.HashMap;
import java.util.HashSet;
import java.util.Map;
import java.util.Set;

public class TrackerConfig {
    private boolean enabled;
    private String baseUrl;
    private String apiKey;
    private int timeoutMs;
    private boolean captureExceptions;
    private boolean captureLogs;
    private Set<String> logLevels;
    private String environment;
    private String release;
    private String application;
    private Map<String, String> tags;
    private Map<String, Object> extra;
    private RequestProvider requestProvider;
    private UserProvider userProvider;
    private BeforeSend beforeSend;

    public TrackerConfig() {
        enabled = resolveBoolean("BEAVERTECH_ERROR_MONITOR_ENABLED", true);
        baseUrl = getEnv("BEAVERTECH_ERROR_MONITOR_BASE_URL", "");
        apiKey = getEnv("BEAVERTECH_ERROR_MONITOR_API_KEY", "");
        timeoutMs = resolveInt("BEAVERTECH_ERROR_MONITOR_TIMEOUT", 2000);
        captureExceptions = resolveBoolean("BEAVERTECH_ERROR_MONITOR_CAPTURE_EXCEPTIONS", true);
        captureLogs = resolveBoolean("BEAVERTECH_ERROR_MONITOR_CAPTURE_LOGS", true);
        logLevels = new HashSet<>();
        logLevels.add("debug");
        logLevels.add("info");
        logLevels.add("notice");
        logLevels.add("warn");
        logLevels.add("warning");
        logLevels.add("error");
        logLevels.add("critical");
        logLevels.add("alert");
        logLevels.add("emergency");
        environment = resolveEnvironment();
        release = resolveRelease();
        application = resolveApplication();
        tags = new HashMap<>();
        extra = new HashMap<>();
    }

    public boolean isEnabled() {
        return enabled;
    }

    public void setEnabled(boolean enabled) {
        this.enabled = enabled;
    }

    public String getBaseUrl() {
        return baseUrl;
    }

    public void setBaseUrl(String baseUrl) {
        this.baseUrl = baseUrl;
    }

    public String getApiKey() {
        return apiKey;
    }

    public void setApiKey(String apiKey) {
        this.apiKey = apiKey;
    }

    public int getTimeoutMs() {
        return timeoutMs;
    }

    public void setTimeoutMs(int timeoutMs) {
        this.timeoutMs = timeoutMs;
    }

    public boolean isCaptureExceptions() {
        return captureExceptions;
    }

    public void setCaptureExceptions(boolean captureExceptions) {
        this.captureExceptions = captureExceptions;
    }

    public boolean isCaptureLogs() {
        return captureLogs;
    }

    public void setCaptureLogs(boolean captureLogs) {
        this.captureLogs = captureLogs;
    }

    public Set<String> getLogLevels() {
        return logLevels;
    }

    public void setLogLevels(Set<String> logLevels) {
        this.logLevels = logLevels;
    }

    public String getEnvironment() {
        return environment;
    }

    public void setEnvironment(String environment) {
        this.environment = environment;
    }

    public String getRelease() {
        return release;
    }

    public void setRelease(String release) {
        this.release = release;
    }

    public String getApplication() {
        return application;
    }

    public void setApplication(String application) {
        this.application = application;
    }

    public Map<String, String> getTags() {
        return tags;
    }

    public void setTags(Map<String, String> tags) {
        this.tags = tags;
    }

    public Map<String, Object> getExtra() {
        return extra;
    }

    public void setExtra(Map<String, Object> extra) {
        this.extra = extra;
    }

    public RequestProvider getRequestProvider() {
        return requestProvider;
    }

    public void setRequestProvider(RequestProvider requestProvider) {
        this.requestProvider = requestProvider;
    }

    public UserProvider getUserProvider() {
        return userProvider;
    }

    public void setUserProvider(UserProvider userProvider) {
        this.userProvider = userProvider;
    }

    public BeforeSend getBeforeSend() {
        return beforeSend;
    }

    public void setBeforeSend(BeforeSend beforeSend) {
        this.beforeSend = beforeSend;
    }

    private static boolean resolveBoolean(String name, boolean fallback) {
        String raw = System.getenv(name);
        if (raw == null || raw.trim().isEmpty()) {
            return fallback;
        }
        String lowered = raw.trim().toLowerCase();
        return !lowered.equals("false") && !lowered.equals("0") && !lowered.equals("no");
    }

    private static int resolveInt(String name, int fallback) {
        String raw = System.getenv(name);
        if (raw == null || raw.trim().isEmpty()) {
            return fallback;
        }
        try {
            return Integer.parseInt(raw.trim());
        } catch (NumberFormatException ex) {
            return fallback;
        }
    }

    private static String resolveEnvironment() {
        String env = System.getenv("BEAVERTECH_ERROR_MONITOR_ENVIRONMENT");
        if (env != null && !env.trim().isEmpty()) {
            return env.trim();
        }
        String profile = System.getProperty("spring.profiles.active");
        if (profile != null && !profile.trim().isEmpty()) {
            return profile.trim();
        }
        return "production";
    }

    private static String resolveRelease() {
        String release = System.getenv("BEAVERTECH_ERROR_MONITOR_RELEASE");
        if (release != null && !release.trim().isEmpty()) {
            return release.trim();
        }
        release = System.getenv("APP_VERSION");
        if (release != null && !release.trim().isEmpty()) {
            return release.trim();
        }
        return null;
    }

    private static String resolveApplication() {
        String app = System.getenv("BEAVERTECH_ERROR_MONITOR_APPLICATION");
        if (app != null && !app.trim().isEmpty()) {
            return app.trim();
        }
        String sys = System.getProperty("spring.application.name");
        if (sys != null && !sys.trim().isEmpty()) {
            return sys.trim();
        }
        return "java-app";
    }

    private static String getEnv(String name, String fallback) {
        String raw = System.getenv(name);
        return raw == null ? fallback : raw;
    }
}
