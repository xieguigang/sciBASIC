# Symbolic Math Engine: Calculus, Algebra and MathML Compiler

A computer-algebra layer for sciBASIC# that parses mathematical text into an immutable expression tree and transforms it functionally - derivatives, integrals, limits, Taylor expansions, polynomial algebra and Boolean minimisation.

## Overview
- Calculus on the expression tree: ordinary, partial, n-th order and implicit derivatives, Jacobians and Hessians, indefinite and definite integration, limits and Taylor expansion with Lagrange remainder.
- Algebraic rewriting: simplification, rationalisation of denominators, expansion, univariate polynomial factorisation, multiplication, division, GCD and substitution.
- Boolean algebra: truth tables, Quine-McCluskey minimisation and SOP/POS expression synthesis.
- MathML interop: compiles a MathML `LambdaExpression` into a LINQ `LambdaExpression` that can be compiled and invoked.

## Key Types
- `Microsoft.VisualBasic.Math.Symbolic.Symbolic` — unified facade: `Simplify`, `Derivative`, `Integrate`, `Limit`, `Taylor`, `Factor`, `Substitute`, `Rationalize`.
- `Microsoft.VisualBasic.Math.Symbolic.Derivative` — ordinary, partial, n-th order and implicit differentiation, Jacobian and Hessian.
- `Microsoft.VisualBasic.Math.Symbolic.Integration` — indefinite integration and numeric definite integrals.
- `Microsoft.VisualBasic.Math.Symbolic.Limit` — symbolic limits of an expression as a variable approaches a target.
- `Microsoft.VisualBasic.Math.Symbolic.Taylor` — Taylor expansion returning the polynomial part plus remainder.
- `Microsoft.VisualBasic.Math.Symbolic.Polynomial` — univariate polynomial model plus factorisation, division, remainder and GCD helpers.
- `Microsoft.VisualBasic.Math.Symbolic.BooleanAlgebra` — truth tables and Quine-McCluskey minimisation.
- `Microsoft.VisualBasic.Math.Lambda.MathMLCompiler` — converts a MathML lambda into a compilable LINQ lambda.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Math.Symbolic
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

Dim d As Expression = Symbolic.Derivative("x^3 + 2*x + 1", "x")
Dim s As Expression = Symbolic.Simplify("(x + 1)^2 - x^2")
Dim i As Expression = Symbolic.Integrate("2*x", "x")
Dim t As Expression = Symbolic.Taylor("sin(x)", "x", "0", 5)

Console.WriteLine(d.ToString())
```

## Package
- Assembly: `Microsoft.VisualBasic.Math.Lambda`
- TargetFramework: `net10.0`
- Tags: `scibasic;symbolic-math;computer-algebra;calculus;mathml;polynomial`

## License
GPL-3.0-or-later
