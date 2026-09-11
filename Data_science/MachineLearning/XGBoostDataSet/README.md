# CSV Tabular Data Loader for XGBoost Training Sets

Training data preparation for sciBASIC# XGBoost: two-pass CSV readers for test and validation tables, and converters building labelled `TrainData` with categorical columns and missing value indexes.

## Overview
- `Tabular` reads CSV tables in two passes: the first scan collects dimensions (feature count, row count, per-column missing counts) and locates categorical columns, the second scan fills labels, feature values and missing-value indexes.
- `ReadTrainData` takes the list of categorical feature names and produces a `TrainData` carrying `origin_feature`, `label`, `missing_index`, `feature_value_index` and `cat_features_cols`; `ReadValidationData` reuses the last CSV column as the label; `ReadTestData` loads features only.
- Empty CSV cells become the per-type `NA` sentinel, matching the missing-value handling of the ternary boosting trainer.
- `Conversion` builds the same objects from in-memory matrices: `ToTrainingSet` (labelled `DoubleTagged(Of Single())` rows plus column names), `ToValidateSet` and `ToTestDataSet`.

## Key Types
- `Microsoft.VisualBasic.MachineLearning.XGBoost.DataSet.Tabular` — two-pass CSV readers for train, validation and test tables.
- `Microsoft.VisualBasic.MachineLearning.XGBoost.DataSet.Conversion` — in-memory matrix to `TrainData` / `ValidationData` / `TestData` converters.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.DataSet
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.train

Dim categorical As String() = {"color", "region"}

Dim train As TrainData = Tabular.ReadTrainData("train.csv", categorical)
Dim validate As ValidationData = Tabular.ReadValidationData("validate.csv")
Dim test As TestData = Tabular.ReadTestData("test.csv")

' or convert matrices that are already in memory
Dim train2 As TrainData = rows.ToTrainingSet(columnNames, categorical)
```

## Package
- Assembly: `Microsoft.VisualBasic.MachineLearning.XGBoost.DataSet`
- TargetFramework: `net10.0`
- Tags: `scibasic;xgboost;dataset;csv;data-preprocessing`

## License
GPL-3.0-or-later
