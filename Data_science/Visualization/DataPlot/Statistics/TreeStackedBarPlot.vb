#Region "Microsoft.VisualBasic::171ec2a69db16d0315d13b3dd1d2c7aa, Data_science\Visualization\DataPlot\Statistics\TreeStackedBarPlot.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 162
    '    Code Lines: 119 (73.46%)
    ' Comment Lines: 16 (9.88%)
    '    - Xml Docs: 56.25%
    ' 
    '   Blank Lines: 27 (16.67%)
    '     File Size: 7.09 KB


    ' Class TreeStackedBarPlot
    ' 
    '     Properties: Categories, SeriesNames, ShowTree, ShowValues, StackValues
    '                 Tree, TreeFraction
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: OrderByTree, SumSeries
    ' 
    '     Sub: DrawCategoryAxis, DrawSeriesLegend, Plot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports std = System.Math

' ============================================================================
'  TreeStackedBarPlot.vb - 聚类树 + 堆叠柱状图联动
'
'  旧实现的 HistStackedBarplot 左侧用 hctree/HCTreePlot 画聚类树，右侧画堆叠柱；
'  这里左边仍然是一棵树，但树数据是调用方给的 <see cref="ClusterTreeNode"/>，
'  右侧的堆叠柱复用统一的绘制管线，两者按分类顺序对齐。
' ============================================================================

''' <summary>聚类树 + 堆叠柱状图</summary>
Public Class TreeStackedBarPlot
    Inherits PlotEngine

    ''' <summary>分类名（同时也是堆叠柱的横轴）</summary>
    Public Property Categories As String() = {}
    ''' <summary>堆叠的系列名</summary>
    Public Property SeriesNames As String() = {}
    ''' <summary>[系列, 分类] 数值矩阵</summary>
    Public Property StackValues As Double(,) = Nothing
    ''' <summary>分类之间的聚类树（可选）</summary>
    Public Property Tree As ClusterTreeNode = Nothing
    ''' <summary>是否绘制左侧树</summary>
    Public Property ShowTree As Boolean = True
    ''' <summary>树占用的画布宽度比例</summary>
    Public Property TreeFraction As Double = 0.22
    ''' <summary>是否在每段上写出数值</summary>
    Public Property ShowValues As Boolean = False

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing, Optional driver As Microsoft.VisualBasic.Imaging.Driver.Drivers = Microsoft.VisualBasic.Imaging.Driver.Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    ''' <summary>直接在外部提供的绘图设备上绘制（图层叠加模式 / 宿主画布）。</summary>
    Public Sub New(g As IGraphics, Optional theme As PlotTheme = Nothing)
        MyBase.New(g, theme)
    End Sub

    Public Sub Plot()
        If StackValues Is Nothing OrElse Categories Is Nothing OrElse Categories.Length = 0 Then
            Throw New InvalidOperationException("TreeStackedBarPlot requires categories and stack values.")
        End If

        Dim nSer = StackValues.GetLength(0)
        Dim nCat = System.Math.Min(Categories.Length, StackValues.GetLength(1))

        DrawBackground()
        ComputePlotArea()
        DrawTitle()

        Dim order = Enumerable.Range(0, nCat).ToArray()
        If ShowTree AndAlso Tree IsNot Nothing Then order = OrderByTree(Tree.Leaves())

        Dim totals = Enumerable.Range(0, nCat).Select(Function(ci) SumSeries(ci)).ToArray()
        Dim vmax = If(totals.Length > 0, totals.Max(), 1)
        If vmax <= 0 Then vmax = 1
        vmax *= 1.08

        Dim treeW = If(ShowTree AndAlso Tree IsNot Nothing, CSng(_plotArea.Width * TreeFraction), 0)
        Dim barRegion = New RectangleF(_plotArea.Left + treeW, _plotArea.Top,
                                       std.Max(40.0F, _plotArea.Width - treeW), _plotArea.Height - 60.0F)

        If ShowTree AndAlso Tree IsNot Nothing Then
            Using pen As New Pen(Theme.AxisColor, Theme.AxisLineWidth)
                ClusterTree.Draw(_g, Tree, New RectangleF(_plotArea.Left, barRegion.Top, treeW, barRegion.Height),
                                 ClusterTree.TreeOrientation.LeftToRight, Theme, pen)
            End Using
        End If

        Dim groupW = barRegion.Width / nCat
        Dim barW = groupW * (1 - Theme.BarPadding * 2)

        For ci = 0 To nCat - 1
            Dim catIdx = order(ci)
            Dim cx As Single = CSng(barRegion.Left + (ci + 0.5) * groupW)
            Dim cursor As Single = ToPixelY(0, 0, vmax)

            For si = 0 To nSer - 1
                Dim val = StackValues(si, catIdx)
                Dim color = Theme.Palette(si Mod Theme.Palette.Length)
                Dim top = ToPixelY(val, 0, vmax)
                Dim h = cursor - top

                Using br As New SolidBrush(color),
                      pen As New Pen(Theme.BorderColor, 0.4F)
                    _g.FillRectangle(br, cx - barW / 2, top, barW, h)
                    _g.DrawRectangle(pen, cx - barW / 2, top, barW, h)
                End Using

                If ShowValues AndAlso h > 14 Then
                    Dim text = Geometry.NumberLabel(val)
                    Dim size = MeasureString(text, Theme.TickLabelFont)
                    Using br As New SolidBrush(ColorScale.ReadableTextColor(color))
                        _g.DrawString(text, Theme.TickLabelFont, br, cx - size.Width / 2, top + h / 2 - size.Height / 2)
                    End Using
                End If

                cursor = top
            Next
        Next

        DrawCategoryAxis(order, nCat, barRegion, groupW)
        DrawSeriesLegend(nSer)
    End Sub

    Private Function SumSeries(catIdx As Integer) As Double
        Dim sum As Double = 0
        For si = 0 To StackValues.GetLength(0) - 1
            sum += StackValues(si, catIdx)
        Next
        Return sum
    End Function

    Private Function OrderByTree(leafOrder As String()) As Integer()
        Dim index As New Dictionary(Of String, Integer)()
        For i = 0 To Categories.Length - 1
            index(Categories(i)) = i
        Next

        Dim list As New List(Of Integer)()
        For Each leaf In leafOrder
            If index.ContainsKey(leaf) AndAlso Not list.Contains(index(leaf)) Then list.Add(index(leaf))
        Next
        For i = 0 To Categories.Length - 1
            If Not list.Contains(i) Then list.Add(i)
        Next

        Return list.Take(StackValues.GetLength(1)).ToArray()
    End Function

    Private Sub DrawCategoryAxis(order As Integer(), nCat As Integer, region As RectangleF, groupW As Single)
        Using pen As New Pen(Theme.AxisColor, Theme.AxisLineWidth)
            _g.DrawLine(pen, region.Left, region.Bottom, region.Right, region.Bottom)
        End Using

        Using br As New SolidBrush(Theme.TextColor)
            For ci = 0 To nCat - 1
                Dim text = Categories(order(ci))
                Dim size = MeasureString(text, Theme.TickLabelFont)

                _g.TranslateTransform(region.Left + (ci + 0.5) * groupW, region.Bottom + 6)
                _g.RotateTransform(-45)
                _g.DrawString(text, Theme.TickLabelFont, br, -size.Width, 0)
                _g.ResetTransform()
            Next
        End Using
    End Sub

    Private Sub DrawSeriesLegend(nSer As Integer)
        Dim legends As New List(Of Series)()
        For si = 0 To nSer - 1
            legends.Add(New Series With {
                .Name = If(SeriesNames IsNot Nothing AndAlso si < SeriesNames.Length, SeriesNames(si), "S" & (si + 1)),
                .Color = Theme.Palette(si Mod Theme.Palette.Length),
                .MarkerShape = MarkerShape.Square
            })
        Next
        DrawLegend(legends)
    End Sub
End Class
