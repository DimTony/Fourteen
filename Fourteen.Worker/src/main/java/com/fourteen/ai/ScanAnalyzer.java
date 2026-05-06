package com.fourteen.ai;

import com.fourteen.model.AiInsight;
import com.fourteen.model.FindingResult;

public interface ScanAnalyzer {
    AiInsight analyze(FindingResult finding);
}
