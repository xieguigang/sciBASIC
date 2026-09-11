# Symbolic Regression via Genetic Programming and GA

Evolves mathematical expressions and polynomial models from data points using genetic programming and genetic algorithms.

## Overview
- Genetic programming over expression trees (`GPTree`) with point and subtree mutation plus subtree crossover; tree depth, population and tournament settings are configurable.
- Genetic algorithm over polynomial individuals (`GAPolynomial`) with random / Gaussian point mutation and simple, arithmetical or simulated binary crossover.
- Pluggable error objectives: mean square error, mean absolute error, sum square error and sum absolute error.
- Random expression generation from a factory of unary, binary and terminal nodes (arithmetic operators, trigonometric, exponential, logarithmic, square root and constants).

## Key Types
- `Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.evolution.Evolution` — runs `evolveTreeFor` (GP) and `evolvePolyFor` (GA) and returns an `EvolutionResult`.
- `Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.evolution.Configuration` / `GPConfiguration` / `GAConfiguration` — evolution parameters with `createDefaultConfig()`.
- `Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.evolution.GPTree` / `GAPolynomial` — individuals implementing `Individual`, with fitness evaluation and comparison.
- `Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.evolution.GPTreeUtils` / `GAPolynomialUtils` — mutation and crossover operators plus tree traversal.
- `Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.evolution.measure.Objective` / `ObjectiveFunction` — error objectives: MSE, MAE, SSE, SAE.
- `Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.model.Expression` / `UnaryExpression` / `BinaryExpression` — expression tree nodes with `eval` and `toStringExpression`.
- `Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.model.factory.ExpressionFactory` — generates random expressions and polynomial individuals.
- `Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.evolution.EvolutionResult` — best expression, fitness, epochs and fitness/time progress.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Math.Scripting
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.evolution
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.model.factory

Dim data As DataPoint() = {
    New DataPoint(1, 3),
    New DataPoint(2, 5),
    New DataPoint(3, 7)
}

Dim gp As New Evolution()
gp.ExpressionFactory = New ExpressionFactory()

Dim best As EvolutionResult = gp.evolveTreeFor(data, GPConfiguration.createDefaultConfig())

Console.WriteLine(best.result.toStringExpression())
Console.WriteLine($"fitness = {best.fitness}")
```

## Package
- Assembly: `Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming`
- TargetFramework: `net10.0`
- Tags: `scibasic;genetic-programming;symbolic-regression;expression-tree;evolutionary-algorithm`

## License
GPL-3.0-or-later
