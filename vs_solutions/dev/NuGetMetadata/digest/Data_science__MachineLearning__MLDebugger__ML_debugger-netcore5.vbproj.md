# Data_science/MachineLearning/MLDebugger/ML_debugger-netcore5.vbproj

- RootNamespace : Microsoft.VisualBasic.MachineLearning.Debugger
- AssemblyName  : Microsoft.VisualBasic.MachineLearning.Debugger
- TargetFramework: net10.0
- Source files  : 3
- Existing Title: Machine Learning Debugger: Error Curves, ROC and Sample Frame Exports
- Existing Desc : Diagnostics helpers for sciBASIC# neural network training: builds demo sample sets, converts XML datasets to tabular rows, extracts fitness and neuron value frames from training netCDF logs, and computes ROC validation curves.
- Existing Tags : scibasic;machine-learning;debugger;roc;error-curve

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.MachineLearning.Debugger)

## Public types
- Module FrameExports (ANN\FrameExports.vb)
- Module ROC (ANN\ROC.vb)
- Module DataSetExtensions (DataSetExtensions.vb)

## Notable public members
- Public Function NormalizeSample(samples As DataSet, method As Methods) As Excel()
- Public Function ExportErrorCurve(cdf As netCDFReader) As DataFrameResolver
- Public Function GetTimeIndex(cdf As netCDFReader) As String()
- Public Iterator Function ExportValueFrames(cdf As netCDFReader) As IEnumerable(Of Excel)
- Public Function ROC(result As IEnumerable(Of Validate), range As DoubleRange, attribute%, Optional n% = 20) As Validation()
- Public Function SampleSetCreator(samples As IEnumerable(Of Sample),
- Public Iterator Function ToTable(raw As DataSet, Optional markOutput As Boolean = False) As IEnumerable(Of Table)

## Imports
- Basic = Microsoft.VisualBasic.Language.Runtime
- DataSet = Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure.DataSet
- Excel = Microsoft.VisualBasic.Data.Framework.IO.DataSet
- Microsoft.VisualBasic.ComponentModel.Ranges.Model
- Microsoft.VisualBasic.Data.Framework.StorageProvider
- Microsoft.VisualBasic.DataMining.ComponentModel.Normalizer
- Microsoft.VisualBasic.DataMining.Evaluation
- Microsoft.VisualBasic.DataStorage.netCDF
- Microsoft.VisualBasic.DataStorage.netCDF.Components
- Microsoft.VisualBasic.DataStorage.netCDF.DataVector
- Microsoft.VisualBasic.Linq
- Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
- Microsoft.VisualBasic.MachineLearning.NeuralNetwork
- Microsoft.VisualBasic.Text.Xml.Models
- System.Runtime.CompilerServices
- Table = Microsoft.VisualBasic.Data.Framework.IO.DataSet

## File tree
- ANN\FrameExports.vb
- ANN\ROC.vb
- DataSetExtensions.vb

