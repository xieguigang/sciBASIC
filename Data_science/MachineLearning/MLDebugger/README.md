# Machine Learning Debugger: Error Curves, ROC and Sample Frame Exports

Diagnostics helpers for sciBASIC# neural network training: they build demo sample sets, flatten XML datasets into tabular rows, extract error and neuron value frames from training netCDF logs, and compute ROC validation curves.

## Overview
- `SampleSetCreator` wraps a collection of `Sample` objects into a `DataSet` with explicit input/output names, typically used to generate demo or unit-test sample sets.
- `ToTable` converts an XML `DataSet` into one tabular row per sample, optionally marking output columns as `[name]` so outputs never collide with input names.
- `ExportErrorCurve` / `GetTimeIndex` / `ExportValueFrames` read a training `netCDF` log: the fitness-vs-iteration error curve, the `T{i}` time index, and one value frame per variable tagged `type = "neuron"`.
- `NormalizeSample` exports a normalised copy of the samples using any `Methods` normalisation, and `ROC` sweeps a threshold range over `Validate` results to build a `Validation()` curve.
- ROC points whose specificity or sensitivity is not a number are filtered out, so the curve is safe to plot directly.

## Key Types
- `Microsoft.VisualBasic.MachineLearning.Debugger.DataSetExtensions` — sample-set construction and XML dataset to table conversion.
- `Microsoft.VisualBasic.MachineLearning.Debugger.ANN.FrameExports` — netCDF training log to error curve and neuron value frames.
- `Microsoft.VisualBasic.MachineLearning.Debugger.ANN.ROC` — extension computing a ROC `Validation()` curve from `Validate` results.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.DataStorage.netCDF
Imports Microsoft.VisualBasic.MachineLearning.Debugger

' 1. build a demo dataset and flatten it to rows
Dim ds As DataSet = samples.SampleSetCreator(inputNames, outputNames)
Dim rows = ds.ToTable(markOutput:=True)

' 2. read the training log
Using cdf As New netCDFReader("training_log.nc")
    Dim timeline As String() = FrameExports.GetTimeIndex(cdf)
    Dim errors = FrameExports.ExportErrorCurve(cdf)

    For Each neuron In FrameExports.ExportValueFrames(cdf)
        ' neuron.ID = variable name, properties keyed by T{i}
    Next
End Using

' 3. ROC curve over 20 thresholds
Dim curve As Validation() = result.ROC(New DoubleRange(0, 1), attribute:=0, n:=20)
```

## Package
- Assembly: `Microsoft.VisualBasic.MachineLearning.Debugger`
- TargetFramework: `net10.0`
- Tags: `scibasic;machine-learning;debugger;roc;error-curve`

## License
GPL-3.0-or-later
