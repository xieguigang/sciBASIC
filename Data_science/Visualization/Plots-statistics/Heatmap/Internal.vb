#Region "Microsoft.VisualBasic::8e09b6b99a641edc1fa4708616c4b949, Data_science\Visualization\Plots-statistics\Heatmap\Internal.vb"

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

    '   Total Lines: 393
    '    Code Lines: 280 (71.25%)
    ' Comment Lines: 59 (15.01%)
    '    - Xml Docs: 54.24%
    ' 
    '   Blank Lines: 54 (13.74%)
    '     File Size: 18.74 KB


    '     Module Internal
    ' 
    '         Function: DataScaleLevels, doPlot, Log
    ' 
    '         Sub: DrawClass
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Scripting
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports Microsoft.VisualBasic.Serialization.JSON
Imports std = System.Math

Namespace Heatmap

    ''' <summary>
    ''' heatmap plot internal
    ''' </summary>
    Module Internal

        ' 假若只有一个数据分组，那么在进行聚类树的构建的时候就会出错
        ' 对于只有一个数据分组的时候，假若是采用的rowscale的方式，那么所有的数值所对应的颜色都是一样的，因为每一行都只有一个数，且该数值为该行的最大值，即自己除以自己总是为1的，所以所有行的样色都会一样

        ''' <summary>
        ''' 因为只是想要缩小距离，并不是真正的数学上的log计算
        ''' 故而，0的log值为0
        ''' 负数的log值为绝对值的log乘上-1
        ''' </summary>
        ''' <param name="v"></param>
        ''' <param name="base#"></param>
        ''' <returns></returns>
        <Extension> Public Function Log(v As Vector, base#) As Vector
            Return v _
                .Select(Function(x)
                            If x = 0R Then
                                Return 0
                            Else
                                Return std.Sign(x) * std.Log(x, base)
                            End If
                        End Function) _
                .AsVector
        End Function

        ''' <summary>
        ''' 如果没有绘制层次聚类树，但是仍然需要绘制出class的颜色条的话，则可以使用这个方法来完成绘制操作
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="orders$"></param>
        ''' <param name="colors"></param>
        ''' <param name="layout">这个是热图矩阵的绘制区域，但是这个函数会使用这个值来计算出class的绘制区域</param>
        ''' <param name="rowClass"></param>
        ''' <param name="widthOrHeight">``row -> width/col -> height``</param>
        <Extension>
        Private Sub DrawClass(g As IGraphics, orders$(), colors As Dictionary(Of String, String), layout As Rectangle, rowClass As Boolean, widthOrHeight%, interval%)
            Dim color As SolidBrush

            If rowClass Then
                ' 绘制行标签的class
                Dim width% = widthOrHeight / 3
                Dim height = layout.Height
                Dim step! = height / orders.Length
                Dim top = layout.Top
                Dim left = layout.Left - width - interval

                For Each rowName$ In orders
                    color = colors(rowName).GetBrush
                    layout = New Rectangle With {
                        .X = left,
                        .Y = top,
                        .Width = width,
                        .Height = height
                    }

                    g.FillRectangle(color, layout)
                    top += [step]
                Next
            Else
                ' 绘制列标签的class
                Dim width% = layout.Width
                Dim height% = widthOrHeight / 3
                Dim step! = width / orders.Length
                Dim top = layout.Top - height - interval
                Dim left = layout.Left

                For Each colName$ In orders
                    color = colors(colName).GetBrush
                    layout = New Rectangle With {
                        .X = left,
                        .Y = top,
                        .Width = [step],
                        .Height = height
                    }

                    g.FillRectangle(color, layout)
                    left += [step]
                Next
            End If
        End Sub

        <Extension>
        Public Function DataScaleLevels(array As DataSet(), keys$(), logScale#, scaleMethod As DrawElements, levels%)
            Dim scaleData As DataSet()

            If logScale > 0 Then
                Dim names As New NamedVectorFactory(keys)

                scaleData = array _
                    .Select(Function(x)
                                Dim vector As Vector = names.AsVector(x.Properties)
                                vector = Vector.Log(vector, logScale)

                                Return New DataSet With {
                                    .ID = x.ID,
                                    .Properties = names.Translate(vector)
                                }
                            End Function) _
                    .ToArray
            Else
                scaleData = array
            End If

            Return scaleData.DoDataScale(scaleMethod, levels - 1)
        End Function

        ''' <summary>
        ''' 一些共同的绘图元素过程
        ''' </summary>
        ''' <param name="drawLabels">是否绘制下面的标签，对于下三角形的热图而言，是不需要绘制下面的标签的，则设置这个参数为False</param>
        ''' <param name="legendSize">这个对象定义了图示的大小</param>
        ''' <param name="rowLabelfont">对行标签或者列标签的字体的定义</param>
        ''' <param name="array">Name为行名称，字典之中的key为列名称</param>
        ''' <param name="scaleMethod">
        ''' + 如果是<see cref="DrawElements.Cols"/>表示按列赋值颜色
        ''' + 如果是<see cref="DrawElements.Rows"/>表示按行赋值颜色
        ''' + 如果是<see cref="DrawElements.None"/>或者<see cref="DrawElements.Both"/>则是表示按照整体数据
        ''' </param>
        <Extension>
        Friend Function doPlot(plot As HowtoDoPlot,
                               array As DataSet(),
                               rowLabelfont As Font, colLabelFont As Font,
                               logScale#,
                               scaleMethod As DrawElements,
                               drawLabels As DrawElements,
                               drawDendrograms As DrawElements,
                               drawClass As (rowClass As Dictionary(Of String, String), colClass As Dictionary(Of String, String)),
                               dendrogramLayout As (A%, B%),
                               reverseClrSeq As Boolean,
                               Optional colors As SolidBrush() = Nothing,
                               Optional mapLevels% = 100,
                               Optional mapName$ = ColorMap.PatternJet,
                               Optional size As Size = Nothing,
                               Optional padding As Padding = Nothing,
                               Optional bg$ = "white",
                               Optional legendTitle$ = "Heatmap Color Legend",
                               Optional legendFont As Font = Nothing,
                               Optional legendLabelFont As Font = Nothing,
                               Optional min# = -1,
                               Optional max# = 1,
                               Optional mainTitle$ = "heatmap",
                               Optional titleFont As Font = Nothing,
                               Optional legendWidth! = -1,
                               Optional legendHasUnmapped As Boolean = True,
                               Optional legendSize As Size = Nothing,
                               Optional rowXOffset% = 0,
                               Optional tick# = -1,
                               Optional legendLayout As Layouts = Layouts.Horizon) As GraphicsData

            Dim keys$() = array.PropertyNames
            Dim angle! = -45

            If colors.IsNullOrEmpty Then
                colors = Designer.GetColors(mapName, mapLevels).GetBrushes
                If reverseClrSeq Then
                    colors = colors.Reverse.ToArray
                End If
            End If

            Dim rowKeys$() ' 经过聚类之后得到的新的排序顺序
            Dim colKeys$()

            Dim configDendrogramCanvas =
                Function(cluster As Cluster, [class] As Dictionary(Of String, String))
                    Return New DendrogramPanelV2(cluster, New Theme)
                End Function
            Dim DATArange As DoubleRange = array _
                .Select(Function(x) x.Properties.Values) _
                .IteratesALL _
                .Join(min, max) _
                .Distinct _
                .ToArray
            Dim ticks#()

            If tick > 0 Then
                ticks = AxisScalling.GetAxisByTick(DATArange, tick)
            Else
                ticks = DATArange.CreateAxisTicks(ticks:=5)
            End If

            Call $"{DATArange.ToString} -> {ticks.GetJson}".__INFO_ECHO

            Dim plotInternal =
                Sub(ByRef g As IGraphics, rect As GraphicsRegion)

                    ' 根据布局计算出矩阵的大小和位置
                    Dim left! = padding.Left + rowXOffset, top! = padding.Top    ' 绘图区域的左上角位置
                    ' 计算出右边的行标签的最大的占用宽度
                    Dim maxRowLabelSize As SizeF = g.MeasureString(array.Keys.MaxLengthString, rowLabelfont)
                    Dim maxColLabelSize As SizeF = g.MeasureString(keys.MaxLengthString, colLabelFont)
                    Dim llayout As New Rectangle With {
                        .Location = New Point(left, top),
                        .Size = legendSize
                    }
                    Dim css As CSSEnvirnment = g.LoadEnvironment

                    If legendLayout = Layouts.Horizon Then
                        ' legend位于整个图片的左上角
                        Call Legends.ColorLegendHorizontal(colors, ticks, g, llayout, scientificNotation:=True)
                    Else
                        ' legend位于整个图片的右上角
                        Call Legends.ColorMapLegend(g, llayout, colors, ticks,
                                                    css.GetFont(CSSFont.TryParse(CSSFont.Win7LargerNormal)),
                                                    legendTitle,
                                                    css.GetFont(CSSFont.TryParse(CSSFont.Win7Normal)),
                                                    Stroke.TryParse(Stroke.StrongHighlightStroke))
                    End If

                    ' 宽度与最大行标签宽度相减得到矩阵的绘制宽度
                    Dim dw = rect.PlotRegion.Width - maxRowLabelSize.Width
                    Dim dh = rect.PlotRegion.Height - maxColLabelSize.Width - legendSize.Height

                    top += legendSize.Height + 20

                    ' 1. 首先要确定layout
                    ' 因为行和列的聚类树需要相互依赖对方来确定各自的绘图区域
                    ' 所以在这里需要分为两步来完成绘制
                    Dim layoutA, layoutB As Integer

                    ' 有行的聚类树
                    If drawDendrograms.HasFlag(DrawElements.Rows) Then
                        ' A
                        left += dendrogramLayout.A
                        dw = dw - dendrogramLayout.A
                        layoutA = dendrogramLayout.A
                    Else
                        layoutA = 0
                    End If
                    If Not drawClass.rowClass.IsNullOrEmpty Then
                        Dim d = dendrogramLayout.A / 3

                        layoutA += d
                        left += d
                        dw -= d
                    End If

                    ' 有列的聚类树
                    If drawDendrograms.HasFlag(DrawElements.Cols) Then
                        ' B
                        top += dendrogramLayout.B
                        dh = dh - dendrogramLayout.B
                        layoutB = dendrogramLayout.B
                    Else
                        layoutB = 0
                    End If
                    If Not drawClass.colClass.IsNullOrEmpty Then
                        Dim d = dendrogramLayout.B / 3

                        layoutB += d
                        top += d
                        dh -= d
                    End If

                    Dim interval% = 10  ' 层次聚类树与热图矩阵之间的距离

                    left += interval
                    top += interval

                    Dim matrixPlotRegion As New Rectangle With {
                        .Location = New Point(left, top),
                        .Size = New Size With {
                            .Width = dw - interval,
                            .Height = dh - interval
                        }
                    }

                    ' 2. 然后才能够进行绘图
                    If drawDendrograms.HasFlag(DrawElements.Rows) Then

                        ' Try
                        ' 绘制出聚类树
                        Dim cluster As Cluster = Time(AddressOf array.RunCluster)
                        Dim topleft As New Point With {
                                .X = rect.Padding.Left,
                                .Y = top
                            }
                        Dim dsize As New Size With {
                                .Width = dendrogramLayout.A,
                                .Height = matrixPlotRegion.Height
                            }
                        rowKeys = configDendrogramCanvas(cluster, drawClass.rowClass) _
                                .Paint(DirectCast(g, Graphics2D), New Rectangle(topleft, dsize)) _
                                .OrderBy(Function(x) x.Value.Y) _
                                .Keys
                        'Catch ex As Exception
                        '    ex.PrintException
                        '    rowKeys = array.Keys
                        'End Try

                    Else
                        rowKeys = array.Keys

                        If Not drawClass.rowClass.IsNullOrEmpty Then
                            ' 没有绘制层次聚类树，但是行的class有值，则会绘制行的class legend
                            Call g.DrawClass(rowKeys, drawClass.rowClass, matrixPlotRegion, True, dendrogramLayout.A, interval)
                        End If
                    End If

                    If drawDendrograms.HasFlag(DrawElements.Cols) Then
                        Dim cluster As Cluster = Time(AddressOf array.Transpose.RunCluster)

                        colKeys = configDendrogramCanvas(cluster, drawClass.colClass) _
                            .Paint(DirectCast(g, Graphics2D), New Rectangle(300, 100, 500, 500)) _
                            .OrderBy(Function(x) x.Value.X) _
                            .Keys
                    Else
                        colKeys = array.PropertyNames

                        If Not drawClass.colClass.IsNullOrEmpty Then
                            ' 没有绘制层次聚类树，但是列的class有值，则会绘制列的class legend
                            Call g.DrawClass(colKeys, drawClass.colClass, matrixPlotRegion, False, dendrogramLayout.B, interval)
                        End If
                    End If



                    Dim args As New PlotArguments With {
                        .colors = colors,
                        .left = left,
                        .levels = array.DataScaleLevels(keys, logScale, scaleMethod, colors.Length),
                        .top = top,
                        .ColOrders = colKeys,
                        .RowOrders = rowKeys,
                        .matrixPlotRegion = matrixPlotRegion
                    }

                    ' 绘制heatmap之中的矩阵内容
                    Call plot(g, rect, args)

                    dw = args.dStep.Width
                    left = args.left
                    top = args.top
                    left += dw / 2   ' x坐标已经向方格的中间移动了，后面就不需要额外的移动操作了

                    ' 绘制下方的矩阵的列标签
                    If drawLabels = DrawElements.Both OrElse drawLabels = DrawElements.Cols Then
                        Dim text As New GraphicsText(DirectCast(g, Graphics2D).Graphics)
                        Dim format As New StringFormat() With {
                            .FormatFlags = StringFormatFlags.MeasureTrailingSpaces
                        }

                        For Each key$ In keys
                            Dim sz = g.MeasureString(key$, colLabelFont) ' 得到斜边的长度
                            Dim dx! = sz.Width * std.Cos(angle) + sz.Height / 2
                            Dim dy! = sz.Width * std.Sin(angle) + (sz.Width / 2) * std.Cos(angle) - sz.Height
                            Dim pos As New PointF(left - dx, top - dy)

                            Call text.DrawString(key$, colLabelFont, Brushes.Black, pos, angle, format)

                            left += dw
                        Next
                    End If

                    Dim titleSize = g.MeasureString(mainTitle, titleFont)
                    Dim titlePosi As New PointF With {
                        .X = args.matrixPlotRegion.Left + (args.matrixPlotRegion.Width - titleSize.Width) / 2, ' 标题在所绘制的矩阵上方居中
                        .Y = (padding.Top - titleSize.Height) / 2
                    }

                    Call g.DrawString(mainTitle, titleFont, Brushes.Black, titlePosi)
                End Sub

            Return g.GraphicsPlots(size, padding, bg$, plotInternal)
        End Function
    End Module
End Namespace
