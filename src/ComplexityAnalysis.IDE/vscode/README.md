# Complexity Hints for VS Code

A VS Code extension that displays algorithmic complexity hints (Big-O notation) directly in the editor as CodeLens annotations above C# methods.

## Features

- **CodeLens Complexity Hints**: See time and space complexity directly above method signatures
- **Confidence Indicators**: Visual indicator of analysis confidence level
- **Debounced Analysis**: Efficient analysis that triggers after you stop typing
- **Review Warnings**: Methods with potential issues are flagged for human review

![Complexity Hints Demo](docs/demo.png)

## Requirements

- **VS Code 1.74+**
- **Node.js 18+** (for development)
- **.NET 8.0 SDK** (required for the analysis backend)
- **Python 3.11+** (optional, for advanced recurrence solving)
- **uv 0.5+** (optional, for Python environment management)

## Installation

### From VSIX

1. Download the `.vsix` file from the releases page
2. In VS Code, open the Command Palette (`Cmd+Shift+P` / `Ctrl+Shift+P`)
3. Run "Extensions: Install from VSIX..."
4. Select the downloaded file

### From Source

```bash
cd src/ComplexityAnalysis.IDE/vscode
npm install
npm run compile
npm run package
```

This creates a `.vsix` file that can be installed locally.

## Usage

1. Open a C# file in VS Code
2. Wait for the extension to activate (you'll see "Complexity Hints" in the status bar)
3. CodeLens annotations will appear above each method showing:
   - **T**: Time complexity (e.g., `O(n log n)`)
   - **S**: Space complexity (e.g., `O(n)`)
   - Confidence score (0-100%)

### Commands

| Command | Description |
|---------|-------------|
| `Complexity: Check Toolchain` | Verify that .NET, Python, and uv are installed |
| `Complexity: Analyze Current File` | Force re-analyze the current file |
| `Complexity: Analyze Method at Cursor` | Analyze only the method under cursor |
| `Complexity: Show Output` | Open the output panel for diagnostics |

## Configuration

| Setting | Default | Description |
|---------|---------|-------------|
| `complexity.enableCodeLens` | `true` | Enable/disable CodeLens hints |
| `complexity.analysisDebounceMs` | `350` | Debounce delay before triggering analysis |
| `complexity.dotnetPath` | `"dotnet"` | Path to .NET CLI |
| `complexity.pythonPath` | `"python3"` | Path to Python interpreter |
| `complexity.uvPath` | `"uv"` | Path to uv package manager |
| `complexity.cliProjectPath` | (auto) | Path to the CLI project for analysis |

## How It Works

The extension uses a multi-stage pipeline:

1. **Document Change Detection**: Monitors C# files for changes
2. **Debounced Analysis**: Waits for typing to stop before analyzing
3. **Backend Communication**: Invokes the .NET CLI tool for Roslyn-based analysis
4. **Result Caching**: Caches results by document version to avoid re-analysis
5. **CodeLens Display**: Shows results as CodeLens annotations

### Analysis Pipeline

```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│  Document   │───▶│  Debouncer  │───▶│   Backend   │
│   Change    │    │  (350ms)    │    │   CLI       │
└─────────────┘    └─────────────┘    └─────────────┘
                                            │
                                            ▼
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│  CodeLens   │◀───│   Result    │◀───│   Roslyn    │
│   Display   │    │   Store     │    │   Analysis  │
└─────────────┘    └─────────────┘    └─────────────┘
```

## Development

### Project Structure

```
src/ComplexityAnalysis.IDE/
├── vscode/                 # VS Code extension (TypeScript)
│   ├── src/
│   │   ├── extension.ts    # Main entry point
│   │   ├── core/           # Shared utilities
│   │   ├── providers/      # CodeLens provider
│   │   └── analysis/       # Backend integration
│   ├── package.json        # Extension manifest
│   └── tsconfig.json       # TypeScript config
└── Cli/                    # .NET CLI tool (C#)
    ├── Program.cs          # CLI entry point
    ├── Commands/           # CLI commands
    └── Models/             # JSON output models
```

### Building

```bash
# Build the TypeScript extension
cd src/ComplexityAnalysis.IDE/vscode
npm install
npm run compile

# Build the .NET CLI
cd src/ComplexityAnalysis.IDE/Cli
dotnet build

# Package the extension
npm run package
```

### Testing

```bash
# Run TypeScript tests
npm test

# Run .NET tests
dotnet test
```

### Debugging

1. Open the workspace in VS Code
2. Press F5 to launch the Extension Development Host
3. Open a C# file in the new window
4. View logs in the "Complexity Hints" output channel

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run tests (`npm test` and `dotnet test`)
5. Submit a pull request

## License

MIT License - see [LICENSE](../../../LICENSE) for details.

## Acknowledgments

- [Roslyn](https://github.com/dotnet/roslyn) for C# analysis
- [VS Code Extension API](https://code.visualstudio.com/api)
- The ComplexityAnalysis.Core team for the underlying analysis engine
