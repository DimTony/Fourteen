package com.fourteen.model;

import java.util.List;
import java.util.UUID;

import com.fasterxml.jackson.annotation.JsonProperty;

public class ScanResult {
    @JsonProperty("ScanId")
    private UUID scanId;

    @JsonProperty("Success")
    private Boolean success;

    @JsonProperty("FailureReason")
    private String failureReason;

    @JsonProperty("Findings")
    private List<FindingResult> findings;

    public ScanResult() {}

    public UUID getScanId() {
        return scanId;
    }

    public Boolean getSuccess() {
        return success;
    }

    public String getFailureReason() {
        return failureReason;
    }

    public List<FindingResult> getFindings() {
        return findings;
    }

    public static ScanResult ok(UUID scanId, List<FindingResult> findings) {
        ScanResult r = new ScanResult();
        r.scanId = scanId; r.success = true; r.findings = findings;
        return r;
    }

    public static ScanResult failed(UUID scanId, String reason) {
        ScanResult r = new ScanResult();
        r.scanId = scanId; r.success = false; r.failureReason = reason;
        return r;
    }
}
