package com.fourteen.worker;

import redis.clients.jedis.DefaultJedisClientConfig;
import redis.clients.jedis.HostAndPort;
import redis.clients.jedis.Jedis;
import redis.clients.jedis.JedisClientConfig;
import redis.clients.jedis.JedisPool;
import redis.clients.jedis.JedisPoolConfig;
import javax.net.ssl.SSLSocketFactory;
import java.util.List;
import java.util.logging.Level;
import java.util.logging.Logger;

public class Worker {
    private static final Logger LOGGER = Logger.getLogger(Worker.class.getName());
    private static final String SCAN_QUEUE = "vulnscan:queue:scan_jobs";
    private static final int POLL_TIMEOUT_SECONDS = 0; // 0 = block indefinitely
    private static final long RECONNECT_DELAY_MS = 5000;

    public static void main(String[] args) {
        String host = System.getenv().getOrDefault("REDIS_HOST", "localhost");
        int port = Integer.parseInt(System.getenv().getOrDefault("REDIS_PORT", "6379"));
        String password = System.getenv().get("REDIS_PASSWORD");

        LOGGER.info("REDIS_HOST=" + host + " REDIS_PORT=" + port);

        // Connectivity pre-check - fail fast with a clear message
        try {
            javax.net.ssl.SSLSocketFactory sslFactory = 
                (javax.net.ssl.SSLSocketFactory) javax.net.ssl.SSLSocketFactory.getDefault();
            try (javax.net.ssl.SSLSocket socket = 
                    (javax.net.ssl.SSLSocket) sslFactory.createSocket(host, port)) {
                socket.setSoTimeout(5000);
                LOGGER.info("TLS connectivity to Redis confirmed");
            }
        } catch (Exception e) {
            LOGGER.severe("Cannot establish TLS connection to Redis at " + host + ":" + port);
            LOGGER.severe("Cause: " + e.getMessage());
            System.exit(1);
        }

        LOGGER.info("Worker starting...");
        LOGGER.info("Connecting to Redis " + host + ":" + port);

        

        JedisPoolConfig poolConfig = new JedisPoolConfig();
        poolConfig.setMaxTotal(10);
        poolConfig.setMaxIdle(5);
        poolConfig.setMinIdle(1);
        poolConfig.setTestOnBorrow(true);
        poolConfig.setTestOnReturn(true);
        poolConfig.setTestWhileIdle(true);

        JedisClientConfig clientConfig = DefaultJedisClientConfig.builder()
            .connectionTimeoutMillis(10_000)
            .socketTimeoutMillis(0)          // must be 0 for brpop blocking
            .password(password)
            .ssl(true)                       // ← this is what was missing
            .build();

        JedisPool jedisPool = new JedisPool(
            poolConfig,
            new HostAndPort(host, port),
            clientConfig
        );

        try {
            consumeJobs(jedisPool);
        } finally {
            jedisPool.destroy();
            LOGGER.info("Worker shutdown complete");
        }
    }

    private static void consumeJobs(JedisPool jedisPool) {
        while (true) {
            try (Jedis jedis = jedisPool.getResource()) {
                LOGGER.info("Waiting for scan jobs on queue: " + SCAN_QUEUE);

                // Blocking pop with timeout - waits for jobs to arrive
                List<String> result = jedis.brpop(POLL_TIMEOUT_SECONDS, SCAN_QUEUE);

                if (result == null || result.isEmpty()) {
                    LOGGER.fine("Poll timeout or no jobs available");
                    continue;
                }

                String jobPayload = result.get(1);
                LOGGER.info("Received scan job: " + jobPayload);

                try {
                    processScanJob(jobPayload, jedis);
                } catch (Exception e) {
                    LOGGER.log(Level.SEVERE, "Error processing scan job: " + jobPayload, e);
                    // Optionally push to dead-letter queue
                    pushToDeadLetterQueue(jedis, jobPayload, e);
                }

            } catch (Exception e) {
                LOGGER.log(Level.SEVERE, "Redis connection error", e);
                LOGGER.info("Reconnecting to Redis in " + RECONNECT_DELAY_MS + "ms...");
                try {
                    Thread.sleep(RECONNECT_DELAY_MS);
                } catch (InterruptedException ie) {
                    Thread.currentThread().interrupt();
                    LOGGER.info("Worker interrupted");
                    break;
                }
            }
        }
    }

    private static void processScanJob(String jobPayload, Jedis jedis) throws Exception {
        long startTime = System.currentTimeMillis();
        LOGGER.info("Processing scan job: " + jobPayload);

        // TODO: Implement actual vulnerability scanning logic
        // This might involve:
        // 1. Parsing the job payload (JSON)
        // 2. Executing the scan
        // 3. Storing results back in Redis
        // 4. Publishing completion event

        // Placeholder: simulate work
        Thread.sleep(2000);

        long processingTime = System.currentTimeMillis() - startTime;
        LOGGER.info("Scan job completed in " + processingTime + "ms: " + jobPayload);
    }

    private static void pushToDeadLetterQueue(Jedis jedis, String jobPayload, Exception error) {
        try {
            String deadLetterQueue = "vulnscan:queue:dead_letters";
            String errorMessage = "Error: " + error.getMessage();
            jedis.lpush(deadLetterQueue, jobPayload + " | " + errorMessage);
            LOGGER.warning("Job pushed to dead-letter queue: " + deadLetterQueue);
        } catch (Exception e) {
            LOGGER.log(Level.SEVERE, "Failed to push job to dead-letter queue", e);
        }
    }
}
