package com.beavertech.tracker;

import java.util.HashMap;
import java.util.Map;

public class TrackerPayload {
    private String application;
    private String environment;
    private String release;
    private String timestamp;
    private TrackerException exception;
    private TrackerRequest request;
    private TrackerUser user;
    private Map<String, String> tags = new HashMap<>();
    private Map<String, Object> extra = new HashMap<>();

    public String getApplication() {
        return application;
    }

    public void setApplication(String application) {
        this.application = application;
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

    public String getTimestamp() {
        return timestamp;
    }

    public void setTimestamp(String timestamp) {
        this.timestamp = timestamp;
    }

    public TrackerException getException() {
        return exception;
    }

    public void setException(TrackerException exception) {
        this.exception = exception;
    }

    public TrackerRequest getRequest() {
        return request;
    }

    public void setRequest(TrackerRequest request) {
        this.request = request;
    }

    public TrackerUser getUser() {
        return user;
    }

    public void setUser(TrackerUser user) {
        this.user = user;
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

    public static class TrackerException {
        private String type;
        private String message;
        private String stackTrace;

        public String getType() {
            return type;
        }

        public void setType(String type) {
            this.type = type;
        }

        public String getMessage() {
            return message;
        }

        public void setMessage(String message) {
            this.message = message;
        }

        public String getStackTrace() {
            return stackTrace;
        }

        public void setStackTrace(String stackTrace) {
            this.stackTrace = stackTrace;
        }
    }

    public static class TrackerRequest {
        private String method;
        private String url;
        private String ip;

        public String getMethod() {
            return method;
        }

        public void setMethod(String method) {
            this.method = method;
        }

        public String getUrl() {
            return url;
        }

        public void setUrl(String url) {
            this.url = url;
        }

        public String getIp() {
            return ip;
        }

        public void setIp(String ip) {
            this.ip = ip;
        }
    }

    public static class TrackerUser {
        private String id;
        private String email;

        public String getId() {
            return id;
        }

        public void setId(String id) {
            this.id = id;
        }

        public String getEmail() {
            return email;
        }

        public void setEmail(String email) {
            this.email = email;
        }
    }
}
