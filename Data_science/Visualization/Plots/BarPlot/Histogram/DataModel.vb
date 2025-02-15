#Region "Microsoft.VisualBasic::4de1e490dd07cc33423d0b9b4fd6cca7, Data_science\Visualization\Plots\BarPlot\Histogram\DataModel.vb"

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

    '   Total Lines: 213
    '    Code Lines: 151 (70.89%)
    ' Comment Lines: 39 (18.31%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 23 (10.80%)
    '     File Size: 7.25 KB


    '     Structure HistogramData
    ' 
    '         Properties: LinePoint, width
    ' 
    '         Function: ToString
    ' 
    '     Class HistogramGroup
    ' 
    '         Properties: Samples, XRange, YRange
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '     Structure HistProfile
    ' 
    '         Properties: SerialData
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: GetLine, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Serialization.JSON

#If NET48 Then
Imports Pen = System.Drawing.Pen
Imports Pens = System.Drawing.Pens
Imports Brush = System.Drawing.Brush
Imports Font = System.Drawing.Font
Imports Brushes = System.Drawing.Brushes
Imports SolidBrush = System.Drawing.SolidBrush
Imports DashStyle = System.Drawing.Drawing2D.DashStyle
Imports Image = System.Drawing.Image
Imports Bitmap = System.Drawing.Bitmap
Imports GraphicsPath = System.Drawing.Drawing2D.GraphicsPath
#Else
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports Pens = Microsoft.VisualBasic.Imaging.Pens
Imports Brush = Microsoft.VisualBasic.Imaging.Brush
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Brushes = Microsoft.VisualBasic.Imaging.Brushes
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
Imports GraphicsPath = Microsoft.VisualBasic.Imaging.GraphicsPath
#End If

Namespace BarPlot.Histogram

    ''' <summary>
    ''' {x, y}，一个柱子的绘图数据
    ''' </summary>
    ''' <remarks>
    ''' <see cref="x1"/>到<see cref="x2"/>之间的距离是直方图的宽度
    ''' </remarks>
    Public Structure HistogramData

        ''' <summary>
        ''' 数据区域范围的下限
        ''' </summary>
        Public x1#
        ''' <summary>
        ''' 数据区域范围的上限
        ''' </summary>
        Public x2#
        ''' <summary>
        ''' 频数
        ''' </summary>
        Public y#
        ''' <summary>
        ''' 一般为平均值
        ''' </summary>
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

        Public Shared Iterator Function CheckHighlightRange(data As IEnumerable(Of HistogramData), range As DoubleRange) As IEnumerable(Of HistogramData)
            Dim checkLeft As Boolean = True
            Dim checkRight As Boolean = True

            For Each bar As HistogramData In data.OrderBy(Function(a) a.x1)
                If checkLeft Then
                    If bar.x1 >= range.Min Then
                        Yield bar
                        checkLeft = False
                    End If
                ElseIf checkRight Then
                    If bar.x2 <= range.Max Then
                        Yield bar
                    Else
                        Exit For
                    End If
                Else
                    Exit For
                End If
            Next
        End Function

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
                .Select(Function(x)
                            Return New NamedValue(Of Color) With {
                                .Name = x.legend.title,
                                .Value = x.legend.color.ToColor
                            }
                        End Function) _
                .ToArray
        End Sub
    End Class

    ''' <summary>
    ''' The histogram serial data.
    ''' </summary>
    Public Structure HistProfile

        ''' <summary>
        ''' The legend plot definition
        ''' </summary>
        Public legend As LegendObject
        Public data As HistogramData()

        Public ReadOnly Property SerialData As NamedValue(Of Color)
            Get
                Return New NamedValue(Of Color) With {
                    .Name = legend.title,
                    .Value = legend.color.TranslateColor
                }
            End Get
        End Property

        Sub New(legend As LegendObject, data As HistogramData())
            Me.legend = legend
            Me.data = data
        End Sub

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

        Public Overrides Function ToString() As String
            Return legend.ToString
        End Function

        Public Function GetLine(color As Color, width!, ptSize!, Optional type As DashStyle = DashStyle.Solid) As SerialData
            Return New SerialData With {
                .color = color,
                .width = width,
                .lineType = type,
                .pointSize = ptSize,
                .pts = data _
                    .Select(Function(x) x.LinePoint) _
                    .ToArray
            }
        End Function

    End Structure
End Namespace
