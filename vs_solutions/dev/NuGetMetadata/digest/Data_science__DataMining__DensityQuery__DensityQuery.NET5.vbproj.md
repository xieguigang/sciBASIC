# Data_science/DataMining/DensityQuery/DensityQuery.NET5.vbproj

- RootNamespace : Microsoft.VisualBasic.DataMining.DensityQuery
- AssemblyName  : Microsoft.VisualBasic.DataMining.DensityQuery
- TargetFramework: net10.0
- Source files  : 5
- Existing Title: Spatial Density Query and SLIC Superpixel Segmentation
- Existing Desc : Estimates local point density on 2D grids and KD-tree indexes, and segments images into compact superpixels with the SLIC algorithm. Useful for outlier detection and image region grouping in the sciBASIC# framework.
- Existing Tags : scibasic;density;superpixel;slic;kd-tree

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.DataMining.DensityQuery)

## Public types
- Module Density2D (Density2D.vb)
- Class GridBox (GridBox.vb)
- Class KDQuery (KDQuery.vb)
- Class Metric (Metric.vb)
- Class SLIC (SLIC.vb) - SLIC (Simple Linear Iterative Clustering) clusters pixels using pixel channels and image plane space to efficiently generate compact, nearly uniform superpixels. The simplicity of approach makes it extremely easy To use a lone parameter specifies the number Of superpixels And the efficiency Of
- Class SLICPixel (SLIC.vb)

## Notable public members
- Public Function WindowSize(Of T)(grid As Grid(Of T), w As Integer, h As Integer) As GridBox(Of T)
- Public Function Density(Of T As INamedValue)(data As IEnumerable(Of T),
- Public Iterator Function Density(Of T)(data As IEnumerable(Of T),
- Public Iterator Function Gridding() As IEnumerable(Of T())
- Public Function QueryDensity(row As ClusterEntity, k As Integer) As NamedValue(Of Double) Implements IQueryDensity(Of ClusterEntity).QueryDensity
- Public Function Raw() As IEnumerable(Of ClusterEntity) Implements IQueryDensity(Of ClusterEntity).Raw
- Public Overrides Sub setByDimension(x As ClusterEntity, dimName As String, value As Double)
- Public Overrides Function GetDimensions() As String()
- Public Overrides Function metric(a As ClusterEntity, b As ClusterEntity) As Double
- Public Overrides Function getByDimension(x As ClusterEntity, dimName As String) As Double
- Public Overrides Function nodeIs(a As ClusterEntity, b As ClusterEntity) As Boolean
- Public Overrides Function activate() As ClusterEntity
- Public Shared Iterator Function ReadImagePixels(bitmap As BitmapBuffer) As IEnumerable(Of SLICPixel)
- Public Iterator Function InitializeCenters(pixels As SLICPixel(), regionSize As Integer) As IEnumerable(Of SLICPixel)
- Public Function MeasureSegments(regionSize As Integer, numIterations As Integer) As SLICPixel()
- Public Sub IterateClustering(pixels As SLICPixel(), centers As List(Of SLICPixel), regionSize As Integer)
- Public Property x As Integer
- Public Property y As Integer
- Public Property color As Double()
- Public Property cluster As Integer
- Public Function DistanceTo(p2 As SLICPixel, regionSize As Integer) As Single

## Imports
- Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.Tqdm
- Microsoft.VisualBasic.ComponentModel.Collection.Generic
- Microsoft.VisualBasic.ComponentModel.DataSourceModel
- Microsoft.VisualBasic.Data.GraphTheory.GridGraph
- Microsoft.VisualBasic.Data.GraphTheory.KdTree
- Microsoft.VisualBasic.DataMining.Clustering
- Microsoft.VisualBasic.DataMining.ComponentModel.EntityModels
- Microsoft.VisualBasic.DataMining.KMeans
- Microsoft.VisualBasic.Imaging.BitmapImage
- Microsoft.VisualBasic.Linq
- Microsoft.VisualBasic.Math
- Microsoft.VisualBasic.Math.Correlations
- std = System.Math
- System.Drawing
- System.Runtime.CompilerServices

## File tree
- Density2D.vb
- GridBox.vb
- KDQuery.vb
- Metric.vb
- SLIC.vb

