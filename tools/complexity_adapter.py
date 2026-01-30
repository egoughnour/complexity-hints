#!/usr/bin/env python3
"""
Complexity notation adapter using complex-expr-parser.

Provides a clean interface for converting human-friendly complexity expressions
(like "n^2", "n*log(n)", "2^n") to SymPy expressions.

This adapter customizes complex-expr-parser for complexity analysis:
- Uses 'n' as the variable (not 'z')
- Avoids conjugate interpretation of 'n*'
- Supports common complexity notation
"""

from __future__ import annotations

import re
from typing import TYPE_CHECKING, Any

from sympy import (
    Abs,
    Symbol,
    factorial,
    floor,
    ceiling,
    binomial,
    log,
    sqrt,
    exp,
    oo,
    pi,
    sympify,
)
from sympy.parsing.sympy_parser import (
    convert_xor,
    function_exponentiation,
    implicit_multiplication_application,
    parse_expr,
    standard_transformations,
)

if TYPE_CHECKING:
    from sympy import Expr


# The complexity variable - positive integer
n: Symbol = Symbol("n", positive=True, integer=True)

# Golden ratio for Fibonacci complexity
phi = (1 + sqrt(5)) / 2


class ComplexityParser:
    """
    Parser for complexity expressions.

    Converts human-friendly notation to SymPy expressions:
    - n^2 → n**2
    - n*log(n) → n*log(n)
    - 2^n → 2**n
    - sqrt(n) → sqrt(n)
    - O(n^2) → n**2 (strips O notation)

    Example:
        >>> parser = ComplexityParser()
        >>> parser.parse("n^2 + n*log(n)")
        n**2 + n*log(n)
    """

    def __init__(self) -> None:
        """Initialize the parser with complexity-specific transformations."""
        self.transformations = (
            standard_transformations
            + (implicit_multiplication_application, convert_xor, function_exponentiation)
        )

        self.local_dict: dict[str, Any] = {
            "n": n,
            "pi": pi,
            "oo": oo,
            "phi": phi,
            "log": log,
            "ln": log,
            "sqrt": sqrt,
            "exp": exp,
            "Abs": Abs,
            "abs": Abs,
            "factorial": factorial,
            "floor": floor,
            "ceiling": ceiling,
            "ceil": ceiling,
            "binomial": binomial,
        }

    def preprocess(self, expr_str: str) -> str:
        """
        Apply complexity-specific preprocessing.

        Args:
            expr_str: The raw expression string.

        Returns:
            Preprocessed string ready for SymPy parsing.
        """
        result = expr_str.strip()

        # Strip O(...) notation if present
        if result.startswith("O(") and result.endswith(")"):
            result = result[2:-1]
        elif result.startswith("Θ(") and result.endswith(")"):
            result = result[2:-1]
        elif result.startswith("Ω(") and result.endswith(")"):
            result = result[2:-1]

        # Handle caret power notation: n^2 → n**2
        result = result.replace("^", "**")

        # Normalize log functions
        result = re.sub(r"\bln\b", "log", result)
        result = re.sub(r"\blog2\b", "log", result)  # log2 → log (base doesn't matter for Big-O)

        # Handle absolute value: |n| → Abs(n)
        result = re.sub(r"\|([^|]+)\|", r"Abs(\1)", result)

        # Handle common aliases
        result = re.sub(r"\bceil\b", "ceiling", result)

        # Ensure proper multiplication between number and variable
        # 2n → 2*n, but don't break 2**n
        result = re.sub(r"(\d)([a-zA-Z])(?!\*\*)", r"\1*\2", result)

        # Handle log^k(n) → log(n)**k
        result = re.sub(r"log\^(\d+)\(n\)", r"log(n)**\1", result)

        # Handle n*log(n) pattern - ensure space doesn't cause issues
        result = re.sub(r"n\s*\*\s*log", "n*log", result)

        return result

    def parse(self, expr_str: str) -> Expr:
        """
        Parse a complexity expression string into a SymPy expression.

        Args:
            expr_str: Human-friendly complexity string.

        Returns:
            A SymPy expression with 'n' as the variable.

        Raises:
            ValueError: If the expression cannot be parsed.

        Example:
            >>> parser = ComplexityParser()
            >>> parser.parse("n^2")
            n**2
            >>> parser.parse("n*log(n)")
            n*log(n)
        """
        original = expr_str
        processed = self.preprocess(expr_str)

        errors: list[str] = []

        # Attempt 1: Direct sympify
        try:
            expr = sympify(processed, locals=self.local_dict)
            return expr
        except Exception as e:
            errors.append(f"Sympify: {e}")

        # Attempt 2: parse_expr with transformations
        try:
            expr = parse_expr(
                processed,
                local_dict=self.local_dict,
                transformations=self.transformations,
            )
            return expr
        except Exception as e:
            errors.append(f"Parse_expr: {e}")

        # Attempt 3: Original with minimal preprocessing
        try:
            basic = expr_str.replace("^", "**")
            expr = sympify(basic, locals=self.local_dict)
            return expr
        except Exception as e:
            errors.append(f"Basic: {e}")

        raise ValueError(
            f"Could not parse complexity expression: '{original}'\n"
            f"Preprocessed to: '{processed}'\n"
            f"Errors: {'; '.join(errors)}"
        )


def parse_complexity(expr_str: str) -> Expr:
    """
    Parse a complexity expression string into a SymPy expression.

    This is a convenience function for one-off parsing.

    Args:
        expr_str: Human-friendly complexity string (e.g., "n^2", "n*log(n)").

    Returns:
        A SymPy expression with 'n' as the variable.

    Example:
        >>> parse_complexity("n^2 + n")
        n**2 + n
        >>> parse_complexity("O(n*log(n))")
        n*log(n)
    """
    parser = ComplexityParser()
    return parser.parse(expr_str)


def validate_complexity(expr_str: str) -> tuple[bool, str | None]:
    """
    Validate a complexity expression without fully processing it.

    Args:
        expr_str: Human-friendly complexity string.

    Returns:
        A tuple of (is_valid, error_message).
        If valid, error_message is None.

    Example:
        >>> validate_complexity("n^2")
        (True, None)
        >>> validate_complexity("n +* 2")
        (False, "Could not parse...")
    """
    if not expr_str or not expr_str.strip():
        return False, "Expression cannot be empty"

    try:
        parser = ComplexityParser()
        _ = parser.parse(expr_str)
        return True, None
    except Exception as e:
        return False, str(e)


# For compatibility with complex-expr-parser API
try:
    from complex_expr_parser import ComplexFunctionParser

    class HybridComplexityParser(ComplexityParser):
        """
        Hybrid parser that can use complex-expr-parser as fallback.

        Tries complexity-specific parsing first, then falls back to
        complex-expr-parser with variable substitution.
        """

        def parse(self, expr_str: str) -> Expr:
            """Parse with fallback to complex-expr-parser."""
            # Try our complexity-specific parser first
            try:
                return super().parse(expr_str)
            except ValueError:
                pass

            # Fallback: use complex-expr-parser with z→n substitution
            try:
                complex_parser = ComplexFunctionParser()
                z_expr_str = expr_str.replace("n", "z")
                z_expr = complex_parser.parse(z_expr_str)
                from sympy import symbols
                z = symbols("z", complex=True)
                return z_expr.subs(z, n)
            except Exception as e:
                raise ValueError(
                    f"Could not parse '{expr_str}' with any strategy: {e}"
                ) from e

except ImportError:
    # complex-expr-parser not available, use base parser
    HybridComplexityParser = ComplexityParser  # type: ignore


if __name__ == "__main__":
    # Test the parser
    test_cases = [
        "1",
        "n",
        "n^2",
        "n**2",
        "n * log(n)",
        "n*log(n)",
        "2^n",
        "sqrt(n)",
        "n^(1/2)",
        "log(n)",
        "log^2(n)",
        "n^2 + n",
        "O(n^2)",
        "phi^n",
        "factorial(n)",
        "n!",
    ]

    parser = ComplexityParser()

    print("Complexity Parser Tests")
    print("=" * 50)
    for expr in test_cases:
        try:
            result = parser.parse(expr)
            print(f"{expr:20} → {result}")
        except Exception as e:
            print(f"{expr:20} → ERROR: {e}")
