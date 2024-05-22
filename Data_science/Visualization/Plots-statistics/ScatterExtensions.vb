#Region "Microsoft.VisualBasic::6ebfee7ff26d32199ff36f28fbcf816e, Data_science\Visualization\Plots-statistics\ScatterExtensions.vb"

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

    '   Total Lines: 179
    '    Code Lines: 128 (71.51%)
    ' Comment Lines: 39 (21.79%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 12 (6.70%)
    '     File Size: 7.03 KB


    ' Module ScatterExtensions
    ' 
    '     Function: FromODE, FromODEList, FromODEs, (+2 Overloads) Plot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot.Histogram
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics.Data
Imports Microsoft.VisualBasic.MIME.HTML.CSS

Public Module ScatterExtensions

    ''' <summary>
    ''' 从一系列的ODE计算结果之中构建出直方图的绘图数据模型
    ''' </summary>
    ''' <param name="odes"></param>
    ''' <param name="colors$"></param>
    ''' <returns></returns>
    <Extension>
    Public Function FromODEList(odes As IEnumerable(Of ODEOutput), Optional colors$() = Nothing) As HistogramGroup
        Dim clData As Color() = If(
            colors.IsNullOrEmpty,
            ChartColors.Shuffles,
            colors.Select(AddressOf ToColor))
        Dim serials = LinqAPI.Exec(Of NamedValue(Of Color)) <=
 _
            From x As SeqValue(Of ODEOutput)
            In odes.SeqIterator
            Select New NamedValue(Of Color) With {
                .Name = x.value.ID,
                .Value = clData(x.i)
            }

        Dim range As DoubleRange = odes.First.xrange
        Dim delta# = range.Length / odes.First.Y.Length
        Dim samples = LinqAPI.Exec(Of HistProfile) <=
 _
            From out As SeqValue(Of ODEOutput)
            In odes.SeqIterator
            Let left = New Value(Of Double)(range.Min)
            Select New HistProfile With {
                .legend = New LegendObject With {
                    .color = serials(out.i).Value.RGBExpression,
                    .fontstyle = CSSFont.Win10Normal,
                    .style = LegendStyles.Rectangle,
                    .title = serials(out.i).Name
                },
                .data = LinqAPI.Exec(Of HistogramData) <=
 _
                    From i As SeqValue(Of Double)
                    In out.value.Y.vector.SeqIterator
                    Let x1 As Double = left
                    Let x2 As Double = (left = left.Value + delta)
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
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function FromODE(ode As ODEOutput, color$,
                            Optional dash As DashStyle = DashStyle.Dash,
                            Optional ptSize As Integer = 30,
                            Optional width As Single = 5) As SerialData

        Return New SerialData With {
            .title = ode.ID,
            .color = color.ToColor,
            .lineType = dash,
            .pointSize = ptSize,
            .width = width,
            .pts = ode.GetPointsData _
                .Select(Function(p)
                            Return New PointData(p)
                        End Function) _
                .ToArray
        }
    End Function

    ''' <summary>
    ''' 绘制积分计算的结果
    ''' </summary>
    ''' <param name="ode"></param>
    ''' <param name="size"></param>
    ''' <param name="bg"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Plot(ode As ODEOutput, Optional size$ = "1600,1200", Optional padding$ = g.DefaultPadding, Optional bg$ = "white") As GraphicsData
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
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Plot(ode As ODEsOut,
                         Optional size$ = "1600,1200",
                         Optional padding$ = g.DefaultPadding,
                         Optional bg As String = "white",
                         Optional ptSize As Single = 30,
                         Optional width As Single = 5) As GraphicsData
        Return Scatter.Plot(ode.FromODEs(, ptSize, width), size, padding, bg)
    End Function

    ReadOnly defaultColorSequence As [Default](Of Color()) = ChartColors

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

        Dim c As Color() = colors _
            .SafeQuery _
            .Select(AddressOf ToColor) Or [defaultColorSequence]

        Return LinqAPI.Exec(Of SerialData) _
 _
            () <= From y As SeqValue(Of NamedCollection(Of Double))
                  In odes.y.Values.SeqIterator
                  Let pts As PointData() = odes.x _
                       .SeqIterator _
                       .Select(Function(i)
                                   Dim x! = CSng(i.value)
                                   Dim yx! = CSng(y.value.value(i))
                                   Return New PointData(x!, yx!)
                               End Function)
                  Let color As Color = c(y.i)
                  Select New SerialData With {
                       .color = color,
                       .lineType = DashStyle.Solid,
                       .pointSize = ptSize,
                       .title = y.value.name,
                       .width = width,
                       .pts = pts
                   }
    End Function
End Module
