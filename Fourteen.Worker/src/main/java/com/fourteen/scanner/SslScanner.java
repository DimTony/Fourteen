package com.fourteen.scanner;

import java.io.InputStream;
import java.io.ByteArrayOutputStream;
import java.util.ArrayList;
import java.util.List;

import com.fourteen.model.FindingResult;

public class SslScanner implements ScannerModule {

    @Override
    public List<FindingResult> run(String target) throws Exception {
        List<FindingResult> findings = new ArrayList<>();

        // Grab cert info via openssl
        String raw = exec("openssl", "s_client",
            "-connect", target + ":443",
            "-servername", target,
            "-showcerts");

        // Check protocol — flag if TLS 1.0/1.1 in use
        if (raw.contains("TLSv1.0") || raw.contains("TLSv1.1")) {
            findings.add(new FindingResult(
                "ssl_weak_protocol", "high",
                "Weak TLS protocol in use on " + target,
                "{\"detail\":\"TLS 1.0 or 1.1 detected\"}"
            ));
        }

        // Check expiry via openssl x509
        String dates = exec("sh", "-c",
            "echo | openssl s_client -connect " + target + ":443 -servername " + target
            + " 2>/dev/null | openssl x509 -noout -dates 2>/dev/null");

        findings.add(new FindingResult(
            "ssl_certificate", "info",
            "SSL certificate details for " + target,
            String.format("{\"dates\":%s}", jsonEscape(dates))
        ));

        // Flag self-signed certs
        if (raw.contains("self signed") || raw.contains("self-signed")) {
            findings.add(new FindingResult(
                "ssl_self_signed", "critical",
                "Self-signed certificate detected on " + target,
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

    private String jsonEscape(String s) {
        return "\"" + s.replace("\\", "\\\\").replace("\"", "\\\"")
                       .replace("\n", "\\n").replace("\r", "") + "\"";
    }
}
