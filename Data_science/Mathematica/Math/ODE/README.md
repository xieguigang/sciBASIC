# Ordinary Differential Equation Solver Core for Dynamic Models

Integrates ordinary differential equations in sciBASIC#, from single scalar equations with a derivative function pointer up to declarative multi-variable dynamic models integrated with Runge-Kutta solvers.

## Overview
- Scalar solvers over an `ODE` model: Euler, RK2, RK4 and Gill (fourth-order Runge-Kutta-Gill).
- Declarative system models: subclass `ODEs` or use `GenericODEs` to declare named variables, initial values and parameters, then integrate over `[a, b]` with `n` steps.
- RK4 system integration via `RungeKutta4`, plus a `SolverIterator` for step-by-step ticking and bound trigger callbacks.
- Result carriers `ODEOutput` (single equation) and `ODEsOut` (system) with merge/join, stream extensions and NaN detection.

## Key Types
- `Microsoft.VisualBasic.Math.Calculus.ODE` — single scalar equation model: `ID`, `df` function pointer and initial value `y0`.
- `Microsoft.VisualBasic.Math.Calculus.ODESolver` — Euler (`Eluer`), `RK2`, `RK4` and `Gill` solvers plus output allocation.
- `Microsoft.VisualBasic.Math.Calculus.ODEOutput` — X/Y result vectors of a single-equation integration.
- `Microsoft.VisualBasic.Math.Calculus.Dynamics.ODEs` — abstract declarative system model with `Solve(n, a, b)`.
- `Microsoft.VisualBasic.Math.Calculus.Dynamics.GenericODEs` — concrete system model driven by a `df(dx, ByRef dy As Vector)` delegate.
- `Microsoft.VisualBasic.Math.Calculus.Dynamics.RungeKutta4` — RK4 integrator for a system of equations.
- `Microsoft.VisualBasic.Math.Calculus.Dynamics.var` — a named Y variable of the ODE system.
- `Microsoft.VisualBasic.Math.Calculus.Dynamics.Data.ODEsOut` — named-collection output of a system integration.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Math.Calculus

Dim eq As New ODE With {
    .ID = "dy/dx = x + y",
    .y0 = 1.0,
    .df = Function(x, y) x + y
}

' integrate y' = x + y over [0, 1] with 1000 steps
Dim out As ODEOutput = eq.RK4(1000, 0, 1)

Console.WriteLine(out.Y.Vector.Last())
```

## Package
- Assembly: `Microsoft.VisualBasic.Math.ODEsSolver`
- TargetFramework: `net10.0`
- Tags: `scibasic;ode;runge-kutta;dynamical-system;numerical-integration;simulation`

## License
GPL-3.0-or-later
