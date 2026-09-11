# Curve Fitting and Regression Modelling Library

A collection of least-squares, local-regression, regularised and non-linear curve fitting algorithms for sciBASIC#.

## Overview
- Linear and polynomial least squares, weighted linear regression and non-negative least squares (NNLS).
- Multiple linear regression with feature scaling (`CurveScale`), normal-equation solving and coefficient confidence intervals.
- Non-linear and regularised fitting: Levenberg-Marquardt (`LMA`, `LmSolver`), Gauss-Newton, LASSO coordinate descent and Bayesian curve fitting.
- Local regression (LOESS / LOWESS) and logistic regression with gradient descent, plus fit-quality evaluation (SSR, SSE, RMSE, R-square).

## Key Types
- `Microsoft.VisualBasic.Data.Bootstrapping.LeastSquares` — `LinearFit` and `PolyFit` entry points returning a `FitResult`.
- `Microsoft.VisualBasic.Data.Bootstrapping.Linear.FitResult` — fitted model with `Slope`, `Intercept`, `R_square`, `AdjustR_square`, `SSR`, `SSE`, `RMSE`, `Residuals` and `GetY(x)`.
- `Microsoft.VisualBasic.Data.Bootstrapping.Multivariate.LinearFittingAlgorithm` — multiple linear regression (`LinearFitting`), `CurveScale` and `ConfidenceInterval`.
- `Microsoft.VisualBasic.Data.Bootstrapping.Multivariate.MLRFit` — multi-dimensional linear regression model with coefficients and error estimates.
- `Microsoft.VisualBasic.Data.Bootstrapping.Linear.WeightedLinearRegression` / `WeightedFit` — weighted linear regression and its result.
- `Microsoft.VisualBasic.Data.Bootstrapping.Linear.NonNegativeLeastSquares` — NNLS solver.
- `Microsoft.VisualBasic.Data.Bootstrapping.LASSO.LassoFitGenerator` / `LassoFit` — regularisation paths via coordinate descent.
- `Microsoft.VisualBasic.Data.Bootstrapping.LMA` and `LevenbergMarquardt.LmSolver` — Levenberg-Marquardt non-linear least squares.
- `Microsoft.VisualBasic.Data.Bootstrapping.LOESS.LOESS` / `LOESSModel` — local regression fitting and prediction.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Data.Bootstrapping

Dim x As Double() = {1, 2, 3, 4, 5}
Dim y As Double() = {2.1, 3.9, 6.2, 7.8, 10.4}

Dim fit As FitResult = LeastSquares.LinearFit(x, y)

Console.WriteLine($"y = {fit.Slope} * x + {fit.Intercept}, R2 = {fit.R_square}")

Dim poly As FitResult = LeastSquares.PolyFit(x, y, poly_n:=2)
Console.WriteLine(poly.GetY(6))
```

## Package
- Assembly: `Microsoft.VisualBasic.Data.Bootstrapping.Fittings`
- TargetFramework: `net10.0`
- Tags: `scibasic;curve-fitting;regression;lasso;loess;levenberg-marquardt`

## License
GPL-3.0-or-later
