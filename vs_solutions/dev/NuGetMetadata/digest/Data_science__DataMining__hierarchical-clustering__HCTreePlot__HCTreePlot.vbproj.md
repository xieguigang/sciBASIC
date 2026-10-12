# Data_science/DataMining/hierarchical-clustering/HCTreePlot/HCTreePlot.vbproj

- RootNamespace : Microsoft.VisualBasic.DataMining.HierarchicalClustering
- AssemblyName  : Microsoft.VisualBasic.DataMining.HierarchicalClustering.Plot
- TargetFramework: net10.0
- Source files  : 15
- Existing Title: Dendrogram and Radial Tree Plotting for Cluster Hierarchies
- Existing Desc : Renders hierarchical clustering results as publication-ready graphics: vertical dendrograms, circular and radial layouts, and horizon or right-to-left panels, with class colour legends, leaf labels and rulers.
- Existing Tags : scibasic;dendrogram;radial-tree;visualization;hierarchical-clustering;plot;tree-layout

## Namespaces
- Global.Microsoft.VisualBasic.Data.Plots.Canvas  [files: 1]
- Global.Microsoft.VisualBasic.Data.Plots.Statistics.Heatmap  [files: 5]

## Public types
- Class Circular (Circular.vb)
- Module DataScaler (DataScaler.vb)
- Module Dendrogram (Dendrogram.vb)
- Class DendrogramPanelV2 (DendrogramPanelV2.vb) - 绘制层次聚类图(竖直方向)
- Module HeatMap (HeatMapCompat.vb) - A heat map (or heatmap) is a graphical representation of data where the individual values contained in a matrix are represented as colors. The term 'heat map' was originally coined and trademarked by software designer Cormac Kinney in 1991, to describe a 2D display
- Class HeatMapPlot (HeatMapPlotCompat.vb)
- Class Horizon (Horizon.vb)
- Class HorizonRightToLeft (HorizonRightToLeft.vb)
- Module Internal (Internal.vb) - heatmap plot internal
- Enum Layouts (Layouts.vb) - 层次聚类树的绘制布局枚举
- Class NamespaceDoc (NamespaceDoc.vb) - Hierarchical clustering tree plotting: circular and radial dendrograms, the dendrogram panels and the tree layout models.
- Module PlotExtensions (PlotExtensions.vb)
- Class RadialDendrogram (RadialDendrogram.vb) - 绘制环状（辐射状）层次聚类树

## Notable public members
- Public Sub New(hist As Cluster, theme As Theme,
- Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
- Public Function DoDataScale(data As IEnumerable(Of DataSet), scaleMethod As DrawElements, levels#) As Dictionary(Of String, DataSet)
- Public Function ScaleByRow(data As IEnumerable(Of DataSet), levels#) As IEnumerable(Of DataSet)
- Public Function ScaleByCol(data As IEnumerable(Of DataSet), levels%) As IEnumerable(Of DataSet)
- Public Function ScaleByALL(data As IEnumerable(Of DataSet), levels%) As IEnumerable(Of DataSet)
- Public Function Plot(hist As Cluster,
- Public ReadOnly Property classinfo As Dictionary(Of String, String)
- Protected Sub New(hist As Cluster, theme As Theme,
- Protected Function GetColor(id As String) As Color
- Public Sub New(hist As Cluster, theme As Theme,
- Public Function Paint(g As IGraphics, layout As Rectangle) As IEnumerable(Of NamedValue(Of PointF))
- Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
- Protected Overridable Overloads Sub DendrogramPlot(partition As Cluster,
- Public Function Plot(data As IEnumerable(Of DataSet),
- Public Property mapLevels As Integer
- Public Property colors As Color()
- Public Property reverseColors As Boolean = False
- Protected Sub New(theme As Theme)
- Protected Function GetBrushes() As SolidBrush()
- Protected Function GetColors() As Color()
- Public Shared Iterator Function Z(scatter As IEnumerable(Of PixelData)) As IEnumerable(Of PixelData)
- Public Property scaleMethod As DrawElements = DrawElements.None
- Public Property drawLabels As DrawElements = DrawElements.Both
- Public Property drawDendrograms As DrawElements = DrawElements.Rows
- Public Property drawClass As (rowClass As Dictionary(Of String, String), colClass As Dictionary(Of String, String))
- Public Property globalRange As DoubleRange
- Public Property LegendLayout As Layouts = Layouts.Horizon
- Public Property legendSize As New Size(600, 100)
- Public Sub New(matrix As IEnumerable(Of DataSet), rowLabelsMaxChars As Integer, dlayout As SizeF, theme As Theme)
- Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
- Public Sub RenderHeatmap(g As IGraphics, region As GraphicsRegion, args As PlotArguments)
- Public Sub New(hist As Cluster, theme As Theme,
- Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
- Protected Overrides Sub DendrogramPlot(partition As Cluster,
- Public Property showLeafLabels As Boolean = False
- Public Property linkColor As Pen
- Public Property labelFont As Font
- Public Property labelPadding As Single = 5
- Public Property pointSize As Single = 5
- Public Property log_scale As Boolean = True
- Public Property log_base As Double = 10
- Public Property GetColor As Func(Of String, Color)
- Public Sub DendrogramPlot(hist As Cluster, g As IGraphics, plotRegion As Rectangle)
- Public Function Log(v As Vector, base#) As Vector
- Friend Sub DrawClass(g As IGraphics, orders$(), colors As Dictionary(Of String, String), layout As Rectangle, rowClass As Boolean, widthOrHeight%, int…
- Public Function DataScaleLevels(array As DataSet(), keys$(), logScale#, scaleMethod As DrawElements, levels%)
- Friend Function doPlot(plot As HowtoDoPlot,
- Public Function LoadDataSet(path As String,
- Public Function KmeansReorder(data As NamedValue(Of Dictionary(Of String, Double))(), Optional n% = 5) As NamedValue(Of Dictionary(Of String, Double))…
- Public Sub New(hist As Cluster, theme As Theme,
- Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)

## Imports
- Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
- Bitmap = System.Drawing.Bitmap
- Brush = Microsoft.VisualBasic.Imaging.Brush
- Brush = System.Drawing.Brush
- Brushes = Microsoft.VisualBasic.Imaging.Brushes
- Brushes = System.Drawing.Brushes
- Chart = Microsoft.VisualBasic.Data.Plots.Canvas.Plot
- DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
- DashStyle = System.Drawing.Drawing2D.DashStyle
- Font = Microsoft.VisualBasic.Imaging.Font
- Font = System.Drawing.Font
- FontStyle = Microsoft.VisualBasic.Imaging.FontStyle
- FontStyle = System.Drawing.FontStyle
- GraphicsPath = Microsoft.VisualBasic.Imaging.GraphicsPath
- GraphicsPath = System.Drawing.Drawing2D.GraphicsPath
- Image = Microsoft.VisualBasic.Imaging.Image
- Image = System.Drawing.Image
- Microsoft.VisualBasic.ApplicationServices
- Microsoft.VisualBasic.ComponentModel.Collection
- Microsoft.VisualBasic.ComponentModel.DataSourceModel
- Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
- Microsoft.VisualBasic.ComponentModel.Ranges.Model
- Microsoft.VisualBasic.Data.Framework.IO
- Microsoft.VisualBasic.Data.Plots
- Microsoft.VisualBasic.Data.Plots.Canvas

## File tree
- Circular.vb
- DataScaler.vb
- Dendrogram.vb
- DendrogramPanel.vb
- DendrogramPanelV2.vb
- HeatMapCompat.vb
- HeatMapPlotBase.vb
- HeatMapPlotCompat.vb
- Horizon.vb
- HorizonRightToLeft.vb
- Internal.vb
- Layouts.vb
- NamespaceDoc.vb
- PlotExtensions.vb
- RadialDendrogram.vb

