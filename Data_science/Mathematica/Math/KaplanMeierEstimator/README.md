# Kaplan-Meier Survival Estimator for Two-Group Studies

Kaplan-Meier survival curve estimation and significance testing for two patient cohorts, with batch evaluation of gene-expression split strategies.

## Overview
- Builds survival curves for two groups from censored patient records, merging the time events of both cohorts.
- Computes group significance from the expected number of failures, evaluated as a chi-square statistic with one degree of freedom.
- Batch-runs gene-expression split strategies (top-N percent by T0, T2 or T0-T2 difference) in parallel, returning ranked gene results with group size and FDR.
- Pluggable `ISplitStrategy` implementations define how patients are divided into the two cohorts for each gene.

## Key Types
- `Microsoft.VisualBasic.Math.KaplanMeierEstimator.KaplanMeierEstimate` — runs the estimator over two groups; exposes `GroupAEvents`, `GroupBEvents`, `TotalFailingA`, `TotalFailingB` and `PValue`.
- `Microsoft.VisualBasic.Math.KaplanMeierEstimator.KaplanMeierStatus` — one time point of a curve: `Time`, `NumberAtRisk`, `NumberFailing`, `SurvivalProbability`.
- `Microsoft.VisualBasic.Math.KaplanMeierEstimator.EventFreeSurvival` — censoring state of a patient: `Censored` or `Death`.
- `Microsoft.VisualBasic.Math.KaplanMeierEstimator.Models.Patient` — patient record with `Id`, `CensorEvent` and `CensorEventTime`.
- `Microsoft.VisualBasic.Math.KaplanMeierEstimator.Models.GeneExpression` — gene expression before and after a procedure for one patient.
- `Microsoft.VisualBasic.Math.KaplanMeierEstimator.Models.GeneResult` — per-gene result: `Estimate`, `GroupSize` and `FDR`.
- `Microsoft.VisualBasic.Math.KaplanMeierEstimator.SplitStrategies.ISplitStrategy` — strategy contract for splitting patients by expression.
- `Microsoft.VisualBasic.Math.KaplanMeierEstimator.StrategyRunner` — executes a strategy over gene groups in parallel and returns ordered results.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Math.KaplanMeierEstimator
Imports Microsoft.VisualBasic.Math.KaplanMeierEstimator.Models

Dim groupA As Patient() = {
    New Patient With {.Id = 1, .CensorEvent = EventFreeSurvival.Death, .CensorEventTime = 12},
    New Patient With {.Id = 2, .CensorEvent = EventFreeSurvival.Censored, .CensorEventTime = 30}
}
Dim groupB As Patient() = {
    New Patient With {.Id = 3, .CensorEvent = EventFreeSurvival.Death, .CensorEventTime = 20},
    New Patient With {.Id = 4, .CensorEvent = EventFreeSurvival.Death, .CensorEventTime = 8}
}

Dim km As New KaplanMeierEstimate(groupA, groupB)
km.RunEstimate()

For Each t As KaplanMeierStatus In km.GroupAEvents
    Console.WriteLine($"{t.Time}: S = {t.SurvivalProbability}")
Next

Console.WriteLine($"p = {km.PValue}")
```

## Package
- Assembly: `Microsoft.VisualBasic.Math.KaplanMeierEstimator`
- TargetFramework: `net10.0`
- Tags: `scibasic;survival-analysis;kaplan-meier;log-rank;gene-expression;biostatistics`

## License
GPL-3.0-or-later
