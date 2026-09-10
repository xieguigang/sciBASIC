# Data_science/MachineLearning/MachineLearning.Data.Extensions/MachineLearning.Data.Extensions.vbproj

- RootNamespace : Microsoft.VisualBasic.MachineLearning.Data
- AssemblyName  : Microsoft.VisualBasic.MachineLearning.Data.Extensions
- TargetFramework: net10.0
- Source files  : 3
- Existing Title: Q-Learning and Labelled Dataset Serialization Extensions
- Existing Desc : Helpers for sciBASIC# machine learning data: export a trained Q-table with state features to netCDF, record Q-value curves per iteration to CSV, and save or load labelled clustering matrices in a compact binary format.
- Existing Tags : scibasic;machine-learning;q-learning;dataset;serialization

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.MachineLearning.Data)

## Public types
- Module Extensions (Extensions.vb)
- Module LabeledData (LabeledData.vb)
- Class QTableDump (QTableDump.vb)

## Notable public members
- Public Function GetInput(dataset As DataSet, data As row) As Double()
- Public Function ExportQTable(Q As IQTable, features As IQStateFeatureSet, file As Stream) As Boolean
- Public Function SaveLabelData(data As IEnumerable(Of EntityClusterModel), buffer As Stream) As Boolean
- Public Iterator Function LoadLabelData(buffer As Stream) As IEnumerable(Of EntityClusterModel)
- Public Sub Dump(table As IQTable, iteration As Integer)
- Public Sub Save(path As String)

## Imports
- Microsoft.VisualBasic.ComponentModel.Collection
- Microsoft.VisualBasic.ComponentModel.DataSourceModel
- Microsoft.VisualBasic.Data.Framework
- Microsoft.VisualBasic.Data.IO
- Microsoft.VisualBasic.DataMining.ComponentModel.EntityModels
- Microsoft.VisualBasic.DataStorage.netCDF
- Microsoft.VisualBasic.DataStorage.netCDF.Components
- Microsoft.VisualBasic.DataStorage.netCDF.Data
- Microsoft.VisualBasic.Linq
- Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
- Microsoft.VisualBasic.MachineLearning.QLearning
- Microsoft.VisualBasic.MachineLearning.QLearning.DataModel
- Microsoft.VisualBasic.Serialization.Bencoding
- Microsoft.VisualBasic.Text
- row = Microsoft.VisualBasic.Data.Framework.IO.DataSet
- System.IO
- System.Runtime.CompilerServices

## File tree
- Extensions.vb
- LabeledData.vb
- QTableDump.vb

