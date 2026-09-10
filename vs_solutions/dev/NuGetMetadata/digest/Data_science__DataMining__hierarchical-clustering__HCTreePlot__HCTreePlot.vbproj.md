# Data_science/DataMining/hierarchical-clustering/HCTreePlot/HCTreePlot.vbproj

- RootNamespace : Microsoft.VisualBasic.DataMining.HierarchicalClustering
- AssemblyName  : Microsoft.VisualBasic.DataMining.HierarchicalClustering.Plot
- TargetFramework: net10.0
- Source files  : 8
- Existing Title: Dendrogram and Radial Tree Plotting for Cluster Hierarchies
- Existing Desc : Renders hierarchical clustering results as publication-ready graphics: vertical dendrograms, circular and radial layouts, and horizon or right-to-left panels, with class colour legends, leaf labels and rulers.
- Existing Tags : scibasic;dendrogram;visualization;hierarchical-clustering;plot

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.DataMining.HierarchicalClustering)

## Public types
- Class Circular (Circular.vb)
- Module Dendrogram (Dendrogram.vb)
- Class DendrogramPanelV2 (DendrogramPanelV2.vb) - 绘制层次聚类图(竖直方向)
- Class Horizon (Horizon.vb)
- Class HorizonRightToLeft (HorizonRightToLeft.vb)
- Enum Layouts (Layouts.vb) - 层次聚类树的绘制布局枚举
- Class RadialDendrogram (RadialDendrogram.vb) - 绘制环状（辐射状）层次聚类树

## Notable public members
- Public Sub New(hist As Cluster, theme As Theme,
- Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
- Public Function Plot(hist As Cluster,
- Public ReadOnly Property classinfo As Dictionary(Of String, String)
- Protected Sub New(hist As Cluster, theme As Theme,
- Protected Function GetColor(id As String) As Color
- Public Sub New(hist As Cluster, theme As Theme,
- Public Function Paint(g As IGraphics, layout As Rectangle) As IEnumerable(Of NamedValue(Of PointF))
- Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
- Protected Overridable Overloads Sub DendrogramPlot(partition As Cluster,
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
- Public Sub New(hist As Cluster, theme As Theme,
- Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)

## Imports
- Chart = Microsoft.VisualBasic.Data.ChartPlots.Graphic.Plot
- Microsoft.VisualBasic.ComponentModel.DataSourceModel
- Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
- Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
- Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
- Microsoft.VisualBasic.Imaging
- Microsoft.VisualBasic.Imaging.d3js.scale
- Microsoft.VisualBasic.Imaging.Drawing2D
- Microsoft.VisualBasic.Imaging.Drawing2D.Colors
- Microsoft.VisualBasic.Imaging.Driver
- Microsoft.VisualBasic.Imaging.Math2D
- Microsoft.VisualBasic.Language
- Microsoft.VisualBasic.Linq
- Microsoft.VisualBasic.MIME.Html.CSS
- Microsoft.VisualBasic.MIME.Html.Render
- std = System.Math
- System.Drawing
- System.Runtime.CompilerServices

## File tree
- Circular.vb
- Dendrogram.vb
- DendrogramPanel.vb
- DendrogramPanelV2.vb
- Horizon.vb
- HorizonRightToLeft.vb
- Layouts.vb
- RadialDendrogram.vb

