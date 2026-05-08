package com.fourteen.model;

import java.util.UUID;

import com.fasterxml.jackson.annotation.JsonProperty;

public class FindingResult {
    @JsonProperty("Type")
    private String type;

    @JsonProperty("Severity")
    private String severity;

    @JsonProperty("Title")
    private String title;

    @JsonProperty("Data")
    private String data;

    public FindingResult(String type, String severity, String title, String data) {
        this.type = type; 
        this.severity = severity;
        this.title = title; 
        this.data = data;
    }

    public String getType() {
        return type;
    }

    public String getSeverity() {
        return severity;
    }

    public String getTitle() {
        return title;
    }

    public String getData() {
        return data;
    }
}

