#Region "Microsoft.VisualBasic::242e71af332e430898bc4deb1d4224a8, ..\sciBASIC#\Data_science\Mathematical\Plots\Histogram.vb"

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
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Shapes
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.Calculus
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' 对经由函数生成的连续数据的图形表述
''' </summary>
Public Module Histogram

    ''' <summary>
    ''' {x, y}
    ''' </summary>
    ''' <remarks>
    ''' <see cref="x1"/>到<see cref="x2"/>之间的距离是直方图的宽度
    ''' </remarks>
    Public Structure HistogramData

        Public x1#, x2#, y#

        ''' <summary>
        ''' delta between <see cref="x1"/> and <see cref="x2"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property width As Double
            Get
                Return x2# - x1#
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="color$">histogram bar fill color</param>
    ''' <param name="bg$">Output image background color</param>
    ''' <param name="size"></param>
    ''' <param name="margin"></param>
    ''' <param name="showGrid"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Plot(data As IEnumerable(Of HistogramData),
                         Optional color$ = "darkblue",
                         Optional bg$ = "white",
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional showGrid As Boolean = True) As Bitmap

        Return New HistogramGroup With {
            .Serials = {
                New NamedValue(Of Color) With {
                    .Name = NameOf(data),
                    .Value = color.ToColor(Drawing.Color.Blue)
                }
            },
            .Samples = {
                New HistProfile With {
                    .legend = New Legend With {
                        .color = color,
                        .fontstyle = CSSFont.Win10Normal,
                        .style = LegendStyles.Rectangle,
                        .title = NameOf(data)
                    },
                    .data = data.ToArray
                }
            }
        }.Plot(bg, size, margin, showGrid)
    End Function

    Public Function Plot(data As IEnumerable(Of Double), xrange As DoubleRange,
                         Optional color$ = "darkblue",
                         Optional bg$ = "white",
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional showGrid As Boolean = True) As Bitmap
        Dim hist As New HistProfile(data, xrange)
        Return Plot(hist.data, color, bg, size, margin, showGrid)
    End Function

    Public Function Plot(xrange As DoubleRange, expression As Func(Of Double, Double),
                         Optional steps# = 0.01,
                         Optional color$ = "darkblue",
                         Optional bg$ = "white",
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional showGrid As Boolean = True) As Bitmap
        Dim data As IEnumerable(Of Double) =
            xrange _
            .seq(steps) _
            .Select(expression)
        Return Plot(data, xrange, color, bg, size, margin, showGrid)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="xrange">For generates the variable value sequence for evaluate the <paramref name="expression"/></param>
    ''' <param name="expression$">Math expression in string format</param>
    ''' <param name="steps#">for <see cref="seq"/> function</param>
    ''' <param name="color$">The histogram bar fill color</param>
    ''' <param name="bg$"></param>
    ''' <param name="size"></param>
    ''' <param name="margin"></param>
    ''' <param name="showGrid"></param>
    ''' <returns></returns>
    Public Function Plot(xrange As NamedValue(Of DoubleRange), expression$,
                         Optional steps# = 0.01,
                         Optional color$ = "darkblue",
                         Optional bg$ = "white",
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional showGrid As Boolean = True) As Bitmap
        Dim data As New List(Of Double)
        Dim engine As New Expression

        For Each x# In xrange.Value.seq(steps)
            Call engine.SetVariable(xrange.Name, x#)
            data += engine.Evaluation(expression$)
        Next

        Return Plot(data, xrange.Value, color, bg, size, margin, showGrid)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="groups"></param>
    ''' <param name="bg$"></param>
    ''' <param name="size"></param>
    ''' <param name="margin"></param>
    ''' <param name="showGrid"></param>
    ''' <param name="legendPos">The legend position on the output image.</param>
    ''' <param name="legendBorder"></param>
    ''' <param name="alpha">Fill color alpha value, [0, 255]</param>
    ''' <returns></returns>
    <Extension>
    Public Function Plot(groups As HistogramGroup,
                         Optional bg$ = "white",
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional showGrid As Boolean = True,
                         Optional legendPos As Point = Nothing,
                         Optional legendBorder As Border = Nothing,
                         Optional alpha% = 255,
                         Optional drawRect As Boolean = True) As Bitmap

        Return GraphicsPlots(
           size, margin,
           bg$,
           Sub(ByRef g, region)
               Dim mapper As New Scaling(groups)
               Dim annotations = groups.Serials.ToDictionary

               Call g.DrawAxis(size, margin, mapper, showGrid)

               For Each hist As HistProfile In mapper.ForEach_histSample(size, margin)
                   Dim ann As NamedValue(Of Color) =
                       annotations(hist.legend.title)
                   Dim b As New SolidBrush(Drawing.Color.FromArgb(alpha, ann.Value))

                   For Each block As HistogramData In hist.data
                       Dim rect As New RectangleF(
                           New PointF(block.x1, block.y),
                           New SizeF(block.width, region.PlotRegion.Bottom - block.y))
                       Call g.FillRectangle(b, rect)
                       If drawRect Then
                           Call g.DrawRectangle(
                                Pens.Black,
                                rect.Left, rect.Top, rect.Width, rect.Height)
                       End If
                   Next
               Next

               If legendPos.IsEmpty Then
                   legendPos = New Point(
                       CInt(size.Width * 0.8),
                       margin.Height)
               End If

               Call g.DrawLegends(
                    legendPos,
                    groups.Samples _
                          .Select(Function(x) x.legend),
                    ,,
                    legendBorder)
           End Sub)
    End Function

    Public Class HistogramGroup : Inherits ProfileGroup

        Public Property Samples As HistProfile()

        Sub New()
        End Sub

        Sub New(data As IEnumerable(Of HistProfile))
            Samples = data
            Serials = data _
                .ToArray(Function(x) New NamedValue(Of Color) With {
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
                Let x2 As Double = (x = x.value + delta)
                Where Not n.IsNaNImaginary
                Select New HistogramData With {
                    .x1 = x1,
                    .x2 = x2,
                    .y = n
                }
        End Sub
    End Structure

    Public Function FromODE(odes As IEnumerable(Of ODE), Optional colors$() = Nothing) As HistogramGroup
        Dim clData As Color() = If(
            colors.IsNullOrEmpty,
            ChartColors.Shuffles,
            colors.ToArray(AddressOf ToColor))
        Dim serials = LinqAPI.Exec(Of NamedValue(Of Color)) <=
 _
            From x As SeqValue(Of ODE)
            In odes.SeqIterator
            Select New NamedValue(Of Color) With {
                .Name = x.obj.Id,
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
                    In out.obj.y.SeqIterator
                    Let x1 As Double = left
                    Let x2 As Double = (left = left.value + delta)
                    Where Not i.obj.IsNaNImaginary
                    Select New HistogramData With {
                        .x1 = x1,
                        .x2 = x2,
                        .y = i.obj
                    }
            }

        Return New HistogramGroup With {
            .Samples = samples,
            .Serials = serials
        }
    End Function
End Module
