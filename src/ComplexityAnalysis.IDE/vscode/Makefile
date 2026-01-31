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

# Bundled CLI location (inside extension for distribution)
BUNDLED_CLI_DIR := out/cli

# Runtime identifier for publishing (adjust for target platform)
# Options: osx-x64, osx-arm64, linux-x64, win-x64
RID ?= osx-arm64

#------------------------------------------------------------------------------
# Help
#------------------------------------------------------------------------------
help:
	@echo "Complexity Hints VS Code Extension Build System"
	@echo ""
	@echo "Usage: make [target] [RID=runtime-id]"
	@echo ""
	@echo "Targets:"
	@echo "  all        - Build everything (default)"
	@echo "  build      - Build TypeScript extension and .NET CLI"
	@echo "  compile    - Compile TypeScript only"
	@echo "  install    - Install npm dependencies"
	@echo "  package    - Create .vsix package with bundled CLI"
	@echo "  publish    - Publish to VS Code Marketplace"
	@echo "  clean      - Remove all build artifacts"
	@echo "  clean-ts   - Remove TypeScript build artifacts only"
	@echo "  clean-dotnet - Remove .NET build artifacts only"
	@echo "  test       - Run all tests"
	@echo "  lint       - Run ESLint on TypeScript code"
	@echo "  watch      - Watch and recompile on changes"
	@echo ""
	@echo "Runtime IDs (RID):"
	@echo "  osx-arm64  - macOS Apple Silicon (default)"
	@echo "  osx-x64    - macOS Intel"
	@echo "  linux-x64  - Linux x64"
	@echo "  win-x64    - Windows x64"
	@echo ""
	@echo "Example: make package RID=linux-x64"
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

# Publish .NET CLI as self-contained executable for bundling
publish-dotnet:
	@echo "Publishing .NET CLI for $(RID)..."
	dotnet publish $(CLI_PROJECT) \
		--configuration Release \
		--runtime $(RID) \
		--self-contained true \
		--output $(BUNDLED_CLI_DIR) \
		-p:PublishSingleFile=true \
		-p:EnableCompressionInSingleFile=true \
		-p:DebugType=none \
		-p:DebugSymbols=false
	@# Remove any leftover debug/doc files
	rm -f $(BUNDLED_CLI_DIR)/*.pdb $(BUNDLED_CLI_DIR)/*.xml
	@echo "✓ .NET CLI published to $(BUNDLED_CLI_DIR)/"

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
# Package for distribution (includes bundled CLI)
#------------------------------------------------------------------------------
package: clean compile publish-dotnet
	@echo ""
	@echo "Bundled CLI contents:"
	@ls -la $(BUNDLED_CLI_DIR)/
	@echo ""
	@echo "Creating VSIX package..."
	npm run package
	@echo ""
	@echo "✓ Package created: $$(ls -1 *.vsix 2>/dev/null | head -1)"
	@echo ""
	@echo "Package includes bundled CLI for $(RID)"
	@echo ""
	@echo "Install with:"
	@echo "  code --install-extension $$(ls -1 *.vsix 2>/dev/null | head -1)"

# Multi-platform package (creates separate vsix for each platform)
package-all:
	@echo "Building packages for all platforms..."
	$(MAKE) package RID=osx-arm64
	mv *.vsix complexity-hints-0.1.0-osx-arm64.vsix || true
	$(MAKE) clean-ts
	$(MAKE) compile
	$(MAKE) publish-dotnet RID=osx-x64
	npm run package
	mv *.vsix complexity-hints-0.1.0-osx-x64.vsix || true
	$(MAKE) clean-ts
	$(MAKE) compile
	$(MAKE) publish-dotnet RID=linux-x64
	npm run package
	mv *.vsix complexity-hints-0.1.0-linux-x64.vsix || true
	$(MAKE) clean-ts
	$(MAKE) compile
	$(MAKE) publish-dotnet RID=win-x64
	npm run package
	mv *.vsix complexity-hints-0.1.0-win-x64.vsix || true
	@echo ""
	@echo "✓ Created platform-specific packages:"
	@ls -la *.vsix

#------------------------------------------------------------------------------
# Publish to marketplace
#------------------------------------------------------------------------------
publish: package
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
