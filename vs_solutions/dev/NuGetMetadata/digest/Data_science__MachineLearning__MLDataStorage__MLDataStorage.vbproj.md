# Data_science/MachineLearning/MLDataStorage/MLDataStorage.vbproj

- RootNamespace : Microsoft.VisualBasic.MachineLearning.DataStorage
- AssemblyName  : Microsoft.VisualBasic.MachineLearning.DataStorage
- TargetFramework: net10.0
- Source files  : 4
- Existing Title: Machine Learning Dataset Storage: DataPack, MNIST and DataFrame Import
- Existing Desc : Dataset persistence layer for sciBASIC# machine learning: imports a data frame into a normalised sample dataset, reads MNIST IDX image and label files, and packs large sample collections into streamable HDSPack archives.
- Existing Tags : scibasic;machine-learning;dataset;mnist;data-storage

## Namespaces
- DataPack  [files: 2]

## Public types
- Module DataImports (DataImports.vb)
- Class PackReader (DataPack\PackReader.vb)
- Class PackWriter (DataPack\PackWriter.vb)
- Class MNIST (MNIST.vb)

## Notable public members
- Public ReadOnly Property output_labels As String()
- Public Function GetMatrix() As NormalizeMatrix
- Public Function ReadSample(id As String) As Sample
- Public Iterator Function GetAllSamples() As IEnumerable(Of Sample)
- Protected Overridable Sub Dispose(disposing As Boolean)
- Public Sub Dispose() Implements IDisposable.Dispose
- Public Sub WriteDataSet(ds As DataSet)
- Protected Overridable Sub Dispose(disposing As Boolean)
- Public Sub Dispose() Implements IDisposable.Dispose
- Public ReadOnly Property ImageSize As Size
- Public Sub Reset()
- Public Shared Function GetImageSize(imagesfile As String) As Size
- Public Iterator Function ExtractImages() As IEnumerable(Of NamedValue(Of Image))
- Public Iterator Function ExtractVectors() As IEnumerable(Of NamedCollection(Of Byte))
- Public Iterator Function ExtractDataSet(Of T As {INamedValue, IVector, IClusterPoint, New, Class})() As IEnumerable(Of T)
- Public Function ConvertImage(raw As NamedCollection(Of Byte)) As NamedValue(Of Image)
- Protected Overridable Sub Dispose(disposing As Boolean)
- Public Sub Dispose() Implements IDisposable.Dispose

## Imports
- Microsoft.VisualBasic.ComponentModel.Collection
- Microsoft.VisualBasic.ComponentModel.Collection.Generic
- Microsoft.VisualBasic.ComponentModel.DataSourceModel
- Microsoft.VisualBasic.Data.Framework
- Microsoft.VisualBasic.Data.IO
- Microsoft.VisualBasic.DataMining.ComponentModel
- Microsoft.VisualBasic.DataStorage.HDSPack
- Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem
- Microsoft.VisualBasic.Imaging
- Microsoft.VisualBasic.Imaging.BitmapImage
- Microsoft.VisualBasic.Linq
- Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
- Microsoft.VisualBasic.Math
- Microsoft.VisualBasic.Math.Distributions.Summary
- Microsoft.VisualBasic.Scripting.Runtime
- Microsoft.VisualBasic.Serialization.JSON
- Microsoft.VisualBasic.Text.Xml.Models
- std = System.Math
- System.Drawing
- System.IO
- System.Runtime.CompilerServices

## File tree
- DataImports.vb
- DataPack\PackReader.vb
- DataPack\PackWriter.vb
- MNIST.vb

