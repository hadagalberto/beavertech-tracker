package com.beavertech.tracker;

public class Tracker {
    private static TrackerClient client;

    public static TrackerClient init() {
        return init(new TrackerConfig());
    }

    public static TrackerClient init(TrackerConfig config) {
        client = new TrackerClient(config);
        client.start();
        return client;
    }

    public static TrackerClient getClient() {
        return client;
    }
}
