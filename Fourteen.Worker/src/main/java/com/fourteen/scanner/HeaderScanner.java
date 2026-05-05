package com.fourteen.scanner;

import java.io.InputStream;
import java.io.ByteArrayOutputStream;
import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Iterator;

import com.fourteen.model.FindingResult;

public class HeaderScanner implements ScannerModule {

    private static final List<String> SECURITY_HEADERS = java.util.Arrays.asList(
        "strict-transport-security",
        "content-security-policy",
        "x-frame-options",
        "x-content-type-options",
        "referrer-policy",
        "permissions-policy"
    );

    @Override
    public List<FindingResult> run(String target) throws Exception {
        List<FindingResult> findings = new ArrayList<>();

        String raw = exec("curl", "-sI", "--max-time", "10",
            "--location", "https://" + target);

        Map<String, String> headers = parseHeaders(raw);

        // Record all present headers as info
        findings.add(new FindingResult(
            "http_headers_raw", "info",
            "HTTP response headers for " + target,
            toJson(headers)
        ));

        // Flag every missing security header
        for (String h : SECURITY_HEADERS) {
            if (!headers.containsKey(h)) {
                findings.add(new FindingResult(
                    "missing_header_" + h.replace("-", "_"),
                    "medium",
                    "Missing security header: " + h,
                    String.format("{\"header\":\"%s\"}", h)
                ));
            }
        }

        return findings;
    }

    private Map<String, String> parseHeaders(String raw) {
        Map<String, String> map = new LinkedHashMap<>();
        for (String line : raw.split("\\r?\\n")) {
            int colon = line.indexOf(':');
            if (colon > 0) {
                map.put(
                    line.substring(0, colon).trim().toLowerCase(),
                    line.substring(colon + 1).trim()
                );
            }
        }
        return map;
    }

    private String exec(String... cmd) throws Exception {
        Process p = new ProcessBuilder(cmd)
            .redirectErrorStream(true)
            .start();

        InputStream is = p.getInputStream();
        ByteArrayOutputStream buffer = new ByteArrayOutputStream();

        byte[] data = new byte[1024];
        int nRead;
        while ((nRead = is.read(data, 0, data.length)) != -1) {
            buffer.write(data, 0, nRead);
        }

        buffer.flush();

        String out = new String(buffer.toByteArray()).trim();
        p.waitFor();
        return out;
    }

    private String toJson(Map<String, String> m) {
        StringBuilder sb = new StringBuilder("{");
        Iterator<Map.Entry<String, String>> it = m.entrySet().iterator();

        while (it.hasNext()) {
            Map.Entry<String, String> e = it.next();
            sb.append("\"").append(e.getKey()).append("\":\"")
            .append(e.getValue().replace("\"", "\\\"")).append("\"");
            if (it.hasNext()) sb.append(",");
        }

        return sb.append("}").toString();
    }
}
