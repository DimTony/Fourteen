package com.fourteen.ai;

import java.util.List;
import java.util.logging.Level;
import java.util.logging.Logger;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.fourteen.model.AiInsight;
import com.fourteen.model.AiJob;
import com.fourteen.model.FindingResult;
import com.fourteen.model.ScanResult;

import redis.clients.jedis.Jedis;
import redis.clients.jedis.JedisPool;

public class AiJobPublisher  {
    private static final Logger LOGGER = Logger.getLogger(AiJobPublisher.class.getName());

    static final String AI_JOBS_QUEUE = "vulnscan:queue:ai_jobs";
    private final ScanAnalyzer analyzer;
    private final ObjectMapper   mapper;

    public AiJobPublisher(ScanAnalyzer analyzer) {
        this.analyzer = analyzer;
        this.mapper   = new ObjectMapper()
            .registerModule(new com.fasterxml.jackson.datatype.jsr310.JavaTimeModule());
    }

    public AiJobPublisher() {
        this(new ClaudeAnalyzer());
    }

    public void publishInsights(ScanResult result, JedisPool jedisPool) {
        List<FindingResult> findings = result.getFindings();
 
        if (findings == null || findings.isEmpty()) {
            LOGGER.info("ScanId " + result.getScanId() + " — no findings to analyse");
            return;
        }
 
        LOGGER.info("ScanId " + result.getScanId()
            + " — analysing " + findings.size() + " finding(s)");
 
        int published = 0;
        int skipped   = 0;
 
        for (FindingResult finding : findings) {
 
            // Skip scanner-internal error markers
            if ("module_error".equalsIgnoreCase(finding.getType())) {
                LOGGER.fine("Skipping module_error finding: " + finding.getTitle());
                skipped++;
                continue;
            }
 
            try {
                AiInsight insight = analyzer.analyze(finding);
 
                AiJob job = AiJob.of(result.getScanId(), finding, insight);
 
                publish(job, jedisPool);
                published++;
 
                LOGGER.info("Published AiAnalysisJob " + job.getJobId()
                    + " for finding type=" + finding.getType()
                    + " severity=" + insight.getSeverity());
 
            } catch (Exception e) {
                LOGGER.log(Level.WARNING,
                    "Failed to analyse/publish finding type=" + finding.getType()
                    + " for scanId=" + result.getScanId() + " — skipping", e);
                skipped++;
            }
        }
 
        LOGGER.info("ScanId " + result.getScanId()
            + " — published=" + published + " skipped=" + skipped);

        try (Jedis jedis = jedisPool.getResource()) {
            jedis.set("vulnscan:ai_pending:" + result.getScanId(), String.valueOf(published));
        }
    }
 
    // ── Private ───────────────────────────────────────────────────────────────
 
    private void publish(AiJob job, JedisPool jedisPool) throws Exception {
        String payload = mapper.writeValueAsString(job);
 
        try (Jedis jedis = jedisPool.getResource()) {
            jedis.lpush(AI_JOBS_QUEUE, payload);
        }
    }
    
}
