/**
 * Complexity Hints VS Code Extension
 * 
 * Provides real-time code complexity analysis with Big-O time/space hints,
 * confidence scores, and amortized/probabilistic analysis indicators
 * displayed as CodeLens annotations above method declarations.
 */

import * as vscode from 'vscode';
import { ComplexityCodeLensProvider } from './providers/codeLensProvider';
import { ResultStore } from './core/resultStore';
import { ComplexityRunner } from './core/complexityRunner';
import { EnvironmentProbe } from './core/environmentProbe';
import { Settings } from './core/settings';
import { OutputLogger } from './core/outputLogger';

let outputLogger: OutputLogger;
let resultStore: ResultStore;
let complexityRunner: ComplexityRunner;
let codeLensProvider: ComplexityCodeLensProvider;
let environmentProbe: EnvironmentProbe;

export async function activate(context: vscode.ExtensionContext): Promise<void> {
    // Initialize output logger
    outputLogger = new OutputLogger();
    outputLogger.info('Complexity Hints extension activating...');

    // Initialize core components
    resultStore = new ResultStore();
    const settings = new Settings();
    
    environmentProbe = new EnvironmentProbe(outputLogger, settings);
    complexityRunner = new ComplexityRunner(resultStore, outputLogger, settings);
    codeLensProvider = new ComplexityCodeLensProvider(resultStore, complexityRunner, settings);

    // Register CodeLens provider for C# files
    const codeLensDisposable = vscode.languages.registerCodeLensProvider(
        { language: 'csharp', scheme: 'file' },
        codeLensProvider
    );
    context.subscriptions.push(codeLensDisposable);

    // Register commands
    context.subscriptions.push(
        vscode.commands.registerCommand('complexity.checkToolchain', async () => {
            await checkToolchainCommand();
        })
    );

    context.subscriptions.push(
        vscode.commands.registerCommand('complexity.analyzeFile', async () => {
            await analyzeFileCommand();
        })
    );

    context.subscriptions.push(
        vscode.commands.registerCommand('complexity.analyzeMethod', async () => {
            await analyzeMethodCommand();
        })
    );

    context.subscriptions.push(
        vscode.commands.registerCommand('complexity.clearCache', () => {
            resultStore.clear();
            codeLensProvider.refresh();
            vscode.window.showInformationMessage('Complexity analysis cache cleared');
        })
    );

    // Subscribe to document changes
    context.subscriptions.push(
        vscode.workspace.onDidChangeTextDocument((event) => {
            if (event.document.languageId === 'csharp') {
                complexityRunner.scheduleAnalysis(event.document);
            }
        })
    );

    // Subscribe to active editor changes
    context.subscriptions.push(
        vscode.window.onDidChangeActiveTextEditor((editor) => {
            if (editor && editor.document.languageId === 'csharp') {
                complexityRunner.scheduleAnalysis(editor.document);
            }
        })
    );

    // Subscribe to settings changes
    context.subscriptions.push(
        vscode.workspace.onDidChangeConfiguration((event) => {
            if (event.affectsConfiguration('complexity')) {
                settings.reload();
                codeLensProvider.refresh();
            }
        })
    );

    // Optionally run environment probe on startup
    if (settings.runEnvProbeOnStartup) {
        environmentProbe.probe().catch(err => {
            outputLogger.error(`Startup environment probe failed: ${err}`);
        });
    }

    // Analyze currently open C# documents
    for (const editor of vscode.window.visibleTextEditors) {
        if (editor.document.languageId === 'csharp') {
            complexityRunner.scheduleAnalysis(editor.document);
        }
    }

    outputLogger.info('Complexity Hints extension activated');
}

export function deactivate(): void {
    outputLogger?.info('Complexity Hints extension deactivating...');
    complexityRunner?.dispose();
    resultStore?.clear();
}

// Command implementations

async function checkToolchainCommand(): Promise<void> {
    outputLogger.show();
    outputLogger.info('Running environment probe...');
    
    try {
        const result = await environmentProbe.probe();
        
        if (result.allOk) {
            vscode.window.showInformationMessage(
                'Complexity Hints: All toolchain checks passed!'
            );
        } else {
            const errors = result.errors.join(', ');
            vscode.window.showWarningMessage(
                `Complexity Hints: Some checks failed - ${errors}`
            );
        }
    } catch (error) {
        const message = error instanceof Error ? error.message : String(error);
        vscode.window.showErrorMessage(
            `Complexity Hints: Environment probe failed - ${message}`
        );
    }
}

async function analyzeFileCommand(): Promise<void> {
    const editor = vscode.window.activeTextEditor;
    if (!editor || editor.document.languageId !== 'csharp') {
        vscode.window.showWarningMessage('Please open a C# file to analyze');
        return;
    }

    outputLogger.info(`Analyzing file: ${editor.document.fileName}`);
    await complexityRunner.analyzeDocument(editor.document, true);
    codeLensProvider.refresh();
    vscode.window.showInformationMessage('Complexity analysis complete');
}

async function analyzeMethodCommand(): Promise<void> {
    const editor = vscode.window.activeTextEditor;
    if (!editor || editor.document.languageId !== 'csharp') {
        vscode.window.showWarningMessage('Please open a C# file to analyze');
        return;
    }

    const position = editor.selection.active;
    outputLogger.info(`Analyzing method at position ${position.line}:${position.character}`);
    
    await complexityRunner.analyzeAtPosition(editor.document, position);
    codeLensProvider.refresh();
}
