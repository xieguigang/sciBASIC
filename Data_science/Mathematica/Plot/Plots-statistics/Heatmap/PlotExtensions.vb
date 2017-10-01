#Region "Microsoft.VisualBasic::01137c06c38710c28b19e921c5204f32, ..\sciBASIC#\Data_science\Mathematica\Plot\Plots-statistics\Heatmap\PlotExtensions.vb"

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
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.Correlations.Correlations
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Public Module PlotExtensions


    ''' <summary>
    ''' 相比于<see cref="LoadDataSet(String, String, Boolean, Correlations.ICorrelation)"/>函数，这个函数处理的是没有经过归一化处理的原始数据
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="correlation">假若这个参数为空，则默认使用<see cref="Correlations.GetPearson(Double(), Double())"/></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function CorrelatesNormalized(
                                    data As IEnumerable(Of DataSet),
                    Optional correlation As Correlations.ICorrelation = Nothing) _
                                         As IEnumerable(Of NamedValue(Of Dictionary(Of String, Double)))

        Dim dataset As DataSet() = data.ToArray
        Dim keys$() = dataset(Scan0) _
                .Properties _
                .Keys _
                .ToArray

        If correlation Is Nothing Then
            correlation = AddressOf Correlations.GetPearson
        End If

        For Each x As DataSet In dataset
            Dim out As New Dictionary(Of String, Double)
            Dim array As Double() = keys.ToArray(Function(o$) x(o))

            For Each y As DataSet In dataset
                out(y.ID) = correlation(
                    array,
                    keys.ToArray(Function(o) y(o)))
            Next

            Yield New NamedValue(Of Dictionary(Of String, Double)) With {
                    .Name = x.ID,
                    .Value = out
                }
        Next
    End Function

    ''' <summary>
    ''' (这个函数是直接加在已经计算好了的相关度数据).假若使用这个直接加载数据来进行heatmap的绘制，
    ''' 请先要确保数据集之中的所有数据都是经过归一化的，假若没有归一化，则确保函数参数
    ''' <paramref name="normalization"/>的值为真
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="uidMap$"></param>
    ''' <param name="normalization">是否对输入的数据集进行归一化处理？</param>
    ''' <param name="correlation">
    ''' 默认为<see cref="Correlations.GetPearson(Double(), Double())"/>方法
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function LoadDataSet(path As String,
                                    Optional uidMap$ = Nothing,
                                    Optional normalization As Boolean = False,
                                    Optional correlation As ICorrelation = Nothing) As NamedValue(Of Dictionary(Of String, Double))()

        Dim ds As IEnumerable(Of DataSet) = DataSet.LoadDataSet(path, uidMap)

        If normalization Then
            Return ds.CorrelatesNormalized(correlation).ToArray
        Else
            Return LinqAPI.Exec(Of NamedValue(Of Dictionary(Of String, Double))) _
 _
                    () <= From x As DataSet
                          In ds
                          Select New NamedValue(Of Dictionary(Of String, Double)) With {
                              .Name = x.ID,
                              .Value = x.Properties
                          }
        End If
    End Function

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
            clusters = entityList.ClusterDataSet(n)
        End If

        Dim out As New List(Of NamedValue(Of Dictionary(Of String, Double)))

        ' 通过kmeans计算出keys的顺序
        Dim keysEntity = keys.ToArray(
            Function(k) New Entity With {
                .uid = k,
                .Properties = data.ToArray(Function(x) x.Value(k))
            })
        Dim keysOrder As New List(Of String)

        For Each cluster In keysEntity.ClusterDataSet(CInt(keys.Length / 5))
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
    Public Function Plot(ode As ODE, Optional size$ = "1600,1200", Optional padding$ = g.DefaultPadding, Optional bg$ = "white") As GraphicsData
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
                         Optional size$ = "1600,1200",
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
            From y As SeqValue(Of NamedCollection(Of Double))
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
