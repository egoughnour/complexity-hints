/**
 * Analysis backend that communicates with the .NET complexity engine.
 * 
 * This backend can operate in two modes:
 * 1. Heuristic mode: Fast built-in analysis using regex patterns
 * 2. Backend mode: Uses the ComplexityAnalysis.IDE.Cli for full Roslyn analysis
 * 
 * By default, heuristics are used for speed. The backend can be enabled
 * via settings for more accurate analysis.
 */

import * as vscode from 'vscode';
import { spawn, ChildProcess } from 'child_process';
import * as path from 'path';
import * as fs from 'fs';
import * as os from 'os';
import { OutputLogger } from '../core/outputLogger';
import { Settings } from '../core/settings';
import { 
    AnalysisRequest, 
    AnalysisResponse, 
    ComplexityHint, 
    MethodHeaderInfo 
} from '../core/types';
import { CSharpMethodLocator } from './csharpMethodLocator';

/**
 * Backend for running complexity analysis.
 * Integrates with the ComplexityAnalysis.Engine via subprocess.
 */
export class AnalysisBackend {
    private readonly _methodLocator: CSharpMethodLocator;
    private _activeProcess: ChildProcess | null = null;

    constructor(
        private readonly _logger: OutputLogger,
        private readonly _settings: Settings
    ) {
        this._methodLocator = new CSharpMethodLocator();
    }

    /**
     * Analyzes a document and returns complexity hints.
     */
    async analyze(request: AnalysisRequest, signal?: AbortSignal): Promise<AnalysisResponse> {
        // Try to use the .NET backend if available, fallback to heuristics
        if (this._settings.useBackendAnalysis) {
            try {
                return await this.analyzeWithDotnetBackend(request, signal);
            } catch (error) {
                this._logger.warn(`Backend analysis failed, falling back to heuristics: ${error}`);
            }
        }
        
        return this.analyzeWithHeuristics(request, signal);
    }

    /**
     * Analyzes using built-in heuristics (fast path).
     * This provides basic complexity analysis without requiring the .NET backend.
     */
    private async analyzeWithHeuristics(
        request: AnalysisRequest, 
        signal?: AbortSignal
    ): Promise<AnalysisResponse> {
        try {
            const methods = this._methodLocator.findMethods(request.documentText);
            
            if (signal?.aborted) {
                return {
                    success: false,
                    error: 'Analysis cancelled',
                    hints: [],
                    methods: [],
                    overallConfidence: 0,
                    isCodeComplete: true,
                    documentVersion: request.documentVersion
                };
            }

            const hints: ComplexityHint[] = [];
            const methodInfos: MethodHeaderInfo[] = [];

            for (const method of methods) {
                if (signal?.aborted) {
                    break;
                }

                // Extract method body text
                const bodyText = request.documentText.substring(method.bodyStart, method.bodyEnd + 1);
                
                // Analyze the method
                const analysis = this.analyzeMethodHeuristically(method.displayName, bodyText);
                
                hints.push({
                    methodId: method.methodId,
                    methodName: method.displayName,
                    timeBigO: analysis.timeBigO,
                    spaceBigO: analysis.spaceBigO,
                    confidence: analysis.confidence,
                    isAmortized: analysis.isAmortized,
                    isProbabilistic: analysis.isProbabilistic,
                    dominantTerm: analysis.dominantTerm,
                    tooltip: analysis.tooltip,
                    updatedAt: new Date(),
                    documentVersion: request.documentVersion
                });

                // Convert to line/character (this will be done by the runner,
                // but we include the raw info here)
                methodInfos.push({
                    methodId: method.methodId,
                    displayName: method.displayName,
                    headerLine: 0, // Will be filled in by runner
                    headerCharacter: 0,
                    bodyStart: method.bodyStart,
                    bodyEnd: method.bodyEnd
                });
            }

            return {
                success: true,
                hints,
                methods: methodInfos,
                overallConfidence: hints.length > 0 
                    ? hints.reduce((sum, h) => sum + h.confidence, 0) / hints.length 
                    : 0,
                isCodeComplete: true,
                documentVersion: request.documentVersion
            };

        } catch (error) {
            const message = error instanceof Error ? error.message : String(error);
            this._logger.error(`Heuristic analysis error: ${message}`);
            
            return {
                success: false,
                error: message,
                hints: [],
                methods: [],
                overallConfidence: 0,
                isCodeComplete: false,
                documentVersion: request.documentVersion
            };
        }
    }

    /**
     * Analyzes a single method using heuristics.
     */
    private analyzeMethodHeuristically(methodName: string, bodyText: string): {
        timeBigO: string;
        spaceBigO?: string;
        confidence: number;
        isAmortized: boolean;
        isProbabilistic: boolean;
        dominantTerm?: string;
        tooltip?: string;
    } {
        // Count various patterns
        const forLoops = (bodyText.match(/\bfor\s*\(/g) || []).length;
        const foreachLoops = (bodyText.match(/\bforeach\s*\(/g) || []).length;
        const whileLoops = (bodyText.match(/\bwhile\s*\(/g) || []).length;
        const totalLoops = forLoops + foreachLoops + whileLoops;

        // Check for recursive calls
        const recursivePattern = new RegExp(`\\b${this.escapeRegex(methodName)}\\s*\\(`, 'g');
        const recursiveCalls = (bodyText.match(recursivePattern) || []).length;

        // Check for logarithmic patterns
        const logPatterns = (bodyText.match(/\/=\s*2|\*=\s*2|>>=|<<=|Math\.Log|BinarySearch/gi) || []).length;

        // Check for nested loops
        const nestedLoopPattern = /\b(for|foreach|while)\s*\([^)]*\)[^{]*\{[^}]*\b(for|foreach|while)\s*\(/g;
        const nestedLoops = (bodyText.match(nestedLoopPattern) || []).length;

        // Check for LINQ operations
        const linqOps = (bodyText.match(/\.(Where|Select|OrderBy|GroupBy|Distinct|Union|Intersect|Except|Join|ToList|ToArray|ToDictionary|Sort)\s*\(/gi) || []).length;

        // Check for collection allocations
        const allocations = (bodyText.match(/new\s+(List|Dictionary|HashSet|Queue|Stack|Array)/gi) || []).length;

        // Determine complexity
        let timeBigO: string;
        let spaceBigO: string;
        let confidence: number;
        let dominantTerm: string | undefined;
        let tooltip: string | undefined;
        let isAmortized = false;
        let isProbabilistic = false;

        if (recursiveCalls > 0) {
            // Recursive method
            if (logPatterns > 0) {
                // Likely divide and conquer
                if (recursiveCalls >= 2) {
                    timeBigO = 'O(n log n)';
                    dominantTerm = 'Divide-and-conquer recursion';
                    confidence = 0.7;
                } else {
                    timeBigO = 'O(log n)';
                    dominantTerm = 'Binary recursion';
                    confidence = 0.75;
                }
                spaceBigO = 'O(log n)';
            } else if (recursiveCalls >= 2) {
                // Multiple recursive calls without log pattern - likely exponential
                timeBigO = 'O(2^n)';
                spaceBigO = 'O(n)';
                dominantTerm = 'Multiple recursive branches';
                confidence = 0.6;
                tooltip = 'May be Fibonacci-like pattern. Consider memoization.';
            } else {
                // Single recursive call
                timeBigO = 'O(n)';
                spaceBigO = 'O(n)';
                dominantTerm = 'Linear recursion';
                confidence = 0.7;
            }
        } else if (nestedLoops > 0) {
            // Nested loops
            const depth = nestedLoops + 1;
            if (depth === 2) {
                timeBigO = 'O(n²)';
                dominantTerm = 'Nested loops';
            } else if (depth === 3) {
                timeBigO = 'O(n³)';
                dominantTerm = 'Triple nested loops';
            } else {
                timeBigO = `O(n^${depth + 1})`;
                dominantTerm = `${depth + 1} levels of nesting`;
            }
            spaceBigO = allocations > 0 ? 'O(n)' : 'O(1)';
            confidence = 0.75;
        } else if (totalLoops > 0) {
            // Single loop level
            if (logPatterns > 0) {
                timeBigO = 'O(log n)';
                dominantTerm = 'Logarithmic loop';
                confidence = 0.8;
            } else if (linqOps > 0 && bodyText.includes('OrderBy')) {
                timeBigO = 'O(n log n)';
                dominantTerm = 'LINQ OrderBy';
                confidence = 0.85;
            } else {
                timeBigO = 'O(n)';
                dominantTerm = 'Linear loop';
                confidence = 0.85;
            }
            spaceBigO = allocations > 0 ? 'O(n)' : 'O(1)';
        } else if (linqOps > 0) {
            // LINQ without explicit loops
            if (bodyText.includes('OrderBy') || bodyText.includes('Sort')) {
                timeBigO = 'O(n log n)';
                dominantTerm = 'LINQ sorting operation';
                confidence = 0.85;
            } else {
                timeBigO = 'O(n)';
                dominantTerm = 'LINQ enumeration';
                confidence = 0.8;
            }
            spaceBigO = bodyText.includes('ToList') || bodyText.includes('ToArray') ? 'O(n)' : 'O(1)';
        } else {
            // No loops or recursion
            timeBigO = 'O(1)';
            spaceBigO = 'O(1)';
            confidence = 0.9;
        }

        // Check for amortized patterns
        if (bodyText.includes('List<') && bodyText.includes('.Add(')) {
            isAmortized = true;
            tooltip = 'List.Add is O(1) amortized due to dynamic array resizing';
        }
        if (bodyText.includes('Dictionary<') && (bodyText.includes('.Add(') || bodyText.includes('[') )) {
            isAmortized = true;
            isProbabilistic = true;
            tooltip = 'Dictionary operations are O(1) expected/amortized';
        }

        // Check for probabilistic patterns
        if (bodyText.includes('Random') || bodyText.includes('HashSet') || bodyText.includes('GetHashCode')) {
            isProbabilistic = true;
        }

        return {
            timeBigO,
            spaceBigO,
            confidence,
            isAmortized,
            isProbabilistic,
            dominantTerm,
            tooltip
        };
    }

    /**
     * Escapes special regex characters in a string.
     */
    private escapeRegex(str: string): string {
        return str.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    }

    /**
     * Analyzes using the .NET backend CLI.
     */
    private async analyzeWithDotnetBackend(
        request: AnalysisRequest,
        signal?: AbortSignal
    ): Promise<AnalysisResponse> {
        // Try to find the CLI executable
        const cliInfo = this.findCli();
        
        if (!cliInfo) {
            throw new Error('CLI not found. Install the extension with bundled CLI or set complexity.cliProjectPath.');
        }

        // Write document text to a temp file (CLI reads files, not stdin by default)
        const tempDir = os.tmpdir();
        const tempFile = path.join(tempDir, `complexity-${Date.now()}.cs`);
        
        try {
            await fs.promises.writeFile(tempFile, request.documentText, 'utf8');

            let command: string;
            let args: string[];

            if (cliInfo.type === 'bundled') {
                // Use bundled self-contained executable directly
                command = cliInfo.path;
                args = ['analyze', '-d', tempFile, '--json'];
            } else {
                // Use dotnet run with project file
                const dotnetPath = this._settings.dotnetPath || 'dotnet';
                command = dotnetPath;
                args = ['run', '--project', cliInfo.path, '--', 'analyze', '-d', tempFile, '--json'];
            }

            this._logger.debug(`Running: ${command} ${args.join(' ')}`);

            const result = await this.runProcess(command, args, signal);

            if (signal?.aborted) {
                return this.createCancelledResponse(request.documentVersion);
            }

            // Parse the JSON output
            const output = JSON.parse(result.stdout);
            
            if (!output.success) {
                throw new Error(output.error || 'Analysis failed');
            }

            // Convert CLI output to our response format
            const hints: ComplexityHint[] = output.methods.map((m: {
                methodId: string;
                methodName: string;
                timeComplexity: string;
                spaceComplexity?: string;
                confidence: number;
                requiresReview: boolean;
                reviewReason?: string;
            }) => ({
                methodId: m.methodId,
                methodName: m.methodName,
                timeBigO: m.timeComplexity,
                spaceBigO: m.spaceComplexity,
                confidence: m.confidence,
                isAmortized: false,
                isProbabilistic: false,
                tooltip: m.requiresReview ? m.reviewReason : undefined,
                updatedAt: new Date(),
                documentVersion: request.documentVersion
            }));

            // Get method infos from our locator (for line numbers)
            const methods = this._methodLocator.findMethods(request.documentText);
            const methodInfos: MethodHeaderInfo[] = methods.map(m => ({
                methodId: m.methodId,
                displayName: m.displayName,
                headerLine: 0,
                headerCharacter: 0,
                bodyStart: m.bodyStart,
                bodyEnd: m.bodyEnd
            }));

            return {
                success: true,
                hints,
                methods: methodInfos,
                overallConfidence: hints.length > 0
                    ? hints.reduce((sum, h) => sum + h.confidence, 0) / hints.length
                    : 0,
                isCodeComplete: true,
                documentVersion: request.documentVersion
            };

        } finally {
            // Clean up temp file
            try {
                await fs.promises.unlink(tempFile);
            } catch {
                // Ignore cleanup errors
            }
        }
    }

    /**
     * Finds the CLI executable or project.
     * Priority:
     * 1. Bundled executable in extension's out/cli folder
     * 2. Settings-specified project path
     * 3. Workspace-relative project path
     */
    private findCli(): { type: 'bundled' | 'project'; path: string } | undefined {
        // 1. Check for bundled CLI in extension directory
        const bundledCli = this.findBundledCli();
        if (bundledCli) {
            return { type: 'bundled', path: bundledCli };
        }

        // 2. Check settings
        if (this._settings.cliProjectPath) {
            if (fs.existsSync(this._settings.cliProjectPath)) {
                return { type: 'project', path: this._settings.cliProjectPath };
            }
        }

        // 3. Check workspace-relative paths
        const workspaceFolders = vscode.workspace.workspaceFolders;
        if (workspaceFolders && workspaceFolders.length > 0) {
            const rootPath = workspaceFolders[0].uri.fsPath;
            const possiblePaths = [
                path.join(rootPath, 'src/ComplexityAnalysis.IDE/Cli/ComplexityAnalysis.IDE.Cli.csproj'),
                path.join(rootPath, 'ComplexityAnalysis.IDE/Cli/ComplexityAnalysis.IDE.Cli.csproj'),
            ];

            for (const p of possiblePaths) {
                if (fs.existsSync(p)) {
                    return { type: 'project', path: p };
                }
            }
        }

        return undefined;
    }

    /**
     * Finds the bundled CLI executable in the extension directory.
     */
    private findBundledCli(): string | undefined {
        // Get extension path from VS Code
        const extension = vscode.extensions.getExtension('complexity-analysis.complexity-hints');
        if (!extension) {
            return undefined;
        }

        const extensionPath = extension.extensionPath;
        
        // Check for the executable in out/cli
        // The executable name varies by platform
        const platform = os.platform();
        const executableNames = platform === 'win32' 
            ? ['complexity-cli.exe']
            : ['complexity-cli'];

        for (const execName of executableNames) {
            const execPath = path.join(extensionPath, 'out', 'cli', execName);
            if (fs.existsSync(execPath)) {
                this._logger.debug(`Found bundled CLI at: ${execPath}`);
                return execPath;
            }
        }

        return undefined;
    }

    /**
     * Finds the CLI project relative to the workspace.
     * @deprecated Use findCli() instead
     */
    private findCliProject(): string | undefined {
        const result = this.findCli();
        return result?.type === 'project' ? result.path : undefined;
    }

    /**
     * Runs a process and returns its output.
     */
    private runProcess(
        command: string, 
        args: string[], 
        signal?: AbortSignal
    ): Promise<{ stdout: string; stderr: string }> {
        return new Promise((resolve, reject) => {
            const process = spawn(command, args, {
                stdio: ['pipe', 'pipe', 'pipe'],
                shell: false
            });

            this._activeProcess = process;

            let stdout = '';
            let stderr = '';

            process.stdout.on('data', (data) => {
                stdout += data.toString();
            });

            process.stderr.on('data', (data) => {
                stderr += data.toString();
            });

            process.on('close', (code) => {
                this._activeProcess = null;
                if (signal?.aborted) {
                    reject(new Error('Analysis cancelled'));
                } else if (code !== 0) {
                    reject(new Error(`Process exited with code ${code}: ${stderr}`));
                } else {
                    resolve({ stdout, stderr });
                }
            });

            process.on('error', (error) => {
                this._activeProcess = null;
                reject(error);
            });

            // Handle cancellation
            if (signal) {
                signal.addEventListener('abort', () => {
                    process.kill('SIGTERM');
                });
            }
        });
    }

    /**
     * Creates a cancelled response.
     */
    private createCancelledResponse(documentVersion: number): AnalysisResponse {
        return {
            success: false,
            error: 'Analysis cancelled',
            hints: [],
            methods: [],
            overallConfidence: 0,
            isCodeComplete: true,
            documentVersion
        };
    }

    /**
     * Cancels any active analysis.
     */
    cancel(): void {
        if (this._activeProcess) {
            this._activeProcess.kill('SIGTERM');
            this._activeProcess = null;
        }
    }

    /**
     * Disposes the backend.
     */
    dispose(): void {
        this.cancel();
    }
}
