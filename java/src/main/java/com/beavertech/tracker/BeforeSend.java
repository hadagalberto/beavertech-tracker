package com.beavertech.tracker;

public interface BeforeSend {
    TrackerPayload apply(TrackerPayload payload);
}
