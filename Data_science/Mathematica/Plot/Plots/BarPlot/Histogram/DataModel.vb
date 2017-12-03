#Region "Microsoft.VisualBasic::329f25fe438a1d34e349abea124c43ad, ..\sciBASIC#\Data_science\Mathematica\Plot\Plots\BarPlot\Histogram\DataModel.vb"

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
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace BarPlot.Histogram

    ''' <summary>
    ''' {x, y}，一个柱子的绘图数据
    ''' </summary>
    ''' <remarks>
    ''' <see cref="x1"/>到<see cref="x2"/>之间的距离是直方图的宽度
    ''' </remarks>
    Public Structure HistogramData

        Public x1#, x2#, y#
        Public pointY#

        Public ReadOnly Property LinePoint As PointData
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New PointData With {
                    .pt = New PointF(x1 + width / 2, pointY)
                }
            End Get
        End Property

        ''' <summary>
        ''' delta between <see cref="x1"/> and <see cref="x2"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property width As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return x2# - x1#
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure

    Public Class HistogramGroup : Inherits ProfileGroup

        Public Property Samples As HistProfile()

        Public ReadOnly Property XRange As DoubleRange
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Samples _
                    .SafeQuery _
                    .Select(Function(s)
                                Return s.data.Select(Function(d) {d.x1, d.x2})
                            End Function) _
                    .IteratesALL _
                    .IteratesALL _
                    .Range
            End Get
        End Property

        Public ReadOnly Property YRange As DoubleRange
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Samples _
                    .SafeQuery _
                    .Select(Function(s)
                                Return s.data.Select(Function(d) d.y)
                            End Function) _
                    .IteratesALL _
                    .Range
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(data As IEnumerable(Of HistProfile))
            Samples = data
            Serials = data _
                .Select(Function(x) New NamedValue(Of Color) With {
                    .Name = x.legend.title,
                    .Value = x.legend.color.ToColor
                })
        End Sub
    End Class

    ''' <summary>
    ''' The histogram serial data.
    ''' </summary>
    Public Structure HistProfile

        ''' <summary>
        ''' The legend plot definition
        ''' </summary>
        Public legend As Legend
        Public data As HistogramData()

        Public ReadOnly Property SerialData As NamedValue(Of Color)
            Get
                Return New NamedValue(Of Color) With {
                    .Name = legend.title,
                    .Value = legend.color.TranslateColor
                }
            End Get
        End Property

        Public Function GetLine(color As Color, width!, ptSize!, Optional type As DashStyle = DashStyle.Solid) As SerialData
            Return New SerialData With {
                .color = color,
                .width = width,
                .lineType = type,
                .PointSize = ptSize,
                .pts = data.Select(Function(x) x.LinePoint)
            }
        End Function

        ''' <summary>
        ''' 仅仅在这里初始化了<see cref="data"/>
        ''' </summary>
        ''' <param name="range"></param>
        ''' <param name="func"></param>
        ''' <param name="steps#"></param>
        Sub New(range As DoubleRange, func As Func(Of Double, Double), Optional steps# = 0.01)
            Me.New(range.seq(steps).Select(func), range)
        End Sub

        ''' <summary>
        ''' 仅仅在这里初始化了<see cref="data"/>
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="xrange"></param>
        Sub New(data As IEnumerable(Of Double), xrange As DoubleRange)
            Dim array#() = data.ToArray
            Dim delta# = xrange.Length / array.Length
            Dim x As New Value(Of Double)(xrange.Min)

            Me.data = LinqAPI.Exec(Of HistogramData) <=
 _
                From n As Double
                In array
                Let x1 As Double = x
                Let x2 As Double = (x = x.Value + delta)
                Where Not n.IsNaNImaginary
                Select New HistogramData With {
                    .x1 = x1,
                    .x2 = x2,
                    .y = n
                }
        End Sub

        ''' <summary>
        ''' Tag值为直方图的高，value值为直方图的平均值连线
        ''' </summary>
        ''' <param name="hist"></param>
        Sub New(hist As Dictionary(Of Double, IntegerTagged(Of Double)), step!)
            data = hist.Select(
                Function(range) New HistogramData With {
                    .x1 = range.Key,
                    .x2 = .x1 + step!,
                    .y = range.Value.Tag,
                    .pointY = range.Value.Value
                }).ToArray
        End Sub
    End Structure
End Namespace
