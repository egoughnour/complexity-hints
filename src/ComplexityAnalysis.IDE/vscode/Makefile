# Makefile for Complexity Hints VS Code Extension
# Builds both the TypeScript extension and the .NET CLI backend

.PHONY: all clean clean-ts clean-dotnet install compile build package publish help

# Default target
all: build

# Path to CLI project (relative to this Makefile)
CLI_PROJECT := ../Cli/ComplexityAnalysis.IDE.Cli.csproj
CLI_BIN_DIR := ../Cli/bin
CLI_OBJ_DIR := ../Cli/obj

# TypeScript output directories
TS_OUT_DIR := out
NODE_MODULES := node_modules

#------------------------------------------------------------------------------
# Help
#------------------------------------------------------------------------------
help:
	@echo "Complexity Hints VS Code Extension Build System"
	@echo ""
	@echo "Usage: make [target]"
	@echo ""
	@echo "Targets:"
	@echo "  all       - Build everything (default)"
	@echo "  build     - Build TypeScript extension and .NET CLI"
	@echo "  compile   - Compile TypeScript only"
	@echo "  install   - Install npm dependencies"
	@echo "  package   - Create .vsix package"
	@echo "  publish   - Publish to VS Code Marketplace"
	@echo "  clean     - Remove all build artifacts"
	@echo "  clean-ts  - Remove TypeScript build artifacts only"
	@echo "  clean-dotnet - Remove .NET build artifacts only"
	@echo "  test      - Run all tests"
	@echo "  lint      - Run ESLint on TypeScript code"
	@echo "  watch     - Watch and recompile on changes"
	@echo ""

#------------------------------------------------------------------------------
# Clean targets
#------------------------------------------------------------------------------
clean: clean-ts clean-dotnet
	@echo "✓ All build artifacts cleaned"

clean-ts:
	@echo "Cleaning TypeScript artifacts..."
	rm -rf $(TS_OUT_DIR)
	rm -f *.vsix
	@echo "✓ TypeScript artifacts cleaned"

clean-dotnet:
	@echo "Cleaning .NET artifacts..."
	rm -rf $(CLI_BIN_DIR)
	rm -rf $(CLI_OBJ_DIR)
	@echo "✓ .NET artifacts cleaned"

#------------------------------------------------------------------------------
# Install dependencies
#------------------------------------------------------------------------------
install: $(NODE_MODULES)

$(NODE_MODULES): package.json
	@echo "Installing npm dependencies..."
	npm install
	@touch $(NODE_MODULES)

#------------------------------------------------------------------------------
# Compile targets
#------------------------------------------------------------------------------
compile: install
	@echo "Compiling TypeScript..."
	npm run compile
	@echo "✓ TypeScript compiled to $(TS_OUT_DIR)/"

compile-dotnet:
	@echo "Building .NET CLI..."
	dotnet build $(CLI_PROJECT) --configuration Release
	@echo "✓ .NET CLI built"

#------------------------------------------------------------------------------
# Build (compile everything)
#------------------------------------------------------------------------------
build: clean compile compile-dotnet
	@echo ""
	@echo "=========================================="
	@echo "✓ Build complete!"
	@echo "=========================================="
	@echo ""
	@echo "TypeScript output: $(TS_OUT_DIR)/"
	@echo ".NET CLI output:   $(CLI_BIN_DIR)/Release/"
	@echo ""
	@echo "Next steps:"
	@echo "  make package   - Create .vsix for distribution"
	@echo "  make watch     - Watch for changes during development"
	@echo ""

#------------------------------------------------------------------------------
# Package for distribution
#------------------------------------------------------------------------------
package: build
	@echo "Creating VSIX package..."
	npm run package
	@echo ""
	@echo "✓ Package created: $$(ls -1 *.vsix 2>/dev/null | head -1)"
	@echo ""
	@echo "Install with:"
	@echo "  code --install-extension $$(ls -1 *.vsix 2>/dev/null | head -1)"

#------------------------------------------------------------------------------
# Publish to marketplace
#------------------------------------------------------------------------------
publish: build
	@echo "Publishing to VS Code Marketplace..."
	npm run publish

#------------------------------------------------------------------------------
# Development helpers
#------------------------------------------------------------------------------
watch: install
	@echo "Starting TypeScript watch mode..."
	npm run watch

lint: install
	@echo "Running ESLint..."
	npm run lint

#------------------------------------------------------------------------------
# Test targets
#------------------------------------------------------------------------------
test: compile
	@echo "Running TypeScript tests..."
	npm test

test-dotnet:
	@echo "Running .NET tests..."
	dotnet test ../../../ComplexityAnalysis.Tests/ComplexityAnalysis.Tests.csproj --verbosity minimal

test-all: test test-dotnet
	@echo "✓ All tests complete"

#------------------------------------------------------------------------------
# Quick rebuild (no clean)
#------------------------------------------------------------------------------
rebuild: compile compile-dotnet
	@echo "✓ Rebuild complete (no clean)"
