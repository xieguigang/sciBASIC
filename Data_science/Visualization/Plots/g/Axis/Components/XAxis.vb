#Region "Microsoft.VisualBasic::f47c0d1dbe063c228365e23d1510d3e6, G:/GCModeller/src/runtime/sciBASIC#/Data_science/Visualization/Plots//g/Axis/Components/XAxis.vb"

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

    '   Total Lines: 190
    '    Code Lines: 162
    ' Comment Lines: 5
    '   Blank Lines: 23
    '     File Size: 7.34 KB


    '     Class XAxis
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Sub: Draw, drawLabel, drawTicks
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.d3js.scale
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser
Imports std = System.Math

Namespace Graphic.Axis

    Public Class XAxis

        ReadOnly plotRegion As Rectangle
        ReadOnly overridesTickLine As Double
        ReadOnly pen As Pen
        ReadOnly noTicks As Boolean
        ReadOnly axisTicks As Vector
        ReadOnly scaler As Scaler
        ReadOnly tickFormat As String
        ReadOnly tickFont As Font
        ReadOnly tickColor As Brush
        ReadOnly label As String
        ReadOnly htmlLabel As Boolean
        ReadOnly xRotate As Double
        ReadOnly labelFont As String
        ReadOnly labelColor As Brush

        Sub New(scaler As DataScaler,
                pen As Pen,
                overridesTickLine As Double,
                noTicks As Boolean,
                tickFormat As String,
                tickfont As Font,
                tickColor As Brush,
                label As String,
                labelFont As String,
                labelColor As Brush,
                htmlLabel As Boolean,
                xRotate As Double)

            Me.plotRegion = scaler.region
            Me.overridesTickLine = overridesTickLine
            Me.pen = pen
            Me.noTicks = noTicks
            Me.axisTicks = scaler.AxisTicks.X
            Me.scaler = scaler.X
            Me.tickFormat = tickFormat
            Me.tickFont = tickfont
            Me.tickColor = tickColor
            Me.label = label
            Me.labelFont = labelFont
            Me.htmlLabel = htmlLabel
            Me.xRotate = xRotate
            Me.labelColor = labelColor
        End Sub

        Sub New(plotRegion As Rectangle,
                scaler As Scaler,
                ticks As Vector,
                pen As Pen,
                overridesTickLine As Double,
                noTicks As Boolean,
                tickFormat As String,
                tickfont As Font,
                tickColor As Brush,
                label As String,
                labelFont As String,
                labelColor As Brush,
                htmlLabel As Boolean,
                xRotate As Double)

            Me.plotRegion = plotRegion
            Me.scaler = scaler
            Me.overridesTickLine = overridesTickLine
            Me.pen = pen
            Me.noTicks = noTicks
            Me.axisTicks = ticks
            Me.tickFormat = tickFormat
            Me.tickFont = tickfont
            Me.tickColor = tickColor
            Me.label = label
            Me.labelFont = labelFont
            Me.htmlLabel = htmlLabel
            Me.xRotate = xRotate
            Me.labelColor = labelColor
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="layout"></param>
        ''' <param name="y0">the plot location of y value when y axis value is ZERO</param>
        ''' <param name="offset"></param>
        Public Sub Draw(g As IGraphics, layout As XAxisLayoutStyles, y0 As Double, offset As PointF)
            Dim Y% = plotRegion.Top + offset.Y
            Dim size As Size = plotRegion.Size

            Select Case layout
                Case XAxisLayoutStyles.Centra
                    Y += size.Height / 2 + offset.Y
                Case XAxisLayoutStyles.Top
                    Y += 0
                Case XAxisLayoutStyles.ZERO
                    Y = y0 + offset.Y
                Case Else
                    Y += size.Height
            End Select

            ' 坐标轴原点
            Dim ZERO As New Point(plotRegion.Left + offset.X, Y)
            ' X轴
            Dim right As New Point(ZERO.X + size.Width, Y)
            Dim d! = If(overridesTickLine <= 0, 10, overridesTickLine)

            ' X轴
            Call g.DrawLine(pen, ZERO, right)

            If Not noTicks Then
                Call drawTicks(g, offset, ZERO, d)
            End If

            If Not label.StripHTMLTags(stripBlank:=True).StringEmpty Then
                Call drawLabel(g, size, d, ZERO)
            End If
        End Sub

        Private Sub drawTicks(g As IGraphics, offset As PointF, ZERO As Point, d As Single)
            ' 绘制坐标轴标签
            Dim ticks As (x#, label$)()

            If TypeOf scaler Is LinearScale Then
                ticks = axisTicks _
                    .Select(Function(tick)
                                If std.Abs(tick) <= 0.000001 Then
                                    Return (scaler(tick), "0")
                                Else
                                    Return (scaler(tick), (tick).ToString(tickFormat))
                                End If
                            End Function) _
                    .ToArray
            Else
                ticks = DirectCast(scaler, OrdinalScale) _
                    .getTerms _
                    .Select(Function(tick) (scaler(tick.value), tick.value)) _
                    .ToArray
            End If

            For Each tick As (X#, label$) In ticks
                Dim x As Single = tick.X + offset.X
                Dim axisX As New PointF(x, ZERO.Y)
                Dim labelText = tick.label
                Dim sz As SizeF = g.MeasureString(labelText, tickFont)

                Call g.DrawLine(pen, axisX, New PointF(x, ZERO.Y + d!))

                If xRotate <> 0 AndAlso TypeOf g Is Graphics2D Then
                    Dim text As New GraphicsText(g)

                    If xRotate > 0 Then
                        Call text.DrawString(labelText, tickFont, tickColor, New Point(x, ZERO.Y + d * 1.2), angle:=xRotate)
                    Else
                        Call text.DrawString(labelText, tickFont, tickColor, New Point(x, ZERO.Y + sz.Height * std.Sin(xRotate * 180 / std.PI)), angle:=xRotate)
                    End If
                Else
                    Call g.DrawString(labelText, tickFont, tickColor, New Point(x - sz.Width / 2, ZERO.Y + d * 1.2))
                End If
            Next
        End Sub

        Private Sub drawLabel(g As IGraphics, size As Size, d As Double, zero As Point)
            If htmlLabel Then
                Dim labelImage As Image = label.__plotLabel(labelFont, False)
                Dim point As New Point With {
                    .X = (size.Width - labelImage.Width) / 2 + plotRegion.Left,
                    .Y = zero.Y + size.Height + tickFont.Height + d * 4
                }

                Call g.DrawImageUnscaled(labelImage, point)
            Else
                Dim font As Font = CSSFont.TryParse(labelFont).GDIObject(g.Dpi)
                Dim fSize As SizeF = g.MeasureString(label, font)
                Dim y1 As Double = zero.Y + tickFont.Height + d * 3
                Dim y2 As Double = zero.Y + ((g.Size.Height - plotRegion.Bottom) - fSize.Height) / 2
                Dim point As New PointF With {
                    .X = (size.Width - fSize.Width) / 2 + plotRegion.Left,
                    .Y = std.Max(y1, y2)
                }

                ' Call $"[X:={label}] {point.ToString}".__INFO_ECHO
                Call g.DrawString(label, font, labelColor, point)
            End If
        End Sub

    End Class
End Namespace
