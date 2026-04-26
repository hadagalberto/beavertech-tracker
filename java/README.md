# BeaverTech Tracker - Java SDK

Java SDK to send exceptions and logs to the BeaverTech error monitoring API.

## Install (local path)

```bash
mvn -f ./sdk/java/pom.xml install
```

Then, in your app:

```xml
<dependency>
  <groupId>com.beavertech</groupId>
  <artifactId>tracker-java</artifactId>
  <version>0.1.0</version>
</dependency>
```

## Environment variables

```
BEAVERTECH_ERROR_MONITOR_BASE_URL=https://your-beavertech.app
BEAVERTECH_ERROR_MONITOR_API_KEY=your_api_key
APP_VERSION=2026.1.5
```

Optional overrides:

```
BEAVERTECH_ERROR_MONITOR_ENVIRONMENT=production
BEAVERTECH_ERROR_MONITOR_RELEASE=2026.1.5
BEAVERTECH_ERROR_MONITOR_APPLICATION=my-java-app
```

## Basic usage

```java
import com.beavertech.tracker.Tracker;
import com.beavertech.tracker.TrackerClient;

TrackerClient client = Tracker.init();

try {
    throw new RuntimeException("boom");
} catch (Exception ex) {
    client.captureException(ex, null, null);
}

client.captureLog("warning", "Something odd", null, null);
```

## Custom providers

```java
import com.beavertech.tracker.*;
import java.util.HashMap;

TrackerConfig config = new TrackerConfig();
config.setRequestProvider(() -> {
    TrackerPayload.TrackerRequest request = new TrackerPayload.TrackerRequest();
    request.setMethod("GET");
    request.setUrl("https://example.com/health");
    request.setIp("127.0.0.1");
    return request;
});
config.setUserProvider(() -> {
    TrackerPayload.TrackerUser user = new TrackerPayload.TrackerUser();
    user.setId("123");
    user.setEmail("user@example.com");
    return user;
});

Tracker.init(config);
```

## Notes

- Exceptions captured via default `UncaughtExceptionHandler`.
- Logs captured by calling `captureLog`.
- If `BEAVERTECH_ERROR_MONITOR_BASE_URL` or `BEAVERTECH_ERROR_MONITOR_API_KEY` is empty, nothing is sent.
