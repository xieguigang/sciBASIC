### Simplex Method implementation for linear optimization

###### Linear Programming

Implementation of the [Simplex Method](https://en.wikipedia.org/wiki/Simplex_algorithm) (simplex algorithm) used to solve linear programming optimization problems.

> github: https://github.com/commandlinegirl/LinearProgramming

###### Code Usage

```vbnet
Dim input = LP.InputReader.readInput("../../../data/LP/input.txt")
Dim converted = LP.InputReader.convertDoublesMatrix(input)
Dim t As New LP.Tableau(converted)
Dim solver As New LP.LinearSolver(LP.OptimizationType.MAX)
Dim result = solver.solve(t)
Dim expected#() = {708.0, 48.0, 84.0, 0.0, 0.0, 0.0, 60.0, 0.0}

Call expected _
    .SequenceEqual(result.Solution) _
    .__DEBUG_ECHO
```