/**
 * Simple regex-based C# method locator for VS Code.
 * 
 * This provides a lightweight alternative to Roslyn for basic method detection.
 * For more accurate parsing, we would integrate with the .NET backend.
 * 
 * Supported patterns:
 * - Regular methods: public void MethodName()
 * - Constructors: public ClassName()
 * - Expression-bodied members: public int Prop => ...
 * - Static methods, async methods, etc.
 */

import { makeMethodId } from '../core/types';

interface RawMethodInfo {
    methodId: string;
    displayName: string;
    headerStart: number;
    bodyStart: number;
    bodyEnd: number;
    syntaxKind: string;
}

export class CSharpMethodLocator {
    /**
     * Regular expression for C# method declarations.
     * Captures: modifiers, return type, method name, parameters
     */
    private static readonly METHOD_PATTERN = /(?:(?:public|private|protected|internal|static|virtual|override|abstract|sealed|async|partial|extern|new|unsafe)\s+)*(?:[\w<>\[\],\s]+)\s+(\w+)\s*\(([^)]*)\)\s*(?:where[^{]*)?({|\s*=>)/g;

    /**
     * Regular expression for constructors.
     */
    private static readonly CONSTRUCTOR_PATTERN = /(?:(?:public|private|protected|internal|static)\s+)+(\w+)\s*\(([^)]*)\)\s*(?::\s*(?:base|this)\s*\([^)]*\)\s*)?({)/g;

    /**
     * Finds all methods in a C# source file.
     */
    findMethods(sourceText: string): RawMethodInfo[] {
        const methods: RawMethodInfo[] = [];
        
        // Find constructors first
        this.findConstructors(sourceText, methods);
        
        // Find regular methods
        this.findRegularMethods(sourceText, methods);
        
        // Sort by position
        methods.sort((a, b) => a.headerStart - b.headerStart);
        
        return methods;
    }

    private findConstructors(sourceText: string, methods: RawMethodInfo[]): void {
        const pattern = new RegExp(CSharpMethodLocator.CONSTRUCTOR_PATTERN);
        let match: RegExpExecArray | null;

        while ((match = pattern.exec(sourceText)) !== null) {
            const className = match[1];
            const headerStart = match.index;
            const braceIndex = match.index + match[0].length - 1;

            // Find matching closing brace
            const bodyEnd = this.findMatchingBrace(sourceText, braceIndex);
            if (bodyEnd === -1) {
                continue;
            }

            const methodId = makeMethodId('ConstructorDeclaration', className, headerStart);

            methods.push({
                methodId,
                displayName: className,
                headerStart,
                bodyStart: braceIndex,
                bodyEnd,
                syntaxKind: 'ConstructorDeclaration'
            });
        }
    }

    private findRegularMethods(sourceText: string, methods: RawMethodInfo[]): void {
        const pattern = new RegExp(CSharpMethodLocator.METHOD_PATTERN);
        let match: RegExpExecArray | null;

        // Track constructor positions to avoid duplicate detection
        const constructorPositions = new Set(
            methods
                .filter(m => m.syntaxKind === 'ConstructorDeclaration')
                .map(m => m.headerStart)
        );

        while ((match = pattern.exec(sourceText)) !== null) {
            const methodName = match[1];
            const headerStart = match.index;

            // Skip if this is actually a constructor we already found
            if (constructorPositions.has(headerStart)) {
                continue;
            }

            // Skip common non-method patterns
            if (this.isLikelyNotMethod(methodName, sourceText, match.index)) {
                continue;
            }

            const lastChar = match[0].trim().slice(-1);
            let bodyStart: number;
            let bodyEnd: number;

            if (lastChar === '{') {
                // Block body
                bodyStart = match.index + match[0].lastIndexOf('{');
                bodyEnd = this.findMatchingBrace(sourceText, bodyStart);
            } else {
                // Expression body (=>)
                bodyStart = match.index + match[0].lastIndexOf('=>');
                bodyEnd = this.findStatementEnd(sourceText, bodyStart);
            }

            if (bodyEnd === -1) {
                continue;
            }

            const methodId = makeMethodId('MethodDeclaration', methodName, headerStart);

            methods.push({
                methodId,
                displayName: methodName,
                headerStart,
                bodyStart,
                bodyEnd,
                syntaxKind: 'MethodDeclaration'
            });
        }
    }

    /**
     * Checks if the match is likely not a method (e.g., if statement, while loop).
     */
    private isLikelyNotMethod(name: string, sourceText: string, index: number): boolean {
        // Common C# keywords that look like methods
        const keywords = new Set([
            'if', 'while', 'for', 'foreach', 'switch', 'using', 'lock', 'fixed',
            'catch', 'when', 'nameof', 'typeof', 'sizeof', 'default', 'checked',
            'unchecked', 'stackalloc', 'delegate', 'get', 'set', 'add', 'remove',
            'init', 'value'
        ]);

        if (keywords.has(name)) {
            return true;
        }

        // Check if it's inside a string or comment
        // This is a simplified check - full parsing would be more accurate
        const before = sourceText.substring(Math.max(0, index - 50), index);
        if (before.includes('//') || before.includes('/*') || before.includes('"')) {
            // Might be in a comment or string - be conservative
            // Actually let's not skip since this is too aggressive
        }

        return false;
    }

    /**
     * Finds the matching closing brace for an opening brace.
     */
    private findMatchingBrace(sourceText: string, openBraceIndex: number): number {
        let depth = 1;
        let i = openBraceIndex + 1;
        let inString = false;
        let inChar = false;
        let inLineComment = false;
        let inBlockComment = false;

        while (i < sourceText.length && depth > 0) {
            const char = sourceText[i];
            const prevChar = i > 0 ? sourceText[i - 1] : '';
            const nextChar = i < sourceText.length - 1 ? sourceText[i + 1] : '';

            // Handle comments
            if (!inString && !inChar) {
                if (inLineComment) {
                    if (char === '\n') {
                        inLineComment = false;
                    }
                    i++;
                    continue;
                }

                if (inBlockComment) {
                    if (char === '*' && nextChar === '/') {
                        inBlockComment = false;
                        i += 2;
                        continue;
                    }
                    i++;
                    continue;
                }

                if (char === '/' && nextChar === '/') {
                    inLineComment = true;
                    i += 2;
                    continue;
                }

                if (char === '/' && nextChar === '*') {
                    inBlockComment = true;
                    i += 2;
                    continue;
                }
            }

            // Handle strings
            if (char === '"' && prevChar !== '\\' && !inChar) {
                inString = !inString;
                i++;
                continue;
            }

            // Handle char literals
            if (char === "'" && prevChar !== '\\' && !inString) {
                inChar = !inChar;
                i++;
                continue;
            }

            // Count braces outside strings/comments
            if (!inString && !inChar) {
                if (char === '{') {
                    depth++;
                } else if (char === '}') {
                    depth--;
                }
            }

            i++;
        }

        return depth === 0 ? i - 1 : -1;
    }

    /**
     * Finds the end of an expression-bodied member.
     */
    private findStatementEnd(sourceText: string, arrowIndex: number): number {
        let i = arrowIndex + 2; // Skip =>
        let depth = 0;
        let inString = false;

        // Skip whitespace
        while (i < sourceText.length && /\s/.test(sourceText[i])) {
            i++;
        }

        while (i < sourceText.length) {
            const char = sourceText[i];
            const prevChar = i > 0 ? sourceText[i - 1] : '';

            // Handle strings
            if (char === '"' && prevChar !== '\\') {
                inString = !inString;
                i++;
                continue;
            }

            if (!inString) {
                // Track parentheses/brackets for nested expressions
                if (char === '(' || char === '[' || char === '{') {
                    depth++;
                } else if (char === ')' || char === ']' || char === '}') {
                    if (depth === 0) {
                        // Found the end (likely a closing brace of containing block)
                        return i - 1;
                    }
                    depth--;
                } else if (char === ';' && depth === 0) {
                    return i;
                }
            }

            i++;
        }

        return sourceText.length - 1;
    }
}
