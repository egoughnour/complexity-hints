/**
 * Complexity analysis runner that orchestrates analysis with debouncing and caching.
 * 
 * This component:
 * 1. Manages per-document debouncing
 * 2. Schedules analysis via the backend
 * 3. Publishes results to the ResultStore
 */

import * as vscode from 'vscode';
import { DebouncerMap } from './debouncer';
import { ResultStore } from './resultStore';
import { OutputLogger } from './outputLogger';
import { Settings } from './settings';
import { AnalysisBackend } from '../analysis/backend';
import { CSharpMethodLocator } from '../analysis/csharpMethodLocator';
import { ComplexityHint, MethodHeaderInfo, AnalysisResponse } from './types';

export class ComplexityRunner {
    private readonly _analysisDebouncers: DebouncerMap;
    private readonly _backend: AnalysisBackend;
    private readonly _methodLocator: CSharpMethodLocator;
    private _isDisposed = false;

    constructor(
        private readonly _resultStore: ResultStore,
        private readonly _logger: OutputLogger,
        private readonly _settings: Settings
    ) {
        this._analysisDebouncers = new DebouncerMap(this._settings.analysisDebounceMs);
        this._backend = new AnalysisBackend(this._logger, this._settings);
        this._methodLocator = new CSharpMethodLocator();
    }

    /**
     * Schedules analysis for a document with debouncing.
     */
    scheduleAnalysis(document: vscode.TextDocument): void {
        if (this._isDisposed) {
            return;
        }

        const documentUri = document.uri.toString();
        const debouncer = this._analysisDebouncers.get(documentUri);

        debouncer.debounceVoid(async (signal) => {
            if (signal.aborted) {
                return;
            }

            await this.analyzeDocument(document, false, signal);
        });
    }

    /**
     * Analyzes a document immediately (without debouncing).
     */
    async analyzeDocument(
        document: vscode.TextDocument,
        force: boolean = false,
        signal?: AbortSignal
    ): Promise<void> {
        if (this._isDisposed) {
            return;
        }

        const documentUri = document.uri.toString();
        const documentVersion = document.version;
        const documentText = document.getText();

        this._logger.debug(`Analyzing document: ${documentUri} (v${documentVersion})`);

        try {
            // Step 1: Find methods in the document using regex-based locator
            const methods = this._methodLocator.findMethods(documentText);

            if (signal?.aborted) {
                return;
            }

            // Convert to MethodHeaderInfo with line/character info
            const methodInfos = methods.map(m => this.toMethodHeaderInfo(document, m));

            // Update method headers in store
            this._resultStore.setMethodsForDocument(documentUri, methodInfos, documentVersion);

            if (methods.length === 0) {
                this._logger.debug(`No methods found in ${documentUri}`);
                return;
            }

            // Step 2: Run analysis via backend
            const response = await this._backend.analyze({
                documentUri,
                documentText,
                documentVersion
            }, signal);

            if (signal?.aborted) {
                return;
            }

            if (response.success && response.hints.length > 0) {
                // Publish hints to store
                this._resultStore.publishHints(response.hints);
                this._logger.debug(`Published ${response.hints.length} hints for ${documentUri}`);
            } else if (!response.success && response.error) {
                this._logger.warn(`Analysis failed: ${response.error}`);
            }

        } catch (error) {
            if (signal?.aborted) {
                return;
            }

            const message = error instanceof Error ? error.message : String(error);
            this._logger.error(`Analysis error for ${documentUri}: ${message}`);
        }
    }

    /**
     * Analyzes a specific position in the document (method at cursor).
     */
    async analyzeAtPosition(
        document: vscode.TextDocument,
        position: vscode.Position,
        signal?: AbortSignal
    ): Promise<void> {
        if (this._isDisposed) {
            return;
        }

        const documentUri = document.uri.toString();
        const documentVersion = document.version;
        const documentText = document.getText();
        const offset = document.offsetAt(position);

        this._logger.debug(`Analyzing at position ${position.line}:${position.character} in ${documentUri}`);

        try {
            // Find method at position
            const methods = this._methodLocator.findMethods(documentText);
            const method = methods.find(m => 
                offset >= m.bodyStart && offset <= m.bodyEnd
            );

            if (!method) {
                this._logger.debug('No method found at cursor position');
                vscode.window.showInformationMessage('No method found at cursor position');
                return;
            }

            if (signal?.aborted) {
                return;
            }

            // Analyze just this method
            const response = await this._backend.analyze({
                documentUri,
                documentText,
                documentVersion,
                methodId: method.methodId,
                position: {
                    line: position.line,
                    character: position.character
                }
            }, signal);

            if (signal?.aborted) {
                return;
            }

            if (response.success && response.hints.length > 0) {
                this._resultStore.publishHints(response.hints);
                
                const hint = response.hints[0];
                const label = `${hint.methodName}: ${hint.timeBigO}`;
                if (hint.spaceBigO) {
                    vscode.window.showInformationMessage(
                        `${label}, Space: ${hint.spaceBigO}, Confidence: ${(hint.confidence * 100).toFixed(0)}%`
                    );
                } else {
                    vscode.window.showInformationMessage(
                        `${label}, Confidence: ${(hint.confidence * 100).toFixed(0)}%`
                    );
                }
            }

        } catch (error) {
            if (signal?.aborted) {
                return;
            }

            const message = error instanceof Error ? error.message : String(error);
            this._logger.error(`Position analysis error: ${message}`);
            vscode.window.showErrorMessage(`Analysis failed: ${message}`);
        }
    }

    /**
     * Converts a raw method location to MethodHeaderInfo.
     */
    private toMethodHeaderInfo(
        document: vscode.TextDocument,
        method: {
            methodId: string;
            displayName: string;
            headerStart: number;
            bodyStart: number;
            bodyEnd: number;
        }
    ): MethodHeaderInfo {
        const headerPosition = document.positionAt(method.headerStart);
        
        return {
            methodId: method.methodId,
            displayName: method.displayName,
            headerLine: headerPosition.line,
            headerCharacter: headerPosition.character,
            bodyStart: method.bodyStart,
            bodyEnd: method.bodyEnd
        };
    }

    /**
     * Cancels all pending analysis.
     */
    cancelAll(): void {
        this._analysisDebouncers.cancelAll();
    }

    /**
     * Disposes the runner.
     */
    dispose(): void {
        this._isDisposed = true;
        this._analysisDebouncers.dispose();
        this._backend.dispose();
    }
}
