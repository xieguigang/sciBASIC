# Core Mathematics Library: Linear Algebra, Numerics and More

Foundational numeric and algebraic toolkit for sciBASIC#, providing the dense and sparse linear algebra, optimisation, statistics and expression-scripting primitives that the rest of the framework builds on.

## Overview
- Dense and sparse matrix algebra with LU, QR, Cholesky, eigen, NMF, full SVD and sparse (truncated) SVD decompositions, plus MDS/SMACOF multidimensional scaling.
- Linear programming: simplex and two-phase simplex solvers, an interior-point solver with simplex crossover, and CSR/CSC sparse normal-equation factors for genome-scale problems.
- Numerics: arbitrary-precision `BigDecimal`, IEEE half precision, L-BFGS-B bound-constrained optimisation and generic numeric vectors.
- Data-oriented helpers: spline and curve interpolation, quantiles, ECDF/bin-box distributions, sample summaries, bootstrapping, time-series downsampling, MinHash/LSH, fuzzy-logic inference and a math expression scripting engine.

## Key Types
- `Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.NumericMatrix` — dense numeric matrix with row/column access and matrix-maths extensions.
- `Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.SingularValueDecomposition` — SVD of a general matrix exposing `U`, `S`, `V`, rank and condition number.
- `Microsoft.VisualBasic.Math.LinearAlgebra.Vector` — double-precision numeric vector used across the algebra and statistics code.
- `Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming.LPP` — simplex-based solver for continuous linear programming problems.
- `Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming.IPMCrossover.LppSolver` — interior-point solver with sparse normal equations and simplex crossover.
- `Microsoft.VisualBasic.Math.Numerics.BigDecimal` — arbitrary-precision decimal arithmetic.
- `Microsoft.VisualBasic.Math.Interpolation.CubicSpline` — cubic spline interpolation over a point series.
- `Microsoft.VisualBasic.Math.Distributions.Summary.SampleDistribution` — summary statistics (mean, variance, quartiles, histogram) of a data sample.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

' singular value decomposition: a -> U, w -> singular values, v -> right vectors
Dim a As Double(,) = {{1.0, 2.0}, {3.0, 4.0}, {5.0, 6.0}}
Dim w As Double()
Dim v As Double(,)

Call SVD.SVDecomposition(a, w, v)

Console.WriteLine(w(0))

' dense matrix built from jagged rows
Dim m As New NumericMatrix({New Double() {1, 2, 3}, New Double() {4, 5, 6}})
```

## Package
- Assembly: `Microsoft.VisualBasic.Math.Core`
- TargetFramework: `net10.0`
- Tags: `scibasic;mathematics;linear-algebra;numerics;linear-programming;spline`

## License
GPL-3.0-or-later
