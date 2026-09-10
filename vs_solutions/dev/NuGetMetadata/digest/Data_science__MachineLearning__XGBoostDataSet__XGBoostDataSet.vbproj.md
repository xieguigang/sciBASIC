# Data_science/MachineLearning/XGBoostDataSet/XGBoostDataSet.vbproj

- RootNamespace : Microsoft.VisualBasic.MachineLearning.XGBoost.DataSet
- AssemblyName  : Microsoft.VisualBasic.MachineLearning.XGBoost.DataSet
- TargetFramework: net10.0
- Source files  : 2
- Existing Title: CSV Tabular Data Loader for XGBoost Training Sets
- Existing Desc : Training data preparation for sciBASIC# XGBoost: two-pass CSV readers for test and validation tables, and converters building labelled TrainData with categorical columns and missing value indexes.
- Existing Tags : scibasic;xgboost;dataset;csv;data-preprocessing

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.MachineLearning.XGBoost.DataSet)

## Public types
- Module Conversion (Conversion.vb)
- Module Tabular (Tabular.vb)

## Notable public members
- Public Function ToTrainingSet(matrix As DoubleTagged(Of Single())(), columns As String(), categorical_features As IEnumerable(Of String)) As TrainData
- Public Function ToValidateSet(matrix As DoubleTagged(Of Single())()) As ValidationData
- Public Function ToTestDataSet(matrix As Single()()) As TestData
- Public Function ReadTestData(file As String) As TestData
- Public Function ReadValidationData(file As String) As ValidationData
- Public Function ReadTrainData(file As String, categorical_features As IEnumerable(Of String)) As TrainData

## Imports
- Microsoft.VisualBasic.ComponentModel.Collection
- Microsoft.VisualBasic.ComponentModel.TagData
- Microsoft.VisualBasic.Language
- Microsoft.VisualBasic.Language.Java
- Microsoft.VisualBasic.Language.Values
- Microsoft.VisualBasic.MachineLearning.XGBoost.train
- System.IO
- System.Runtime.CompilerServices

## File tree
- Conversion.vb
- Tabular.vb

