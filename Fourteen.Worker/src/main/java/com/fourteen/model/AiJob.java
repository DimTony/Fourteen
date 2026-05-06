package com.fourteen.model;

import java.time.Instant;
import java.util.UUID;

import com.fasterxml.jackson.annotation.JsonProperty;

public class AiJob {
    @JsonProperty("JobId")
    private UUID jobId;

    @JsonProperty("ScanId")
    private UUID scanId;

    @JsonProperty("EnqueuedAt")
    private Instant enqueuedAt;

    /** The raw finding that was analysed — kept for audit / re-processing. */
    @JsonProperty("RawFinding")
    private FindingResult rawFinding;

    /** Claude's structured insight. */
    @JsonProperty("Insight")
    private AiInsight insight;

    public AiJob() {}
 
    private AiJob(UUID jobId, UUID scanId, FindingResult rawFinding, AiInsight insight) {
        this.jobId      = jobId;
        this.scanId     = scanId;
        this.enqueuedAt = Instant.now();
        this.rawFinding = rawFinding;
        this.insight    = insight;
    }
 
    public static AiJob of(UUID scanId, FindingResult rawFinding, AiInsight insight) {
        return new AiJob(UUID.randomUUID(), scanId, rawFinding, insight);
    }
 
    // ── Getters ──────────────────────────────────────────────────────────────
 
    public UUID getJobId()               { return jobId; }
    public UUID getScanId()              { return scanId; }
    public Instant getEnqueuedAt()       { return enqueuedAt; }
    public FindingResult getRawFinding() { return rawFinding; }
    public AiInsight getInsight()        { return insight; }

}
