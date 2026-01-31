/**
 * CodeLens provider for displaying complexity hints above methods.
 * 
 * This provider:
 * 1. Queries the ResultStore for cached hints
 * 2. Returns CodeLens items positioned at method headers
 * 3. Triggers analysis via ComplexityRunner when needed
 * 4. Refreshes when results are published
 */

import * as vscode from 'vscode';
import { ResultStore } from '../core/resultStore';
import { ComplexityRunner } from '../core/complexityRunner';
import { Settings } from '../core/settings';
import { ComplexityHint, formatHintLabel, formatHintTooltip, MethodHeaderInfo } from '../core/types';

/**
 * Custom CodeLens with attached complexity hint data.
 */
class ComplexityCodeLens extends vscode.CodeLens {
    constructor(
        range: vscode.Range,
        public readonly methodId: string,
        public readonly methodName: string
    ) {
        super(range);
    }
}

export class ComplexityCodeLensProvider implements vscode.CodeLensProvider {
    private readonly _onDidChangeCodeLenses = new vscode.EventEmitter<void>();
    public readonly onDidChangeCodeLenses = this._onDidChangeCodeLenses.event;

    private _unsubscribe: (() => void) | null = null;

    constructor(
        private readonly _resultStore: ResultStore,
        private readonly _runner: ComplexityRunner,
        private readonly _settings: Settings
    ) {
        // Subscribe to result store changes to refresh CodeLenses
        this._unsubscribe = this._resultStore.subscribeGlobal(() => {
            this._onDidChangeCodeLenses.fire();
        });
    }

    /**
     * Triggers a refresh of all CodeLenses.
     */
    refresh(): void {
        this._onDidChangeCodeLenses.fire();
    }

    /**
     * Disposes the provider.
     */
    dispose(): void {
        this._unsubscribe?.();
        this._onDidChangeCodeLenses.dispose();
    }

    /**
     * Provides CodeLens items for the document.
     */
    provideCodeLenses(
        document: vscode.TextDocument,
        token: vscode.CancellationToken
    ): vscode.ProviderResult<vscode.CodeLens[]> {
        if (!this._settings.enableCodeLens) {
            return [];
        }

        if (document.languageId !== 'csharp') {
            return [];
        }

        const documentUri = document.uri.toString();
        const methods = this._resultStore.getMethodsForDocument(documentUri);

        // If we don't have method info yet, trigger analysis
        if (methods.length === 0) {
            // Fire and forget - results will come via refresh
            this._runner.scheduleAnalysis(document);
            return [];
        }

        const codeLenses: vscode.CodeLens[] = [];

        for (const method of methods) {
            if (token.isCancellationRequested) {
                break;
            }

            const range = new vscode.Range(
                method.headerLine,
                method.headerCharacter,
                method.headerLine,
                method.headerCharacter + method.displayName.length
            );

            codeLenses.push(new ComplexityCodeLens(range, method.methodId, method.displayName));
        }

        return codeLenses;
    }

    /**
     * Resolves a CodeLens by filling in its command with the label.
     */
    resolveCodeLens(
        codeLens: vscode.CodeLens,
        token: vscode.CancellationToken
    ): vscode.ProviderResult<vscode.CodeLens> {
        if (!(codeLens instanceof ComplexityCodeLens)) {
            return codeLens;
        }

        if (token.isCancellationRequested) {
            return codeLens;
        }

        const hint = this._resultStore.getHint(codeLens.methodId);

        if (hint) {
            // We have a cached result
            const label = formatHintLabel(
                hint,
                this._settings.showSpaceComplexity,
                this._settings.showConfidence
            );

            codeLens.command = {
                title: label,
                command: 'complexity.analyzeMethod',
                tooltip: formatHintTooltip(hint)
            };
        } else {
            // No result yet - show analyzing indicator
            codeLens.command = {
                title: 'Complexity: analyzing...',
                command: 'complexity.analyzeMethod',
                tooltip: 'Click to analyze this method'
            };
        }

        return codeLens;
    }
}
