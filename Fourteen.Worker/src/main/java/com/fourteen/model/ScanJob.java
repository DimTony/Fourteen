package com.fourteen.model;

import java.time.Instant;
import java.util.UUID;
import com.fasterxml.jackson.annotation.JsonProperty;

public class ScanJob {
    @JsonProperty("ScanId")
    private UUID scanId;

    @JsonProperty("DomainId")
    private UUID domainId;

    @JsonProperty("DomainName")
    private String domainName;

    @JsonProperty("ScanType")
    private String scanType;

    @JsonProperty("EnqueuedAt")
    private Instant enqueuedAt;

    public ScanJob() {}

    public UUID getScanId() {
        return scanId;
    }

    public UUID getDomainId() {
        return domainId;
    }

    public String getDomainName() {
        return domainName;
    }

    public String getScanType() {
        return scanType;
    }

    public Instant getEnqueuedAt() {
        return enqueuedAt;
    }

}
