#Region "Microsoft.VisualBasic::378ac06f6d1bb4a1e0b3ab77909e19a3, gr\Microsoft.VisualBasic.Imaging\PostScript\PSElements\Arc.vb"

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

    '   Total Lines: 61
    '    Code Lines: 52 (85.25%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (14.75%)
    '     File Size: 2.43 KB


    '     Class Arc
    ' 
    '         Properties: height, startAngle, stroke, sweepAngle, width
    '                     x, y
    ' 
    '         Function: GetSize, GetXy, ScaleTo
    ' 
    '         Sub: Paint, WriteAscii
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports std = System.Math

Namespace PostScript.Elements

    Public Class Pie : Inherits PSElement

        Public Property x As Single
        Public Property y As Single
        Public Property width As Single
        Public Property height As Single
        Public Property startAngle As Single
        Public Property sweepAngle As Single
        Public Property fill As String

        Friend Overrides Sub WriteAscii(ps As Writer)
            Throw New NotImplementedException()
        End Sub

        Friend Overrides Sub Paint(g As IGraphics)
            Call g.FillPie(fill.GetBrush, x, y, width, height, startAngle, sweepAngle)
        End Sub

        Friend Overrides Function GetXy() As PointF
            Return New PointF(x, y)
        End Function

        Friend Overrides Function GetSize() As SizeF
            Return New SizeF(width, height)
        End Function

        Friend Overrides Function ScaleTo(scaleX As d3js.scale.LinearScale, scaleY As d3js.scale.LinearScale) As PSElement
            Return New Pie With {
                .comment = comment,
                .fill = fill,
                .height = scaleY(height),
                .startAngle = startAngle,
                .sweepAngle = sweepAngle,
                .width = scaleX(width),
                .x = scaleX(x),
                .y = scaleY(y)
            }
        End Function
    End Class

    Public Class Arc : Inherits PSElement

        Public Property stroke As Stroke
        Public Property x As Single
        Public Property y As Single
        Public Property width As Single
        Public Property height As Single
        Public Property startAngle As Single
        Public Property sweepAngle As Single

        Friend Overrides Sub WriteAscii(ps As Writer)
            Dim startAngleRad As Double = startAngle * (std.PI / 180)
            Dim sweepAngleRad As Double = sweepAngle * (std.PI / 180)
            Dim startX As Double = x + (width / 2) + std.Cos(startAngleRad) * (width / 2)
            Dim startY As Double = y + (height / 2) + std.Sin(startAngleRad) * (height / 2)
            Dim endX As Double = x + (width / 2) + std.Cos(startAngleRad + sweepAngleRad) * (width / 2)
            Dim endY As Double = y + (height / 2) + std.Sin(startAngleRad + sweepAngleRad) * (height / 2)
            Dim pen As Pen = ps.pen(stroke)

            Call ps.linewidth(pen.Width)
            Call ps.color(pen.Color)
            Call ps.moveto(startX, startY)
            Call ps.arct(x, y, width, height, startAngle, sweepAngle)
            Call ps.stroke()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Overrides Sub Paint(g As IGraphics)
            Call g.DrawArc(g.LoadEnvironment.GetPen(stroke), x, y, width, height, startAngle, sweepAngle)
        End Sub

        Friend Overrides Function ScaleTo(scaleX As d3js.scale.LinearScale, scaleY As d3js.scale.LinearScale) As PSElement
            Return New Arc With {
                .height = scaleY(height),
                .startAngle = startAngle,
                .stroke = stroke,
                .sweepAngle = sweepAngle,
                .width = scaleX(width),
                .x = scaleX(x),
                .y = scaleY(y),
                .comment = comment
            }
        End Function

        Friend Overrides Function GetXy() As PointF
            Return New PointF(x, y)
        End Function

        Friend Overrides Function GetSize() As SizeF
            Return New SizeF(width, height)
        End Function
    End Class
End Namespace
