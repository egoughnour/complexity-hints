/**
 * Output logging for the Complexity Hints extension.
 * Provides structured logging to VS Code's Output panel.
 */

import * as vscode from 'vscode';

export enum LogLevel {
    Debug = 0,
    Info = 1,
    Warn = 2,
    Error = 3
}

export class OutputLogger {
    private readonly _channel: vscode.OutputChannel;
    private _minLevel: LogLevel = LogLevel.Info;

    constructor() {
        this._channel = vscode.window.createOutputChannel('Complexity Engine');
    }

    /**
     * Sets the minimum log level to display.
     */
    setMinLevel(level: LogLevel): void {
        this._minLevel = level;
    }

    /**
     * Shows the output channel panel.
     */
    show(): void {
        this._channel.show(true);
    }

    /**
     * Hides the output channel panel.
     */
    hide(): void {
        this._channel.hide();
    }

    /**
     * Clears the output channel.
     */
    clear(): void {
        this._channel.clear();
    }

    /**
     * Logs a debug message.
     */
    debug(message: string): void {
        this.log(LogLevel.Debug, message);
    }

    /**
     * Logs an info message.
     */
    info(message: string): void {
        this.log(LogLevel.Info, message);
    }

    /**
     * Logs a warning message.
     */
    warn(message: string): void {
        this.log(LogLevel.Warn, message);
    }

    /**
     * Logs an error message.
     */
    error(message: string): void {
        this.log(LogLevel.Error, message);
    }

    /**
     * Logs a message at the specified level.
     */
    log(level: LogLevel, message: string): void {
        if (level < this._minLevel) {
            return;
        }

        const timestamp = new Date().toISOString().substring(11, 23);
        const levelStr = LogLevel[level].toUpperCase().padEnd(5);
        const formatted = `[${timestamp}] [${levelStr}] ${message}`;
        
        this._channel.appendLine(formatted);
    }

    /**
     * Logs a separator line.
     */
    separator(title?: string): void {
        if (title) {
            this._channel.appendLine(`\n${'='.repeat(60)}`);
            this._channel.appendLine(`  ${title}`);
            this._channel.appendLine(`${'='.repeat(60)}`);
        } else {
            this._channel.appendLine('-'.repeat(60));
        }
    }

    /**
     * Logs the result of an environment probe.
     */
    logProbeResult(result: {
        dotnetOk: boolean;
        dotnetVersion?: string;
        pythonOk: boolean;
        pythonVersion?: string;
        uvOk: boolean;
        uvVersion?: string;
        nodeOk: boolean;
        nodeVersion?: string;
        errors: string[];
    }): void {
        this.separator('Environment Probe Results');
        
        const ok = '✓';
        const fail = '✗';

        this._channel.appendLine(
            `dotnet: ${result.dotnetOk ? ok : fail} ${result.dotnetVersion || 'not found'}`
        );
        this._channel.appendLine(
            `python: ${result.pythonOk ? ok : fail} ${result.pythonVersion || 'not found'}`
        );
        this._channel.appendLine(
            `uv:     ${result.uvOk ? ok : fail} ${result.uvVersion || 'not found'}`
        );
        this._channel.appendLine(
            `node:   ${result.nodeOk ? ok : fail} ${result.nodeVersion || 'not found'}`
        );

        if (result.errors.length > 0) {
            this._channel.appendLine('\nErrors:');
            for (const error of result.errors) {
                this._channel.appendLine(`  - ${error}`);
            }
        }

        const allOk = result.dotnetOk && result.pythonOk && result.uvOk;
        this._channel.appendLine(
            `\n${allOk ? 'All required checks passed.' : 'Some checks failed.'}`
        );
        this.separator();
    }

    /**
     * Disposes the output channel.
     */
    dispose(): void {
        this._channel.dispose();
    }
}
