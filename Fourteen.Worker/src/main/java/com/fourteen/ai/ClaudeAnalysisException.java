package com.fourteen.ai;

public class ClaudeAnalysisException extends RuntimeException {
 
    public ClaudeAnalysisException(String message) {
        super(message);
    }
 
    public ClaudeAnalysisException(String message, Throwable cause) {
        super(message, cause);
    }
}
