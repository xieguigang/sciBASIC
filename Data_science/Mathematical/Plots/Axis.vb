#Region "Microsoft.VisualBasic::ca519ff8d2a0e8a26449602f0dee934c, ..\sciBASIC#\Data_science\Mathematical\Plots\Axis.vb"

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

Module Axis

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
                        Optional ylabel$ = "")

        Dim o As New Point(margin.Width + offset.X, size.Height - margin.Height + offset.Y) ' 坐标轴原点
        Dim right As New Point(size.Width - margin.Width + offset.X, o.Y + offset.Y)  ' X轴
        Dim top As New Point(margin.Width + offset.X, margin.Height + offset.Y)       ' Y轴
        Dim pen As New Pen(Color.Black, 5)

        Call g.DrawLine(pen, o, right)
        Call g.DrawLine(pen, o, top)

        Dim fontLarge As New Font(FontFace.MicrosoftYaHei, 28, FontStyle.Regular)
        Call g.DrawString(scaler.xmin, fontLarge, Brushes.Black, New PointF(o.X + 10, o.Y + 10))
        Call g.DrawString(xlabel, fontLarge, Brushes.Black, New PointF(right.X + 20, right.Y - 5))
        Call g.DrawString(ylabel, fontLarge, Brushes.Black, New PointF(top.X - 10, top.Y - 50))

        Dim fontSmall As New Font(FontFace.MicrosoftYaHei, 14)
        fontLarge = New Font(FontFace.MicrosoftYaHei, 20, FontStyle.Regular)

        Dim dx As Single = scaler.dx / 10 '+ scaler.xmin
        Dim dy As Single = scaler.dy / 10 '+ scaler.ymin
        Dim sx = scaler.XScaler(size, margin)
        Dim sy = scaler.YScaler(size, margin)
        Dim gridPenX As New Pen(Color.LightGray, 1) With {
            .DashStyle = Drawing2D.DashStyle.Dash
        }
        Dim gridPenY As New Pen(Color.LightGray, 1) With {
            .DashStyle = Drawing2D.DashStyle.Dot
        }

        pen = New Pen(Color.Black, 3)

        For i As Integer = 0 To 9
            Dim label# = dx * (i + 1)
            Dim sz As SizeF

            If dx <> 0R Then
                Dim x = sx(label + scaler.xmin) + offset.X
                Dim axisX As New PointF(x, o.Y)

                Dim labelText = (label + scaler.xmin).FormatNumeric(2)
                sz = g.MeasureString(labelText, fontLarge)

                Call g.DrawLine(pen, axisX, New PointF(x, o.Y + margin.Height * 0.2))
                Call g.DrawString(labelText, fontLarge, Brushes.Black, New Point(x - sz.Width / 2, o.Y + margin.Height * 0.3))

                If showGrid Then
                    Call g.DrawLine(gridPenX, axisX, New PointF(x, margin.Height))
                End If
            End If

            label = dy * (i + 1)

            If dy <> 0R Then
                Dim y = sy(label + scaler.ymin) + offset.Y
                Dim axisY As New PointF(o.X, y)
                Dim ddd = 10

                Call g.DrawLine(pen, axisY, New PointF(o.X - ddd, y))

                Dim labelText = (label + scaler.ymin).FormatNumeric(2)
                sz = g.MeasureString(labelText, fontSmall)
                g.DrawString(labelText, fontSmall, Brushes.Black, New Point(o.X - ddd - sz.Width, y - sz.Height / 2))

                If showGrid Then
                    Call g.DrawLine(gridPenY, axisY, New PointF(size.Width - margin.Width, y))
                End If
            End If
        Next
    End Sub
End Module
