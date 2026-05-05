package com.fourteen.worker;

import redis.clients.jedis.Jedis;

import java.util.List;

public class Worker {

    public static void main(String[] args) {

        String host = System.getenv().getOrDefault("REDIS_HOST", "localhost");
        int port = Integer.parseInt(System.getenv().getOrDefault("REDIS_PORT", "6379"));

        System.out.println("Worker starting...");
        System.out.println("Connecting to Redis " + host + ":" + port);

        try (Jedis jedis = new Jedis(host, port)) {

            while (true) {
                System.out.println("Waiting for job...");

                // Blocking pop (waits until job arrives)
                List<String> job = jedis.brpop(0, "jobs");

                String payload = job.get(1);

                System.out.println("Received job: " + payload);

                processJob(payload);
            }

        }
    }

    private static void processJob(String job) {
        System.out.println("Processing: " + job);

        try {
            Thread.sleep(2000); // simulate work
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
        }

        System.out.println("Done: " + job);
    }
}
