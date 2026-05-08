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
import com.fourteen.exceptions.GeminiAnalyzerException;

/**
 * {@link ScanAnalyzer} implementation backed by Google Gemini
 * (generateContent REST API).
 *
 * <p>Drop-in replacement for {@link ClaudeAnalyzer}: same constructor,
 * same {@code analyze(FindingResult)} signature, same {@link AiInsight}
 * return type, same fallback behaviour on parse failure.
 *
 * <p>Requires the environment variable {@code GEMINI_API_KEY} to be set.
 */
public class GeminiAnalyzer implements ScanAnalyzer {

    private static final Logger LOGGER = Logger.getLogger(GeminiAnalyzer.class.getName());

    /**
     * Gemini generateContent endpoint.
     * The API key is appended as a query parameter per Google's REST convention.
     */
    private static final String API_URL_TEMPLATE =
            "https://generativelanguage.googleapis.com/v1beta/models/%s:generateContent?key=%s";

    /** Use the latest stable flash model — fast and cheap for structured output. */
    private static final String MODEL = "gemini-2.0-flash";

    /** Mirror the token budget used in ClaudeAnalyzer. */
    private static final int MAX_TOKENS = 512;

    private static final Duration HTTP_TIMEOUT = Duration.ofSeconds(30);

    private final HttpClient   http;
    private final ObjectMapper mapper;
    private final String       apiKey;

    public GeminiAnalyzer() {
        this.http   = HttpClient.newBuilder().connectTimeout(Duration.ofSeconds(10)).build();
        this.mapper = new ObjectMapper()
                .registerModule(new com.fasterxml.jackson.datatype.jsr310.JavaTimeModule());
        this.apiKey = requireEnv("GEMINI_API_KEY");
    }

    // ── ScanAnalyzer ─────────────────────────────────────────────────────────

    @Override
    public AiInsight analyze(FindingResult finding) {
        String prompt = buildPrompt(finding);

        HttpRequest request;
        try {
            String body = buildRequestBody(prompt);
            String url  = String.format(API_URL_TEMPLATE, MODEL, apiKey);

            request = HttpRequest.newBuilder()
                    .uri(URI.create(url))
                    .header("Content-Type", "application/json")
                    .timeout(HTTP_TIMEOUT)
                    .POST(HttpRequest.BodyPublishers.ofString(body))
                    .build();
        } catch (Exception e) {
            throw new GeminiAnalysisException("Failed to build Gemini request", e);
        }

        HttpResponse<String> response;
        try {
            response = http.send(request, HttpResponse.BodyHandlers.ofString());
        } catch (Exception e) {
            throw new GeminiAnalysisException("HTTP call to Gemini API failed", e);
        }

        if (response.statusCode() != 200) {
            throw new GeminiAnalysisException(
                    "Gemini API returned HTTP " + response.statusCode() + ": " + response.body());
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

    /**
     * Builds the Gemini {@code generateContent} request body.
     *
     * <pre>
     * {
     *   "contents": [
     *     { "role": "user", "parts": [{ "text": "..." }] }
     *   ],
     *   "generationConfig": {
     *     "maxOutputTokens": 512,
     *     "responseMimeType": "application/json"
     *   }
     * }
     * </pre>
     *
     * Setting {@code responseMimeType} to {@code application/json} instructs
     * Gemini to skip markdown fences and emit raw JSON, matching the behaviour
     * requested in the prompt.
     */
    private String buildRequestBody(String prompt) throws Exception {
        ObjectNode root = mapper.createObjectNode();

        // contents array
        ArrayNode contents = root.putArray("contents");
        ObjectNode turn    = contents.addObject();
        turn.put("role", "user");
        ArrayNode parts = turn.putArray("parts");
        parts.addObject().put("text", prompt);

        // generationConfig
        ObjectNode genConfig = root.putObject("generationConfig");
        genConfig.put("maxOutputTokens",  MAX_TOKENS);
        genConfig.put("responseMimeType", "application/json");

        return mapper.writeValueAsString(root);
    }

    /**
     * Navigates the Gemini response envelope:
     * {@code candidates[0].content.parts[0].text} → JSON string → {@link AiInsight}.
     *
     * <p>Falls back to a degraded insight on any parse failure so a single bad
     * LLM response never blocks the processing queue.
     */
    private AiInsight parseResponse(String responseBody, FindingResult finding) {
        try {
            JsonNode root = mapper.readTree(responseBody);

            // candidates[0].content.parts[0].text
            JsonNode candidates = root.path("candidates");
            if (!candidates.isArray() || candidates.isEmpty()) {
                throw new GeminiAnalysisException(
                        "No candidates in Gemini response: " + responseBody);
            }

            String text = candidates
                    .get(0)
                    .path("content")
                    .path("parts")
                    .get(0)
                    .path("text")
                    .asText("")
                    .trim();

            if (text.isEmpty()) {
                throw new GeminiAnalysisException(
                        "Empty text part in Gemini response: " + responseBody);
            }

            // Strip accidental markdown fences in case the model ignores responseMimeType
            text = stripFences(text);

            AiInsight insight = mapper.readValue(text, AiInsight.class);
            LOGGER.fine("Gemini insight parsed for type=" + finding.getType());
            return insight;

        } catch (GeminiAnalysisException e) {
            throw e;
        } catch (Exception e) {
            LOGGER.log(Level.WARNING,
                    "Could not parse Gemini response for finding type=" + finding.getType()
                    + "; using fallback insight. Raw body: " + responseBody, e);
            return new AiInsight(
                    finding.getTitle(),
                    capitalise(finding.getSeverity()),
                    "Automated analysis unavailable. Raw finding: " + finding.getData(),
                    "Review manually."
            );
        }
    }

    // ── Utilities ─────────────────────────────────────────────────────────────

    private static String stripFences(String text) {
        return text.replaceAll("(?s)^```[a-z]*\\s*", "")
                   .replaceAll("(?s)\\s*```$", "")
                   .trim();
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