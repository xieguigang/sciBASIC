# Labelled Data Matrix, Correlation and Distance Utilities

Matrix utilities for pairwise comparison analysis: labelled numeric matrices, correlation and distance matrices, transforms and sparse matrix file IO.

## Overview
- `DataMatrix` model: a named NxN numeric matrix convertible to a graph under a cutoff, with row/column vector access and object visiting.
- Correlation and distance builders: Pearson / Spearman correlation matrices with p-value matrices, Euclidean distance, cosine similarity and general pairwise evaluation.
- Missing-value handling and transforms: imputation and missing-value simulation by feature or by sample, plus log, center, 0-1, standard and Z-score scaling over data frames.
- Matrix Market and Harwell-Boeing IO: MTX (with optional gzip compression) and RUA sparse matrix readers and writers.

## Key Types
- `Microsoft.VisualBasic.Math.Matrix.DataMatrix` — labelled numeric matrix; `GetVector`, `HasObject`, `Visit`, `ArrayPack`, `PopulateRows`.
- `Microsoft.VisualBasic.Math.Matrix.CorrelationMatrix` — correlation matrix joined with its p-value matrix (`pvalue(i, j)`, `GetPvalueMatrix`, `Power`, `PositiveMatrix`).
- `Microsoft.VisualBasic.Math.Matrix.DistanceMatrix` — dissimilarity / similarity matrix with quantile estimation and `CreateMatrix`.
- `Microsoft.VisualBasic.Math.Matrix.Builder` — `MatrixBuilder` extensions that build data / correlation / distance matrices from datasets or a `DataFrame`.
- `Microsoft.VisualBasic.Math.Matrix.Correlation` — `Pearson`, `Spearman` and correlation-matrix helpers over named datasets.
- `Microsoft.VisualBasic.Math.Matrix.Distance` — `Euclidean`, `Correlation` and `Similarity` matrix builders.
- `Microsoft.VisualBasic.Math.Matrix.Impute` — missing-value simulation and inference (`InferMethods`).
- `Microsoft.VisualBasic.Math.Matrix.MathFormula` — `Log`, `Center`, `Scale01`, `Standard`, `ZScale`, `Evaluate` and `GetDataFrame` over data frames.
- `Microsoft.VisualBasic.Math.Matrix.MatrixMarket.MTXFormat` / `RUAFormat` — Matrix Market MTX and Harwell-Boeing RUA file IO.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Math.Matrix
Imports Microsoft.VisualBasic.Math.Matrix.MatrixMarket

Dim mat As New DataMatrix({"a", "b", "c"}, {
    New Double() {1, 0.21, 0.55},
    New Double() {0.21, 1, 0.33},
    New Double() {0.55, 0.33, 1}
})

Console.WriteLine(String.Join(", ", mat.GetVector("a")))

Dim df As DataFrame = MathFormula.GetDataFrame(mat)
Dim z As DataFrame = MathFormula.ZScale(df)

' sparse matrix IO
Dim net As SparseMatrix = MTXFormat.ReadMatrix("network.mtx")
MTXFormat.WriteMatrix(net, "network-copy.mtx")
```

## Package
- Assembly: `Microsoft.VisualBasic.Math.Matrix`
- TargetFramework: `net10.0`
- Tags: `scibasic;data-matrix;correlation;distance-matrix;matrix-market;imputation`

## License
GPL-3.0-or-later
