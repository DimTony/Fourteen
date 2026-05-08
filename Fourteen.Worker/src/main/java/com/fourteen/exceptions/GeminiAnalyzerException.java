package com.fourteen.exceptions;

public class GeminiAnalyzerException extends RuntimeException {
 
    public GeminiAnalyzerException(String message) {
        super(message);
    }
 
    public GeminiAnalyzerException(String message, Throwable cause) {
        super(message, cause);
    }
}
