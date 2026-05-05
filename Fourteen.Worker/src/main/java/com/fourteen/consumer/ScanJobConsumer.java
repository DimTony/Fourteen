package com.fourteen.consumer;

import redis.clients.jedis.Jedis;
import redis.clients.jedis.JedisPool;
import redis.clients.jedis.params.SetParams;

import java.util.List;
import java.util.logging.Level;
import java.util.logging.Logger;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.fourteen.model.ScanJob;
import com.fourteen.model.ScanResult;
import com.fourteen.orchestrator.ScanOrchestrator;

public class ScanJobConsumer {

    private static final Logger LOGGER = Logger.getLogger(ScanJobConsumer.class.getName());

    private static final String SCAN_QUEUE        = "vulnscan:queue:scan_jobs";
    private static final String RESULTS_QUEUE     = "vulnscan:queue:scan_results";
    private static final String DEAD_LETTER_QUEUE = "vulnscan:queue:dead_letter";
    private static final String SEEN_PREFIX       = "vulnscan:seen:";

    private static final int  POLL_TIMEOUT_SECONDS = 0;
    private static final long RECONNECT_DELAY_MS   = 5000;
    private static final long SEEN_TTL_SECONDS      = Long.parseLong(
        System.getenv().getOrDefault("VULNSCAN_SEEN_TTL_SECONDS", "3600")
    );

    private final ScanOrchestrator orchestrator = new ScanOrchestrator();
    private final ObjectMapper mapper = new ObjectMapper()
        .registerModule(new com.fasterxml.jackson.datatype.jsr310.JavaTimeModule());

    public void start(JedisPool jedisPool) {
        while (true) {
            try (Jedis jedis = jedisPool.getResource()) {
                LOGGER.info("Waiting for scan jobs on queue: " + SCAN_QUEUE);

                List<String> result = jedis.brpop(POLL_TIMEOUT_SECONDS, SCAN_QUEUE);

                if (result == null || result.isEmpty()) {
                    LOGGER.fine("Poll timeout or no jobs available");
                    continue;
                }

                String jobPayload = result.get(1);
                LOGGER.info("Received scan job: " + jobPayload);

                // ── Deserialize & validate
                ScanJob job;
                try {
                    job = mapper.readValue(jobPayload, ScanJob.class);

                    if (job.getScanId() == null || job.getDomainName() == null || job.getScanType() == null) {
                        throw new IllegalArgumentException("Missing required fields in payload");
                    }
                } catch (Exception e) {
                    LOGGER.log(Level.SEVERE, "Malformed job payload, routing to dead-letter: " + jobPayload, e);
                    pushToDeadLetter(jedis, jobPayload, e);
                    continue;
                }

                // ── Deduplication guard
                String seenKey = SEEN_PREFIX + job.getScanId();
                String claimed = jedis.set(seenKey, "1", SetParams.setParams().nx().ex(SEEN_TTL_SECONDS));

                if (claimed == null) {
                    LOGGER.info("Duplicate scanId " + job.getScanId() + " — skipping.");
                    continue;
                }

                // ── Run scan
                ScanResult scanResult;
                try {
                    scanResult = orchestrator.run(job);
                } catch (Exception e) {
                    scanResult = ScanResult.failed(job.getScanId(), "Unexpected orchestrator error: " + e.getMessage());
                }

                // ── Ensure failed results have a reason before publishing
                if (!scanResult.getSuccess() && (scanResult.getFailureReason() == null || (scanResult.getFailureReason() == null || scanResult.getFailureReason().trim().isEmpty()))) {
                    scanResult = ScanResult.failed(job.getScanId(), "Scan failed with no reason provided");
                }

                // ── Publish result
                try {
                    String resultPayload = mapper.writeValueAsString(scanResult);
                    jedis.lpush(RESULTS_QUEUE, resultPayload);
                    LOGGER.info("Published scan result for scanId: " + job.getScanId()
                        + " success=" + scanResult.getSuccess());
                } catch (Exception e) {
                    LOGGER.log(Level.SEVERE, "Failed to publish result for scanId: " + job.getScanId(), e);
                }

            } catch (Exception e) {
                LOGGER.log(Level.SEVERE, "Redis connection error", e);
                LOGGER.info("Reconnecting in " + RECONNECT_DELAY_MS + "ms...");
                try {
                    Thread.sleep(RECONNECT_DELAY_MS);
                } catch (InterruptedException ie) {
                    Thread.currentThread().interrupt();
                    LOGGER.info("Worker interrupted, shutting down");
                    break;
                }
            }
        }
    }

    private void pushToDeadLetter(Jedis jedis, String jobPayload, Exception error) {
        try {
            // jedis.lpush(DEAD_LETTER_QUEUE, payload);
            // LOGGER.info("Payload routed to dead-letter queue");
            String errorMessage = "Error: " + error.getMessage();
            jedis.lpush(DEAD_LETTER_QUEUE, jobPayload + " | " + errorMessage);
            LOGGER.warning("Job pushed to dead-letter queue: " + DEAD_LETTER_QUEUE);
        } catch (Exception e) {
            LOGGER.log(Level.SEVERE, "Failed to push to dead-letter queue", e);
        }
    }

}