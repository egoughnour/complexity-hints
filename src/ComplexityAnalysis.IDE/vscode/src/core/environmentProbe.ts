/**
 * Environment probe for checking toolchain availability.
 * Validates that required tools (dotnet, python, uv) are installed and accessible.
 */

import * as vscode from 'vscode';
import { spawn } from 'child_process';
import { OutputLogger } from './outputLogger';
import { Settings } from './settings';
import { EnvProbeResult } from './types';

export class EnvironmentProbe {
    private _lastResult: EnvProbeResult | null = null;

    constructor(
        private readonly _logger: OutputLogger,
        private readonly _settings: Settings
    ) {}

    /**
     * Gets the last probe result, if any.
     */
    get lastResult(): EnvProbeResult | null {
        return this._lastResult;
    }

    /**
     * Runs the environment probe.
     */
    async probe(): Promise<EnvProbeResult> {
        this._logger.separator('Environment Probe');
        this._logger.info('Checking toolchain availability...');

        const result: EnvProbeResult = {
            dotnetOk: false,
            pythonOk: false,
            uvOk: false,
            nodeOk: false,
            allOk: false,
            errors: [],
            timestamp: new Date()
        };

        // Check dotnet
        try {
            const dotnetVersion = await this.runCommand(
                this._settings.getDotnetCommand(),
                ['--version']
            );
            if (dotnetVersion) {
                result.dotnetOk = true;
                result.dotnetVersion = dotnetVersion.trim();
                this._logger.info(`dotnet: ${result.dotnetVersion}`);
            } else {
                result.errors.push('dotnet not found or failed to run');
                this._logger.warn('dotnet: not found');
            }
        } catch (error) {
            const message = error instanceof Error ? error.message : String(error);
            result.errors.push(`dotnet: ${message}`);
            this._logger.warn(`dotnet: ${message}`);
        }

        // Check Python
        try {
            const pythonVersion = await this.runCommand(
                this._settings.getPythonCommand(),
                ['--version']
            );
            if (pythonVersion) {
                result.pythonOk = true;
                result.pythonVersion = pythonVersion.trim().replace('Python ', '');
                this._logger.info(`python: ${result.pythonVersion}`);
            } else {
                result.errors.push('python not found or failed to run');
                this._logger.warn('python: not found');
            }
        } catch (error) {
            const message = error instanceof Error ? error.message : String(error);
            result.errors.push(`python: ${message}`);
            this._logger.warn(`python: ${message}`);
        }

        // Check uv
        try {
            const uvVersion = await this.runCommand(
                this._settings.getUvCommand(),
                ['--version']
            );
            if (uvVersion) {
                result.uvOk = true;
                // uv --version outputs "uv 0.5.0" format
                result.uvVersion = uvVersion.trim().replace('uv ', '');
                this._logger.info(`uv: ${result.uvVersion}`);
            } else {
                result.errors.push('uv not found or failed to run');
                this._logger.warn('uv: not found');
            }
        } catch (error) {
            const message = error instanceof Error ? error.message : String(error);
            result.errors.push(`uv: ${message}`);
            this._logger.warn(`uv: ${message}`);
        }

        // Check Node.js (optional, for future use)
        try {
            const nodeVersion = await this.runCommand('node', ['--version']);
            if (nodeVersion) {
                result.nodeOk = true;
                result.nodeVersion = nodeVersion.trim().replace('v', '');
                this._logger.info(`node: ${result.nodeVersion}`);
            }
        } catch {
            // Node is optional, don't add to errors
            this._logger.debug('node: not found (optional)');
        }

        // Run canary test if uv and python are available
        if (result.uvOk && result.pythonOk) {
            try {
                const canary = await this.runCommand(
                    this._settings.getUvCommand(),
                    ['run', 'python', '-c', 'import sys; print(sys.version_info[:2])']
                );
                if (canary) {
                    this._logger.info(`uv run canary: ${canary.trim()}`);
                }
            } catch (error) {
                const message = error instanceof Error ? error.message : String(error);
                result.errors.push(`uv run canary: ${message}`);
                this._logger.warn(`uv run canary failed: ${message}`);
            }
        }

        // Determine overall status
        result.allOk = result.dotnetOk && result.pythonOk && result.uvOk;

        // Log summary
        this._logger.logProbeResult(result);

        this._lastResult = result;
        return result;
    }

    /**
     * Runs a command and returns stdout.
     */
    private runCommand(command: string, args: string[]): Promise<string | null> {
        return new Promise((resolve, reject) => {
            const proc = spawn(command, args, {
                shell: true,
                timeout: 10000 // 10 second timeout
            });

            let stdout = '';
            let stderr = '';

            proc.stdout?.on('data', (data) => {
                stdout += data.toString();
            });

            proc.stderr?.on('data', (data) => {
                stderr += data.toString();
            });

            proc.on('error', (error) => {
                reject(error);
            });

            proc.on('close', (code) => {
                if (code === 0) {
                    resolve(stdout || stderr);
                } else if (stdout || stderr) {
                    // Some commands output to stderr even on success
                    resolve(stdout || stderr);
                } else {
                    reject(new Error(`Command exited with code ${code}`));
                }
            });

            // Handle timeout
            proc.on('exit', (code, signal) => {
                if (signal === 'SIGTERM') {
                    reject(new Error('Command timed out'));
                }
            });
        });
    }
}
