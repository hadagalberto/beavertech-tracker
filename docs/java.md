# BeaverTech Tracker - Java (pt-BR)

SDK para enviar exceptions e logs ao BeaverTech Tracker.

## Instalacao (local)

```bash
mvn -f ./sdk/java/pom.xml install
```

Depois, no seu projeto:

```xml
<dependency>
  <groupId>com.beavertech</groupId>
  <artifactId>tracker-java</artifactId>
  <version>0.1.0</version>
</dependency>
```

## Configuracao no ambiente

```
BEAVERTECH_ERROR_MONITOR_BASE_URL=https://seu-dominio.com
BEAVERTECH_ERROR_MONITOR_API_KEY=sua_api_key
APP_VERSION=2026.1.5
```

Opcional:

```
BEAVERTECH_ERROR_MONITOR_ENVIRONMENT=production
BEAVERTECH_ERROR_MONITOR_RELEASE=2026.1.5
BEAVERTECH_ERROR_MONITOR_APPLICATION=meu-java-app
```

## Uso basico

```java
import com.beavertech.tracker.Tracker;
import com.beavertech.tracker.TrackerClient;

TrackerClient client = Tracker.init();

try {
    throw new RuntimeException("boom");
} catch (Exception ex) {
    client.captureException(ex, null, null);
}

client.captureLog("warning", "Algo estranho", null, null);
```

## Providers (request/user)

```java
import com.beavertech.tracker.*;

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

## Observacoes

- Exceptions capturadas pelo `UncaughtExceptionHandler` padrao.
- Logs capturados via `captureLog`.
- Se `BEAVERTECH_ERROR_MONITOR_BASE_URL` ou `BEAVERTECH_ERROR_MONITOR_API_KEY` estiverem vazios, nada sera enviado.
