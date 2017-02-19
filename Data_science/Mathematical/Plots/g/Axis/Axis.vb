#Region "Microsoft.VisualBasic::c768083fcac42c909dbb967e85d1f1d2, ..\sciBASIC#\Data_science\Mathematical\Plots\Axis.vb"

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
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Public Module Axis

    <Extension>
    Public Sub DrawAxis(ByRef g As Graphics, region As GraphicsRegion,
                        scaler As Scaling,
                        showGrid As Boolean,
                        Optional offset As Point = Nothing,
                        Optional xlabel$ = "",
                        Optional ylabel$ = "")
        With region
            Call g.DrawAxis(.Size, .Margin,
                scaler,
                showGrid,
                offset, xlabel, ylabel)
        End With
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="size"></param>
    ''' <param name="margin"></param>
    ''' <param name="scaler">Drawing Point data auto scaler</param>
    ''' <param name="showGrid">Show axis grid on the plot region?</param>
    <Extension>
    Public Sub DrawAxis(ByRef g As Graphics,
                        size As Size,
                        margin As Size,
                        scaler As Scaling,
                        showGrid As Boolean,
                        Optional offset As Point = Nothing,
                        Optional xlabel$ = "",
                        Optional ylabel$ = "",
                        Optional labelFontStyle$ = CSSFont.UbuntuLarge)

        Dim ZERO As New Point(margin.Width + offset.X, size.Height - margin.Height + offset.Y) ' 坐标轴原点
        Dim right As New Point(size.Width - margin.Width + offset.X, ZERO.Y + offset.Y)  ' X轴
        Dim top As New Point(margin.Width + offset.X, margin.Height + offset.Y)       ' Y轴
        Dim pen As New Pen(Color.Black, 5)

        Call g.DrawLine(pen, ZERO, right)
        Call g.DrawLine(pen, ZERO, top)

        Dim fontLarge As Font = CSSFont.TryParse(labelFontStyle)
        Call g.DrawString(scaler.xmin, fontLarge, Brushes.Black, New PointF(ZERO.X + 10, ZERO.Y + 10))
        Call g.DrawString(xlabel, fontLarge, Brushes.Black, New PointF(right.X + 20, right.Y - 5))
        Call g.DrawString(ylabel, fontLarge, Brushes.Black, New PointF(top.X - 10, top.Y - 50))

        Dim fontSmall As New Font(FontFace.MicrosoftYaHei, 14)

        Dim dx As Double() = AxisScalling.GetAxisValues(scaler.xrange) '+ scaler.xmin
        Dim dy As Double() = AxisScalling.GetAxisValues(scaler.yrange) '+ scaler.ymin
        Dim sx = scaler.XScaler(size, margin)
        Dim sy = scaler.YScaler(size, margin)
        Dim gridPenX As New Pen(Color.LightGray, 1) With {
            .DashStyle = Drawing2D.DashStyle.Dash
        }
        Dim gridPenY As New Pen(Color.LightGray, 1) With {
            .DashStyle = Drawing2D.DashStyle.Dot
        }

        pen = New Pen(Color.Black, 3)
        fontLarge = New Font(FontFace.MicrosoftYaHei, 20, FontStyle.Regular)

        For i As Integer = 0 To 9
            Dim label# = dx(i)
            Dim sz As SizeF

            If scaler.dx <> 0R Then
                Dim x = sx(label) + offset.X
                Dim axisX As New PointF(x, ZERO.Y)

                Dim labelText = (label).FormatNumeric(2)
                sz = g.MeasureString(labelText, fontLarge)

                Call g.DrawLine(pen, axisX, New PointF(x, ZERO.Y + margin.Height * 0.2))
                Call g.DrawString(labelText, fontLarge, Brushes.Black, New Point(x - sz.Width / 2, ZERO.Y + margin.Height * 0.3))

                If showGrid Then
                    Call g.DrawLine(gridPenX, axisX, New PointF(x, margin.Height))
                End If
            End If

            label = dy(i)

            If scaler.dy <> 0R Then
                Dim y = sy(label) + offset.Y
                Dim axisY As New PointF(ZERO.X, y)
                Dim ddd = 10

                Call g.DrawLine(pen, axisY, New PointF(ZERO.X - ddd, y))

                Dim labelText = (label).FormatNumeric(2)
                sz = g.MeasureString(labelText, fontSmall)
                g.DrawString(labelText, fontSmall, Brushes.Black, New Point(ZERO.X - ddd - sz.Width, y - sz.Height / 2))

                If showGrid Then
                    Call g.DrawLine(gridPenY, axisY, New PointF(size.Width - margin.Width, y))
                End If
            End If
        Next
    End Sub
End Module
