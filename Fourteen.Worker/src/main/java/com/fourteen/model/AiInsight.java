package com.fourteen.model;

import com.fasterxml.jackson.annotation.JsonProperty;

public class AiInsight {
    @JsonProperty("issue")
    private String issue;
 
    @JsonProperty("severity")
    private String severity;
 
    @JsonProperty("explanation")
    private String explanation;
 
    @JsonProperty("fix")
    private String fix;

    public AiInsight() {}
 
    public AiInsight(String issue, String severity, String explanation, String fix) {
        this.issue       = issue;
        this.severity    = severity;
        this.explanation = explanation;
        this.fix         = fix;
    }
 
    public String getIssue()       { return issue; }
    public String getSeverity()    { return severity; }
    public String getExplanation() { return explanation; }
    public String getFix()         { return fix; }
}
