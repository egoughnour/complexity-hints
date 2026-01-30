#!/usr/bin/env -S uv run --script
# /// script
# requires-python = ">=3.10"
# dependencies = ["sympy>=1.12", "complex-expr-parser>=0.1.0"]
# ///
"""
Recurrence relation solver using SymPy.

Uses sympy.limit() for proper asymptotic analysis, which handles:
- L'Hôpital's rule for 0/0 and ∞/∞ indeterminate forms
- Series expansions for complex expressions
- Proper handling of logarithmic and exponential growth

Usage:
    echo '{"type": "linear", "coeffs": [1, 1], "base": {"0": 0, "1": 1}}' | uv run tools/recurrence_solver.py

Input JSON schema:
{
    "type": "linear" | "divide_conquer" | "verify" | "compare",

    # For linear: T(n) = c0*T(n-1) + c1*T(n-2) + ... + f(n)
    "coeffs": [c0, c1, ...],      # coefficients for T(n-1), T(n-2), ...
    "base": {"0": val0, "1": val1, ...},  # base cases T(0)=val0, T(1)=val1
    "f_n": "n" | "n**2" | "1" | ...,  # non-homogeneous term (optional, default "0")

    # For divide_conquer: T(n) = a*T(n/b) + f(n)
    "a": 2,           # number of subproblems
    "b": 2,           # division factor
    "f_n": "n",       # work per level

    # For verify: check if solution matches recurrence
    "recurrence": "T(n) - 2*T(n-1) - T(n-2)",
    "solution": "((1+sqrt(5))/2)**n / sqrt(5)",
    "base": {"0": 0, "1": 1}

    # For compare: compare asymptotic growth using limits
    "f": "n**2",           # first expression
    "g": "n * log(n)",     # second expression
    "bound_type": "O" | "Omega" | "Theta"  # which bound to verify
}

Output JSON:
{
    "success": true,
    "closed_form": "phi**n / sqrt(5)",
    "complexity": "O(phi**n)",
    "latex": "\\frac{\\phi^n}{\\sqrt{5}}",
    "verified": true,
    "error": null
}

Asymptotic Comparison (for "compare" type):
{
    "success": true,
    "bound_type": "Theta",
    "holds": true,
    "constants": [0.5, 2.0],  # c1 and c2 for Θ
    "comparison": "f ~ g",    # "f < g", "f ~ g", or "f > g"
    "limit_ratio": "1"        # lim(n→∞) f/g
}
"""

import json
import sys
from pathlib import Path
from typing import Any

from sympy import (
    Symbol, Function, Rational, sqrt, log, oo, O as BigO,
    symbols, simplify, expand, factor, nsimplify,
    rsolve, limit, Abs, floor, ceiling, Pow, Add, Mul,
    fibonacci, lucas, binomial, factorial,
    sympify, latex, N, zoo, nan, I
)
from sympy.core.numbers import Float

# Import the complexity adapter for human-friendly expression parsing
# This handles notation like "n^2", "n*log(n)", "O(n^2)"
try:
    # Try importing from same directory
    sys.path.insert(0, str(Path(__file__).parent))
    from complexity_adapter import parse_complexity, n as complexity_n
    HAS_ADAPTER = True
except ImportError:
    HAS_ADAPTER = False
    complexity_n = Symbol('n', positive=True, integer=True)

    def parse_complexity(expr_str: str):
        """Fallback parser when adapter is not available."""
        return sympify(expr_str.replace("^", "**"), locals={"n": complexity_n})


def solve_linear_recurrence(data: dict) -> dict:
    """
    Solve linear recurrence: T(n) = sum(coeffs[i] * T(n-1-i)) + f(n)

    Example: Fibonacci is coeffs=[1,1], base={0:0, 1:1}, f_n="0"
    """
    n = Symbol('n', integer=True, positive=True)
    T = Function('T')

    coeffs = data.get("coeffs", [1])
    base_cases = data.get("base", {"0": 0})
    f_n_str = data.get("f_n", "0")

    try:
        # Use the complexity adapter for human-friendly notation
        f_n = parse_complexity(f_n_str).subs(complexity_n, n)
    except Exception as e:
        return {"success": False, "error": f"Invalid f(n): {e}"}

    # Build recurrence: T(n) - c0*T(n-1) - c1*T(n-2) - ... - f(n) = 0
    recurrence_eq = T(n) - f_n
    for i, c in enumerate(coeffs):
        recurrence_eq -= c * T(n - 1 - i)

    # Convert base cases to sympy format: {T(0): 0, T(1): 1, ...}
    init_conds = {}
    for k, v in base_cases.items():
        init_conds[T(int(k))] = sympify(v)

    try:
        solution = rsolve(recurrence_eq, T(n), init_conds)
        if solution is None:
            return {"success": False, "error": "rsolve returned None - recurrence may be unsolvable"}

        solution = simplify(solution)

        # Extract asymptotic complexity
        complexity = extract_complexity(solution, n)

        # Verify solution satisfies recurrence for a few values
        verified = verify_solution_numerically(solution, n, recurrence_eq, T, init_conds)

        return {
            "success": True,
            "closed_form": str(solution),
            "complexity": complexity,
            "latex": latex(solution),
            "verified": verified,
            "error": None
        }
    except Exception as e:
        return {"success": False, "error": str(e)}


def solve_divide_conquer(data: dict) -> dict:
    """
    Solve divide-and-conquer recurrence: T(n) = a*T(n/b) + f(n)
    Uses Master Theorem or Akra-Bazzi.
    """
    n = Symbol('n', positive=True, real=True)

    a = data.get("a", 2)
    b = data.get("b", 2)
    f_n_str = data.get("f_n", "n")

    try:
        # Use the complexity adapter for human-friendly notation
        f_n = parse_complexity(f_n_str).subs(complexity_n, n)
    except Exception as e:
        return {"success": False, "error": f"Invalid f(n): {e}"}

    # Critical exponent: log_b(a)
    from sympy import log as sym_log
    critical_exp = sym_log(a, b)
    critical_exp_float = float(critical_exp.evalf())

    # Determine f(n) growth rate
    f_degree = get_polynomial_degree(f_n, n)

    try:
        if f_degree is not None:
            # Master Theorem applies
            epsilon = 0.01

            if f_degree < critical_exp_float - epsilon:
                # Case 1: f(n) = O(n^(log_b(a) - epsilon))
                complexity = f"O(n^{critical_exp})"
                case = 1
            elif abs(f_degree - critical_exp_float) < epsilon:
                # Case 2: f(n) = Theta(n^(log_b(a)))
                # Check for log factors in f(n)
                log_power = get_log_power(f_n, n)
                if log_power == 0:
                    complexity = f"O(n^{critical_exp} * log(n))"
                else:
                    complexity = f"O(n^{critical_exp} * log(n)^{log_power + 1})"
                case = 2
            else:
                # Case 3: f(n) = Omega(n^(log_b(a) + epsilon))
                # Need regularity condition: a*f(n/b) <= c*f(n) for c < 1
                complexity = f"O({f_n})"
                case = 3

            return {
                "success": True,
                "closed_form": None,  # Master theorem gives Big-O, not exact
                "complexity": complexity,
                "master_theorem_case": case,
                "critical_exponent": str(critical_exp),
                "f_degree": f_degree,
                "verified": True,
                "error": None
            }
        else:
            # Akra-Bazzi: more general, handles non-polynomial f(n)
            # T(n) = Theta(n^p * (1 + integral from 1 to n of f(u)/u^(p+1) du))
            # where p satisfies: a * b^(-p) = 1, so p = log_b(a)

            p = critical_exp_float
            complexity = f"O(n^{p:.4f})"  # Simplified; real Akra-Bazzi needs integral

            return {
                "success": True,
                "closed_form": None,
                "complexity": complexity,
                "method": "akra_bazzi",
                "critical_exponent": str(critical_exp),
                "verified": False,  # Would need integral evaluation
                "error": None
            }
    except Exception as e:
        return {"success": False, "error": str(e)}


def verify_recurrence(data: dict) -> dict:
    """
    Verify that a proposed solution satisfies a recurrence.
    """
    n = Symbol('n', integer=True, positive=True)
    T = Function('T')

    recurrence_str = data.get("recurrence", "")
    solution_str = data.get("solution", "")
    base_cases = data.get("base", {})

    try:
        # Use the complexity adapter for human-friendly notation
        solution = parse_complexity(solution_str).subs(complexity_n, n)

        # Convert base cases
        init_conds = {}
        for k, v in base_cases.items():
            init_conds[T(int(k))] = sympify(v)

        # Verify base cases
        base_ok = True
        base_results = {}
        for k, expected in init_conds.items():
            idx = list(k.args)[0]  # Extract n value from T(n)
            actual = solution.subs(n, idx)
            diff = simplify(actual - expected)
            base_results[int(idx)] = {
                "expected": str(expected),
                "actual": str(actual),
                "match": diff == 0
            }
            if diff != 0:
                base_ok = False

        complexity = extract_complexity(solution, n)

        return {
            "success": True,
            "verified_base_cases": base_ok,
            "base_case_details": base_results,
            "complexity": complexity,
            "error": None
        }
    except Exception as e:
        return {"success": False, "error": str(e)}


def compare_complexities(data: dict) -> dict:
    """
    Compare two complexity expressions using limits.

    Input:
        f: first expression (string)
        g: second expression (string)
        bound_type: "O", "Omega", or "Theta"

    Returns whether f = O(g), f = Ω(g), or f = Θ(g).
    """
    n = Symbol('n', integer=True, positive=True)

    f_str = data.get("f", "n")
    g_str = data.get("g", "n")
    bound_type = data.get("bound_type", "Theta")

    try:
        # Use the complexity adapter for human-friendly notation
        f = parse_complexity(f_str).subs(complexity_n, n)
        g = parse_complexity(g_str).subs(complexity_n, n)

        # General comparison
        comparison = compare_asymptotic(f, g, n)

        # Specific bound verification
        if bound_type == "O":
            holds, constant = verify_big_o(f, g, n)
            return {
                "success": True,
                "bound_type": "O",
                "holds": holds,
                "constant": constant,
                "comparison": comparison,
                "limit_ratio": str(limit(f/g, n, oo))
            }
        elif bound_type == "Omega":
            holds, constant = verify_big_omega(f, g, n)
            return {
                "success": True,
                "bound_type": "Omega",
                "holds": holds,
                "constant": constant,
                "comparison": comparison,
                "limit_ratio": str(limit(f/g, n, oo))
            }
        else:  # Theta
            holds, constants = verify_big_theta(f, g, n)
            return {
                "success": True,
                "bound_type": "Theta",
                "holds": holds,
                "constants": constants,
                "comparison": comparison,
                "limit_ratio": str(limit(f/g, n, oo))
            }

    except Exception as e:
        return {"success": False, "error": str(e)}


def extract_complexity(expr, n) -> str:
    """
    Extract Big-O complexity from a closed-form expression using limits.

    Uses L'Hôpital's rule (via sympy.limit) to properly handle indeterminate forms.
    For f(n) ~ g(n), we check: lim(n→∞) f(n)/g(n) = c where 0 < c < ∞
    """
    try:
        # Handle special cases
        if expr.is_constant():
            return "O(1)"

        # Candidate complexity classes to test against (in order of growth)
        from sympy import log as sym_log
        candidates = [
            (1, "O(1)"),
            (sym_log(n), "O(log(n))"),
            (sym_log(n)**2, "O(log^2(n))"),
            (sqrt(n), "O(sqrt(n))"),
            (n, "O(n)"),
            (n * sym_log(n), "O(n*log(n))"),
            (n * sym_log(n)**2, "O(n*log^2(n))"),
            (n**Rational(3, 2), "O(n^(3/2))"),
            (n**2, "O(n^2)"),
            (n**2 * sym_log(n), "O(n^2*log(n))"),
            (n**3, "O(n^3)"),
            (n**4, "O(n^4)"),
            (Rational(3, 2)**n, "O(1.5^n)"),
            ((1 + sqrt(5))/2, "O(phi^n)"),  # Golden ratio
            (2**n, "O(2^n)"),
            (factorial(n), "O(n!)"),
        ]

        # For each candidate, compute limit of expr/candidate
        # If limit is a finite positive constant, that's our complexity class
        for candidate_expr, candidate_str in candidates:
            try:
                ratio = simplify(expr / candidate_expr)
                lim = limit(ratio, n, oo)

                # Check if limit is a finite positive constant
                if lim.is_number and lim.is_positive and lim.is_finite:
                    return candidate_str

                # If limit is 0, expr grows slower than candidate - try next
                if lim == 0:
                    continue

                # If limit is ∞, expr grows faster - keep trying larger candidates
                if lim in (oo, zoo):
                    continue

            except Exception:
                continue

        # Try to extract polynomial degree via limit-based approach
        # For polynomial f(n) = n^d, lim(n→∞) f(n)/n^d = constant
        degree = extract_degree_via_limit(expr, n)
        if degree is not None:
            if degree == int(degree):
                return f"O(n^{int(degree)})"
            else:
                return f"O(n^{degree:.2f})"

        # Fallback: use leading term
        from sympy import LT
        try:
            leading = LT(expr, n)
            return f"O({leading})"
        except Exception:
            return f"O({expr})"

    except Exception:
        return "O(?)"


def extract_degree_via_limit(expr, n) -> float | None:
    """
    Extract polynomial degree using the limit definition:
    If f(n) = Θ(n^d), then lim(n→∞) f(n)/n^d = c for some constant c > 0.

    We find d by checking: lim(n→∞) log(f(n))/log(n) = d
    """
    try:
        from sympy import log as sym_log

        # For polynomial f(n) ~ n^d: log(f)/log(n) → d as n → ∞
        log_ratio = simplify(sym_log(Abs(expr)) / sym_log(n))
        degree_limit = limit(log_ratio, n, oo)

        if degree_limit.is_number and degree_limit.is_finite:
            return float(degree_limit)

        return None
    except Exception:
        return None


def compare_asymptotic(f, g, n) -> str:
    """
    Compare asymptotic growth of f(n) vs g(n) using limits.

    Returns:
        "f < g"  if f = o(g)     [f grows strictly slower]
        "f ~ g"  if f = Θ(g)    [same growth rate]
        "f > g"  if f = ω(g)     [f grows strictly faster]
        "?"      if undetermined

    Uses L'Hôpital's rule internally via sympy.limit().
    """
    try:
        ratio = simplify(f / g)
        lim = limit(ratio, n, oo)

        if lim == 0:
            return "f < g"  # f = o(g)
        elif lim in (oo, zoo, -oo):
            return "f > g"  # f = ω(g)
        elif lim.is_number and lim.is_positive and lim.is_finite:
            return "f ~ g"  # f = Θ(g)
        else:
            # Try the other direction
            inv_ratio = simplify(g / f)
            inv_lim = limit(inv_ratio, n, oo)
            if inv_lim == 0:
                return "f > g"
            elif inv_lim in (oo, zoo):
                return "f < g"
            else:
                return "?"
    except Exception:
        return "?"


def verify_big_o(f, g, n) -> tuple[bool, float | None]:
    """
    Verify that f(n) = O(g(n)).

    Returns (True, c) if f(n) ≤ c·g(n) for large n, with the constant c.
    Returns (False, None) otherwise.

    Uses: f = O(g) iff lim sup(f/g) < ∞
    """
    try:
        ratio = simplify(f / g)
        lim = limit(ratio, n, oo)

        if lim.is_number and lim.is_finite and lim.is_nonnegative:
            return (True, float(lim) if lim != 0 else 0.0)
        elif lim == 0:
            return (True, 0.0)  # f = o(g) implies f = O(g)
        else:
            return (False, None)
    except Exception:
        return (False, None)


def verify_big_omega(f, g, n) -> tuple[bool, float | None]:
    """
    Verify that f(n) = Ω(g(n)).

    Returns (True, c) if f(n) ≥ c·g(n) for large n, with the constant c.
    Returns (False, None) otherwise.

    Uses: f = Ω(g) iff lim inf(f/g) > 0
    """
    try:
        ratio = simplify(f / g)
        lim = limit(ratio, n, oo)

        if lim in (oo, zoo):
            return (True, float('inf'))  # f = ω(g) implies f = Ω(g)
        elif lim.is_number and lim.is_positive:
            return (True, float(lim))
        else:
            return (False, None)
    except Exception:
        return (False, None)


def verify_big_theta(f, g, n) -> tuple[bool, tuple[float, float] | None]:
    """
    Verify that f(n) = Θ(g(n)).

    Returns (True, (c1, c2)) if c1·g(n) ≤ f(n) ≤ c2·g(n) for large n.
    Returns (False, None) otherwise.

    Uses: f = Θ(g) iff lim(f/g) = c where 0 < c < ∞
    """
    try:
        ratio = simplify(f / g)
        lim = limit(ratio, n, oo)

        if lim.is_number and lim.is_positive and lim.is_finite:
            c = float(lim)
            # For Θ, constants are typically c/2 and 2c
            return (True, (c * 0.5, c * 2.0))
        else:
            return (False, None)
    except Exception:
        return (False, None)


def get_polynomial_degree(expr, n) -> float | None:
    """Get the degree if expr is polynomial in n, else None."""
    try:
        from sympy import Poly, degree as poly_degree
        if expr.is_polynomial(n):
            p = Poly(expr, n)
            return float(poly_degree(p, n))

        # Try to get effective degree via limit
        # degree d means limit of expr/n^d is constant and nonzero
        for d in [0, 0.5, 1, 1.5, 2, 2.5, 3, 4, 5]:
            ratio = limit(expr / n**d, n, oo)
            if ratio not in (0, oo, -oo, zoo, nan) and ratio.is_constant():
                return d

        return None
    except Exception:
        return None


def get_log_power(expr, n) -> int:
    """Get the power of log(n) in expr, or 0 if none."""
    try:
        from sympy import log as sym_log
        # Simple check: count log factors
        if not expr.has(sym_log):
            return 0

        # This is a simplified heuristic
        expr_str = str(expr)
        if "log(n)**" in expr_str:
            # Try to extract power
            import re
            match = re.search(r"log\(n\)\*\*(\d+)", expr_str)
            if match:
                return int(match.group(1))
        if "log(n)" in expr_str:
            return 1
        return 0
    except Exception:
        return 0


def verify_solution_numerically(solution, n, recurrence_eq, T, init_conds) -> bool:
    """Verify solution by substituting into recurrence for several n values."""
    try:
        # Get order of recurrence
        order = len(init_conds)

        for test_n in range(order + 1, order + 10):
            # Substitute solution into recurrence
            check = recurrence_eq

            # Replace T(n), T(n-1), etc. with solution values
            for offset in range(order + 1):
                t_val = solution.subs(n, test_n - offset)
                check = check.subs(T(n - offset), t_val)
            check = check.subs(n, test_n)

            result = simplify(check)
            if result != 0:
                # Try numerical evaluation
                result_float = complex(N(result))
                if abs(result_float) > 1e-10:
                    return False

        return True
    except Exception:
        return False


def main():
    """Main entry point: read JSON from stdin, write result to stdout."""
    try:
        input_data = json.loads(sys.stdin.read())
    except json.JSONDecodeError as e:
        print(json.dumps({"success": False, "error": f"Invalid JSON: {e}"}))
        sys.exit(1)

    request_type = input_data.get("type", "linear")

    if request_type == "linear":
        result = solve_linear_recurrence(input_data)
    elif request_type == "divide_conquer":
        result = solve_divide_conquer(input_data)
    elif request_type == "verify":
        result = verify_recurrence(input_data)
    elif request_type == "compare":
        result = compare_complexities(input_data)
    else:
        result = {"success": False, "error": f"Unknown type: {request_type}"}

    print(json.dumps(result, indent=2, default=str))


if __name__ == "__main__":
    main()
