/**
 * Settings management for the Complexity Hints extension.
 * Wraps VS Code configuration API with typed accessors.
 */

import * as vscode from 'vscode';

export class Settings {
    private _config: vscode.WorkspaceConfiguration;

    constructor() {
        this._config = vscode.workspace.getConfiguration('complexity');
    }

    /**
     * Reloads settings from VS Code configuration.
     */
    reload(): void {
        this._config = vscode.workspace.getConfiguration('complexity');
    }

    /**
     * Whether CodeLens hints are enabled.
     */
    get enableCodeLens(): boolean {
        return this._config.get<boolean>('enableCodeLens', true);
    }

    /**
     * Debounce delay (ms) before running complexity analysis.
     */
    get analysisDebounceMs(): number {
        return this._config.get<number>('analysisDebounceMs', 350);
    }

    /**
     * Debounce delay (ms) before updating CodeLens tags.
     */
    get tagDebounceMs(): number {
        return this._config.get<number>('tagDebounceMs', 100);
    }

    /**
     * Minimum confidence score to show as 'high confidence'.
     */
    get confidenceThreshold(): number {
        return this._config.get<number>('confidenceThreshold', 0.7);
    }

    /**
     * Whether to show space complexity alongside time complexity.
     */
    get showSpaceComplexity(): boolean {
        return this._config.get<boolean>('showSpaceComplexity', true);
    }

    /**
     * Whether to show confidence score in CodeLens.
     */
    get showConfidence(): boolean {
        return this._config.get<boolean>('showConfidence', true);
    }

    /**
     * Path to dotnet executable (empty to use PATH).
     */
    get dotnetPath(): string {
        return this._config.get<string>('dotnetPath', '');
    }

    /**
     * Path to Python executable (empty to use PATH).
     */
    get pythonPath(): string {
        return this._config.get<string>('pythonPath', '');
    }

    /**
     * Path to uv executable (empty to use PATH).
     */
    get uvPath(): string {
        return this._config.get<string>('uvPath', '');
    }

    /**
     * Whether to run environment probe on extension startup.
     */
    get runEnvProbeOnStartup(): boolean {
        return this._config.get<boolean>('runEnvProbeOnStartup', false);
    }

    /**
     * Whether to use the .NET backend for analysis (vs heuristics).
     */
    get useBackendAnalysis(): boolean {
        return this._config.get<boolean>('useBackendAnalysis', false);
    }

    /**
     * Path to the CLI project for backend analysis.
     */
    get cliProjectPath(): string {
        return this._config.get<string>('cliProjectPath', '');
    }

    /**
     * Gets the effective dotnet command.
     */
    getDotnetCommand(): string {
        return this.dotnetPath || 'dotnet';
    }

    /**
     * Gets the effective Python command.
     */
    getPythonCommand(): string {
        return this.pythonPath || 'python3';
    }

    /**
     * Gets the effective uv command.
     */
    getUvCommand(): string {
        return this.uvPath || 'uv';
    }
}
