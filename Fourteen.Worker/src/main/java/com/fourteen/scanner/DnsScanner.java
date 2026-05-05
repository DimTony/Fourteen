package com.fourteen.scanner;

import java.io.InputStream;
import java.io.ByteArrayOutputStream;
import java.util.ArrayList;
import java.util.List;

import com.fourteen.model.FindingResult;

public class DnsScanner implements ScannerModule {
    
    @Override
    public List<FindingResult> run(String target) throws Exception {
        List<FindingResult> findings = new ArrayList<>();

        // Run: dig +short A <target>
        String aRecords = exec("dig", "+short", "A", target);
        // Run: dig +short MX <target>
        String mxRecords = exec("dig", "+short", "MX", target);

        String data = String.format(
            "{\"a_records\":%s,\"mx_records\":%s}",
            toJsonArray(aRecords), toJsonArray(mxRecords)
        );

        findings.add(new FindingResult(
            "dns_records", "info",
            "DNS record enumeration for " + target,
            data
        ));

        // Flag if no MX records — possible mail hygiene issue
        if (mxRecords == null || mxRecords.trim().isEmpty()) {
            findings.add(new FindingResult(
                "dns_no_mx", "low",
                "No MX records found for " + target,
                "{}"
            ));
        }

        return findings;
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

    private String toJsonArray(String raw) {
        if (raw == null || raw.trim().isEmpty()) return "[]";
        String[] lines = raw.split("\\n");
        StringBuilder sb = new StringBuilder("[");
        for (int i = 0; i < lines.length; i++) {
            sb.append("\"").append(lines[i].trim().replace("\"", "\\\"")).append("\"");
            if (i < lines.length - 1) sb.append(",");
        }
        return sb.append("]").toString();
    }
}
