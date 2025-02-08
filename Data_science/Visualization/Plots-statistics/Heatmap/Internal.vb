#Region "Microsoft.VisualBasic::ee5e04398f5fc205e59e67b7f556854c, Data_science\Visualization\Plots-statistics\Heatmap\Internal.vb"

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

    '   Total Lines: 395
    '    Code Lines: 278 (70.38%)
    ' Comment Lines: 63 (15.95%)
    '    - Xml Docs: 50.79%
    ' 
    '   Blank Lines: 54 (13.67%)
    '     File Size: 18.76 KB


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
        <Extension>
        Public Function Log(v As Vector, base#) As Vector
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


                End Sub

            Return g.GraphicsPlots(size, padding, bg$, plotInternal)
        End Function
    End Module
End Namespace
