# XGBoost Model Predictor and Gradient Boosting Tree Trainer

Gradient boosting toolkit for sciBASIC#: reads and scores XGBoost model files through GBTree, GBLinear and Dart boosters, and trains ternary gradient boosting trees with categorical and missing value handling.

## Overview
- `Predictor` loads a trained XGBoost model from a stream (classic `binf` format, the standard format, and xgboost4j-spark classification/regression headers) and scores feature vectors through `predict`, `predictSingle` and `predictLeaf`.
- Booster implementations `GBTree`, `GBLinear` and `Dart` are resolved by `GradBooster_Factory`; objective functions (`RegLossObjLogistic`, `SoftmaxMultiClassObjClassify`, `SoftmaxMultiClassObjProb`) are resolved by `ObjFunction.fromName` and overridden by `PredictorConfiguration`.
- The TGBoost trainer (`train.GBM`) builds level-wise ternary trees using SLIQ-style attribute and class lists, handling missing values by enumerating left / right / missing children and ordering categorical features by their gradient statistics.
- Training supports early stopping on a validation set, row/column subsampling, L2 (`lambda`) and `gamma` regularisation, weighted loss, parallel `num_thread`, and prediction from `TestData` or raw `Single()()`.

## Key Types
- `Microsoft.VisualBasic.MachineLearning.XGBoost.Predictor` — loads an XGBoost model and generates predictions from an `FVec`.
- `Microsoft.VisualBasic.MachineLearning.XGBoost.util.FVecTransformer` — builds `FVec` feature vectors from dense arrays or sparse maps.
- `Microsoft.VisualBasic.MachineLearning.XGBoost.gbm.GBTree` / `GBLinear` / `Dart` — gradient booster implementations behind `GradBooster`.
- `Microsoft.VisualBasic.MachineLearning.XGBoost.train.GBM` — TGBoost gradient boosting model with `fit` / `predict`.
- `Microsoft.VisualBasic.MachineLearning.XGBoost.train.TrainData` / `ValidationData` / `TestData` — training, validation and test matrices consumed by the trainer.
- `Microsoft.VisualBasic.MachineLearning.XGBoost.util.ModelReader` — binary reader for the XGBoost model format.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.MachineLearning.XGBoost
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.train
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.util

' score with a trained XGBoost model
Using fs As Stream = New FileStream("xgboost.model", FileMode.Open)
    Dim model As New Predictor(fs)
    Dim vec As FVec = FVecTransformer.fromArray(
        values:=New Single() {1.0F, 2.5F, Single.NaN},
        treatsZeroAsNA:=False)

    Dim score As Double() = model.predict(vec)
End Using

' train a ternary gradient boosting tree model
Dim booster As New GBM()
Call booster.fit(trainData, validateData,
                 eval_metric:=Metrics.auc,
                 loss:="logloss",
                 num_boost_round:=20)
Dim preds As Double() = booster.predict(testData)
```

## Package
- Assembly: `Microsoft.VisualBasic.MachineLearning.XGBoost`
- TargetFramework: `net10.0`
- Tags: `scibasic;xgboost;gradient-boosting;decision-tree;predictor`

## License
GPL-3.0-or-later
