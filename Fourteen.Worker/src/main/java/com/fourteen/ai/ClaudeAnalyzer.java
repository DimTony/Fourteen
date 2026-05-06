package com.fourteen.ai;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.time.Duration;
import java.util.logging.Level;
import java.util.logging.Logger;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.node.ArrayNode;
import com.fasterxml.jackson.databind.node.ObjectNode;
import com.fourteen.model.AiInsight;
import com.fourteen.model.FindingResult;

public class ClaudeAnalyzer implements ScanAnalyzer {
    private static final Logger LOGGER = Logger.getLogger(ClaudeAnalyzer.class.getName());

    private static final String API_URL   = "https://api.anthropic.com/v1/messages";
    private static final String MODEL     = "claude-opus-4-5";
    private static final String API_VER   = "2023-06-01";
    private static final int    MAX_TOKENS = 512;

    /** How long to wait for a response before giving up. */
    private static final Duration HTTP_TIMEOUT = Duration.ofSeconds(30);

    private final java.net.http.HttpClient   http;
    private final ObjectMapper mapper;
    private final String       apiKey;

    public ClaudeAnalyzer() {
        this.http   = HttpClient.newBuilder().connectTimeout(Duration.ofSeconds(10)).build();
        this.mapper = new ObjectMapper()
            .registerModule(new com.fasterxml.jackson.datatype.jsr310.JavaTimeModule());
        this.apiKey = requireEnv("ANTHROPIC_API_KEY");
    }

    public AiInsight analyze(FindingResult finding) {
        String prompt = buildPrompt(finding);
 
        HttpRequest request;
        try {
            String body = buildRequestBody(prompt);
 
            request = HttpRequest.newBuilder()
                .uri(URI.create(API_URL))
                .header("Content-Type",       "application/json")
                .header("x-api-key",          apiKey)
                .header("anthropic-version",  API_VER)
                .timeout(HTTP_TIMEOUT)
                .POST(HttpRequest.BodyPublishers.ofString(body))
                .build();
        } catch (Exception e) {
            throw new ClaudeAnalysisException("Failed to build Anthropic request", e);
        }
 
        HttpResponse<String> response;
        try {
            response = http.send(request, HttpResponse.BodyHandlers.ofString());
        } catch (Exception e) {
            throw new ClaudeAnalysisException("HTTP call to Anthropic API failed", e);
        }
 
        if (response.statusCode() != 200) {
            throw new ClaudeAnalysisException(
                "Anthropic API returned HTTP " + response.statusCode() + ": " + response.body());
        }
 
        return parseResponse(response.body(), finding);
    }
 
    // ── Private helpers ───────────────────────────────────────────────────────
 
    private String buildPrompt(FindingResult finding) {
        return String.format(
            """
            You are a cybersecurity analyst. Analyse the following vulnerability scan finding and \
            return ONLY a JSON object — no markdown fences, no extra text.
 
            Finding:
              type     : %s
              severity : %s
              title    : %s
              data     : %s
 
            Return exactly this JSON shape:
            {
              "issue":       "<concise name of the issue>",
              "severity":    "<Critical | High | Medium | Low | Informational>",
              "explanation": "<2-3 sentence plain-English explanation of the risk>",
              "fix":         "<concrete remediation step>"
            }
            """,
            finding.getType(),
            finding.getSeverity(),
            finding.getTitle(),
            finding.getData()
        );
    }
 
    private String buildRequestBody(String prompt) throws Exception {
        ObjectNode root = mapper.createObjectNode();
        root.put("model",      MODEL);
        root.put("max_tokens", MAX_TOKENS);
 
        ArrayNode messages = root.putArray("messages");
        ObjectNode msg     = messages.addObject();
        msg.put("role",    "user");
        msg.put("content", prompt);
 
        return mapper.writeValueAsString(root);
    }
 
    /**
     * Extracts the text content block from the Anthropic response and parses
     * it as an {@link AiInsight}.  Falls back to a best-effort insight if
     * parsing fails so that a single bad LLM response never blocks the queue.
     */
    private AiInsight parseResponse(String responseBody, FindingResult finding) {
        try {
            JsonNode root    = mapper.readTree(responseBody);
            JsonNode content = root.path("content");
 
            if (!content.isArray() || content.isEmpty()) {
                throw new ClaudeAnalysisException("Unexpected Anthropic response shape: " + responseBody);
            }
 
            // Find the first text block
            String text = null;
            for (JsonNode block : content) {
                if ("text".equals(block.path("type").asText())) {
                    text = block.path("text").asText("").trim();
                    break;
                }
            }
 
            if (text == null || text.isEmpty()) {
                throw new ClaudeAnalysisException("No text block in Anthropic response");
            }
 
            // Strip accidental markdown fences if Claude included them
            text = stripFences(text);
 
            AiInsight insight = mapper.readValue(text, AiInsight.class);
            LOGGER.fine("Claude insight parsed for type=" + finding.getType());
            return insight;
 
        } catch (ClaudeAnalysisException e) {
            throw e;
        } catch (Exception e) {
            LOGGER.log(Level.WARNING,
                "Could not parse Claude response for finding type=" + finding.getType()
                + "; using fallback insight. Raw body: " + responseBody, e);
            // Return a degraded-but-valid insight so the job still gets enqueued
            return new AiInsight(
                finding.getTitle(),
                capitalise(finding.getSeverity()),
                "Automated analysis unavailable. Raw finding: " + finding.getData(),
                "Review manually."
            );
        }
    }
 
    private static String stripFences(String text) {
        // Remove ```json ... ``` or ``` ... ``` wrappers
        return text.replaceAll("(?s)^```[a-z]*\\s*", "").replaceAll("(?s)\\s*```$", "").trim();
    }
 
    private static String capitalise(String s) {
        if (s == null || s.isEmpty()) return s;
        return Character.toUpperCase(s.charAt(0)) + s.substring(1).toLowerCase();
    }
 
    private static String requireEnv(String name) {
        String value = System.getenv(name);
        if (value == null || value.isBlank()) {
            throw new IllegalStateException("Required environment variable not set: " + name);
        }
        return value;
    }
    
}
