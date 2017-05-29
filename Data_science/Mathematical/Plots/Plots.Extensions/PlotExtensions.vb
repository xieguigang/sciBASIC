#Region "Microsoft.VisualBasic::704b0559965c155f69d8bf278531b705, ..\sciBASIC#\Data_science\Mathematical\Plots\Plots.Extensions\PlotExtensions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot.Histogram
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.Calculus
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Public Module PlotExtensions

    ''' <summary>
    ''' 为heatmap的列和行之中的元素的排列位置提供排列顺序。<see cref="ReorderProvider"/>
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    <Extension>
    Public Function KmeansReorder(data As NamedValue(Of Dictionary(Of String, Double))(), Optional n% = 5) As NamedValue(Of Dictionary(Of String, Double))()
        Dim keys$() = data(Scan0%).Value.Keys.ToArray
        Dim entityList As Entity() = LinqAPI.Exec(Of Entity) <=
            From x As NamedValue(Of Dictionary(Of String, Double))
            In data
            Select New Entity With {
                .uid = x.Name,
                .Properties = keys.ToArray(Function(k) x.Value(k))
            }
        Dim clusters As ClusterCollection(Of Entity)

        n = entityList.Length / n

        If n = 0 OrElse entityList.Length <= 2 Then
            clusters = New ClusterCollection(Of Entity)

            For Each x As Entity In entityList
                Dim c As New KMeansCluster(Of Entity)

                Call c.Add(x)
                Call clusters.Add(c)
            Next
        Else
            clusters = ClusterDataSet(n, entityList)
        End If

        Dim out As New List(Of NamedValue(Of Dictionary(Of String, Double)))

        ' 通过kmeans计算出keys的顺序
        Dim keysEntity = keys.ToArray(
            Function(k) New Entity With {
                .uid = k,
                .Properties = data.ToArray(Function(x) x.Value(k))
            })
        Dim keysOrder As New List(Of String)

        For Each cluster In ClusterDataSet(CInt(keys.Length / 5), keysEntity)
            For Each k In cluster
                keysOrder += k.uid
            Next
        Next

        For Each cluster In clusters
            For Each entity As Entity In cluster
                out += New NamedValue(Of Dictionary(Of String, Double)) With {
                    .Name = entity.uid,
                    .Value = keysOrder _
                        .SeqIterator _
                        .ToDictionary(Function(x) x.value,
                                      Function(x) entity.Properties(x.i))
                }
            Next
        Next

        Return out
    End Function

    ''' <summary>
    ''' 从一系列的ODE计算结果之中构建出直方图的绘图数据模型
    ''' </summary>
    ''' <param name="odes"></param>
    ''' <param name="colors$"></param>
    ''' <returns></returns>
    <Extension>
    Public Function FromODEList(odes As IEnumerable(Of ODE), Optional colors$() = Nothing) As HistogramGroup
        Dim clData As Color() = If(
            colors.IsNullOrEmpty,
            ChartColors.Shuffles,
            colors.ToArray(AddressOf ToColor))
        Dim serials = LinqAPI.Exec(Of NamedValue(Of Color)) <=
 _
            From x As SeqValue(Of ODE)
            In odes.SeqIterator
            Select New NamedValue(Of Color) With {
                .Name = x.value.Id,
                .Value = clData(x.i)
            }

        Dim range As DoubleRange = odes.First.xrange
        Dim delta# = range.Length / odes.First.y.Length
        Dim samples = LinqAPI.Exec(Of HistProfile) <=
 _
            From out As SeqValue(Of ODE)
            In odes.SeqIterator
            Let left = New Value(Of Double)(range.Min)
            Select New HistProfile With {
                .legend = New Legend With {
                    .color = serials(out.i).Value.RGBExpression,
                    .fontstyle = CSSFont.Win10Normal,
                    .style = LegendStyles.Rectangle,
                    .title = serials(out.i).Name
                },
                .data = LinqAPI.Exec(Of HistogramData) <=
 _
                    From i As SeqValue(Of Double)
                    In out.value.y.SeqIterator
                    Let x1 As Double = left
                    Let x2 As Double = (left = left.value + delta)
                    Where Not i.value.IsNaNImaginary
                    Select New HistogramData With {
                        .x1 = x1,
                        .x2 = x2,
                        .y = i.value
                    }
            }

        Return New HistogramGroup With {
            .Samples = samples,
            .Serials = serials
        }
    End Function

    ''' <summary>
    ''' Scatter plot
    ''' </summary>
    ''' <param name="ode"></param>
    ''' <param name="color$"></param>
    ''' <param name="dash"></param>
    ''' <param name="ptSize"></param>
    ''' <param name="width"></param>
    ''' <returns></returns>
    <Extension>
    Public Function FromODE(ode As ODE, color$,
                            Optional dash As DashStyle = DashStyle.Dash,
                            Optional ptSize As Integer = 30,
                            Optional width As Single = 5) As SerialData

        Return New SerialData With {
            .title = ode.Id,
            .color = color.ToColor,
            .lineType = dash,
            .PointSize = ptSize,
            .width = width,
            .pts = LinqAPI.Exec(Of PointData) <=
                From x As SeqValue(Of Double)
                In ode.x.SeqIterator
                Select New PointData(CSng(x.value), CSng(ode.y(x.i)))
        }
    End Function

    ''' <summary>
    ''' 绘制积分计算的结果
    ''' </summary>
    ''' <param name="ode"></param>
    ''' <param name="size"></param>
    ''' <param name="bg"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Plot(ode As ODE, Optional size As Size = Nothing, Optional padding$ = g.DefaultPadding, Optional bg As String = "white") As GraphicsData
        Return Scatter.Plot({ode.FromODE("cyan")}, size, padding, bg)
    End Function

    ''' <summary>
    ''' 绘制常微分方程组的计算结果
    ''' </summary>
    ''' <param name="ode"></param>
    ''' <param name="size"></param>
    ''' <param name="bg"></param>
    ''' <param name="ptSize"></param>
    ''' <param name="width"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Plot(ode As ODEsOut,
                         Optional size As Size = Nothing,
                         Optional padding$ = g.DefaultPadding,
                         Optional bg As String = "white",
                         Optional ptSize As Single = 30,
                         Optional width As Single = 5) As GraphicsData
        Return Scatter.Plot(ode.FromODEs(, ptSize, width), size, padding, bg)
    End Function

    ''' <summary>
    ''' Convert ODEs result as scatter plot serial model.
    ''' </summary>
    ''' <param name="odes"></param>
    ''' <param name="colors"></param>
    ''' <param name="ptSize!"></param>
    ''' <param name="width"></param>
    ''' <returns></returns>
    <Extension>
    Public Function FromODEs(odes As ODEsOut,
                             Optional colors As IEnumerable(Of String) = Nothing,
                             Optional ptSize! = 30,
                             Optional width As Single = 5) As SerialData()
        Dim c As Color() = If(
            colors.IsNullOrEmpty,
            ChartColors.Shuffles,
            colors.ToArray(AddressOf ToColor))

        Return LinqAPI.Exec(Of SerialData) <=
 _
            From y As SeqValue(Of NamedValue(Of Double()))
            In odes.y.Values.SeqIterator
            Let pts As PointData() = odes.x _
                .SeqIterator _
                .ToArray(Function(x) New PointData(CSng(+x), CSng(y.value.Value(x))))
            Select New SerialData With {
                .color = c(y.i),
                .lineType = DashStyle.Solid,
                .PointSize = ptSize,
                .title = y.value.Name,
                .width = width,
                .pts = pts
            }
    End Function
End Module
