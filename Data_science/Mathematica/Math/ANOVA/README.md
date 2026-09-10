# ANOVA and Multivariate Analysis Toolkit for VB.NET

Group-comparison statistics and omics-style multivariate analysis for the sciBASIC# framework.

## Overview
- One-way ANOVA F-test (`AnovaFTest`) with between/within degrees of freedom, F statistic and p-value.
- Kruskal-Wallis non-parametric testing over abundance matrices, with tie correction, result formatting and Bonferroni / Benjamini-Hochberg p-value correction.
- Multivariate analysis for omics matrices: PCA, PLS / PLS-DA (VIP, regression coefficients, cross-validation) and OPLS / OPLS-DA.
- Input datasets are built from `DataFrame`, named vector rows or `NamedCollection(Of Double)` via `DataSetHelper`, with scaling and transform options.

## Key Types
- `Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA.AnovaFTest` — one-way ANOVA over grouped `Double()` vectors; exposes `DfBetween`, `DfWithin`, `FStatistic`, `PValue`.
- `Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA.KruskalWallisModule` — Kruskal-Wallis H test for a single feature row or a whole matrix, plus tie correction and multiple-testing correction.
- `Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA.KWResult` — per-feature test result (statistic, p-value, corrected p-value).
- `Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA.StatisticsObject` — scaled / transformed numeric dataset (X, Y, labels) consumed by the multivariate routines.
- `Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA.DataSetHelper` — builds `StatisticsObject` from data frames, `INamedValue`+`IVector` rows or named collections.
- `Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA.PCA` — principal component analysis returning a `MultivariateAnalysisResult`.
- `Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA.PLS` — partial least squares with VIP, coefficient and cross-validation helpers.
- `Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA.OPLS` — orthogonal projections to latent structures (OPLS / OPLS-DA).

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA

' one-way ANOVA over three groups
Dim groups As Double()() = {
    New Double() {5.1, 4.9, 5.3},
    New Double() {6.2, 6.4, 6.1},
    New Double() {7.3, 7.1, 7.5}
}
Dim anova As New AnovaFTest(groups)

Console.WriteLine($"F = {anova.FStatistic}, p = {anova.PValue}")

' non-parametric alternative: rows = features, columns = samples
Dim results As KWResult() = KruskalWallisModule.KruskalWallisMatrixTest(matrix, groupLabels)
Dim qval As Double() = KruskalWallisModule.BenjaminiHochbergCorrection(results)

' multivariate: PCA over a labelled data set
Dim stat As StatisticsObject = DataSetHelper.CommonDataSet(df, labels)
Dim pca As MultivariateAnalysisResult = PCA.PrincipalComponentAnalysis(stat, maxPC:=2)
```

## Package
- Assembly: `Microsoft.VisualBasic.Math.Statistics.ANOVA`
- TargetFramework: `net10.0`
- Tags: `scibasic;anova;kruskal-wallis;multivariate-analysis;pca;pls`

## License
GPL-3.0-or-later
