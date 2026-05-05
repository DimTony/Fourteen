package com.fourteen.scanner;

import java.util.List;

import com.fourteen.model.FindingResult;

public interface ScannerModule {
    List<FindingResult> run(String target) throws Exception;
}
