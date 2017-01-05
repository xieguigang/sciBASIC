#Region "Microsoft.VisualBasic::5ec7fa628375e7c4e2ec9d4afd783e64, ..\sciBASIC#\Data_science\Mathematical\Plots\Plots.Extensions\PlotExtensions.vb"

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
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.Calculus
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Public Module PlotExtensions

    ''' <summary>
    ''' <see cref="ReorderProvider"/>
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    <Extension>
    Public Function KmeansReorder(data As NamedValue(Of Dictionary(Of String, Double))()) As NamedValue(Of Dictionary(Of String, Double))()
        Dim keys$() = data(Scan0%).Value.Keys.ToArray
        Dim entityList As Entity() = LinqAPI.Exec(Of Entity) <=
            From x As NamedValue(Of Dictionary(Of String, Double))
            In data
            Select New Entity With {
                .uid = x.Name,
                .Properties = keys.ToArray(Function(k) x.Value(k))
            }
        Dim clusters = ClusterDataSet(entityList.Length / 5, entityList)
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

    <Extension>
    Public Function Plot(ode As ODE, Optional size As Size = Nothing, Optional margin As Size = Nothing, Optional bg As String = "white") As Bitmap
        Return Scatter.Plot({ode.FromODE("cyan")}, size, margin, bg)
    End Function

    <Extension>
    Public Function Plot(ode As ODEsOut,
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional bg As String = "white",
                         Optional ptSize As Single = 30,
                         Optional width As Single = 5) As Bitmap
        Return Scatter.Plot(ode.FromODEs(, ptSize, width), size, margin, bg)
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
