# Changelog

All notable changes to the "Complexity Hints" extension will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.1.0] - 2025-01-xx

### Added
- Initial release
- CodeLens complexity hints for C# methods
- Time complexity analysis using Roslyn
- Confidence scoring for analysis results
- Configurable debounce for analysis trigger
- Environment probe command for toolchain validation
- Commands:
  - `complexity.checkToolchain` - Verify required tools
  - `complexity.analyzeFile` - Re-analyze current file
  - `complexity.analyzeMethod` - Analyze method at cursor
  - `complexity.showOutput` - Open output panel

### Known Limitations
- Only C# files are supported in this release
- Space complexity analysis is not yet implemented
- Partial code analysis may have reduced accuracy
