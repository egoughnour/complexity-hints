/**
 * Core type definitions for the Complexity Hints extension.
 * These types mirror the C# ComplexityHint and related structures
 * for cross-process communication with the .NET backend.
 */

/**
 * Represents a complexity analysis result for a single method.
 */
export interface ComplexityHint {
    /** Unique identifier for the method: {SyntaxKind}::{MethodName}::{SpanStart} */
    methodId: string;
    
    /** Human-readable method name */
    methodName: string;
    
    /** Time complexity in Big-O notation (e.g., "O(n log n)") */
    timeBigO: string;
    
    /** Space complexity in Big-O notation (e.g., "O(n)") */
    spaceBigO?: string;
    
    /** Confidence score from 0.0 to 1.0 */
    confidence: number;
    
    /** Whether this is an amortized complexity */
    isAmortized: boolean;
    
    /** Whether this involves probabilistic analysis */
    isProbabilistic: boolean;
    
    /** Brief explanation of the dominant complexity term */
    dominantTerm?: string;
    
    /** Detailed tooltip text */
    tooltip?: string;
    
    /** When this hint was last updated */
    updatedAt: Date;
    
    /** Document version this hint was computed for */
    documentVersion: number;
}

/**
 * Information about a method's location for CodeLens placement.
 */
export interface MethodHeaderInfo {
    /** Unique method identifier */
    methodId: string;
    
    /** Display name for the method */
    displayName: string;
    
    /** Line number where the method header starts (0-based) */
    headerLine: number;
    
    /** Character offset within the header line */
    headerCharacter: number;
    
    /** Start offset of the method body in the document */
    bodyStart: number;
    
    /** End offset of the method body in the document */
    bodyEnd: number;
}

/**
 * Result of an environment probe.
 */
export interface EnvProbeResult {
    /** Whether the dotnet CLI is available */
    dotnetOk: boolean;
    
    /** dotnet CLI version if available */
    dotnetVersion?: string;
    
    /** Whether Python is available */
    pythonOk: boolean;
    
    /** Python version if available */
    pythonVersion?: string;
    
    /** Whether uv is available */
    uvOk: boolean;
    
    /** uv version if available */
    uvVersion?: string;
    
    /** Whether Node.js is available (for future use) */
    nodeOk: boolean;
    
    /** Node.js version if available */
    nodeVersion?: string;
    
    /** Whether all required checks passed */
    allOk: boolean;
    
    /** List of error messages */
    errors: string[];
    
    /** When the probe was run */
    timestamp: Date;
}

/**
 * Request to analyze a document or method.
 */
export interface AnalysisRequest {
    /** Document URI */
    documentUri: string;
    
    /** Full document text */
    documentText: string;
    
    /** Document version for cache invalidation */
    documentVersion: number;
    
    /** Optional: specific method ID to analyze */
    methodId?: string;
    
    /** Optional: position in document to focus analysis */
    position?: {
        line: number;
        character: number;
    };
}

/**
 * Response from the analysis backend.
 */
export interface AnalysisResponse {
    /** Whether the analysis succeeded */
    success: boolean;
    
    /** Error message if analysis failed */
    error?: string;
    
    /** Analysis results for methods */
    hints: ComplexityHint[];
    
    /** Methods found in the document */
    methods: MethodHeaderInfo[];
    
    /** Overall confidence for the document */
    overallConfidence: number;
    
    /** Whether the code was complete (no syntax errors) */
    isCodeComplete: boolean;
    
    /** Document version this response is for */
    documentVersion: number;
}

/**
 * Cache key for result storage.
 */
export interface CacheKey {
    /** Document URI */
    documentUri: string;
    
    /** Method ID */
    methodId: string;
    
    /** Document version */
    documentVersion: number;
}

/**
 * Formats a complexity hint for display in CodeLens.
 */
export function formatHintLabel(hint: ComplexityHint, showSpace: boolean, showConfidence: boolean): string {
    const parts: string[] = [];
    
    // Time complexity
    let timeStr = `T: ${hint.timeBigO}`;
    if (hint.isAmortized) {
        timeStr += ' amortized';
    }
    if (hint.isProbabilistic) {
        timeStr += ' (expected)';
    }
    parts.push(timeStr);
    
    // Space complexity
    if (showSpace && hint.spaceBigO) {
        parts.push(`S: ${hint.spaceBigO}`);
    }
    
    // Confidence
    if (showConfidence) {
        parts.push(hint.confidence.toFixed(2));
    }
    
    return parts.join(' | ');
}

/**
 * Formats a tooltip for a complexity hint.
 */
export function formatHintTooltip(hint: ComplexityHint): string {
    const lines: string[] = [];
    
    lines.push(`**${hint.methodName}**`);
    lines.push('');
    lines.push(`**Time Complexity:** ${hint.timeBigO}`);
    
    if (hint.spaceBigO) {
        lines.push(`**Space Complexity:** ${hint.spaceBigO}`);
    }
    
    lines.push(`**Confidence:** ${(hint.confidence * 100).toFixed(0)}%`);
    
    if (hint.isAmortized) {
        lines.push('');
        lines.push('*Amortized analysis* - worst-case per operation over a sequence');
    }
    
    if (hint.isProbabilistic) {
        lines.push('');
        lines.push('*Probabilistic analysis* - expected complexity under random input');
    }
    
    if (hint.dominantTerm) {
        lines.push('');
        lines.push(`**Dominant term:** ${hint.dominantTerm}`);
    }
    
    if (hint.tooltip) {
        lines.push('');
        lines.push(hint.tooltip);
    }
    
    return lines.join('\n');
}

/**
 * Creates a method ID from its components.
 */
export function makeMethodId(syntaxKind: string, methodName: string, spanStart: number): string {
    return `${syntaxKind}::${methodName}::${spanStart}`;
}

/**
 * Parses a method ID into its components.
 */
export function parseMethodId(methodId: string): { syntaxKind: string; methodName: string; spanStart: number } | null {
    const parts = methodId.split('::');
    if (parts.length !== 3) {
        return null;
    }
    
    const spanStart = parseInt(parts[2], 10);
    if (isNaN(spanStart)) {
        return null;
    }
    
    return {
        syntaxKind: parts[0],
        methodName: parts[1],
        spanStart
    };
}
