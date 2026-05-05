package com.fourteen.orchestrator;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.TimeoutException;
import java.util.logging.Logger;

import com.fourteen.consumer.ScanJobConsumer;
import com.fourteen.model.FindingResult;
import com.fourteen.model.ScanJob;
import com.fourteen.model.ScanResult;
import com.fourteen.scanner.DnsScanner;
import com.fourteen.scanner.HeaderScanner;
import com.fourteen.scanner.ScannerModule;
import com.fourteen.scanner.SslScanner;

public class ScanOrchestrator {
    
    private static final Logger LOGGER = Logger.getLogger(ScanJobConsumer.class.getName());
    private static final int    TIMEOUT_SECONDS = 120;
    private final DnsScanner    dns  = new DnsScanner();
    private final SslScanner    ssl  = new SslScanner();
    private final HeaderScanner http = new HeaderScanner();

    public ScanResult run(ScanJob job) {
        ExecutorService exec = Executors.newFixedThreadPool(10);

        try {
            Future<ScanResult> future = exec.submit(() -> runModules(job));
            return future.get(TIMEOUT_SECONDS, TimeUnit.SECONDS);
        } catch (TimeoutException e) {
            return ScanResult.failed(job.getScanId(),
                "Orchestrator timed out after " + TIMEOUT_SECONDS + "s");
        } catch (Exception e) {
            return ScanResult.failed(job.getScanId(),
                "Orchestrator error: " + e.getMessage());
        } finally {
            exec.shutdownNow();
        }
    }

    private ScanResult runModules(ScanJob job) {
        List<ScannerModule> modules = selectModules(job.getScanType());
        List<FindingResult> allFindings = new ArrayList<>();

        for (ScannerModule module : modules) {
            try {
                allFindings.addAll(module.run(job.getDomainName()));
            } catch (Exception e) {
                // One module failing shouldn't abort the whole scan
                allFindings.add(new FindingResult(
                    "module_error", "info",
                    module.getClass().getSimpleName() + " failed: " + e.getMessage(),
                    "{}"
                ));
            }
        }

        return ScanResult.ok(job.getScanId(), allFindings);
    }

    private List<ScannerModule> selectModules(String scanType) {
        if (scanType == null) {
            throw new IllegalArgumentException("scanType cannot be null");
        }

        switch (scanType.toLowerCase()) {
            case "dns":
                return java.util.Arrays.asList(dns);
            case "ssl":
                return java.util.Arrays.asList(ssl);
            case "http":
                return java.util.Arrays.asList(http);
            case "full":
                return java.util.Arrays.asList(dns, ssl, http);
            default:
                LOGGER.info("Unknown scan type: " + scanType + " — defaulting to full");
                return java.util.Arrays.asList(dns, ssl, http);
        }
    }
}
